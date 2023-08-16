using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static XpAndRepBot.Consts;
using Mirror.ChatGpt.Models.ChatGpt;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace XpAndRepBot
{
    public static class ChatHandlers
    {
        public static async Task LvlUp(ITelegramBotClient botClient, Update update, InfoContext db, Users user,
            CancellationToken cancellationToken)
        {
            while (user.CurXp >= Сalculation.GenerateXpForLevel(user.Lvl + 1))
            {
                user.Lvl++;
                user.CurXp -= Сalculation.GenerateXpForLevel(user.Lvl);
                if (user.Lvl <= 4) continue;
                if (update.Message == null) continue;
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $"{user.Name} получает {user.Lvl} lvl",
                    cancellationToken: cancellationToken);
                if (user.Lvl != 10) continue;
                if (update.Message.From != null)
                    await botClient.RestrictChatMemberAsync(update.Message.Chat.Id, update.Message.From.Id,
                        new ChatPermissions
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
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        public static async Task RequestChatGpt(ITelegramBotClient botClient, Update update, string mes,
            CancellationToken cancellationToken)
        {
            if (mes.Contains("@XpAndRepBot") || (update.Message?.Chat.Id == Mid &&
                                                 update.Message.ReplyToMessage == null)
                                             || (update.Message?.Chat.Id == Iid &&
                                                 update.Message.ReplyToMessage == null))
            {
                if (update.Message != null)
                {
                    Program.Context.Add(update.Message.MessageId, new MessageEntry[100]);
                    Program.ListBranches.Add(new List<int>() { update.Message.MessageId });
                }
            }

            mes = mes.Replace("@XpAndRepBot", "");
            if (mes.Any(char.IsLetter) || mes.Any(char.IsNumber) || mes.Any(char.IsPunctuation))
            {
                if (update.Message != null)
                {
                    var id = update.Message.MessageId;
                    if (update.Message.ReplyToMessage != null)
                    {
                        Regex regex = new(@"\b\d+\b$");
                        if (update.Message.ReplyToMessage.Text != null)
                        {
                            var match = regex.Match(update.Message.ReplyToMessage.Text);
                            var number = 0;
                            if (match.Success)
                            {
                                number = int.Parse(match.Value);
                            }

                            var targetList = Program.ListBranches.Find(list => list.Contains(number));
                            targetList?.Add(update.Message.MessageId);
                            if (targetList is { Count: > 0 })
                            {
                                id = targetList[0];
                            }
                        }
                    }

                    var subArray = new MessageEntry[1];
                    if (Program.Context.TryGetValue(id, out var array))
                    {
                        var index = Array.IndexOf(array, null);
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
                                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                    replyToMessageId: update.Message.MessageId,
                                    text: await ResponseHandlers.RequestChatGpt(id, subArray),
                                    cancellationToken: cancellationToken);
                            }
                            catch
                            {
                                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                    text: await ResponseHandlers.RequestChatGpt(id, subArray),
                                    cancellationToken: cancellationToken);
                            }
                        }
                        catch
                        {
                            await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                text:
                                "Произошла ошибка. Попробуйте повторить запрос. Чтобы начать новую ветку, упомяните бота. " +
                                "Чтобы продолжить ветку, ответьте на сообщение от бота, где есть число в конце",
                                cancellationToken: cancellationToken);
                        }
                    }
                }
            }
        }

        public static async Task NewMember(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            await using var db = new InfoContext();
            var newMembers = update.Message?.NewChatMembers;
            if (newMembers != null)
                foreach (var member in newMembers)
                {
                    var userMute = db.TableUsers.FirstOrDefault(x => x.Id == member.Id);
                    if (userMute == null || member.IsBot || DateTime.Now < userMute.DateMute) continue;
                    userMute.CheckEnter = false;
                    await db.SaveChangesAsync(cancellationToken);

                    Random random = new();
                    var array = new int[8];
                    for (var i = 0; i < array.Length; i++)
                    {
                        array[i] = i;
                    }

                    for (var i = 0; i < array.Length; i++)
                    {
                        var randomIndex = random.Next(i, array.Length);
                        (array[randomIndex], array[i]) = (array[i], array[randomIndex]);
                    }

                    var randomNumber = random.Next(0, 8);
                    var buttons = new InlineKeyboardButton[1][];
                    buttons[0] = new InlineKeyboardButton[array.Length];
                    for (var i = 0; i < array.Length; i++)
                    {
                        if (array[i] != randomNumber)
                            buttons[0][i] = InlineKeyboardButton.WithCallbackData(array[i].ToString(),
                                $"{array[i]}n{member.Id}");
                        else
                            buttons[0][i] = InlineKeyboardButton.WithCallbackData(array[i].ToString(),
                                $"{array[i]}y{member.Id}");
                    }

                    InlineKeyboardMarkup inlineKeyboard = new(buttons);
                    await botClient.RestrictChatMemberAsync(chatId: update.Message.Chat.Id, member.Id,
                        new ChatPermissions
                        {
                            CanSendMessages = false,
                            CanSendMediaMessages = false,
                        }, cancellationToken: cancellationToken);
                    try
                    {
                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                            replyMarkup: inlineKeyboard, replyToMessageId: update.Message.MessageId,
                            text:
                            $"Нажми на {randomNumber}, чтобы можно было писать. Бан за неверный ответ (press {randomNumber} to be able to write. Ban for a wrong answer).",
                            cancellationToken: cancellationToken);
                    }
                    catch
                    {
                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                            replyMarkup: inlineKeyboard,
                            text:
                            $"Нажми на {randomNumber}, чтобы можно было писать. Бан за неверный ответ (press {randomNumber} to be able to write. Ban for a wrong answer).",
                            cancellationToken: cancellationToken);
                    }
                }
        }

        public static async Task ReputationUp(ITelegramBotClient botClient, Update update, InfoContext db, string mes,
            CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message != null)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: ResponseHandlers.RepUp(update, db, mes),
                        cancellationToken: cancellationToken);
                }
            }
            catch
            {
                // ignored
            }
        }

        private static bool ContainsMessageId(InfoContext db, long targetMessageId)
        {
            return db.TableMessageIdsForDelete.Any(row => row.MessageIds.Contains(targetMessageId.ToString()));
        }

        public static async Task Delete(ITelegramBotClient botClient, Update update, Users user,
            CancellationToken cancellationToken)
        {
            try
            {
                var message = update.Message;
                if (message == null) return;

                await using var db = new InfoContext();
                if (message.From is { Id: 777000 })
                {
                    db.TableMessageIdsForDelete.Add(new MessageIdsForDelete(message.MessageId,
                        message.MessageId.ToString()));
                    await db.SaveChangesAsync(cancellationToken);
                }

                var flagForDelete = message.ReplyToMessage != null &&
                                    ContainsMessageId(db, message.ReplyToMessage.MessageId);
                if (flagForDelete)
                {
                    var rowToUpdate = db.TableMessageIdsForDelete.FirstOrDefault(row =>
                        row.MessageIds.Contains(message.ReplyToMessage.MessageId.ToString()));
                    if (rowToUpdate != null)
                    {
                        rowToUpdate.MessageIds += " " + message.MessageId;
                        await db.SaveChangesAsync(cancellationToken);
                    }
                }

                var flag = message.MessageId % 10 == 0;
                var flagForbiddenWords = flagForDelete && ForbiddenWords.Any(s =>
                    !string.IsNullOrEmpty(message.Text) && message.Text.ToLower().Contains(s));

                if (flagForbiddenWords)
                {
                    await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancellationToken);
                    return;
                }

                if (message.Animation != null && (user.Lvl < 10 || flagForDelete) ||
                    message.Sticker != null && (user.Lvl < 15 || flagForDelete) ||
                    message.Poll != null && (user.Lvl < 20 || flagForDelete))
                {
                    if (!flagForDelete && flag)
                    {
                        var text = message.Animation != null
                            ?
                            "Гифки с 10 лвла и в ответ кидать нельзя.\n/m - посмотреть свой лвл"
                            :
                            message.Sticker != null
                                ? "Стикеры с 15 лвла и в ответ кидать нельзя.\n/m - посмотреть свой лвл"
                                :
                                message.Poll != null
                                    ? "Опросы с 20 лвла.\n/m - посмотреть свой лвл"
                                    : null;
                        
                        if (!string.IsNullOrEmpty(text))
                        {
                            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: text,
                                cancellationToken: cancellationToken);
                        }
                    }

                    await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancellationToken);
                    return;
                }

                if (flagForDelete && (message.Video != null || message.Voice != null || message.VideoNote != null ||
                                      message.Document != null || message.Audio != null))
                {
                    await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancellationToken);
                }
            }
            catch
            {
                // ignored
            }
        }

        public static async Task AddWordsInLexicon(Users user, string mes)
        {
            await using SqlConnection connection = new(ConnectionString);
            await connection.OpenAsync();
            var listWords = mes.Split(new[] { " ", "\r\n", "\n" }, StringSplitOptions.None);
            var validWords =
                (from t in listWords
                    select Regex.Replace(t, @"[^\w\d\s]", "")
                    into cleanedWord
                    where !string.IsNullOrWhiteSpace(cleanedWord)
                    select cleanedWord.ToLower()).ToList();
            foreach (var word in validWords)
            {
                var cutWord = word;
                if (word.Length > 100)
                {
                    cutWord = word[..100];
                }

                SqlCommand updateCommand =
                    new(
                        $"update dbo.TableUsersLexicons set [Count] = [Count] + 1 where [Word] = '{cutWord}' and [UserID] = {user.Id}",
                        connection);
                var rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                if (rowsAffected != 0) continue;
                SqlCommand insertCommand =
                    new($"insert into dbo.TableUsersLexicons (UserID, Word, Count) values ({user.Id}, '{cutWord}', 1)",
                        connection);
                await insertCommand.ExecuteNonQueryAsync();
            }
        }

        public static void Mention(List<Users> users, string name, long chatId, ITelegramBotClient botClient,
            CancellationToken cancellationToken)
        {
            var res = $"{name} призывает ";
            using var db = new InfoContext();
            var count = users.Count - 1;
            for (var i = 0; i < count; i++)
            {
                res += $"<a href=\"tg://user?id={users[i].Id}\">{users[i].Name}</a> ";
                if ((i + 1) % 5 != 0) continue;
                botClient.SendTextMessageAsync(chatId: chatId, text: res, parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
                res = $"{name} призывает ";
            }

            res += $"<a href=\"tg://user?id={users[count].Id}\">{users[count].Name}</a>";
            botClient.SendTextMessageAsync(chatId: chatId, text: res, parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        public static async Task<string> Nfc(Users user, ITelegramBotClient botClient,
            CancellationToken cancellationToken)
        {
            await using var db = new InfoContext();
            user.Nfc = false;
            var ts = DateTime.Now - user.StartNfc;
            if (ts.Ticks > user.BestTime) user.BestTime = ts.Ticks;
            await db.SaveChangesAsync(cancellationToken);
            var t = $"{ts.Days} d, {ts.Hours} h, {ts.Minutes} m.";
            var muteDate = DateTime.Now.AddMinutes(15);
            try
            {
                await botClient.RestrictChatMemberAsync(
                    chatId: IgruhaChatId,
                    userId: user.Id,
                    new ChatPermissions
                    {
                        CanSendMessages = false,
                        CanSendMediaMessages = false
                    },
                    untilDate: muteDate,
                    cancellationToken: cancellationToken
                );
                await botClient.SendTextMessageAsync(
                    chatId: IgruhaChatId,
                    text: $"{user.Name} получил мут на 15 минут до {muteDate:dd.MM.yyyy HH:mm}",
                    cancellationToken: cancellationToken
                );
            }
            catch
            {
                // ignored
            }

            return $"{user.Name} нарушил условия no fuck challenge. Время: {t}";
        }
    }
}