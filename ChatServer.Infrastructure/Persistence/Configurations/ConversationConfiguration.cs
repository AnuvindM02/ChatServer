using ChatServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Infrastructure.Persistence.Configurations
{
    public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
    {
        public void Configure(EntityTypeBuilder<Conversation> builder)
        {
            builder.ToTable("Conversations");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type).IsRequired();
            builder.HasIndex(x => x.Type);
            builder.Property(x => x.GroupName).HasMaxLength(50);
            builder.HasIndex(x => x.GroupName);
        }
    }
}
