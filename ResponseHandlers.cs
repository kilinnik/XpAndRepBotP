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
using System.Data.SqlClient;
using static XpAndRepBot.Consts;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Mirror.ChatGpt;
using Mirror.ChatGpt.Models.ChatGpt;
using Dapper;
using System.Threading;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;

namespace XpAndRepBot
{
    public class ResponseHandlers
    {
        public static async Task<string> Me(long idUser)
        {
            using var db = new InfoContext();
            using var connection = new SqlConnection(ConStringDbLexicon);
            await connection.OpenAsync();
            var user = db.TableUsers.First(x => x.Id == idUser);
            using var command = new SqlCommand($"SELECT COUNT(*) FROM dbo.[{user.Id}]", connection);
            int count = (int)await command.ExecuteScalarAsync();
            using var command2 = new SqlCommand($"select top 10 * from dbo.[{user.Id}] order by [Count] desc", connection);
            using var reader = await command2.ExecuteReaderAsync();
            var words = new List<Words>();
            while (await reader.ReadAsync())
            {
                var word = new Words
                {
                    Word = reader.GetString(0),
                    Count = reader.GetInt32(1)
                };
                words.Add(word);
            }
            var result = new StringBuilder($"👨‍❤️‍👨 Имя: {user.Name}\n🕰 Время последнего сообщения: {user.TimeLastMes:yy/MM/dd HH:mm:ss}\n⭐️ Lvl: {user.Lvl}({user.CurXp}/{Сalculation.Genlvl(user.Lvl + 1)})\n🎭 Роли: {user.Roles}\n🏆 Место в топе по уровню: {Сalculation.PlaceLvl(user.Id, db.TableUsers)}\n😇 Rep: {user.Rep}\n🥇 Место в топе по репутации: {Сalculation.PlaceRep(user.Id, db.TableUsers)}\n🔤 Лексикон: {count} слов\n🎖 Место в топе по лексикону: {await Сalculation.PlaceLexicon(user)}\n🤬 Кол-во варнов: {user.Warns}/3\n🗓 Дата последнего варна/снятия варна: {user.LastTime:yyyy-MM-dd}\n");
            result.AppendLine("📖 Личный топ слов:");
            for (int i = 0; i < words.Count; i++)
            {
                result.AppendLine($"{Keycaps[i]} {words[i].Word} || {words[i].Count}");
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

        public static async Task<string> MeWords(CallbackQuery callbackQuery, int number)
        {
            using var db = new InfoContext();
            var idUser = callbackQuery.Message.ReplyToMessage.From.Id;
            var user = db.TableUsers.First(x => x.Id == idUser);
            Match match = Regex.Match(callbackQuery.Message.Text, @"^\d+");

            var words = new List<Words>();
            int i = 0;
            var query = $"SELECT * FROM [dbo].[{user.Id}] ORDER BY [Count] DESC OFFSET @Offset ROWS FETCH NEXT 10 ROWS ONLY";
            var offset = 10;

            if (match.Success)
            {
                offset = int.Parse(match.Value) + number - 1;
                i = offset + 1;
            }
            else
            {
                i = 11;              
            }

            using var connection = new SqlConnection(ConStringDbLexicon);
            var result = new StringBuilder();
            var rows = await connection.QueryAsync<Words>(query, new { Offset = offset });
            rows.ToList().ForEach((word) =>
            {
                words.Add(word);
                result.AppendLine($"{i}. {word.Word} || {word.Count}");
                i++;
            });
            return result.ToString();
        }

        public static async Task<string> TopWords(int number)
        {
            var connectionString = ConStringDbLexicon;
            var tables = new List<string>();
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var cmd = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES", connection)
                {
                    CommandTimeout = 0
                };
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }
            var query = new StringBuilder($"select [Word], SUM([Count]) as [Count] from (select [Word],[Count] from [dbo].[{tables[0]}]");
            for (int i = 1; i < tables.Count; i++)
            {
                query.Append($" union select [Word],[Count] from [dbo].[{tables[i]}]");
            }
            query.Append($") as a group by [Word] order by [Count] desc");

            var words = new List<Words>();
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var cmd = new SqlCommand(query.ToString(), connection)
                {
                    CommandTimeout = 0
                };
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    words.Add(new Words
                    {
                        Word = reader.GetString(0),
                        Count = reader.GetInt32(1)
                    });
                }
            }

            int k = number + 1;
            var result = new StringBuilder("📖 Топ слов:\n");
            foreach (var word in words.Skip(number).Take(50))
            {
                result.AppendLine($"{k}. {word.Word} || {word.Count}");
                k++;
            }
            return result.ToString();
        }

        public static async Task<string> TopLexicon(int number)
        {
            using var connection = new SqlConnection(ConStringDbLexicon);
            await connection.OpenAsync();

            var tablesName = new List<string>();
            using (var command = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES", connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    tablesName.Add(reader.GetString(0));
                }
            }

