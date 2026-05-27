using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Users;

internal sealed class TwoFactorRememberTokenConfiguration : IEntityTypeConfiguration<TwoFactorRememberToken>
{
    public void Configure(EntityTypeBuilder<TwoFactorRememberToken> builder)
    {
        builder.ToTable("two_factor_remember_tokens");

        builder.HasKey(r => r.Id);

        builder.HasIndex(r => r.TokenHash).IsUnique();

        builder.HasIndex(r => r.UserId);

        builder.HasOne<User>(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId);

        builder.Property(r => r.TokenHash).HasMaxLength(128);
    }
}
