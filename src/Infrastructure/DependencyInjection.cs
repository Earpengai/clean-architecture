using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Application.Abstractions.Authentication;
using Application.Abstractions.Billing;
using Application.Abstractions.Data;
using Application.Abstractions.Email;
using Application.Abstractions.Jobs;
using Application.Abstractions.SubscriptionFeatures;
using Domain.Tenants;
using Domain.Users;
using DomainRole = Domain.Tenants.Role;
using Finbuckle.MultiTenant;
using Infrastructure.Authentication;
using Infrastructure.Authorization;
using Infrastructure.Billing;
using Infrastructure.Data;
using Infrastructure.Database;
using Infrastructure.DomainEvents;
using Infrastructure.Email;
using Infrastructure.Jobs;
using Infrastructure.Multitenancy;
using Infrastructure.SubscriptionFeatures;
using Infrastructure.Time;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;
using StackExchange.Redis;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment) =>
        services
            .AddServices(configuration)
            .AddDatabase(configuration)
            .AddIdentityInternal()
            .AddHealthChecks(configuration)
            .AddAuthenticationInternal(configuration, environment)
            .AddAuthorizationInternal()
            .AddMultiTenancy();

    private static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        services.AddTransient<DataSeeder>();

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            string? connectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";

            var options = ConfigurationOptions.Parse(connectionString);
            options.AbortOnConnectFail = false;

            return ConnectionMultiplexer.Connect(options);
        });

        services.AddScoped<IBackgroundJobQueue, DatabaseBackgroundJobQueue>();

        services.AddScoped<IEmailService, QueuedEmailService>();

        services.AddScoped<MailHogEmailService>();

        services.AddScoped<IBackgroundJobHandler<SendEmailJob>, SendEmailJobHandler>();

        services.AddScoped<ISubscriptionFeatureProvider, SubscriptionFeatureProvider>();

        services.Configure<BakongOptions>(configuration.GetSection(BakongOptions.Section));

        services.AddHttpClient();

        services.AddScoped<IBakongService, BakongService>();

        services.AddHostedService<BackgroundJobProcessor>();

        services.AddHostedService<SubscriptionExpirationService>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        services.AddSingleton<TenantSaveInterceptor>();

        services.AddDbContext<ApplicationDbContext>(
            (sp, options) => options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<TenantSaveInterceptor>()));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    private static IServiceCollection AddIdentityInternal(this IServiceCollection services)
    {
        services.AddIdentityCore<User>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddRoles<DomainRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Database")!)
            .AddRedis(configuration["Redis:ConnectionString"] ?? "localhost:6379");

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = !environment.IsDevelopment();
                o.MapInboundClaims = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                };

                o.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        IServiceScopeFactory scopeFactory = context.HttpContext.RequestServices
                            .GetRequiredService<IServiceScopeFactory>();

                        using IServiceScope scope = scopeFactory.CreateScope();

                        ILoggerFactory loggerFactory = scope.ServiceProvider
                            .GetRequiredService<ILoggerFactory>();
                        ILogger logger = loggerFactory.CreateLogger("JwtSecurityStamp");

                        string? userId = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
                        string? securityStampClaim = context.Principal?.FindFirstValue("security_stamp");

                        if (userId is null || securityStampClaim is null)
                        {
                            logger.LogWarning(
                                "JWT missing required claims. Sub='{Sub}', SecurityStamp='{Stamp}' - Path: {Path}",
                                userId ?? "null",
                                securityStampClaim ?? "null",
                                context.HttpContext.Request.Path);

                            context.Fail("Token is missing required claims.");
                            return;
                        }

                        if (!Guid.TryParse(userId, out Guid parsedUserId))
                        {
                            logger.LogWarning(
                                "JWT contains invalid user identifier: '{UserId}' - Path: {Path}",
                                userId,
                                context.HttpContext.Request.Path);

                            context.Fail("Invalid user identifier in token.");
                            return;
                        }

                        try
                        {
                            UserManager<User> userManager = scope.ServiceProvider
                                .GetRequiredService<UserManager<User>>();

                            User? user = await userManager.FindByIdAsync(parsedUserId.ToString());

                            if (user is null)
                            {
                                logger.LogWarning(
                                    "JWT user not found: '{UserId}' - Path: {Path}",
                                    parsedUserId,
                                    context.HttpContext.Request.Path);

                                context.Fail("User no longer exists.");
                                return;
                            }

                            if (!string.Equals(securityStampClaim, user.SecurityStamp, StringComparison.Ordinal))
                            {
                                logger.LogWarning(
                                    "JWT security stamp mismatch for user '{UserId}'. Token invalidated. Path: {Path}",
                                    parsedUserId,
                                    context.HttpContext.Request.Path);

                                context.Fail("Security stamp has changed. Token is no longer valid.");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex,
                                "JWT security stamp validation failed for user '{UserId}'. Path: {Path}",
                                parsedUserId,
                                context.HttpContext.Request.Path);

                            context.Fail("Token validation error.");
                        }
                    },
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/problem+json";

                        string detail = !string.IsNullOrEmpty(context.ErrorDescription)
                            ? context.ErrorDescription
                            : "Authentication is required to access this resource.";

                        var problem = new
                        {
                            type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                            title = "Unauthorized",
                            status = StatusCodes.Status401Unauthorized,
                            detail,
                            instance = context.HttpContext.Request.Path.ToString()
                        };

                        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
                    }
                };
            });

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<ITokenProvider, TokenProvider>();

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization();

        services.AddScoped<PermissionProvider>();

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        services.AddTransient<IAuthorizationHandler, SubscriptionFeatureAuthorizationHandler>();

        return services;
    }

    private static IServiceCollection AddMultiTenancy(this IServiceCollection services)
    {
        services.AddMultiTenant<AppTenantInfo>()
            .WithStrategy<TenantMembershipStrategy>(ServiceLifetime.Scoped)
            .WithStore<TenantStore>(ServiceLifetime.Scoped);

        return services;
    }

    public static IApplicationBuilder UseTenantEnforcement(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantEnforcementMiddleware>();
    }
}