            var query = new StringBuilder($"SELECT '{tablesName[0]}' AS TableName, COUNT(*) AS CountRow FROM [dbo].[{tablesName[0]}]");
            for (int i = 1; i < tablesName.Count; i++)
            {
                query.Append($" UNION ALL SELECT '{tablesName[i]}' AS TableName, COUNT(*) AS CountRow FROM [dbo].[{tablesName[i]}]");
            }

            var lexicons = new List<Lexicon>();
            using (var command = new SqlCommand(query.ToString(), connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    lexicons.Add(new Lexicon
                    {
                        TableName = reader.GetString(0),
                        CountRow = reader.GetInt32(1)
                    });
                }
            }

            var topLexicons = lexicons.OrderByDescending(b => b.CountRow).Skip(number).Take(50).ToList();
            var result = new StringBuilder("🎖 Топ по лексикону:\n");

            using var db = new InfoContext();
            foreach (var lexicon in topLexicons)
            {
                var user = db.TableUsers.FirstOrDefault(x => x.Id.ToString() == lexicon.TableName);
                if (user != null)
                {
                    result.AppendLine($"{number + 1}. {user.Name} || {lexicon.CountRow}");
                    number++;
                }
            }

            return result.ToString();
        }

        public static string RepUp(Update update, InfoContext db, string mes)
        {
            mes = Regex.Replace(mes, @"[^\w\d\s+👍👍🏼👍🏽👍🏾👍🏿]", "").ToLower();
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

        //public static async Task<string> RequestChatGPT(string prompt)
        //{
        //    var services = new ServiceCollection();
        //    const string token = Token;
        //    services.AddBingClient(new() { Token = token });
        //    var app = services.BuildServiceProvider();
        //    var service = app.GetRequiredService<BingClient>();
        //    var chatCts = new CancellationTokenSource();
        //    //Set timeout by CancellationTokenSource
        //    chatCts.CancelAfter(TimeSpan.FromMinutes(5));
        //    var res = await service.ChatAsync(new(prompt), chatCts.Token);
        //    return res.Text;
        //}

        public static async Task<string> RequestChatGPT(string prompt)
        {
            var services = new ServiceCollection();
            services.AddChatGptClient(new() { ApiKey = SSHKey });
            var app = services.BuildServiceProvider();
            var service = app.GetRequiredService<ChatGptClient>();
            var res = await service.ChatAsync(new ChatCompletionRequest
            {
                Model = "gpt-3.5-turbo", //model name,required. only gpt-3.5-turbo or gpt-3.5-turbo-0301 can be chosen now
                Messages = new[] { new MessageEntry { Role = Roles.System, Content = prompt } }
            }, default);
            return res.Choices[0].Message.Content;
            //string apiKey = SSHKey;

            //// Set the model for the request
            //string model = "text-davinci-003";

            //// Create the JSON object for the request body
            //var jsonObject = new JObject { { "model", model }, { "prompt", prompt }, { "max_tokens", 2048 } };

            //// Send the request to the OpenAI API
            //using var client = new HttpClient();
            //client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            //var response = await client.PostAsync("https://api.openai.com/v1/completions", new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json"));

            //// Read the response and extract the answer
            //var json = await response.Content.ReadAsStringAsync();
            //var result = JObject.Parse(json);
            //string answer;
            //try
            //{
            //    answer = result["choices"][0]["text"].ToString();
            //}
            //catch { answer = "Произошла ошибка. Попробуйте повторить вопрос."; }

            //// Return the answer
            //return answer.TrimStart('\n', '\r');
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
        public static async Task<string> RequestBalaboba(string str)
        {
            var url = "https://yandex.ru/lab/api/yalm/text3";
            var json = JsonSerializer.Serialize(new { filter = 1, intro = 0, query = str });
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(responseContent);
            string answer;
            try
            {
                answer = result["text"].ToString();
            }
            catch
            {
                answer = "Произошла ошибка. Попробуйте повторить запрос.";
            }
            return answer.TrimStart('\n', '\r');
        }

        public static string GiveRole(long id, string role)
        {
            using var db = new InfoContext();
            var user = db.TableUsers.First(x => x.Id == id);
            if (user.Roles is null) user.Roles = role;
            else user.Roles += $", {role}";
            db.SaveChanges();
            return $"{user.Name} получает роль {role}";
        }

        public static string DelRole(long id, string role)
        {
            using var db = new InfoContext();
            var user = db.TableUsers.First(x => x.Id == id);
            string separator = ", ";
            string prefix = role + separator;
            string suffix = separator + role;
            if (user.Roles == role) user.Roles = null;
            else if (user.Roles.StartsWith(prefix)) user.Roles = user.Roles.Replace(prefix, "");
            else user.Roles = user.Roles.Replace(suffix, "");
            db.SaveChanges();
            return $"{user.Name} теряет роль {role}";
        }

        public static string GetRoles()
        {
            using var db = new InfoContext();
            var roleUsers = db.TableUsers.Where(u => u.Roles != null).Select(u => new { u.Roles, u.Name }).ToList();
            var roles = roleUsers.SelectMany(u => u.Roles.Split(", ", StringSplitOptions.RemoveEmptyEntries)).Distinct().ToList();
            var sb = new StringBuilder("Список ролей:\n");
            foreach (var role in roles)
            {
                var users = roleUsers.Where(u => u.Roles.StartsWith(role) || u.Roles.Contains(", " + role)).Select(u => u.Name).ToList();
                string userList = string.Join(", ", users);
                sb.Append($"<code>{role}</code>: {userList}\n");
            }
            return sb.ToString();
        }

        public static string PrintNfc()
        {
            using var db = new InfoContext();
            List<Users> usersWithNfc = db.TableUsers.Where(u => u.Nfc == true).OrderBy(u => u.StartNfc).ToList();
            string res = "Без мата 👮‍♂️\n";
            for (int i = 0; i < usersWithNfc.Count; i++)
            {
                var ts = DateTime.Now - usersWithNfc[i].StartNfc;
                res += $"{i + 1} || {usersWithNfc[i].Name}: {ts.Days} d, {ts.Hours} h, {ts.Minutes} m.\n";
            }
            return res;
        }

        public static async Task<InlineKeyboardMarkup> VoteBan(InlineKeyboardMarkup inlineKeyboard, CallbackQuery callbackQuery, string option, ChatId chatId, ITelegramBotClient botClient, CancellationToken cancellationToken, long userId)
        {
            string pattern = @"\d+";
            Match match = Regex.Match(option, pattern);
            var count = int.Parse(match.Value);
            inlineKeyboard = callbackQuery.Message.ReplyMarkup;
            if (option[0] == 'y')
            {
                if (!option.Contains(callbackQuery.From.Id.ToString()) && !inlineKeyboard.InlineKeyboard.Last().Last().CallbackData.Contains(callbackQuery.From.Id.ToString()))
                {
                    await botClient.SendTextMessageAsync(chatId: chatId, replyToMessageId: callbackQuery.Message.MessageId, text: $"{callbackQuery.From.FirstName} {callbackQuery.From.LastName} проголосовал за", cancellationToken: cancellationToken);
                    Regex regex = new(pattern);
                    count++;
                    option = regex.Replace(option, count.ToString(), 1);
                    option += $"_{callbackQuery.From.Id}";
                    var mes = $"Да ✅ - {count}";
                    inlineKeyboard.InlineKeyboard.First().First().Text = mes;
                    inlineKeyboard.InlineKeyboard.First().First().CallbackData = option;
                    match = Regex.Match(inlineKeyboard.InlineKeyboard.Last().Last().Text, pattern);
                    var count2 = int.Parse(match.Value);
                    if (count >= 5 && count2 < count)
                    {
                        var db = new InfoContext();
                        var user = db.TableUsers.First(x => x.Id == userId);
                        await botClient.BanChatMemberAsync(chatId: chatId, userId: userId, cancellationToken: cancellationToken);
                        inlineKeyboard = null;
                        await botClient.SendTextMessageAsync(chatId: chatId, replyToMessageId: callbackQuery.Message.MessageId, text: $"{user.Name} забанен", cancellationToken: cancellationToken);
                    }
                    else if (count2 >= 5 && count2 >= count)
                    {
                        inlineKeyboard = null;
                        var db = new InfoContext();
                        var user = db.TableUsers.First(x => x.Id == userId);
                        await botClient.SendTextMessageAsync(chatId: chatId, replyToMessageId: callbackQuery.Message.MessageId, text: $"{user.Name} не забанен", cancellationToken: cancellationToken);
                    }
                }
            }
            else
            {
                if (!option.Contains(callbackQuery.From.Id.ToString()) && !inlineKeyboard.InlineKeyboard.First().First().CallbackData.Contains(callbackQuery.From.Id.ToString()))
                {
                    await botClient.SendTextMessageAsync(chatId: chatId, replyToMessageId: callbackQuery.Message.MessageId, text: $"{callbackQuery.From.FirstName} {callbackQuery.From.LastName} проголосовал против", cancellationToken: cancellationToken);
                    Regex regex = new(pattern);
                    count++;
                    option = regex.Replace(option, count.ToString(), 1);
                    option += $"_{callbackQuery.From.Id}";
                    var mes = $"Нет ❌ - {count}";
                    inlineKeyboard.InlineKeyboard.Last().Last().Text = mes;
                    inlineKeyboard.InlineKeyboard.Last().Last().CallbackData = option;
                }
            }
            return inlineKeyboard;
        }
    }
}
