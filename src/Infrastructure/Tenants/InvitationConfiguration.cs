using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Tenants;

internal sealed class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.HasKey(i => i.Id);

        builder.HasIndex(i => i.Token).IsUnique();

        builder.HasOne<Tenant>(i => i.Tenant)
            .WithMany()
            .HasForeignKey(i => i.TenantId);

        builder.HasOne<Role>(i => i.Role)
            .WithMany()
            .HasForeignKey(i => i.RoleId);
    }
}
