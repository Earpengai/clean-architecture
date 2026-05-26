using FluentValidation;

namespace Application.Tenants.ClearDemoData;

internal sealed class ClearDemoDataCommandValidator : AbstractValidator<ClearDemoDataCommand>
{
    public ClearDemoDataCommandValidator()
    {
        RuleFor(c => c.TenantId).NotEmpty();
    }
}
