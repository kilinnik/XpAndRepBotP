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
using SixLabors.ImageSharp;
using System.Diagnostics.Metrics;

namespace XpAndRepBot
{
    public class ResponseHandlers
    {
        public static async Task<string> Me(long idUser)
        {
            using var db = new InfoContext();
            var user = db.TableUsers.First(x => x.Id == idUser);
            var result = new StringBuilder($"👨‍❤️‍👨 Имя: {user.Name}\n🕰 Время последнего сообщения: {user.TimeLastMes:yy/MM/dd HH:mm:ss}\n⭐️ Lvl: {user.Lvl}({user.CurXp}/{Сalculation.Genlvl(user.Lvl + 1)})\n🎭 Роли: {user.Roles}\n🏆 Место в топе по уровню: {Сalculation.PlaceLvl(user.Id, db.TableUsers)}\n😇 Rep: {user.Rep}\n🥇 Место в топе по репутации: {Сalculation.PlaceRep(user.Id, db.TableUsers)}\n🎖 Место в топе по лексикону: {await Сalculation.PlaceLexicon(user)}\n🤬 Кол-во варнов: {user.Warns}/3\n🗓 Дата последнего варна/снятия варна: {user.LastTime:yyyy-MM-dd}\n");
            using var connection = new SqlConnection(ConStringDbLexicon);
            await connection.OpenAsync();
            using var command = new SqlCommand($"SELECT COUNT(*) FROM dbo.TableUsersLexicons where [UserID] = {user.Id}", connection);
            int count = (int)await command.ExecuteScalarAsync();
            result.AppendLine($"🔤 Лексикон: {count} слов");
            result.AppendLine("📖 Личный топ слов:");
            using var command2 = new SqlCommand($"select top 10 * from dbo.TableUsersLexicons where [UserID] = {user.Id} order by [Count] desc", connection);
            using var reader = await command2.ExecuteReaderAsync();
            var words = new List<Words>();
            while (await reader.ReadAsync())
            {
                var word = new Words
                {
                    Word = reader.GetString(1),
                    Count = reader.GetInt32(2)
                };
                words.Add(word);
            }
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
            if (offset == 0) return await Me(idUser);
            using var connection = new SqlConnection(ConStringDbLexicon);
            var result = new StringBuilder();
            var rows = await connection.QueryAsync<Words>($"select * from dbo.TableUsersLexicons where [UserID] = {user.Id} ORDER BY [Count] DESC OFFSET @Offset ROWS FETCH NEXT 10 ROWS ONLY", new { Offset = offset });
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
            using SqlConnection connection = new(ConStringDbLexicon);
            await connection.OpenAsync();
            SqlCommand command = new($"SELECT ROW_NUMBER() OVER (ORDER BY SUM([Count]) DESC) AS RowNumber, [Word], SUM([Count]) AS WordCount FROM dbo.TableUsersLexicons GROUP BY [Word] order by [WordCount] desc OFFSET {number} ROWS FETCH NEXT 50 ROWS ONLY", connection);
            SqlDataReader reader = await command.ExecuteReaderAsync();
            var result = new StringBuilder("📖 Топ слов:\n");
            while (await reader.ReadAsync())
            {
                result.AppendLine($"{reader.GetInt64(0)}. {reader.GetString(1)} || {reader.GetInt32(2)}");
            }
            reader.Close();
            return result.ToString();
        }

        public static async Task<string> TopLexicon(int number)
        {
            using SqlConnection connection = new(ConStringDbLexicon);
            await connection.OpenAsync();
            SqlCommand command = new($"SELECT ROW_NUMBER() OVER (ORDER BY COUNT(*) DESC) AS RowNumber, UserID, COUNT(*) AS UserCount FROM dbo.TableUsersLexicons GROUP BY UserID ORDER BY UserCount desc OFFSET {number} ROWS FETCH NEXT 50 ROWS ONLY", connection);
            SqlDataReader reader = await command.ExecuteReaderAsync();
            var result = new StringBuilder("🎖 Топ по лексикону:\n");
            using var db = new InfoContext();
            while (await reader.ReadAsync())
            {
                result.AppendLine($"{reader.GetInt64(0)}. {db.TableUsers.FirstOrDefault(x => x.Id == reader.GetInt64(1)).Name} || {reader.GetInt32(2)}");
            }
            reader.Close();
            return result.ToString();
        }
        public static async Task<string> PersonalWord(long id, string word)
        {
            using var db = new InfoContext();
            var user = db.TableUsers.First(x => x.Id == id);
            using var connection = new SqlConnection(ConStringDbLexicon);
            await connection.OpenAsync();
            SqlCommand command = new($"select * from ( SELECT ROW_NUMBER() OVER (ORDER BY SUM([Count]) DESC) AS RowNumber, [Word], SUM([Count]) AS WordCount FROM dbo.TableUsersLexicons where [UserID] = {id} GROUP BY [Word]) as T where Word = '{word}'", connection);
            SqlDataReader reader = await command.ExecuteReaderAsync();
            var result = "";
            while (await reader.ReadAsync())
            {
                result = $"✍🏿 {user.Name} употреблял слово {word} {reader.GetInt32(2)} раз. Оно занимает {reader.GetInt64(0)} место по частоте употребления";
            }
            reader.Close();
            if (result == "") result = $"{user.Name} ни разу не употреблял слово {word}";
            return result;
        }

