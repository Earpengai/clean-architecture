using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Users;

internal sealed class RegistrationInfoConfiguration : IEntityTypeConfiguration<RegistrationInfo>
{
    public void Configure(EntityTypeBuilder<RegistrationInfo> builder)
    {
        builder.ToTable("registration_info");

        builder.HasKey(r => r.Id);

        builder.HasIndex(r => r.Email);

        builder.Property(r => r.Email).HasMaxLength(256);
        builder.Property(r => r.FirstName).HasMaxLength(100);
        builder.Property(r => r.LastName).HasMaxLength(100);
        builder.Property(r => r.CompanyName).HasMaxLength(200);
        builder.Property(r => r.Industry).HasMaxLength(100);
        builder.Property(r => r.Country).HasMaxLength(100);
        builder.Property(r => r.AcceptedTerms).IsRequired();
    }
}
