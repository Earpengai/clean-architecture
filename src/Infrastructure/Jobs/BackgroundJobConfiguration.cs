using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Jobs;

internal sealed class BackgroundJobConfiguration : IEntityTypeConfiguration<BackgroundJob>
{
    public void Configure(EntityTypeBuilder<BackgroundJob> builder)
    {
        builder.ToTable("background_jobs");

        builder.HasKey(j => j.Id);

        builder.HasIndex(j => j.Status);
        builder.HasIndex(j => j.CreatedAt);
        builder.HasIndex(j => j.ScheduledAt);

        builder.Property(j => j.JobType).HasMaxLength(500).IsRequired();
        builder.Property(j => j.Payload).IsRequired();
        builder.Property(j => j.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(j => j.Error);
        builder.Property(j => j.TenantId);
    }
}
