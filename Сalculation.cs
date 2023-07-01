using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static XpAndRepBot.Consts;

namespace XpAndRepBot
{
    public static class Сalculation
    {
        public static int PlaceLvl(long idUser, DbSet<Users> tableUsers)
        {
            var users = tableUsers.OrderByDescending(b => b.Lvl).ThenByDescending(n => n.CurXp).ToList();          
            return users.IndexOf(users.First(x => x.Id == idUser)) + 1;
        }

        public static int PlaceRep(long idUser, DbSet<Users> tableUsers)
        {
            var users = tableUsers.OrderByDescending(b => b.Rep).ToList();
            return users.IndexOf(users.First(x => x.Id == idUser)) + 1;
        }

        public static async Task<int> PlaceLexicon(Users user)
        {
            await using SqlConnection connection = new(ConStringDbLexicon);
            await connection.OpenAsync();
            SqlCommand command = new(
                $"SELECT RowNumber from (SELECT ROW_NUMBER() OVER (ORDER BY Count(*) DESC) AS RowNumber, UserID, " +
                $"COUNT(*) AS UserCount FROM dbo.TableUsersLexicons GROUP BY UserID) AS T WHERE UserID = {user.Id}", connection);
            var reader = await command.ExecuteReaderAsync();
            var position = 1;
            while (await reader.ReadAsync())
            {
                position = (int)reader.GetInt64(0);
            }
            await reader.CloseAsync();
            return position;
        }

        public static int GenerateXpForLevel(int x)
        {          
            return XpForLvlUp[x];
        }
    }
}
