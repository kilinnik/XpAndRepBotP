using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class XpAndRepBotDbContext(DbContextOptions<XpAndRepBotDbContext> options)
    : DbContext(options)
{
    public DbSet<ChatUser> Users { get; init; }
    public DbSet<UserRole> UserRoles { get; init; }
    public DbSet<UserReputation> UserReputations { get; init; }
    public DbSet<UserLevel> UserLevels { get; init; }
    public DbSet<UserNfc> UserNfcs { get; init; }
    public DbSet<UserModeration> UserModerations { get; init; }
    public DbSet<UserComplaint> UserComplaints { get; init; }

    public DbSet<UserMarriage> UserMarriages { get; init; }
    public DbSet<UserWord> UserWords { get; init; }
    public DbSet<DeletableMessage> DeletableMessages { get; init; }
    public DbSet<Chat> Chats { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<UserWord>()
            .HasKey(
                uw =>
                    new
                    {
                        uw.UserId,
                        uw.Word,
                        uw.ChatId
                    }
            );

        modelBuilder.Entity<ChatUser>().HasKey(u => new { u.UserId, u.ChatId });

        modelBuilder.Entity<UserRole>().HasKey(u => new { u.UserId, u.ChatId });

        modelBuilder.Entity<UserReputation>().HasKey(u => new { u.UserId, u.ChatId });

        modelBuilder.Entity<UserLevel>().HasKey(u => new { u.UserId, u.ChatId });

        modelBuilder.Entity<UserNfc>().HasKey(u => new { u.UserId, u.ChatId });

        modelBuilder.Entity<UserModeration>().HasKey(u => new { u.UserId, u.ChatId });

        modelBuilder.Entity<UserComplaint>().HasKey(u => new { u.UserId, u.ChatId });

        modelBuilder.Entity<UserMarriage>().HasKey(u => new { u.UserId, u.ChatId });

        modelBuilder
            .Entity<UserMarriage>()
            .HasOne(u => u.Partner)
            .WithMany()
            .HasForeignKey(u => new { u.PartnerId, u.ChatId })
            .HasPrincipalKey(u => new { u.UserId, u.ChatId });

        modelBuilder
            .Entity<DeletableMessage>()
            .HasKey(dm => new { UserId = dm.FirstMessageId, dm.ChatId });

        modelBuilder
            .Entity<ChatUser>()
            .HasIndex(u => new { u.UserId, u.ChatId })
            .HasDatabaseName("Index_UserId_ChatId");

        modelBuilder
            .Entity<UserLevel>()
            .HasIndex(u => new { u.Level, u.CurrentExperience })
            .HasDatabaseName("Index_Level_CurrentExperience");

        modelBuilder
            .Entity<UserReputation>()
            .HasIndex(u => u.Reputation)
            .HasDatabaseName("Index_Reputation");

        modelBuilder
            .Entity<UserWord>()
            .HasIndex(uw => new { uw.UserId, uw.ChatId })
            .HasDatabaseName("Index_UserWord_UserId_ChatId");

        modelBuilder
            .Entity<UserWord>()
            .HasIndex(uw => uw.Word)
            .HasDatabaseName("Index_UserWord_Word");

        modelBuilder.Entity<Chat>().HasIndex(c => c.ChatId).HasDatabaseName("Index_ChatId");
    }
}
