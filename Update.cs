using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static XpAndRepBot.Consts;

namespace XpAndRepBot
{
    internal class UpdateHandler : IUpdateHandler
    {
        private static readonly Dictionary<string, ICommand> Commands = new()
        {
            { "/m", new MeCommand() },
            { "/tl", new TopLvlCommand() },
            { "/tr", new TopReputationCommand() },
            { "/r", new RulesCommand() },
            { "/h", new HelpCommand() },
            { "/help", new HelpCommand() },
            { "/mr", new MessagesReputationCommand() },
            { "/g", new GamesCommand() },
            { "/tw", new TopWordsCommand() },
            { "/porno", new RoflCommand() },
            { "/hc", new HelpChatGptCommand() },
            { "/im", new ImageDalleCommand() },
            { "/l", new TopLexiconCommand() },
            { "/tge", new TgEmpressCommand() },
            { "/role", new RoleCommand() },
            { "/roles", new RolesCommand() },
            { "/nfc", new NoFuckChallengeCommand() },
            { "/unr", new UnRoleCommand() },
            { "/b", new BalabobaCommand() },
            { "/w", new WordCommand() },
            { "mariage", new MariageCommand() },
            { "mariages", new MariagesCommand() },
            { "status", new StatusCommand() },
            { "divorce", new DivorceCommand() },
            { "/ha", new HelpAdminCommand() },
            { "/ban", new BanCommand() },
            { "/unb", new UnBanCommand() },
            { "/warn", new WarnCommand() },
            { "/unwarn", new UnwarnCommand() },
            { "/unw", new UnwarnCommand() },
            { "/mute", new MuteCommand() },
            { "/rew", new HelpRewardsCommand() }
        };

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Debug.WriteLine(JsonSerializer.Serialize(update));

            //handle callback
            if (update.Type == UpdateType.CallbackQuery)
            {
                await HandleCallbackQuery(botClient, update.CallbackQuery, cancellationToken);
            }
            else
            {
                await HandleNonCallbackUpdate(botClient, update, cancellationToken);
            }
        }

