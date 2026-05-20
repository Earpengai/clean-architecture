using FluentValidation;

namespace Application.Tenants.CreateTenant;

internal sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.Identifier).NotEmpty().MaximumLength(100);
        RuleFor(c => c.OwnerId).NotEmpty();
    }
}
