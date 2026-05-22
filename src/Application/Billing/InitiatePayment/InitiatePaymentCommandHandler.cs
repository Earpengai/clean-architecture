using Application.Abstractions.Authentication;
using Application.Abstractions.Billing;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Payments;
using Domain.SubscriptionFeatures;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Billing.InitiatePayment;

internal sealed class InitiatePaymentCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    IBakongService bakongService)
    : ICommandHandler<InitiatePaymentCommand, InitiatePaymentResponse>
{
    public async Task<Result<InitiatePaymentResponse>> Handle(
        InitiatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        Guid tenantId = userContext.TenantId ?? Guid.Empty;
        Guid userId = userContext.UserId;

        Membership? membership = await context.Memberships
            .Include(m => m.Role)
            .FirstOrDefaultAsync(
                m => m.UserId == userId && m.TenantId == tenantId,
                cancellationToken);

        if (membership is null || membership.Role.Name != "Owner")
        {
            return Result.Failure<InitiatePaymentResponse>(PaymentErrors.NotOwner);
        }

        Tenant? tenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant is null)
        {
            return Result.Failure<InitiatePaymentResponse>(TenantErrors.NotFound(tenantId));
        }

        decimal amount = SubscriptionPricing.GetPrice(command.Plan, command.BillingPeriod);

        BakongGenerationRequest qrRequest = new()
        {
            Amount = amount,
            Currency = "USD",
            MerchantName = tenant.Name
        };

        QrGenerationResult qrResult = await bakongService.GenerateQrAsync(qrRequest, cancellationToken);

        Payment payment = new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            Plan = command.Plan,
            BillingPeriod = command.BillingPeriod,
            Amount = amount,
            Currency = "USD",
            Status = PaymentStatus.Pending,
            Md5Hash = qrResult.Md5,
            QrData = qrResult.Qr,
            CreatedAt = DateTime.UtcNow
        };

        context.Payments.Add(payment);

        await context.SaveChangesAsync(cancellationToken);

        InitiatePaymentResponse response = new()
        {
            PaymentId = payment.Id,
            Qr = qrResult.Qr,
            Md5 = qrResult.Md5
        };

        return response;
    }
}