        private static async Task HandleNonCallbackUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await using var db = new InfoContext();
            if (update.Message?.Chat.Id is { } and (IgruhaChatId or MyChatId or Mid or Iid))
            {
                if (update.Message.From != null)
                {
                    var user = await GetOrCreateUser(db, update);
                    await UpdateUserLastMessageTime(db, user, update, cancellationToken);
                    await HandleNewChatMembers(botClient, update, cancellationToken);
                    await HandleUserWarns(botClient, db, user, update, cancellationToken);
                    await DeleteUnwantedMessages(botClient, update, user, cancellationToken);
                    await ProcessMessageContent(botClient, db, update, user, cancellationToken);
                }
            }
        }

        private static Task<Users> GetOrCreateUser(InfoContext db, Update update)
        {
            if (update.Message is not { From: not null }) return Task.FromResult<Users>(null);
            var idUser = update.Message.From.Id;
            var user = db.TableUsers.FirstOrDefault(x => x.Id == idUser);

            //add new user
            if (user == null)
            {
                var name = update.Message.From.FirstName;
                if (!string.IsNullOrEmpty(update.Message.From.LastName))
                {
                    name += " " + update.Message.From.LastName;
                }

                user = new Users(idUser, name, 0, 0, 0);
                db.TableUsers.Add(user);
            }

            user.Username = update.Message.From.Username ?? user.Username;
            if (user.Name != "0") return Task.FromResult(user);
            user.Name = update.Message.From.FirstName;
            if (!string.IsNullOrEmpty(update.Message.From.LastName))
            {
                user.Name += " " + update.Message.From.LastName;
            }

            return Task.FromResult(user);
        }

        private static async Task UpdateUserLastMessageTime(DbContext db, Users user, Update update, CancellationToken cancellationToken)
        {
            if (update.Message != null)
            {
                user.TimeLastMes = update.Message.Date.AddHours(3);
            }
            await db.SaveChangesAsync(cancellationToken);
        }

        private static async Task HandleNewChatMembers(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //captcha 
            if (update.Message?.NewChatMembers != null)
            {
                await ChatHandlers.NewMember(botClient, update, cancellationToken);
            }
        }

        private static async Task HandleUserWarns(ITelegramBotClient botClient, InfoContext db, Users user, Update update, CancellationToken cancellationToken)
        {
            //remove warn
            if (user.Warns > 0)
            {
                var difference = DateTime.Now - user.LastTime;
                if (difference.TotalDays >= 30)
                {
                    user.Warns--;
                    user.LastTime = DateTime.Now;
                    await db.SaveChangesAsync(cancellationToken);
                    await NotifyUserAboutWarnRemoval(botClient, user, update, cancellationToken);
                }
            }
        }

        private static async Task NotifyUserAboutWarnRemoval(ITelegramBotClient botClient, Users user, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message != null)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        replyToMessageId: update.Message.MessageId,
                        text: $"С {user.Name} снимается 1 варн({user.Warns}/3)",
                        cancellationToken: cancellationToken);
                }
            }
            catch
            {
                if (update.Message != null)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: $"С {user.Name} снимается 1 варн({user.Warns}/3)",
                        cancellationToken: cancellationToken);
                }
            }
        }

        private static async Task DeleteUnwantedMessages(ITelegramBotClient botClient, Update update, Users user, CancellationToken cancellationToken)
        {
            //delete gifs, stickers, pools and forbidden words 
            await ChatHandlers.Delete(botClient, update, user, cancellationToken);
        }

        private static async Task ProcessMessageContent(ITelegramBotClient botClient, InfoContext db, Update update,Users user, CancellationToken cancellationToken)
        {
            if (ShouldProcessMessage(update))
            {
                var mes = GetMessageText(update);

                //experience
                user.CurXp += mes.Length;

                await HandleReputationUp(botClient, update, db, mes, cancellationToken);
                await ChatHandlers.AddWordsInLexicon(user, mes);
                await HandleLevelUp(botClient, update, db, user, cancellationToken);
                await HandleCommands(botClient, update, cancellationToken);
                await HandleMentions(db, user, update, botClient, cancellationToken);
                await HandleNfc(botClient, update, user, cancellationToken);
                await HandleRepeatedMessages(botClient, update, user, cancellationToken);
                await HandleChatGptRequest(botClient, update, mes, cancellationToken);

                await db.SaveChangesAsync(cancellationToken);
            }
        }

        private static bool ShouldProcessMessage(Update update)
        {
            return update.Message?.Text != null || update.Message?.Caption != null;
        }

        private static string GetMessageText(Update update)
        {
            if (update.Message != null) return update.Message.Caption ?? update.Message.Text;
            return null;
        }

        private static async Task HandleReputationUp(ITelegramBotClient botClient, Update update, InfoContext db, string messageText, CancellationToken cancellationToken)
        {
            if (update.Message is { ReplyToMessage.From: not null } &&
                (!update.Message.ReplyToMessage.From.IsBot || update.Message.ReplyToMessage.From.Id == BotId))
            {
                await ChatHandlers.ReputationUp(botClient, update, db, messageText, cancellationToken);
            }
        }

        private static async Task HandleChatGptRequest(ITelegramBotClient botClient, Update update, string messageText, CancellationToken cancellationToken)
        {
            if (update.Message != null)
            {
                var chatId = update.Message.Chat.Id;
                if (!Commands.Keys.Any(messageText.StartsWith) && (update.Message?.ReplyToMessage is { From.Id: BotId } ||
                     messageText.Contains("@XpAndRepBot") || chatId == Mid || chatId == Iid))
                {
                    await ChatHandlers.RequestChatGpt(botClient, update, messageText, cancellationToken);
                }
            }
        }

        private static async Task HandleLevelUp(ITelegramBotClient botClient, Update update, InfoContext db, Users user, CancellationToken cancellationToken)
        {
            if (update.Message?.Chat.Id != Mid)
            {
                await ChatHandlers.LvlUp(botClient, update, db, user, cancellationToken);
            }
        }

        private static async Task HandleCommands(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var messageText = GetMessageText(update);
            var match = Regex.Match(messageText, @"^.*?([\w/]+)");
            if (Commands.ContainsKey(match.Value.ToLower()))
            {
                var command = Commands[match.Value.ToLower()];
                await command.ExecuteAsync(botClient, update, cancellationToken);
            }
        }

        private static Task HandleMentions(InfoContext db, Users user, Update update, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            var messageText = GetMessageText(update);
            if (messageText.Length >= 100) return Task.CompletedTask;
            var mentionUsers = db.TableUsers.Where(x =>
                x.Roles.Equals(messageText.Substring(1)) || x.Roles.StartsWith(messageText.Substring(1) + ",") ||
                x.Roles.Contains(", " + messageText.Substring(1) + ",") ||
                x.Roles.EndsWith(", " + messageText.Substring(1))).ToList();
            if (messageText[0] != '@' || mentionUsers.Count <= 0) return Task.CompletedTask;
            if (update.Message != null)
            {
                ChatHandlers.Mention(mentionUsers, user.Name, update.Message.Chat.Id, botClient, cancellationToken);
            }

            return Task.CompletedTask;
        }

        private static async Task HandleNfc(ITelegramBotClient botClient, Update update, Users user, CancellationToken cancellationToken)
        {
            if (user.Nfc == true)
            {
                const string filePath = "bw.txt";
                var words = await ReadFileLinesToLower(filePath);
                var messageWords = GetMessageText(update)
                    .Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
                var containsWord = messageWords.Any(w => words.Contains(w.ToLower()));
                if (containsWord)
                {
                    if (update.Message != null)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text: await ChatHandlers.Nfc(user, botClient, cancellationToken),
                            cancellationToken: cancellationToken);
                        
                    }
                }
            }
        }

        private static async Task<List<string>> ReadFileLinesToLower(string filePath)
        {
            var words = new List<string>();
            using StreamReader reader = new(filePath);
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (line != null) words.Add(line.ToLower());
            }

            return words;
        }

        private static async Task HandleRepeatedMessages(ITelegramBotClient botClient, Update update, Users user, CancellationToken cancellationToken)
        {
            var messageText = GetMessageText(update);
            if (user.LastMessage == messageText)
            {
                user.CountRepeatMessage++;
            }
            else
            {
                user.LastMessage = messageText;
                user.CountRepeatMessage = 1;
            }

            if (user.CountRepeatMessage > 3)
            {
                if (update.Message != null)
                {
                    await botClient.DeleteMessageAsync(
                        update.Message.Chat.Id, 
                        update.Message.MessageId,
                        cancellationToken);
                    
                }
            }
        }

        private static async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Message == null) return;

            var option = callbackQuery.Data;
            try
            {
                switch (option)
                {
                    case "backmw":
                    case "nextmw":
                        await HandleMeWordsCallbackQuery(botClient, callbackQuery, cancellationToken);
                        break;
                    case "backtw":
                    case "nexttw":
                        await HandleTopWordsCallbackQuery(botClient, callbackQuery, cancellationToken);
                        break;
                    case "backtl":
                    case "nexttl":
                        await HandleTopLvlCallbackQuery(botClient, callbackQuery, cancellationToken);
                        break;
                    case "backtr":
                    case "nexttr":
                        await HandleTopRepCallbackQuery(botClient, callbackQuery, cancellationToken);
                        break;
                    case "backl":
                    case "nextl":
                        await HandleTopLexiconCallbackQuery(botClient, callbackQuery, cancellationToken);
                        break;
                    case "backr":
                    case "nextr":
                        await HandleRolesCallbackQuery(botClient, callbackQuery, cancellationToken);
                        break;
                    default:
                        await HandleDefaultCallbackQuery(botClient, callbackQuery, cancellationToken);
                        break;
                }
            }
            catch
            {
                // ignored
            }
        }

        private static async Task HandleMeWordsCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var match = GetMatchFromMessage(callbackQuery.Message, @"^\d+");
            var number = match is { Success: true } ? int.Parse(match.Value) : 0;
            var isBackward = callbackQuery.Data == "backmw";
            var offset = isBackward ? -10 : 10;
            if (isBackward && number <= 10) return;

            var newText = await ResponseHandlers.MeWords(callbackQuery, offset);
            var inlineKeyboard = CreateInlineKeyboard("backmw", "nextmw");
            await EditMessageText(botClient, callbackQuery, newText, inlineKeyboard, cancellationToken);
        }

        private static async Task HandleTopWordsCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var match = GetMatchFromMessage(callbackQuery.Message, @"\n\d+", RegexOptions.Multiline);
            var number = match != null ? int.Parse(match.Value) : 0;
            var isBackward = callbackQuery.Data == "backtw";
            var offset = isBackward ? -51 : 49;
            if (isBackward && number <= 50) return;

            var newText = await ResponseHandlers.TopWords(number + offset);
            var inlineKeyboard = CreateInlineKeyboard("backtw", "nexttw");
            await EditMessageText(botClient, callbackQuery, newText, inlineKeyboard, cancellationToken);
        }

        private static async Task HandleTopRepCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var match = GetMatchFromMessage(callbackQuery.Message, @"\n\d+", RegexOptions.Multiline);
            var number = match != null ? int.Parse(match.Value) : 0;
            var isBackward = callbackQuery.Data == "backtr";
            var offset = isBackward ? -51 : 49;
            if (isBackward && number <= 50) return;

            var newText = ResponseHandlers.TopRep(number + offset);
            var inlineKeyboard = CreateInlineKeyboard("backtr", "nexttr");
            await EditMessageText(botClient, callbackQuery, newText, inlineKeyboard, cancellationToken);
        }

        private static async Task HandleTopLvlCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var match = GetMatchFromMessage(callbackQuery.Message, @"\n\d+", RegexOptions.Multiline);
            var number = match != null ? int.Parse(match.Value) : 0;
            var isBackward = callbackQuery.Data == "backtl";
            var offset = isBackward ? -51 : 49;
            if (isBackward && number <= 50) return;

            var newText = ResponseHandlers.TopLvl(number + offset);
            var inlineKeyboard = CreateInlineKeyboard("backtl", "nexttl");
            await EditMessageText(botClient, callbackQuery, newText, inlineKeyboard, cancellationToken);
        }

        private static async Task HandleTopLexiconCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var match = GetMatchFromMessage(callbackQuery.Message, @"\n\d+", RegexOptions.Multiline);
            var number = match != null ? int.Parse(match.Value) : 0;
            var isBackward = callbackQuery.Data == "backl";
            var offset = isBackward ? -51 : 49;
            if (isBackward && number <= 50) return;

            var newText = await ResponseHandlers.TopLexicon(number + offset);
            var inlineKeyboard = CreateInlineKeyboard("backl", "nextl");
            await EditMessageText(botClient, callbackQuery, newText, inlineKeyboard, cancellationToken);
        }

        private static async Task HandleRolesCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery,
            CancellationToken cancellationToken)
        {
            var match = GetMatchFromMessage(callbackQuery.Message, @"\d+", RegexOptions.Multiline);
            var number = match != null ? int.Parse(match.Value) : 0;
            var isBackward = callbackQuery.Data == "backr";
            var offset = isBackward ? -21 : 19;
            if (isBackward && number <= 20) return;

            var newText = ResponseHandlers.GetRoles(number + offset);
            var inlineKeyboard = CreateInlineKeyboard("backr", "nextr");
            await EditMessageText(botClient, callbackQuery, newText, inlineKeyboard, cancellationToken);
        }

        private static async Task HandleDefaultCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Message != null)
            {
                var chatId = callbackQuery.Message.Chat.Id;
                var messageId = callbackQuery.Message.MessageId;
                if (callbackQuery.Message.ReplyToMessage?.From != null)
                {
                    var userId = callbackQuery.Message.ReplyToMessage.From.Id;
                    var newText = callbackQuery.Message.Text;
                    var inlineKeyboard = callbackQuery.Message.ReplyMarkup;
                    if (callbackQuery.Data != null && callbackQuery.Data[0] == 'm')
                    {
                        inlineKeyboard = await ResponseHandlers.AcceptMariage(callbackQuery.Data, callbackQuery,
                            botClient, chatId, userId, cancellationToken);
                    }
                    else
                    {
                        var flag = await ResponseHandlers.GetChatPermissions(callbackQuery.Data, callbackQuery,
                            botClient, chatId, cancellationToken);
                        if (flag)
                        {
                            await botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId, cancellationToken);
                            return;
                        }
                    }

                    await EditMessageText(botClient, callbackQuery, newText, inlineKeyboard, cancellationToken);
                }
            }
        }

        private static Match GetMatchFromMessage(Message message, string pattern, RegexOptions options = RegexOptions.None)
        {
            return message.Text != null ? Regex.Match(message.Text, pattern, options) : null;
        }

        private static InlineKeyboardMarkup CreateInlineKeyboard(string backOption, string nextOption)
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", backOption),
                    InlineKeyboardButton.WithCallbackData("Вперёд", nextOption),
                }
            });
        }

        private static async Task EditMessageText(ITelegramBotClient botClient, CallbackQuery callbackQuery,
            string newText, InlineKeyboardMarkup inlineKeyboard, CancellationToken cancellationToken)
        {
            if (callbackQuery.Message != null)
            {
                var messageId = callbackQuery.Message.MessageId;
                var chatId = callbackQuery.Message.Chat.Id;
                await botClient.EditMessageTextAsync(
                    chatId: chatId, 
                    replyMarkup: inlineKeyboard, 
                    messageId: messageId,
                    text: newText, 
                    parseMode: ParseMode.Html, 
                    cancellationToken: cancellationToken);
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.Error.WriteLine(exception);
            return Task.CompletedTask;
        }
    }
}