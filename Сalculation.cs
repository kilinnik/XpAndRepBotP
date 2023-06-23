using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static XpAndRepBot.Consts;

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

        public static async Task<int> PlaceLexicon(Users user)
        {
            using SqlConnection connection = new(ConStringDbLexicon);
            await connection.OpenAsync();
            SqlCommand command = new($"SELECT RowNumber from (SELECT ROW_NUMBER() OVER (ORDER BY Count(*) DESC) AS RowNumber, UserID, COUNT(*) AS UserCount FROM dbo.TableUsersLexicons GROUP BY UserID) AS T WHERE UserID = {user.Id}", connection);
            SqlDataReader reader = await command.ExecuteReaderAsync();
            int position = 1;
            var str = user.Id.ToString();
            while (await reader.ReadAsync())
            {
                position = (int)reader.GetInt64(0);
            }
            reader.Close();
            return position;
        }

        public static int Genlvl(int x)
        {          
            return XpForLvlUp[x];
        }
    }
}
