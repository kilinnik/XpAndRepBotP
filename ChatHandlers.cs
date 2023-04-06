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
            if (update?.Message?.ReplyToMessage?.From?.Id != 5759112130) mes = mes.Replace("@XpAndRepBot", "");
            mes = mes.Replace("\"", "");
            mes = mes.Replace("\n", " ");
            if (mes.Any(x => char.IsLetter(x)) || mes.Any(x => char.IsNumber(x)) || mes.Any(x => char.IsPunctuation(x)))
            {
                try
                {
                    try
                    {
                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: await ResponseHandlers.RequestChatGPT(mes), cancellationToken: cancellationToken);
                    }
                    catch
                    {
                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: await ResponseHandlers.RequestChatGPT(mes), cancellationToken: cancellationToken);
                    }
                }
                catch { await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: "Произошла ошибка. Попробуйте повторить вопрос", cancellationToken: cancellationToken); }
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
            if (update?.Message?.Animation != null && user.Lvl < 10) //удаление гифок
            {
                await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
            }
            if (update?.Message?.Sticker != null && user.Lvl < 15) //удаление стикеров
            {
                await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
            }
            if (update?.Message?.Poll != null && user.Lvl < 20) //удаление опросов
            {
                await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
            }
        }

        public static async Task CreateAndFillTable(Users user, string mes)
        {
            using SqlConnection connection = new(ConStringDbLexicon);
            await connection.OpenAsync();
            SqlCommand createTableCommand = new($"IF OBJECT_ID('{user.Id}', 'U') IS NULL create table \"{user.Id}\" (Word varchar(200) primary key, Count int default 0)", connection);
            await createTableCommand.ExecuteNonQueryAsync();

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
                try
                {
                    SqlCommand insertCommand = new($"insert into dbo.\"{user.Id}\" (Word, Count) values (@Word, 1)", connection);
                    insertCommand.Parameters.AddWithValue("@Word", word);
                    await insertCommand.ExecuteNonQueryAsync();
                }
                catch
                {
                    SqlCommand updateCommand = new($"update dbo.\"{user.Id}\" set [Count] = [Count] + 1 where [Word] = @Word", connection);
                    updateCommand.Parameters.AddWithValue("@Word", word);
                    await updateCommand.ExecuteNonQueryAsync();
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
