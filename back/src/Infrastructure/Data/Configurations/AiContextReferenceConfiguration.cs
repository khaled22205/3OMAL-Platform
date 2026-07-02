using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations;

public class AiContextReferenceConfiguration : IEntityTypeConfiguration<AiContextReference>
{
    public void Configure(EntityTypeBuilder<AiContextReference> builder)
    {
        builder.HasIndex(r => r.MessageId);
        builder.Property(r => r.SourceType).HasMaxLength(50);
        builder.Property(r => r.Title).HasMaxLength(300);
        builder.Property(r => r.Excerpt).HasColumnType("nvarchar(max)");
    }
}
