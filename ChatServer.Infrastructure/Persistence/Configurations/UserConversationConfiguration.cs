using ChatServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Infrastructure.Persistence.Configurations
{
    public class UserConversationConfiguration : IEntityTypeConfiguration<UserConversation>
    {
        public void Configure(EntityTypeBuilder<UserConversation> builder)
        {
            builder.ToTable("UserConversations");
            builder.HasKey(x => new { x.UserId, x.ConversationId });
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.ConversationId);

            builder.HasOne(x => x.User)
                .WithMany(x => x.UserConversations)
                .HasForeignKey(x => x.UserId);

            builder.HasOne(x => x.Conversation)
                .WithMany(x => x.UserConversations)
                .HasForeignKey(x => x.ConversationId);
        }
    }
}
