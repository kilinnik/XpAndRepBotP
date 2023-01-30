using Microsoft.EntityFrameworkCore;

namespace XpAndRepBot
{
    public class Words
    {
        public string Word { get; set; }
        public int Count { get; set; }
    }

    public class Users
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Lvl { get; set; }
        public int CurXp { get; set; }
        public int Rep { get; set; }
        public Users (long id, string name, int lvl, int curXp, int rep)
        {
            Id = id; Name = name;  Lvl = lvl; CurXp = curXp; Rep = rep; 
        }
    }
    public class InfoContext : DbContext
    {
        public DbSet<Users> TableUsers { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Consts.ConStrindDbUsers);
        }
    }
}
