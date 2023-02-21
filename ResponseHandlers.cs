using System.Threading.Tasks;
using Telegram.Bot.Types;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using System;
using System.IO;
using System.Net;
using Telegram.Bot.Types.InputFiles;

namespace XpAndRepBot
{
    public class ResponseHandlers
    { 
        public static string Me(Update update)
        {
            using var db = new InfoContext();
            DataClasses1DataContext dbl = new(Consts.ConStrindDbLexicon);
            var idUser = update.Message.From.Id;
            var user = db.TableUsers.First(x => x.Id == idUser);
            int count = dbl.ExecuteQuery<int>($"SELECT COUNT(*) FROM dbo.[{user.Id}]").Single();
            var words = dbl.ExecuteQuery<Words>($"select top 10 * from dbo.[{user.Id}] order by [Count] desc").ToList();
            List<string> keycaps = new() { EmojiList.Keycap_1, EmojiList.Keycap_2, EmojiList.Keycap_3, EmojiList.Keycap_4, EmojiList.Keycap_5, EmojiList.Keycap_6, EmojiList.Keycap_7, EmojiList.Keycap_8, EmojiList.Keycap_9, EmojiList.Keycap_10 };
            var result = new StringBuilder($"👨‍❤️‍👨 Имя: {user.Name}\n⭐️ Lvl: {user.Lvl}({user.CurXp}/{Сalculation.Genlvl(user.Lvl + 1)})\n🏆 Место в топе по уровню: {Сalculation.PlaceLvl(user.Id, db.TableUsers)}\n😇 Rep: {user.Rep}\n🥇 Место в топе по репутации: {Сalculation.PlaceRep(user.Id, db.TableUsers)}\n🔤 Лексикон: {count} слов\n🎖 Место в топе по лексикону: {Сalculation.PlaceLexicon(user)}\n🤬 Кол-во варнов: {user.Warns}/3\n🗓 Дата последнего варна/снятия варна: {user.LastTime:yyyy-MM-dd}\n");
            result.AppendLine("📖 Личный топ слов:");
            for (int i = 0; i < words.Count; i++)
            {
                result.AppendLine($"{keycaps[i]} {words[i].Word} || {words[i].Count}");
            }
            return result.ToString();
        }

        public static string Warn(Update update)
        {
            using var db = new InfoContext();
            var idUser = update.Message.ReplyToMessage.From.Id;
            var user = db.TableUsers.First(x => x.Id == idUser);
            user.Warns++; user.LastTime = DateTime.Now;
            db.SaveChanges();
            return $"{user.Name} получает предупреждение({user.Warns}/3)";
        }

        public static string Unwarn(Update update)
        {
            using var db = new InfoContext();
            var idUser = update.Message.ReplyToMessage.From.Id;
            var user = db.TableUsers.First(x => x.Id == idUser);
            user.Warns--; user.LastTime = DateTime.Now;
            db.SaveChanges();
            return $"С {user.Name} снимается 1 варн({user.Warns}/3)";
        }

        public static string TopLvl(int number)
        {
            using var db = new InfoContext();
            var users = db.TableUsers.OrderByDescending(x => x.Lvl).ThenByDescending(y => y.CurXp).Skip(number).Take(50);
            StringBuilder sb = new();
            sb.Append("🏆 \n");
            int i = number + 1;
            foreach (var user in users)
            {
                sb.AppendLine($"{i}. {user.Name} lvl {user.Lvl}({user.CurXp}/{Сalculation.Genlvl(user.Lvl + 1)})");
                i++;
            }
            return sb.ToString();
        }

        public static string TopRep(int number)
        {
            using var db = new InfoContext();
            var users = db.TableUsers.OrderByDescending(x => x.Rep).Skip(number).Take(50);
            var resultBuilder = new StringBuilder("🥇 \n");
            int i = number + 1;
            foreach (var user in users)
            {
                resultBuilder.AppendLine($"{i}. {user.Name} rep {user.Rep}");
                i++;
            }
            return resultBuilder.ToString();
        }

