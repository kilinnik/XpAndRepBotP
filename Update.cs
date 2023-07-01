using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            Debug.WriteLine(JsonSerializer.Serialize(update));

            //handle callback
            if (update.Type == UpdateType.CallbackQuery)
            {
                await HandleCallbackQuery(botClient, update.CallbackQuery, cancellationToken);
            }
            else
            {
                await using var db = new InfoContext();
                if (update.Message?.Chat.Id is { } id and (IgruhaChatId or MyChatId or Mid or Iid))
                {
                    if (update.Message.From != null)
                    {
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
                        if (user.Name == "0")
                        {
                            user.Name = update.Message.From.FirstName;
                            if (!string.IsNullOrEmpty(update.Message.From.LastName))
                            {
                                user.Name += " " + update.Message.From.LastName;
                            }
                        }

                        user.TimeLastMes = update.Message.Date.AddHours(3);
                        await db.SaveChangesAsync(cancellationToken);

                        //captcha 
                        if (update.Message?.NewChatMembers != null)
                        {
                            await ChatHandlers.NewMember(botClient, update, cancellationToken);
                        }

                        //remove warn
                        if (user.Warns > 0)
                        {
                            var difference = DateTime.Now - user.LastTime;
                            if (difference.TotalDays >= 30)
                            {
                                user.Warns--;
                                user.LastTime = DateTime.Now;
                                await db.SaveChangesAsync(cancellationToken);
                                try
                                {
                                    if (update.Message != null)
                                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                            replyToMessageId: update.Message.MessageId,
                                            text: $"С {user.Name} снимается 1 варн({user.Warns}/3)",
                                            cancellationToken: cancellationToken);
                                }
                                catch
                                {
                                    if (update.Message != null)
                                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                            text: $"С {user.Name} снимается 1 варн({user.Warns}/3)",
                                            cancellationToken: cancellationToken);
                                }
                            }
                        }

                        //delete gifs, stickers, pools and forbidden words 
                        await ChatHandlers.Delete(botClient, update, user, cancellationToken);

                        if (update.Message?.Text != null || update.Message?.Caption != null)
                        {
                            var mes = update.Message.Caption ?? update.Message.Text;

                            //experience
                            user.CurXp += mes.Length;

                            //reputation up
                            if (update.Message.ReplyToMessage is { From: not null } &&
                                (!update.Message.ReplyToMessage.From.IsBot ||
                                 update.Message.ReplyToMessage.From.Id == BotId))
                                await ChatHandlers.ReputationUp(botClient, update, db, mes, cancellationToken);

                            //request to ChatGPT
                            var match = Regex.Match(mes, @"^.*?([\w/]+)");
                            if (!Commands.Keys.Any(key => mes.StartsWith(key)) &&
                                (update.Message?.ReplyToMessage is { From.Id: BotId } ||
                                 mes.Contains("@XpAndRepBot") || id == Mid || id == Iid))
                            {
                                await ChatHandlers.RequestChatGpt(botClient, update, mes, cancellationToken);
                            }

                            //fill lexicon table
                            await ChatHandlers.AddWordsInLexicon(user, mes);
                            //level up
                            if (update.Message?.Chat?.Id != Mid)
                                await ChatHandlers.LvlUp(botClient, update, db, user, cancellationToken);
                            //commands
                            if (Commands.ContainsKey(match.Value.ToLower()))
                            {
                                var command = Commands[match.Value.ToLower()];
                                await command.ExecuteAsync(botClient, update, cancellationToken);
                            }

                            List<Users> users = new();
                            //mention
                            if (mes.Length < 100)
                                users = db.TableUsers.Where(x =>
                                    x.Roles.Equals(mes.Substring(1)) || x.Roles.StartsWith(mes.Substring(1) + ",") ||
                                    x.Roles.Contains(", " + mes.Substring(1) + ",") ||
                                    x.Roles.EndsWith(", " + mes.Substring(1))).ToList();
                            if (mes[0] == '@' && users.Count > 0)
                                ChatHandlers.Mention(users, user.Name, update.Message.Chat.Id, botClient,
                                    cancellationToken);
                            List<string> words = new();
                            if (user.Nfc == true)
                            {
                                using (StreamReader reader = new("bw.txt"))
                                {
                                    while (!reader.EndOfStream)
                                    {
                                        var line = await reader.ReadLineAsync();
                                        words.Add(line.ToLower());
                                    }
                                }

                                var messageWords = mes.Split(new[] { ' ', ',', '.', '!', '?' },
                                    StringSplitOptions.RemoveEmptyEntries);
                                var containsWord = messageWords.Any(w => words.Contains(w.ToLower()));
                                if (containsWord)
                                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                        text: await ChatHandlers.Nfc(user, db, botClient, cancellationToken),
                                        cancellationToken: cancellationToken);
                            }

                            if (user.LastMessage == mes)
                            {
                                user.CountRepeatMessage++;
                            }
                            else
                            {
                                user.LastMessage = mes;
                                user.CountRepeatMessage = 1;
                            }

                            if (user.CountRepeatMessage > 3)
                                await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId,
                                    cancellationToken);
                            await db.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
            }
        }

        private static async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery,
            CancellationToken cancellationToken)
        {
            var option = callbackQuery.Data;
            if (callbackQuery.Message != null)
            {
                var messageId = callbackQuery.Message.MessageId;
                var chatId = callbackQuery.Message.Chat.Id;
                var newText = "";
                try
                {
                    InlineKeyboardMarkup inlineKeyboard = null;
                    var number = 0;
                    Match match = null;
                    switch (option)
                    {
                        case "backmw":
                            if (callbackQuery.Message.Text != null)
                                match = Regex.Match(callbackQuery.Message.Text, @"^\d+");
                            if (match is { Success: true })
                            {
                                number = int.Parse(match.Value);
                                if (number > 10) newText = await ResponseHandlers.MeWords(callbackQuery, -10);
                            }

                            inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Назад", "backmw"),
                                    InlineKeyboardButton.WithCallbackData("Вперёд", "nextmw"),
                                }
                            });
                            break;
                        case "nextmw":
                            newText = await ResponseHandlers.MeWords(callbackQuery, 10);
                            inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Назад", "backmw"),
                                    InlineKeyboardButton.WithCallbackData("Вперёд", "nextmw"),
                                }
                            });
                            break;
                        case "backtw":
                            if (callbackQuery.Message.Text != null)
                                match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                            if (match != null) number = int.Parse(match.Value);
                            if (number > 50) newText = await ResponseHandlers.TopWords(number - 51);
                            inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Назад", "backtw"),
                                    InlineKeyboardButton.WithCallbackData("Вперёд", "nexttw"),
                                }
                            });
                            break;
                        case "nexttw":
                            if (callbackQuery.Message.Text != null)
                                match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                            if (match != null) newText = await ResponseHandlers.TopWords(int.Parse(match.Value) + 49);
                            inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Назад", "backtw"),
                                    InlineKeyboardButton.WithCallbackData("Вперёд", "nexttw"),
                                }
                            });
                            break;
                        case "backtl":
                            if (callbackQuery.Message.Text != null)
                                match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                            if (match != null) number = int.Parse(match.Value);
                            if (number > 50) newText = ResponseHandlers.TopLvl(number - 51);
                            inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Назад", "backtl"),
                                    InlineKeyboardButton.WithCallbackData("Вперёд", "nexttl"),
                                }
                            });
                            break;
                        case "nexttl":
                            if (callbackQuery.Message.Text != null)
                                match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                            if (match != null) newText = ResponseHandlers.TopLvl(int.Parse(match.Value) + 49);
                            inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Назад", "backtl"),
                                    InlineKeyboardButton.WithCallbackData("Вперёд", "nexttl"),
                                }
                            });
                            break;
                        case "backtr":
                            if (callbackQuery.Message.Text != null)
                                match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                            if (match != null) number = int.Parse(match.Value);
                            if (number > 50) newText = ResponseHandlers.TopRep(number - 51);
                            inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Назад", "backtr"),
                                    InlineKeyboardButton.WithCallbackData("Вперёд", "nexttr"),
                                }
                            });
                            break;
                        case "nexttr":
                            if (callbackQuery.Message.Text != null)
                                match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                            if (match != null) newText = ResponseHandlers.TopRep(int.Parse(match.Value) + 49);
                            inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Назад", "backtr"),
                                    InlineKeyboardButton.WithCallbackData("Вперёд", "nexttr"),
                                }
                            });
                            break;
                        case "backl":
                            if (callbackQuery.Message.Text != null)
                                match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                            if (match != null) number = int.Parse(match.Value);
                            if (number > 50) newText = await ResponseHandlers.TopLexicon(number - 51);
                            inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Назад", "backl"),
                                    InlineKeyboardButton.WithCallbackData("Вперёд", "nextl"),
                                }
                            });
                            break;
                        case "nextl":
                            if (callbackQuery.Message.Text != null)
                                match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                            if (match != null) newText = await ResponseHandlers.TopLexicon(int.Parse(match.Value) + 49);
                            inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Назад", "backl"),
                                    InlineKeyboardButton.WithCallbackData("Вперёд", "nextl"),
                                }
                            });
                            break;
                        case "backr":
                            if (callbackQuery.Message.Text != null)
                                match = Regex.Match(callbackQuery.Message.Text, @"\d+", RegexOptions.Multiline);
                            if (match != null) number = int.Parse(match.Value);
                            if (number > 20) newText = ResponseHandlers.GetRoles(number - 21);
                            inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Назад", "backr"),
                                    InlineKeyboardButton.WithCallbackData("Вперёд", "nextr"),
                                }
                            });
                            break;
                        case "nextr":
                            if (callbackQuery.Message.Text != null)
                                match = Regex.Match(callbackQuery.Message.Text, @"\d+", RegexOptions.Multiline);
                            if (match != null) newText = ResponseHandlers.GetRoles(int.Parse(match.Value) + 19);
                            inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Назад", "backr"),
                                    InlineKeyboardButton.WithCallbackData("Вперёд", "nextr"),
                                }
                            });
                            break;
                        default:
                            if (callbackQuery.Message.ReplyToMessage is { From: not null })
                            {
                                var userId = callbackQuery.Message.ReplyToMessage.From.Id;
                                newText = callbackQuery.Message.Text;
                                if (option != null && option[0] == 'm')
                                {
                                    inlineKeyboard = await ResponseHandlers.AcceptMariage(option, callbackQuery,
                                        botClient, chatId, userId, cancellationToken);
                                }
                                else
                                {
                                    var flag = await ResponseHandlers.GetChatPermissions(option, callbackQuery,
                                        botClient, chatId, cancellationToken);
                                    inlineKeyboard = callbackQuery.Message.ReplyMarkup;
                                    if (flag)
                                        await botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId,
                                            cancellationToken);
                                }
                            }

                            break;
                    }

                    if (inlineKeyboard != null)
                        if (newText != null)
                            await botClient.EditMessageTextAsync(chatId: chatId, replyMarkup: inlineKeyboard,
                                messageId: messageId, text: newText, parseMode: ParseMode.Html,
                                cancellationToken: cancellationToken);
                }
                catch
                {
                    // ignored
                }
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
            CancellationToken cancellationToken)
        {
            Console.Error.WriteLine(exception);
            return Task.CompletedTask;
        }
    }
}