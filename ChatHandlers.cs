﻿using System.Collections.Generic;
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
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace XpAndRepBot
{
    public class ChatHandlers
    {
        public static async Task LvlUp(ITelegramBotClient botClient, Update update, InfoContext db, Users user, CancellationToken cancellationToken)
        {
            while (user.CurXp >= Сalculation.GenerateXpForLevel(user.Lvl + 1))
            {
                user.Lvl++;
                user.CurXp -= Сalculation.GenerateXpForLevel(user.Lvl);
                if (user.Lvl > 4) await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: $"{user.Name} получает {user.Lvl} lvl", cancellationToken: cancellationToken);
            }
            db.SaveChanges();
        }

        public static async Task RequestChatGpt(ITelegramBotClient botClient, Update update, string mes, CancellationToken cancellationToken)
        {
            if (mes.Contains("@XpAndRepBot") || (update.Message?.Chat?.Id == Mid && update.Message.ReplyToMessage == null) || (update.Message?.Chat?.Id == Iid && update.Message.ReplyToMessage == null))
            {
                Program.Context.Add(update.Message.MessageId, new MessageEntry[100]);
                Program.ListBranches.Add(new List<int>() { update.Message.MessageId });
            }
            mes = mes.Replace("@XpAndRepBot", "");
            if (mes.Any(x => char.IsLetter(x)) || mes.Any(x => char.IsNumber(x)) || mes.Any(x => char.IsPunctuation(x)))
            {
                var id = update.Message.MessageId;
                if (update.Message.ReplyToMessage != null)
                {
                    Regex regex = new(@"\b\d+\b$");
                    var match = regex.Match(update.Message.ReplyToMessage.Text);
                    var number = 0;
                    if (match.Success)
                    {
                        number = int.Parse(match.Value);
                    }
                    var targetList = Program.ListBranches.Find(list => list.Contains(number));
                    targetList?.Add(update.Message.MessageId);
                    if (targetList != null && targetList.Count > 0)
                    {
                        id = targetList[0];
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
                            await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: await ResponseHandlers.RequestChatGpt(id, subArray), cancellationToken: cancellationToken);
                        }
                        catch
                        {
                            await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: await ResponseHandlers.RequestChatGpt(id, subArray), cancellationToken: cancellationToken);
                        }
                    }
                    catch { await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: "Произошла ошибка. Попробуйте повторить запрос. Чтобы начать новую ветку, упомяните бота. Чтобы продолжить ветку, ответьте на сообщение от бота, где есть число в конце", cancellationToken: cancellationToken); }
                }
            }
        }

        public static async Task NewMember(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            using var db = new InfoContext();
            var newMembers = update.Message.NewChatMembers;
            foreach (var member in newMembers)
            {
                var userMute = db.TableUsers.FirstOrDefault(x => x.Id == member.Id);
                if (!member.IsBot && DateTime.Now >= userMute.DateMute)
                {
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
                        if (array[i] != randomNumber) buttons[0][i] = InlineKeyboardButton.WithCallbackData(array[i].ToString(), $"{array[i]}n{member.Id}");
                        else buttons[0][i] = InlineKeyboardButton.WithCallbackData(array[i].ToString(), $"{array[i]}y{member.Id}");
                    }
                    InlineKeyboardMarkup inlineKeyboard = new(buttons);
                    await botClient.RestrictChatMemberAsync(chatId: update.Message.Chat.Id, member.Id, new ChatPermissions
                    {
                        CanSendMessages = false,
                        CanSendMediaMessages = false,
                    }, cancellationToken: cancellationToken);
                    try
                    {
                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard, replyToMessageId: update.Message.MessageId, text: $"Нажми на {randomNumber}, чтобы можно было писать. Бан за неверный ответ (press {randomNumber} to be able to write. Ban for a wrong answer).", cancellationToken: cancellationToken);
                    }
                    catch
                    {
                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard, text: $"Нажми на {randomNumber}, чтобы можно было писать. Бан за неверный ответ (press {randomNumber} to be able to write. Ban for a wrong answer).", cancellationToken: cancellationToken);
                    }
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
                var flag = update.Message.MessageId % 10 == 0;
                if (update.Message.ReplyToMessage?.From.Id == 777000 && ForbiddenWords.Any(s => (bool)(update.Message.Text?.ToLower().Contains(s)))) //удаление запрещённых слов
                {
                    await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
                }
                else if (update?.Message?.Animation != null && user.Lvl < 10) //удаление гифок
                {
                    if (flag) await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: "Гифки с 10 лвла.\n/m - посмотреть свой лвл", cancellationToken: cancellationToken);
                    await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
                }
                else if (update?.Message?.Sticker != null && user.Lvl < 15) //удаление стикеров
                {
                    var sticker = update.Message.Sticker;
                    var set = await botClient.GetStickerSetAsync(sticker.SetName, cancellationToken: cancellationToken);
                    if (set.Name != "UnoWarStickers" && set.Name != "UsedWorm_by_fStikBot")
                    {
                        if (flag) await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: "Стикеры с 15 лвла, кроме стикеров чата и Уно.\n/m - посмотреть свой лвл", cancellationToken: cancellationToken);
                        await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
                    }
                }
                else if (update?.Message?.Poll != null && user.Lvl < 20) //удаление опросов
                {
                    if (flag) await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: "Опросы с 20 лвла.\n/m - посмотреть свой лвл", cancellationToken: cancellationToken);
                    await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
                }
            }
            catch { }
        }

        public static async Task AddWordsInLexicon(Users user, string mes)
        {
            using SqlConnection connection = new(ConStringDbLexicon);
            await connection.OpenAsync();
            var listWords = mes.Split(new string[] { " ", "\r\n", "\n" }, StringSplitOptions.None);
            List<string> validWords = new();
            for (var i = 0; i < listWords.Length; i++)
            {
                var cleanedWord = Regex.Replace(listWords[i], @"[^\w\d\s]", "");
                if (string.IsNullOrWhiteSpace(cleanedWord)) continue;
                cleanedWord = cleanedWord.ToLower();
                validWords.Add(cleanedWord);
            }
            foreach (var word in validWords)
            {
                SqlCommand updateCommand = new($"update dbo.TableUsersLexicons set [Count] = [Count] + 1 where [Word] = '{word}' and [UserID] = {user.Id}", connection);
                var rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    SqlCommand insertCommand = new($"insert into dbo.TableUsersLexicons (UserID, Word, Count) values ({user.Id}, '{word}', 1)", connection);
                    await insertCommand.ExecuteNonQueryAsync();
                }
            }
        }

        public static void Mention(List<Users> users, string name, long chatId, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            var res = $"{name} призывает ";
            using var db = new InfoContext();
            var count = users.Count - 1;
            for (var i = 0; i < count; i++)
            {
                res += $"<a href=\"tg://user?id={users[i].Id}\">{users[i].Name}</a> ";
                if ((i + 1) % 5 == 0)
                {
                    botClient.SendTextMessageAsync(chatId: chatId, text: res, parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                    res = $"{name} призывает ";
                }
            }
            res += $"<a href=\"tg://user?id={users[count].Id}\">{users[count].Name}</a>";
            botClient.SendTextMessageAsync(chatId: chatId, text: res, parseMode: ParseMode.Html, cancellationToken: cancellationToken);
        }

        public static async Task<string> Nfc(Users user, DbContext db, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            user.Nfc = false;
            var ts = DateTime.Now - user.StartNfc;
            if (ts.Ticks > user.BestTime) user.BestTime = ts.Ticks;
            db.SaveChanges();
            var t = string.Format("{0} d, {1} h, {2} m.", ts.Days, ts.Hours, ts.Minutes);
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
            catch { }
            return $"{user.Name} нарушил условия no fuck challenge. Время: {t}";
        }
    }
}
