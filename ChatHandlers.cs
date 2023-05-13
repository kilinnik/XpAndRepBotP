using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using static XpAndRepBot.Consts;
using Mirror.ChatGpt.Models.ChatGpt;

namespace XpAndRepBot
{
    public class ChatHandlers
    {
        public static async Task LvlUp(ITelegramBotClient botClient, Update update, InfoContext db, Users user, CancellationToken cancellationToken)
        {
            while (user.CurXp >= Сalculation.Genlvl(user.Lvl + 1))
            {
                user.Lvl++;
                user.CurXp -= Сalculation.Genlvl(user.Lvl);
                if (user.Lvl > 4) await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: $"{user.Name} получает {user.Lvl} lvl", cancellationToken: cancellationToken);
            }
            db.SaveChanges();
        }

        public static async Task RequestChatGPT(ITelegramBotClient botClient, Update update, string mes, CancellationToken cancellationToken)
        {
            if (mes.Contains("@XpAndRepBot") || (update.Message?.Chat?.Id == MID && update.Message.ReplyToMessage == null) || (update.Message?.Chat?.Id == IID && update.Message.ReplyToMessage == null))
            {
                Program.Context.Add(update.Message.MessageId, new MessageEntry[1000]);
                Program.ListBranches.Add(new List<int>() { update.Message.MessageId });
            }
            if (update?.Message?.ReplyToMessage?.From?.Id != 5759112130) mes = mes.Replace("@XpAndRepBot", "");
            //mes = mes.Replace("\"", "");
            //mes = mes.Replace("\n", " ");
            if (mes.Any(x => char.IsLetter(x)) || mes.Any(x => char.IsNumber(x)) || mes.Any(x => char.IsPunctuation(x)))
            {
                var id = update.Message.MessageId;
                if (update.Message.ReplyToMessage != null)
                {
                    Regex regex = new(@"\b\d+\b$");
                    Match match = regex.Match(update.Message.ReplyToMessage.Text);
                    int number = 0;
                    if (match.Success)
                    {
                        number = int.Parse(match.Value);
                    }
                    List<int> targetList = Program.ListBranches.Find(list => list.Contains(number));
                    if (targetList != null)
                    {
                        targetList.Add(update.Message.MessageId);
                    }
                    if (targetList != null && targetList.Count > 0)
                    {
                        id = targetList[0];
                    }
                }
                MessageEntry[] subArray = new MessageEntry[1];
                if (Program.Context.TryGetValue(id, out MessageEntry[] array))
                {
                    int index = Array.IndexOf(array, null);
                    array[index] = new MessageEntry { Role = Roles.User, Content = mes };
                    Program.Context[id] = array;
                    subArray = new ArraySegment<MessageEntry>(array, 0, index + 1).ToArray();

                }
                if (subArray[0] != null)
                {
                    try
                    {
                        try
                        {
                            await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: await ResponseHandlers.RequestChatGPT(id, subArray), cancellationToken: cancellationToken);
                        }
                        catch
                        {
                            await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: await ResponseHandlers.RequestChatGPT(id, subArray), cancellationToken: cancellationToken);
                        }
                    }
                    catch { await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: "Произошла ошибка. Попробуйте повторить вопрос. Чтобы начать новую ветку, упомяните бота. Чтобы продолжить ветку, ответьте на сообщение от бота", cancellationToken: cancellationToken); }
                }
            }
        }

        public static async Task ReputationUp(ITelegramBotClient botClient, Update update, InfoContext db, string mes, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: ResponseHandlers.RepUp(update, db, mes), cancellationToken: cancellationToken);
            }
            catch { }
        }

        public static async Task Delete(ITelegramBotClient botClient, Update update, Users user, CancellationToken cancellationToken)
        {
            try
            {
                if (update?.Message?.Animation != null && user.Lvl < 10) //удаление гифок
                {
                    await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
                }
                if (update?.Message?.Sticker != null && user.Lvl < 15) //удаление стикеров
                {
                    var sticker = update.Message.Sticker;
                    var set = await botClient.GetStickerSetAsync(sticker.SetName, cancellationToken: cancellationToken);
                    if (set.Name != "UnoWarStickers")
                    {
                        await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
                    }
                }
                if (update?.Message?.Poll != null && user.Lvl < 20) //удаление опросов
                {
                    await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
                }
            }
            catch { }
        }

        public static async Task CreateAndFillTable(Users user, string mes)
        {
            using SqlConnection connection = new(ConStringDbLexicon);
            await connection.OpenAsync();
            var listWords = mes.Split(new string[] { " ", "\r\n", "\n" }, StringSplitOptions.None);
            List<string> validWords = new();
            for (int i = 0; i < listWords.Length; i++)
            {
                string cleanedWord = Regex.Replace(listWords[i], @"[^\w\d\s]", "");
                if (string.IsNullOrWhiteSpace(cleanedWord)) continue;
                cleanedWord = cleanedWord.ToLower();
                validWords.Add(cleanedWord);
            }
            foreach (var word in validWords)
            {
                SqlCommand updateCommand = new($"update dbo.TableUsersLexicons set [Count] = [Count] + 1 where [Word] = '{word}' and [UserID] = {user.Id}", connection);
                int rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    SqlCommand insertCommand = new($"insert into dbo.TableUsersLexicons (UserID, Word, Count) values ({user.Id}, '{word}', 1)", connection);
                    await insertCommand.ExecuteNonQueryAsync();
                }
            }
        }

        public static string Mention(List<Users> users)
        {
            string res = "";
            using var db = new InfoContext();
            var count = users.Count - 1;
            for (int i = 0; i < count; i++)
            {
                res += $"<a href=\"tg://user?id={users[i].Id}\">{users[i].Name}</a>, ";
            }
            res += $"<a href=\"tg://user?id={users[count].Id}\">{users[count].Name}</a>.";
            return res;
        }

        public static string Nfc(Users user, DbContext db)
        {
            user.Nfc = false;
            TimeSpan ts = DateTime.Now - user.StartNfc;
            if (ts.Ticks > user.BestTime) user.BestTime = ts.Ticks;
            db.SaveChanges();
            string t = string.Format("{0} d, {1} h, {2} m.", ts.Days, ts.Hours, ts.Minutes);
            return $"{user.Name} нарушил условия no fuck challenge. Время: {t}";
        }
    }
}