        public static string MeWords(CallbackQuery callbackQuery, int number)
        {
            using var db = new InfoContext();
            DataClasses1DataContext dbl = new(Consts.ConStrindDbLexicon);
            var idUser = callbackQuery.Message.ReplyToMessage.From.Id;
            var user = db.TableUsers.First(x => x.Id == idUser);
            Match match = Regex.Match(callbackQuery.Message.Text, @"^\d+");
            List<Words> words;
            int i = 0;
            if (match.Success)
            {
                words = dbl.ExecuteQuery<Words>($"select * from dbo.[{user.Id}] order by [Count] desc OFFSET {int.Parse(match.Value) + number - 1} ROWS FETCH NEXT 10 ROWS ONLY").ToList();
                i = int.Parse(match.Value) + number;
            }
            else
            {
                words = dbl.ExecuteQuery<Words>($"select * from dbo.[{user.Id}] order by [Count] desc OFFSET 10 ROWS FETCH NEXT 10 ROWS ONLY").ToList();
                i = 11;
            }
            var result = new StringBuilder();

            foreach (var word in words)
            {
                result.AppendLine($"{i}. {word.Word} || {word.Count}");
                i++;
            }
            return result.ToString();
        }

        public static string TopWords(int number)
        {
            DataClasses1DataContext dbl = new(Consts.ConStrindDbLexicon);
            var tablesName = dbl.ExecuteQuery<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES").ToArray();
            var query = new StringBuilder($"select [Word], SUM([Count]) as [Count] from (select [Word],[Count] from [dbo].[{tablesName[0]}]");
            for (int i = 1; i < tablesName.Length; i++)
            {
                query.Append($" union select [Word],[Count] from [dbo].[{tablesName[i]}]");
            }
            query.Append($") as a group by [Word] order by [Count] desc");
            var words = dbl.ExecuteQuery<Words>(query.ToString()).ToList().Skip(number).Take(50);
            int k = number + 1;
            var result = new StringBuilder("📖 Топ слов:\n");
            foreach (var word in words)
            {
                result.AppendLine($"{k}. {word.Word} || {word.Count}");
                k++;
            }
            return result.ToString();
        }

        public static string TopLexicon(int number)
        {
            DataClasses1DataContext dbl = new(Consts.ConStrindDbLexicon);
            var tablesName = dbl.ExecuteQuery<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES").ToArray();
            var query = new StringBuilder($"SELECT '{tablesName[0]}' AS TableName, COUNT(*) AS CountRow FROM [dbo].[{tablesName[0]}]");
            for (int i = 1; i < tablesName.Length; i++)
            {
                query.Append($" UNION ALL SELECT '{tablesName[i]}' AS TableName, COUNT(*) AS CountRow FROM [dbo].[{tablesName[i]}]");
            }
            var lexicons = dbl.ExecuteQuery<Lexicon>(query.ToString()).ToList().OrderByDescending(b => b.CountRow).Skip(number).Take(50);
            int k = number + 1;
            var result = new StringBuilder("🎖 Топ по лексикону:\n");
            using var db = new InfoContext();
            foreach (var lexicon in lexicons)
            {
                var user = db.TableUsers.First(x => x.Id.ToString() == lexicon.TableName);
                result.AppendLine($"{k}. {user.Name} || {lexicon.CountRow}");
                k++;
            }
            return result.ToString();
        }

        static readonly HashSet<string> repWords = new () { "+", "спс", "спасибо", "пасиб", "сяб", "класс", "молодец", "жиза", "гц", "грац", "дяк", "дякую", "база", "соглы", "danke schön", "danke", "данке", "viele danke", "👍", "👍🏼", "👍🏽", "👍🏾", "👍🏿" };

        public static string RepUp(Update update, InfoContext db, string mes)
        {
            mes = Regex.Replace(mes, @"[^\w\d\s+]", "").ToLower();
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

        public static async Task<InputOnlineFile> GenerateImage(IOpenAIService sdk, string promt)
        {
            try
            {
                var imageResult = await sdk.Image.CreateImage(new ImageCreateRequest
                {
                    Prompt = promt,
                    N = 1,
                    Size = StaticValues.ImageStatics.Size.Size256,
                    ResponseFormat = StaticValues.ImageStatics.ResponseFormat.Url,
                    User = "TestUser"
                });

                if (imageResult.Successful)
                {
                    string imageUrl = string.Join("\n", imageResult.Results.Select(r => r.Url));

                    using WebClient webClient = new();
                    byte[] imageBytes = webClient.DownloadData(imageUrl);
                    using var ms = new MemoryStream(imageBytes);
                    InputOnlineFile image = new(new MemoryStream(imageBytes), "image.png");
                    return image;
                }
                else
                {
                    if (imageResult.Error == null)
                    {
                        throw new Exception("Unknown Error");
                    }

                    Console.WriteLine($"{imageResult.Error.Code}: {imageResult.Error.Message}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return null;
        }
    }
}
