using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using System.Linq;
using System.Collections.Generic;
using Telegram.Bot.Types.Enums;
using System.Text.RegularExpressions;
using Telegram.Bot.Types.ReplyMarkups;
using static XpAndRepBot.Consts;
using System.Data;
using System.IO;

namespace XpAndRepBot
{
    class UpdateHandler : IUpdateHandler
    {
        static readonly Dictionary<string, ICommand> _commands = new()
        {
            {"/m", new MeCommand() },
            {"/tl", new TopLvlCommand() },
            {"/tr", new TopReputationCommand() },
            {"/r", new RulesCommand() },
            {"/h", new HelpCommand() },
            {"/help", new HelpCommand() },
            {"/mr", new MessagesReputationCommand() },
            {"/g", new GamesCommand() },
            {"/tw", new TopWordsCommand() },
            {"/porno", new RoflCommand() },
            {"/hc", new HelpChatGPTCommand() },
            {"/im", new ImageDalleCommand() },
            {"/l", new TopLexiconCommand() },
            {"/tge", new TgEmpressCommand() },
            {"/role", new RoleCommand() },
            {"/roles", new RolesCommand() },
            {"/nfc", new NoFuckChallengeCommand() },
            {"/unr", new UnRoleCommand() },
            {"/b", new BalabobaCommand() },
            {"/w", new WordCommand() },
            {"брак", new MariageCommand() },
            {"браки", new MariagesCommand() },
            {"статус", new StatusCommand() },
            {"развод", new DivorceCommand() },
            {"/ha", new HelpAdminCommand() },
            {"/ban", new BanCommand() },
            {"/unb", new UnBanCommand() },
            {"/warn", new WarnCommand() },
            {"/unwarn", new UnwarnCommand() },
            {"/unw", new UnwarnCommand() },
            {"/mute", new MuteCommand() },
            {"/rew", new HelpRewardsCommand() }
        };

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Debug.WriteLine(JsonSerializer.Serialize(update));
            //обработка нажатия кнопок
            if (update.Type == UpdateType.CallbackQuery)
            {
                await HandleCallbackQuery(botClient, update.CallbackQuery, cancellationToken);
            }
            using var db = new InfoContext();
            if (update.Message?.Chat?.Id is long id && (id == IgruhaChatID || id == MyChatID || id == MID || id == IID))
            {
                var idUser = update.Message.From.Id;
                var user = db.TableUsers.FirstOrDefault(x => x.Id == idUser);
                //добавить нового юзера в БД
                if (user == null)
                {
                    string name = update.Message.From.FirstName;
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
                db.SaveChanges();
                //капча при входе
                if (update.Message?.NewChatMembers != null)
                {
                    ChatHandlers.NewMember(botClient, update, cancellationToken);
                }
                //снятие варна 
                if (user.Warns > 0)
                {
                    TimeSpan difference = DateTime.Now - user.LastTime;
                    if (difference.TotalDays >= 30)
                    {
                        user.Warns--;
                        user.LastTime = DateTime.Now;
                        db.SaveChanges();
                        try
                        {
                            await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: $"С {user.Name} снимается 1 варн({user.Warns}/3)", cancellationToken: cancellationToken);
                        }
                        catch
                        {
                            await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: $"С {user.Name} снимается 1 варн({user.Warns}/3)", cancellationToken: cancellationToken);
                        }
                    }
                }
                //удаление гифок, стикеров и опросов
                await ChatHandlers.Delete(botClient, update, user, cancellationToken);
                if (update?.Message?.Text != null || update?.Message?.Caption != null)
                {
                    var mes = update.Message.Caption ?? update.Message.Text;
                    //опыт
                    user.CurXp += mes.Length;
                    //повышение репутации
                    if (update.Message.ReplyToMessage != null && (!update.Message.ReplyToMessage.From.IsBot || update.Message.ReplyToMessage.From.Id == BotID)) await ChatHandlers.ReputationUp(botClient, update, db, mes, cancellationToken);
                    //запрос к chatgdp
                    Match match = Regex.Match(mes, @"^.*?([\w/]+)");
                    if (!_commands.ContainsKey(match.Value) && ((update?.Message?.ReplyToMessage != null && update?.Message?.ReplyToMessage.From.Id == BotID) || (mes.Contains("@XpAndRepBot") && !_commands.Keys.Any(key => mes.StartsWith(key))) || id == MID || id == IID))
                    {
                        ChatHandlers.RequestChatGPT(botClient, update, mes, cancellationToken);
                    }
                    //заполнение таблицы лексикона
                    await ChatHandlers.AddWordsInLexicon(user, mes);
                    //повышение уровня
                    if (update.Message?.Chat?.Id != MID) await ChatHandlers.LvlUp(botClient, update, db, user, cancellationToken);
                    //команды
                    if (_commands.ContainsKey(match.Value.ToLower()))
                    {
                        var command = _commands[match.Value.ToLower()];
                        command.ExecuteAsync(botClient, update, cancellationToken);
                    }
                    List<Users> users = new();
                    //упоминание роли
                    if (mes.Length < 100) users = db.TableUsers.Where(x => x.Roles.Equals(mes.Substring(1)) || x.Roles.StartsWith(mes.Substring(1) + ",") || x.Roles.Contains(", " + mes.Substring(1) + ",") || x.Roles.EndsWith(", " + mes.Substring(1))).ToList();
                    if (mes[0] == '@' && users.Count > 0) ChatHandlers.Mention(users, user.Name, update.Message.Chat.Id, botClient, cancellationToken);
                    List<string> words = new();
                    if (user.Nfc == true)
                    {
                        using (StreamReader reader = new("bw.txt"))
                        {
                            while (!reader.EndOfStream)
                            {
                                string line = reader.ReadLine();
                                words.Add(line.ToLower());
                            }
                        }
                        string[] messageWords = mes.Split(new char[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
                        bool containsWord = messageWords.Any(w => words.Contains(w.ToLower()));
                        if (containsWord) botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: await ChatHandlers.Nfc(user, db, botClient, cancellationToken), cancellationToken: cancellationToken);
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
                    if (user.CountRepeatMessage > 3) botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
                    db.SaveChanges();
                }
            }
        }
        public static async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var option = callbackQuery.Data;
            var messageId = callbackQuery.Message.MessageId;
            var chatId = callbackQuery.Message.Chat.Id;
            string newText = "";
            int number;
            Match match;
            try
            {
                InlineKeyboardMarkup inlineKeyboard;
                switch (option)
                {
                    case "backmw":
                        match = Regex.Match(callbackQuery.Message.Text, @"^\d+");
                        if (match.Success)
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
                        match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                        number = int.Parse(match.Value);
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
                        match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                        newText = await ResponseHandlers.TopWords(int.Parse(match.Value) + 49);
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
                        match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                        number = int.Parse(match.Value);
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
                        match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                        newText = ResponseHandlers.TopLvl(int.Parse(match.Value) + 49);
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
                        match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                        number = int.Parse(match.Value);
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
                        match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                        newText = ResponseHandlers.TopRep(int.Parse(match.Value) + 49);
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
                        match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                        number = int.Parse(match.Value);
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
                        match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                        newText = await ResponseHandlers.TopLexicon(int.Parse(match.Value) + 49);
                        inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Назад", "backl"),
                                InlineKeyboardButton.WithCallbackData("Вперёд", "nextl"),
                            }
                        });
                        break;
                    default:
                        var userId = callbackQuery.Message.ReplyToMessage.From.Id;
                        newText = callbackQuery.Message.Text;
                        if (option[0] == 'm')
                        {
                            inlineKeyboard = await ResponseHandlers.AcceptMariage(option, callbackQuery, botClient, chatId, userId, cancellationToken);
                        }
                        else
                        {
                            var flag = await ResponseHandlers.GetChatPermissions(option, callbackQuery, botClient, chatId, cancellationToken);
                            inlineKeyboard = callbackQuery.Message.ReplyMarkup;
                            if (flag) await botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId, cancellationToken);
                        }
                        break;
                }
                await botClient.EditMessageTextAsync(chatId: chatId, replyMarkup: inlineKeyboard, messageId: messageId, text: newText, cancellationToken: cancellationToken);
            }
            catch { }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.Error.WriteLine(exception);
            return Task.CompletedTask;
        }
    }
}

