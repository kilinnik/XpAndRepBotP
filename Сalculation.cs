using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static async Task<int> PlaceLexicon(Users user)
        {
            using SqlConnection connection = new(Consts.ConStringDbLexicon);
            await connection.OpenAsync();
            SqlCommand command = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES", connection);
            var tablesName = new List<string>();
            SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tablesName.Add(reader.GetString(0));
            }
            reader.Close();

            var query = new StringBuilder($"SELECT '{tablesName[0]}' AS TableName, COUNT(*) AS CountRow FROM [dbo].[{tablesName[0]}]");
            for (int i = 1; i < tablesName.Count; i++)
            {
                query.Append($" UNION ALL SELECT '{tablesName[i]}' AS TableName, COUNT(*) AS CountRow FROM [dbo].[{tablesName[i]}]");
            }

            command.CommandText = query.ToString();
            reader = await command.ExecuteReaderAsync();

            var lexicons = new List<Lexicon>();
            while (await reader.ReadAsync())
            {
                lexicons.Add(new Lexicon { TableName = reader.GetString(0), CountRow = reader.GetInt32(1) });
            }
            reader.Close();

            return lexicons.IndexOf(lexicons.First(x => x.TableName == user.Id.ToString())) + 1;
        }

        public static int Genlvl(int x)
        {
            int[] xplvl = new int[] { 0, 100, 235, 505, 810, 1250, 1725, 2335, 2980, 3760, 4575, 5525, 6510, 7630, 8785, 10075, 11400, 12860, 14355, 15985, 17650, 19450, 21285, 23255, 25260, 27400, 29575, 31885, 34230, 36710, 39225, 41875, 44560, 47380, 50235, 53225, 56250, 59410, 62605, 65935, 69300 };
            return xplvl[x];
        }
    }
}
