using System.Threading.Tasks;
using Telegram.Bot.Types;
using System.Linq;
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
    public static class ResponseHandlers
    {
        public static async Task<string> Me(long idUser)
        {
            await using var db = new InfoContext();
            var user = db.TableUsers.First(x => x.Id == idUser);
            var result = new StringBuilder($"👨‍❤️‍👨 Имя: {user.Name}" +
                                           $"\n🕰 Время последнего сообщения: {user.TimeLastMes:yy/MM/dd HH:mm:ss}" +
                                           $"\n⭐️ Lvl: {user.Lvl}({user.CurXp}/{Сalculation.GenerateXpForLevel(user.Lvl + 1)})" +
                                           $"\n🎭 Роли: {user.Roles}" +
                                           $"\n🏆 Место в топе по уровню: {Сalculation.PlaceLvl(user.Id, db.TableUsers)}" +
                                           $"\n😇 Rep: {user.Rep}" +
                                           $"\n🥇 Место в топе по репутации: {Сalculation.PlaceRep(user.Id, db.TableUsers)}" +
                                           $"\n🎖 Место в топе по лексикону: {await Сalculation.PlaceLexicon(user)}" +
                                           $"\n🤬 Кол-во варнов: {user.Warns}/3" +
                                           $"\n🗓 Дата последнего варна/снятия варна: {user.LastTime:yyyy-MM-dd}\n");
            await using var connection = new SqlConnection(ConStringDbLexicon);
            await connection.OpenAsync();
            await using var command =
                new SqlCommand($"SELECT COUNT(*) FROM dbo.TableUsersLexicons where [UserID] = {user.Id}", connection);
            var count = (int)(await command.ExecuteScalarAsync())!;
            result.AppendLine($"🔤 Лексикон: {count} слов");
            result.AppendLine("📖 Личный топ слов:");
            await using var command2 = new SqlCommand($"select top 10 * from dbo.TableUsersLexicons where " +
                                                      $"[UserID] = {user.Id} order by [Count] desc", connection);
            await using var reader = await command2.ExecuteReaderAsync();
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

            for (var i = 0; i < words.Count; i++)
            {
                result.AppendLine($"{Keycaps[i]} {words[i].Word} || {words[i].Count}");
            }

            return result.ToString();
        }

        public static string Warn(Update update)
        {
            using var db = new InfoContext();
            if (update.Message?.ReplyToMessage?.From == null) return null;
            var idUser = update.Message.ReplyToMessage.From.Id;
            var user = db.TableUsers.First(x => x.Id == idUser);
            user.Warns++;
            user.LastTime = DateTime.Now;
            db.SaveChanges();
            return $"{user.Name} получает предупреждение({user.Warns}/3)";
        }

        public static string Unwarn(Update update)
        {
            using var db = new InfoContext();
            if (update.Message?.ReplyToMessage?.From != null)
            {
                var idUser = update.Message.ReplyToMessage.From.Id;
                var user = db.TableUsers.First(x => x.Id == idUser);
                user.Warns--;
                user.LastTime = DateTime.Now;
                db.SaveChanges();
                return $"С {user.Name} снимается 1 варн({user.Warns}/3)";
            }

            return null;
        }

        public static string TopLvl(int number)
        {
            using var db = new InfoContext();
            var users = db.TableUsers.OrderByDescending(x => x.Lvl).ThenByDescending(y => y.CurXp).Skip(number)
                .Take(50);
            StringBuilder sb = new();
            sb.Append("🏆 \n");
            var i = number + 1;
            foreach (var user in users)
            {
                sb.AppendLine(
                    $"{i}. {user.Name} lvl {user.Lvl}({user.CurXp}/{Сalculation.GenerateXpForLevel(user.Lvl + 1)})");
                i++;
            }

            return sb.ToString();
        }

        public static string TopRep(int number)
        {
            using var db = new InfoContext();
            var users = db.TableUsers.OrderByDescending(x => x.Rep).Skip(number).Take(50);
            var resultBuilder = new StringBuilder("🥇 \n");
            var i = number + 1;
            foreach (var user in users)
            {
                resultBuilder.AppendLine($"{i}. {user.Name} rep {user.Rep}");
                i++;
            }

            return resultBuilder.ToString();
        }

        public static async Task<string> MeWords(CallbackQuery callbackQuery, int number)
        {
            await using var db = new InfoContext();
            if (callbackQuery.Message?.ReplyToMessage?.From == null) return null;
            var idUser = callbackQuery.Message.ReplyToMessage.From.Id;
            var user = db.TableUsers.First(x => x.Id == idUser);
            if (callbackQuery.Message.Text == null) return null;
            var match = Regex.Match(callbackQuery.Message.Text, @"^\d+");

            int i;
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
            await using var connection = new SqlConnection(ConStringDbLexicon);
            var result = new StringBuilder();
            var rows = await connection.QueryAsync<Words>($"select * from dbo.TableUsersLexicons " +
                                                          $"where [UserID] = {user.Id} ORDER BY [Count] DESC OFFSET @Offset " +
                                                          $"ROWS FETCH NEXT 10 ROWS ONLY", new { Offset = offset });
            rows.ToList().ForEach((word) =>
            {
                result.AppendLine($"{i}. {word.Word} || {word.Count}");
                i++;
            });
            return result.ToString();
        }

        public static async Task<string> TopWords(int number)
        {
            await using SqlConnection connection = new(ConStringDbLexicon);
            await connection.OpenAsync();
            SqlCommand command =
                new(
                    $"SELECT ROW_NUMBER() OVER (ORDER BY SUM([Count]) DESC) AS RowNumber, [Word], SUM([Count]) AS WordCount FROM dbo.TableUsersLexicons GROUP BY [Word] order by [WordCount] desc OFFSET {number} ROWS FETCH NEXT 50 ROWS ONLY",
                    connection);
            var reader = await command.ExecuteReaderAsync();
            var result = new StringBuilder("📖 Топ слов:\n");
            while (await reader.ReadAsync())
            {
                result.AppendLine($"{reader.GetInt64(0)}. {reader.GetString(1)} || {reader.GetInt32(2)}");
            }

            await reader.CloseAsync();
            return result.ToString();
        }

        public static async Task<string> TopLexicon(int number)
        {
            await using SqlConnection connection = new(ConStringDbLexicon);
            await connection.OpenAsync();
            SqlCommand command =
                new(
                    $"SELECT ROW_NUMBER() OVER (ORDER BY COUNT(*) DESC) AS RowNumber, UserID, COUNT(*) AS UserCount FROM dbo.TableUsersLexicons GROUP BY UserID ORDER BY UserCount desc OFFSET {number} ROWS FETCH NEXT 50 ROWS ONLY",
                    connection);
            var reader = await command.ExecuteReaderAsync();
            var result = new StringBuilder("🎖 Топ по лексикону:\n");
            await using var db = new InfoContext();
            while (await reader.ReadAsync())
            {
                result.AppendLine(
                    $"{reader.GetInt64(0)}. {db.TableUsers.FirstOrDefault(x => x.Id == reader.GetInt64(1))?.Name} || {reader.GetInt32(2)}");
            }

            await reader.CloseAsync();
            return result.ToString();
        }

        public static async Task<string> PersonalWord(long id, string word)
        {
            await using var db = new InfoContext();
            var user = db.TableUsers.First(x => x.Id == id);
            await using var connection = new SqlConnection(ConStringDbLexicon);
            await connection.OpenAsync();
            SqlCommand command =
                new(
                    $"select * from ( SELECT ROW_NUMBER() OVER (ORDER BY SUM([Count]) DESC) AS RowNumber, [Word], SUM([Count]) AS WordCount FROM dbo.TableUsersLexicons where [UserID] = {id} GROUP BY [Word]) as T where Word = '{word}'",
                    connection);
            var reader = await command.ExecuteReaderAsync();
            var result = "";
            while (await reader.ReadAsync())
            {
                result = $"✍🏿 {user.Name} употреблял слово {word} {reader.GetInt32(2)} раз. Оно занимает " +
                         $"{reader.GetInt64(0)} место по частоте употребления";
            }

            await reader.CloseAsync();
            if (result == "") result = $"{user.Name} ни разу не употреблял слово {word}";
            return result;
        }

        public static async Task<string> Word(string word)
        {
            await using SqlConnection connection = new(ConStringDbLexicon);
            await connection.OpenAsync();
            SqlCommand command = new($"select * from ( SELECT ROW_NUMBER() OVER (ORDER BY SUM([Count]) DESC) " +
                                     $"AS RowNumber, [Word], SUM([Count]) AS WordCount FROM dbo.TableUsersLexicons " +
                                     $"GROUP BY [Word]) as T where Word = '{word}'", connection);
            var reader = await command.ExecuteReaderAsync();
            var result = "";
            while (await reader.ReadAsync())
            {
                result = $"✍🏿 Слово {word} употреблялось {reader.GetInt32(2)} раз. Оно занимает " +
                         $"{reader.GetInt64(0)} место по частоте употребления";
            }

            await reader.CloseAsync();
            if (result == "") result = "Слово ни разу не употреблялось";
            return result;
        }

        public static string RepUp(Update update, InfoContext db, string mes)
        {
            mes = Regex.Replace(mes, @"[^\w\d\s+👍👍🏼👍🏽👍🏾👍🏿]", "").ToLower();
            if (update.Message is not { From: not null, ReplyToMessage.From: not null } ||
                update.Message.ReplyToMessage.From.Id == update.Message.From.Id) return "";
            var list = mes.Split(" ");
            if (!list.Any(RepWords.Contains)) return "";
            var idUser = update.Message.ReplyToMessage.From.Id;
            var user = db.TableUsers.FirstOrDefault(x => x.Id == idUser);
            if (user == null) return "";
            {
                user.Rep++;
                db.SaveChanges();
                var user1 = db.TableUsers.FirstOrDefault(x => x.Id == update.Message.From.Id);
                if (user1 != null)
                    return $"{user1.Name}({user1.Rep}) увеличил репутацию {user.Name} на 1({user.Rep})";
            }
            return "";
        }

        public static async Task<string> RequestChatGpt(int id, MessageEntry[] messages)
        {
            var services = new ServiceCollection();
            services.AddChatGptClient(new ChatGptClientOptions { ApiKey = SshKey });
            var app = services.BuildServiceProvider();
            var service = app.GetRequiredService<ChatGptClient>();
            var res = await service.ChatAsync(new ChatCompletionRequest
            {
                Model = "gpt-3.5-turbo", //only gpt-3.5-turbo or gpt-3.5-turbo-0301 can be chosen now
                Messages = messages
            }, default);
            if (!Program.Context.TryGetValue(id, out var array))
                return res.Choices[0].Message.Content + "\n" + id.ToString();
            var index = Array.IndexOf(array, null);
            array[index] = new MessageEntry
                { Role = res.Choices[0].Message.Role, Content = res.Choices[0].Message.Content };
            Program.Context[id] = array;

            return res.Choices[0].Message.Content + "\n" + id.ToString();
        }

        [Obsolete("Obsolete")]
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
                    var imageUrl = string.Join("\n", imageResult.Results.Select(r => r.Url));
                    using WebClient webClient = new();
                    var imageBytes = webClient.DownloadData(imageUrl);
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
                answer = result["text"]?.ToString();
            }
            catch
            {
                answer = "Произошла ошибка. Попробуйте повторить запрос.";
            }

            return answer?.TrimStart('\n', '\r');
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
            var separator = ", ";
            var prefix = role + separator;
            var suffix = separator + role;
            if (user.Roles == role) user.Roles = null;
            else if (user.Roles.StartsWith(prefix)) user.Roles = user.Roles.Replace(prefix, "");
            else user.Roles = user.Roles.Replace(suffix, "");
            db.SaveChanges();
            return $"{user.Name} теряет роль {role}";
        }

        public static string GetRoles(int start)
        {
            using var db = new InfoContext();
            var roleUsers = db.TableUsers.Where(u => u.Roles != null).Select(u => new { u.Roles, u.Name }).ToList();
            var roles = roleUsers.SelectMany(u => u.Roles.Split(", ", StringSplitOptions.RemoveEmptyEntries)).Distinct()
                .ToList();
            roles.Sort();
            roles = roles.Skip(start).Take(20).ToList();
            var sb = new StringBuilder("");
            foreach (var role in roles)
            {
                var users = roleUsers.Where(u => u.Roles.StartsWith(role) || u.Roles.Contains(", " + role))
                    .Select(u => u.Name).ToList();
                var userList = string.Join(", ", users);
                start++;
                sb.Append($"{start} || <code>{role}</code>: {userList}\n");
            }

            return sb.ToString();
        }

        public static string PrintNfc()
        {
            using var db = new InfoContext();
            var usersWithNfc = db.TableUsers.Where(u => u.Nfc == true).OrderBy(u => u.StartNfc).ToList();
            var res = "Без мата 👮‍♂️\n";
            for (var i = 0; i < usersWithNfc.Count; i++)
            {
                var ts = DateTime.Now - usersWithNfc[i].StartNfc;
                res += $"{i + 1} || {usersWithNfc[i].Name}: {ts.Days} d, {ts.Hours} h, {ts.Minutes} m.\n";
            }

            return res;
        }

        public static string Mariages()
        {
            using var db = new InfoContext();
            var users = db.TableUsers.Where(x => x.Mariage != 0).OrderBy(y => y.DateMariage).ToList();
            StringBuilder sb = new();
            sb.Append("💒 список браков: \n");
            var number = 1;
            for (var i = 0; i < users.Count; i++)
            {
                var ts = DateTime.Now - users[i].DateMariage;
                if (users[i].Id == users[i].Mariage)
                {
                    sb.AppendLine(
                        $"{number}. {users[i].Name} и {users[i].Name} c {users[i].DateMariage:yy/MM/dd HH:mm:ss} {ts.Days} d, {ts.Hours} h, {ts.Minutes} m");
                    number++;
                }
                else if (i != users.Count - 1 && users[i + 1].Id == users[i].Mariage)
                {
                    sb.AppendLine(
                        $"{number}. {users[i].Name} и {users[i + 1].Name} c {users[i].DateMariage:yy/MM/dd HH:mm:ss} {ts.Days} d, {ts.Hours} h, {ts.Minutes} m");
                    number++;
                }
            }

            return sb.ToString();
        }

        public static async Task<InlineKeyboardMarkup> AcceptMariage(string option, CallbackQuery callbackQuery,
            ITelegramBotClient botClient, long chatId, long userId, CancellationToken cancellationToken)
        {
            await using var db = new InfoContext();
            if (callbackQuery.Message == null) return null;
            var inlineKeyboard = callbackQuery.Message.ReplyMarkup;
            var user2 = db.TableUsers.First(x => x.Id == userId);
            if (callbackQuery.From.Id != userId || user2.Mariage != 0) return inlineKeyboard;
            var pattern = @"\d+";
            {
                if (pattern == null) throw new ArgumentNullException(nameof(pattern));
                var match = Regex.Match(option, pattern);
                var id = long.Parse(match.Value);
                var user1 = db.TableUsers.First(x => x.Id == id);
                if (option[1] == 'y')
                {
                    user1.Mariage = user2.Id;
                    user2.Mariage = user1.Id;
                    user1.DateMariage = DateTime.Now;
                    user2.DateMariage = user1.DateMariage;
                    await db.SaveChangesAsync(cancellationToken);
                    await botClient.SendTextMessageAsync(chatId: chatId,
                        text: $"👰🏿 👰🏿 {user2.Name} и {user1.Name} заключили брак",
                        cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId: chatId,
                        text: $"{user2.Name} отказался от брака c {user1.Name}", cancellationToken: cancellationToken);
                }
            }

            return null;
        }

        public static async Task<bool> GetChatPermissions(string option, CallbackQuery callbackQuery,
            ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            var userId = long.Parse(option[2..]);
            if (callbackQuery.From.Id != userId) return false;
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
                    CanPinMessages = true
                }, cancellationToken: cancellationToken);
                await botClient.SendTextMessageAsync(chatId: chatId,
                    text: $"Привет, {callbackQuery.From.FirstName}.{Greeting}",
                    cancellationToken: cancellationToken);
            }
            else
            {
                await botClient.BanChatMemberAsync(chatId: chatId, userId: userId,
                    cancellationToken: cancellationToken);
            }

            return true;
        }
    }
}