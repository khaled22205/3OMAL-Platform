using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations;

public class AiUsageLogConfiguration : IEntityTypeConfiguration<AiUsageLog>
{
    public void Configure(EntityTypeBuilder<AiUsageLog> builder)
    {
        builder.HasIndex(l => l.UserId);
        builder.HasIndex(l => l.CreatedAt);
        builder.Property(l => l.Role).HasMaxLength(20);
        builder.Property(l => l.Model).HasMaxLength(100);
        builder.Property(l => l.ErrorMessage).HasMaxLength(1000);
    }
}
