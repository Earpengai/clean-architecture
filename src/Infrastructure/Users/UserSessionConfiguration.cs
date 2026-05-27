using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Users;

internal sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("user_sessions");

        builder.HasKey(s => s.Id);

        builder.HasIndex(s => new { s.UserId, s.IsActive });

        builder.HasIndex(s => s.RefreshTokenId);

        builder.HasOne<User>(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId);

        builder.HasOne<RefreshToken>()
            .WithMany()
            .HasForeignKey(s => s.RefreshTokenId)
            .IsRequired(false);

        builder.Property(s => s.IpAddress).HasMaxLength(128);
        builder.Property(s => s.UserAgent).HasMaxLength(512);
        builder.Property(s => s.Browser).HasMaxLength(128);
        builder.Property(s => s.OperatingSystem).HasMaxLength(128);
    }
}
