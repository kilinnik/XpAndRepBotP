using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace XpAndRepBot
{
    public static class Сalculation
    {
        public static int PlaceLvl(long idUser, DbSet<Users> TableUsers)
        {
            var users = TableUsers.OrderByDescending(b => b.Lvl).ThenByDescending(n => n.CurXp).ToList();          
            return users.IndexOf(users.First(x => x.Id == idUser)) + 1;
        }

        public static int PlaceRep(long idUser, DbSet<Users> TableUsers)
        {
            var users = TableUsers.OrderByDescending(b => b.Rep).ToList();
            return users.IndexOf(users.First(x => x.Id == idUser)) + 1;
        }

        public static int Genlvl(int x)
        {
            int[] xplvl = new int[] { 0, 100, 235, 505, 810, 1250, 1725, 2335, 2980, 3760, 4575, 5525, 6510, 7630, 8785, 10075, 11400, 12860, 14355, 15985, 17650, 19450, 21285, 23255, 25260, 27400, 29575, 31885, 34230, 36710, 39225 };
            return xplvl[x];
        }
    }
}
