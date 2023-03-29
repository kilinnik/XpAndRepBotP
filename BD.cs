using Microsoft.EntityFrameworkCore;
using System;

namespace XpAndRepBot
{
    public class Words
    {
        public string Word { get; set; }
        public int Count { get; set; }
    }

    public class Lexicon
    {
        public string TableName { get; set; }
        public int CountRow { get; set; }
    }

    public class Users
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Lvl { get; set; }
        public int CurXp { get; set; }
        public int Rep { get; set; }
        public int Warns { get; set; }
        public DateTime LastTime { get; set; }
        public string Roles { get; set; }
        public bool? Nfc { get; set; }
        public DateTime StartNfc { get; set; }
        public long BestTime { get; set; }
        public DateTime TimeLastMes { get; set; }
        public Users (long id, string name, int lvl, int curXp, int rep)
        {
            Id = id; 
            Name = name;  
            Lvl = lvl;
            CurXp = curXp; 
            Rep = rep; Warns = 0;
            LastTime = DateTime.ParseExact("1900-01-01 00:00:00.000", "yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
            Nfc = false; 
            StartNfc = DateTime.ParseExact("1900-01-01 00:00:00.000", "yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
            BestTime = 0;
            TimeLastMes = DateTime.ParseExact("1900-01-01 00:00:00.000", "yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    public class InfoContext : DbContext
    {
        public DbSet<Users> TableUsers { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Consts.ConStringDbUsers);
        }
    }
}
