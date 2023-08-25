using Microsoft.EntityFrameworkCore;
using XpAndRepBot.Database.Models;

namespace XpAndRepBot.Database;

public class DbUsersContext : DbContext
{
    public DbSet<Users> Users { get; set; }
    public DbSet<UserWords> UserLexicons { get; set; }
    public DbSet<MessageIdsForDelete> MessageIdsForDeletion { get; set; }
    public DbSet<Chat> Chats { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(Constants.ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserWords>()
            .HasKey(uw => new { uw.UserId, uw.ChatId });
        modelBuilder.Entity<Users>()
            .HasKey(uw => new { uw.UserId, uw.ChatId });
        modelBuilder.Entity<MessageIdsForDelete>()
            .HasKey(uw => new { UserId = uw.FirstMessageId, uw.ChatId });
    }
}