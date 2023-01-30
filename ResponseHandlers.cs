using System.Threading.Tasks;
using Telegram.Bot.Types;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace XpAndRepBot
{
    public static class ResponseHandlers
    { 
        public static string Me(Update update)
        {
            using var db = new InfoContext();
            DataClasses1DataContext dbl = new(Consts.ConStrindDbLexicon);
            var idUser = update.Message.From.Id;
            var user = db.TableUsers.First(x => x.Id == idUser);
            var words = dbl.ExecuteQuery<Words>($"select top 10 * from dbo.[{user.Id}] order by [Count] desc").ToList();
            List<string> keycaps = new() { EmojiList.Keycap_1, EmojiList.Keycap_2, EmojiList.Keycap_3, EmojiList.Keycap_4, EmojiList.Keycap_5, EmojiList.Keycap_6, EmojiList.Keycap_7, EmojiList.Keycap_8, EmojiList.Keycap_9, EmojiList.Keycap_10 };
            var result = new StringBuilder($"👨‍❤️‍👨 Имя: {user.Name}\r\n⭐️ Lvl: {user.Lvl}({user.CurXp}/{Сalculation.Genlvl(user.Lvl + 1)})\r\n🏆 Место в топе по уровню: {Сalculation.PlaceLvl(user.Id, db.TableUsers)}\r\n😇 Rep: {user.Rep}\r\n🥇 Место в топе по репутации: {Сalculation.PlaceRep(user.Id, db.TableUsers)}\r\n");
            result.AppendLine("📖 Личный топ слов:");
            for (int i = 0; i < words.Count; i++)
            {
                result.AppendLine($"{keycaps[i]} {words[i].Word} || {words[i].Count}");
            }
            return result.ToString();
        }

        public static string TopLvl()
        {
            using var db = new InfoContext();
            var users = db.TableUsers.OrderByDescending(x => x.Lvl).ThenByDescending(y => y.CurXp).Take(50);
            StringBuilder sb = new();
            sb.Append("🏆 \r\n");
            int i = 1;
            foreach (var user in users)
            {
                sb.AppendLine($"{i}. {user.Name} lvl {user.Lvl}({user.CurXp}/{Сalculation.Genlvl(user.Lvl + 1)})");
                i++;
            }
            return sb.ToString();
        }


        public static string TopRep()
        {
            using var db = new InfoContext();
            var users = db.TableUsers.OrderByDescending(x => x.Rep).Take(50);
            var resultBuilder = new StringBuilder("🥇 \r\n");
            int i = 1;
            foreach (var user in users)
            {
                resultBuilder.AppendLine($"{i}. {user.Name} rep {user.Rep}");
                i++;
            }
            return resultBuilder.ToString();
        }

        public static string TopWords()
        {
            DataClasses1DataContext dbl = new(Consts.ConStrindDbLexicon);
            var tablesName = dbl.ExecuteQuery<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES").ToArray();
            var query = new StringBuilder($"select [Word], SUM([Count]) as [Count] from (select [Word],[Count] from [dbo].[{tablesName[0]}]");
            for (int i = 1; i < tablesName.Length; i++)
            {
                query.Append($" union select [Word],[Count] from [dbo].[{tablesName[i]}]");
            }
            query.Append($") as a group by [Word] order by [Count] desc");
            var words = dbl.ExecuteQuery<Words>(query.ToString()).ToList().Take(50);
            int k = 1;
            var result = new StringBuilder("📖 Топ слов:\r\n");
            foreach (var word in words)
            {
                result.AppendLine($"{k}. {word.Word} || {word.Count}");
                k++;
            }
            return result.ToString();
        }

        public static async Task<string> RequestChatGPT(string prompt)
        {
            // Replace YOUR_API_KEY with your actual API key
            string apiKey = Consts.SSHKey;

            // Set the model for the request
            string model = "text-davinci-003";

            // Create the JSON object for the request body
            var jsonObject = new JObject { { "model", model }, { "prompt", prompt }, { "max_tokens", 2048 } };

            // Send the request to the OpenAI API
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await client.PostAsync("https://api.openai.com/v1/completions", new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json"));

            // Read the response and extract the answer
            var json = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(json);
            string answer;
            try
            {
                answer = result["choices"][0]["text"].ToString();
            }
            catch { answer = "Произошла ошибка. Попробуйте повторить вопрос."; }

            // Return the answer
            return answer.TrimStart('\n', '\r');
        }

        static readonly HashSet<string> repWords = new () { "+", "спс", "спасибо", "пасиб", "сяб", "класс", "молодец", "жиза", "гц", "грац", "дяк", "дякую", "база", "соглы", "danke schön", "danke", "данке", "viele danke", "👍", "👍🏼", "👍🏽", "👍🏾", "👍🏿" };

        public static string RepUp(Update update, InfoContext db, string mes)
        {
            mes = Regex.Replace(mes, @"[^\w\d\s]", "").ToLower();
            if (update.Message.ReplyToMessage.From.Id != update.Message.From.Id)
            {
                var list = mes.Split(" ");
                if (list.Any(repWords.Contains))
                {
                    var idUser = update.Message.ReplyToMessage.From.Id;
                    var user = db.TableUsers.FirstOrDefault(x => x.Id == idUser);
                    user.Rep++;
                    db.SaveChanges();
                    var user1 = db.TableUsers.FirstOrDefault(x => x.Id == update.Message.From.Id);
                    return $"{user1.Name}({user1.Rep}) увеличил репутацию {user.Name} на 1({user.Rep})";
                }
            }
            return "";
        }
    }
}
