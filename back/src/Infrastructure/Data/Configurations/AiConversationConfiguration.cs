using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations;

public class AiConversationConfiguration : IEntityTypeConfiguration<AiConversation>
{
    public void Configure(EntityTypeBuilder<AiConversation> builder)
    {
        builder.Property(c => c.UserId).IsRequired(false);
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.SessionId);
        builder.Property(c => c.SessionId).HasMaxLength(100);
        builder.Property(c => c.UserRole).HasMaxLength(50).HasDefaultValue("Guest");
        builder.HasIndex(c => c.CreatedAt);
        builder.Property(c => c.Title).HasMaxLength(200);
        builder.Property(c => c.Language).HasMaxLength(10);
        builder.Property(c => c.IsArchived).HasDefaultValue(false);
        builder.Property(c => c.IsHidden).HasDefaultValue(false);
        builder.HasQueryFilter(c => !c.IsDeleted);
        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
