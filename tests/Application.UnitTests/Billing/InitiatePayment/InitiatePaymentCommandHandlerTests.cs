using Application.Abstractions.Authentication;
using Application.Abstractions.Billing;
using Application.Abstractions.Data;
using Application.Abstractions.Jobs;
using Application.Abstractions.Messaging;
using Application.Billing.InitiatePayment;
using Application.Billing.ProcessPayment;
using Application.UnitTests.TestHelpers;
using Domain.Payments;
using Domain.Subscriptions;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Billing.InitiatePayment;

public sealed class InitiatePaymentCommandHandlerTests
{
    private readonly IApplicationDbContext _context = Substitute.For<IApplicationDbContext>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly IBakongService _bakongService = Substitute.For<IBakongService>();
    private readonly IBackgroundJobQueue _jobQueue = Substitute.For<IBackgroundJobQueue>();
    private readonly InitiatePaymentCommandHandler _handler;

    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public InitiatePaymentCommandHandlerTests()
    {
        _userContext.TenantId.Returns(_tenantId);
        _userContext.UserId.Returns(_userId);

        _handler = new InitiatePaymentCommandHandler(_context, _userContext, _bakongService, _jobQueue);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsNotOwner()
    {
        List<Membership> memberships = [];
        DbSet<Membership> membershipDbSet = CreateAsyncMockDbSet(memberships);
        _context.Memberships.Returns(membershipDbSet);

        var command = new InitiatePaymentCommand(Guid.NewGuid(), SubscriptionBillingPeriod.Monthly);

        Result<InitiatePaymentResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PaymentErrors.NotOwner);
        await _jobQueue.DidNotReceive().EnqueueAsync(
            Arg.Any<ProcessPaymentJob>(),
            Arg.Any<DateTime?>(),
            Arg.Any<int?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTenantNotFound()
    {
        Membership membership = new() { UserId = _userId, TenantId = _tenantId, Role = new Role { Name = "Owner" } };
        List<Membership> memberships = [membership];
        DbSet<Membership> membershipDbSet = CreateAsyncMockDbSet(memberships);
        _context.Memberships.Returns(membershipDbSet);

        List<Tenant> tenants = [];
        DbSet<Tenant> tenantDbSet = CreateAsyncMockDbSet(tenants);
        _context.Tenants.Returns(tenantDbSet);

        var command = new InitiatePaymentCommand(Guid.NewGuid(), SubscriptionBillingPeriod.Monthly);

        Result<InitiatePaymentResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(TenantErrors.NotFound(_tenantId));
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPlanNotFound()
    {
        Membership membership = new() { UserId = _userId, TenantId = _tenantId, Role = new Role { Name = "Owner" } };
        List<Membership> memberships = [membership];
        DbSet<Membership> membershipDbSet = CreateAsyncMockDbSet(memberships);
        _context.Memberships.Returns(membershipDbSet);

        Tenant tenant = new() { Id = _tenantId, Name = "Test Tenant" };
        List<Tenant> tenants = [tenant];
        DbSet<Tenant> tenantDbSet = CreateAsyncMockDbSet(tenants);
        _context.Tenants.Returns(tenantDbSet);

        List<SubscriptionPlan> plans = [];
        DbSet<SubscriptionPlan> planDbSet = CreateAsyncMockDbSet(plans);
        _context.SubscriptionPlans.Returns(planDbSet);

        var planId = Guid.NewGuid();
        var command = new InitiatePaymentCommand(planId, SubscriptionBillingPeriod.Monthly);

        Result<InitiatePaymentResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SubscriptionErrors.PlanNotFound(planId));
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_AndEnqueueBackgroundJob()
    {
        Membership membership = new() { UserId = _userId, TenantId = _tenantId, Role = new Role { Name = "Owner" } };
        List<Membership> memberships = [membership];
        DbSet<Membership> membershipDbSet = CreateAsyncMockDbSet(memberships);
        _context.Memberships.Returns(membershipDbSet);

        Tenant tenant = new() { Id = _tenantId, Name = "Test Tenant" };
        List<Tenant> tenants = [tenant];
        DbSet<Tenant> tenantDbSet = CreateAsyncMockDbSet(tenants);
        _context.Tenants.Returns(tenantDbSet);

        var planId = Guid.NewGuid();
        SubscriptionPlan plan = new()
        {
            Id = planId,
            Name = "Pro",
            PriceMonthly = 29.99m,
            PriceYearly = 299.99m
        };
        List<SubscriptionPlan> plans = [plan];
        DbSet<SubscriptionPlan> planDbSet = CreateAsyncMockDbSet(plans);
        _context.SubscriptionPlans.Returns(planDbSet);

        List<Payment> payments = [];
        DbSet<Payment> paymentDbSet = CreateAsyncMockDbSet(payments);
        _context.Payments.Returns(paymentDbSet);

        QrGenerationResult qrResult = new() { Qr = "qr-data", Md5 = "md5-hash" };
        _bakongService.GenerateQrAsync(Arg.Any<BakongGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(qrResult);

        _context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new InitiatePaymentCommand(planId, SubscriptionBillingPeriod.Monthly);

        Result<InitiatePaymentResponse> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Qr.ShouldBe("qr-data");
        result.Value.Md5.ShouldBe("md5-hash");
        result.Value.PaymentId.ShouldNotBe(Guid.Empty);

        await _jobQueue.Received(1).EnqueueAsync(
            Arg.Is<ProcessPaymentJob>(j => j.PaymentId == result.Value.PaymentId && j.Attempt == 0),
            Arg.Any<DateTime?>(),
            Arg.Is<int?>(1),
            Arg.Any<CancellationToken>());
    }

    private static DbSet<TEntity> CreateAsyncMockDbSet<TEntity>(List<TEntity> data) where TEntity : class
    {
        TestAsyncEnumerable<TEntity> asyncEnumerable = new(data);
        IQueryable<TEntity> queryable = asyncEnumerable;
        DbSet<TEntity> dbSet = Substitute.For<DbSet<TEntity>, IQueryable<TEntity>, IAsyncEnumerable<TEntity>>();

        ((IQueryable<TEntity>)dbSet).Provider.Returns(queryable.Provider);
        ((IQueryable<TEntity>)dbSet).Expression.Returns(queryable.Expression);
        ((IQueryable<TEntity>)dbSet).ElementType.Returns(queryable.ElementType);
        ((IQueryable<TEntity>)dbSet).GetEnumerator().Returns(queryable.GetEnumerator());
        ((IAsyncEnumerable<TEntity>)dbSet).GetAsyncEnumerator(Arg.Any<CancellationToken>())
            .Returns(asyncEnumerable.GetAsyncEnumerator());

        return dbSet;
    }
}