        public static async Task<string> Word(string word)
        {
            using SqlConnection connection = new(ConStringDbLexicon);
            await connection.OpenAsync();
            SqlCommand command = new($"select * from ( SELECT ROW_NUMBER() OVER (ORDER BY SUM([Count]) DESC) AS RowNumber, [Word], SUM([Count]) AS WordCount FROM dbo.TableUsersLexicons GROUP BY [Word]) as T where Word = '{word}'", connection);
            SqlDataReader reader = await command.ExecuteReaderAsync();
            var result = "";
            while (await reader.ReadAsync())
            {
                result = $"✍🏿 Слово {word} употреблялось {reader.GetInt32(2)} раз. Оно занимает {reader.GetInt64(0)} место по частоте употребления";
            }
            reader.Close();
            if (result == "") result = "Слово ни разу не употреблялось";
            return result;
        }

        public static string RepUp(Update update, InfoContext db, string mes)
        {
            mes = Regex.Replace(mes, @"[^\w\d\s+👍👍🏼👍🏽👍🏾👍🏿]", "").ToLower();
            if (update.Message.ReplyToMessage.From.Id != update.Message.From.Id)
            {
                var list = mes.Split(" ");
                if (list.Any(RepWords.Contains))
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

        public static async Task<string> RequestChatGPT(int id, MessageEntry[] messages)
        {
            var services = new ServiceCollection();
            services.AddChatGptClient(new() { ApiKey = SSHKey });
            var app = services.BuildServiceProvider();
            var service = app.GetRequiredService<ChatGptClient>();
            var res = await service.ChatAsync(new ChatCompletionRequest
            {
                Model = "gpt-3.5-turbo", //only gpt-3.5-turbo or gpt-3.5-turbo-0301 can be chosen now
                Messages = messages
            }, default);
            if (Program.Context.TryGetValue(id, out MessageEntry[] array))
            {
                int index = Array.IndexOf(array, null);
                array[index] = new MessageEntry { Role = res.Choices[0].Message.Role, Content = res.Choices[0].Message.Content };
                Program.Context[id] = array;
            }
            return res.Choices[0].Message.Content + "\n" + id.ToString();
        }

        public static async Task<InputOnlineFile> GenerateImage(IOpenAIService sdk, string prompt)
        {
            try
            {
                var imageResult = await sdk.Image.CreateImage(new ImageCreateRequest
                {
                    Prompt = prompt,
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
            roles.Sort();
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

        public static string Mariages()
        {
            using var db = new InfoContext();
            var users = db.TableUsers.Where(x => x.Mariage != 0).OrderBy(y => y.DateMariage).ToList();
            StringBuilder sb = new();
            sb.Append("💒 список браков: \n");
            int number = 1;
            for (int i = 0; i < users.Count; i++)
            {
                TimeSpan ts = DateTime.Now - users[i].DateMariage;
                if (users[i].Id == users[i].Mariage)
                {
                    sb.AppendLine($"{number}. {users[i].Name} и {users[i].Name} c {users[i].DateMariage:yy/MM/dd HH:mm:ss} {ts.Days} d, {ts.Hours} h, {ts.Minutes} m");
                    number++;
                }
                else if (i != users.Count - 1 && users[i + 1].Id == users[i].Mariage)
                {
                    sb.AppendLine($"{number}. {users[i].Name} и {users[i + 1].Name} c {users[i].DateMariage:yy/MM/dd HH:mm:ss} {ts.Days} d, {ts.Hours} h, {ts.Minutes} m");
                    number++;
                }
            }
            return sb.ToString();
        }

        public static async Task<InlineKeyboardMarkup> AcceptMariage(string option, CallbackQuery callbackQuery, ITelegramBotClient botClient, long chatId, long userId, CancellationToken cancellationToken)
        {
            using var db = new InfoContext();
            InlineKeyboardMarkup inlineKeyboard = callbackQuery.Message.ReplyMarkup;
            var user2 = db.TableUsers.First(x => x.Id == userId);
            if (callbackQuery.From.Id == userId && user2.Mariage == 0)
            {
                string pattern = @"\d+";
                Match match = Regex.Match(option, pattern);
                var id = long.Parse(match.Value);
                var user1 = db.TableUsers.First(x => x.Id == id);
                inlineKeyboard = null;
                if (option[1] == 'y')
                {
                    user1.Mariage = user2.Id;
                    user2.Mariage = user1.Id;
                    user1.DateMariage = DateTime.Now;
                    user2.DateMariage = user1.DateMariage;
                    db.SaveChanges();
                    await botClient.SendTextMessageAsync(chatId: chatId, text: $"👰🏿 👰🏿 {user2.Name} и {user1.Name} заключили брак", cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: $"{user2.Name} отказался от брака c {user1.Name}", cancellationToken: cancellationToken);
                }
            }
            return inlineKeyboard;
        }

        public static async Task<bool> GetChatPermissions(string option, CallbackQuery callbackQuery, ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            long userId = long.Parse(option[2..]);
            var flag = false;
            if (callbackQuery.From.Id == userId)
            {
                if (option.Contains('y'))
                {
                    await botClient.RestrictChatMemberAsync(chatId: chatId, userId, new ChatPermissions
                    {
                        CanSendMessages = true,
                        CanSendMediaMessages = true,
                        CanSendOtherMessages = true,
                        CanSendPolls = true,
                        CanAddWebPagePreviews = true,
                        CanChangeInfo = true,
                        CanInviteUsers = true,
                        CanPinMessages = true,
                    }, cancellationToken: cancellationToken);
                    await botClient.SendTextMessageAsync(chatId: chatId, text: $"Привет, {callbackQuery.From.FirstName}.{Greeting}", cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.BanChatMemberAsync(chatId: chatId, userId: userId, cancellationToken: cancellationToken);
                }
                flag = true;
            }
            return flag;
        }
    }
}
