using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Tenants;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => new { r.TenantId, r.Name }).IsUnique();

        builder.HasOne<Tenant>(r => r.Tenant)
            .WithMany()
            .HasForeignKey(r => r.TenantId);

        builder.HasMany(r => r.Permissions)
            .WithOne(p => p.Role)
            .HasForeignKey(p => p.RoleId);
    }
}
