using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations;

public class AiConversationConfiguration : IEntityTypeConfiguration<AiConversation>
{
    public void Configure(EntityTypeBuilder<AiConversation> builder)
    {
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.CreatedAt);
        builder.Property(c => c.Title).HasMaxLength(200);
        builder.Property(c => c.Language).HasMaxLength(10);
        builder.HasQueryFilter(c => !c.IsDeleted);
        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
