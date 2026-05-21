using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Users;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(r => r.Id);

        builder.HasIndex(r => r.TokenHash).IsUnique();

        builder.HasIndex(r => r.UserId);

        builder.HasOne<User>(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId);

        builder.Property(r => r.TokenHash).HasMaxLength(128);
        builder.Property(r => r.ReplacedByTokenHash).HasMaxLength(128);
    }
}
