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
            {"/i", new ImageCommand() },
            {"/warn", new WarnCommand() },
            {"/unwarn", new UnwarnCommand() },
            {"/unw", new UnwarnCommand() },
            {"/l", new TopLexiconCommand() },
            {"/tge", new TgEmpressCommand() },
            {"/role", new RoleCommand() },
            {"/roles", new RolesCommand() },
            {"/nfc", new NoFuckChallengeCommand() },
            {"/unr", new UnRoleCommand() },
            {"/b", new BalabobaCommand() },
            {"/vb", new VoteBanCommand() },
            {"/ban", new BanCommand() }
        };

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Debug.WriteLine(JsonSerializer.Serialize(update));
            //inline buttons
            if (update.Type == UpdateType.CallbackQuery)
            {
                await HandleCallbackQuery(botClient, update.CallbackQuery, cancellationToken);
            }
            using var db = new InfoContext();
            if (update.Message?.Chat?.Id is long id && (id == IgruhaChatID || id == MyChatID || id == MID || id == IID) && !update.Message.From.IsBot)
            {
                var idUser = update.Message.From.Id;
                var user = db.TableUsers.FirstOrDefault(x => x.Id == idUser);
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
                if (update.Message?.NewChatMembers != null)
                {
                    var newMembers = update.Message.NewChatMembers;
                    foreach (var member in newMembers)
                    {
                        try
                        {
                            await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: $"Привет, {user.Name}.{Greeting}", cancellationToken: cancellationToken);
                        }
                        catch
                        {
                            await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: $"Привет, {user.Name}.{Greeting}", cancellationToken: cancellationToken);
                        }
                    }
                }
                //var ids = db.TableUsers.Select(x => x.Id).ToList();
                //for (int i = 0; i < db.TableUsers.Count(); i++)
                //{
                //    try
                //    {
                //        var chatMember = await botClient.GetChatMemberAsync(IgruhaChatID, ids[i]);
                //        var user3 = db.TableUsers.First(x => x.Id == ids[i]);
                //        var user2 = chatMember.User;
                //        user3.Name = user2.FirstName + " " + user2.LastName;
                //        db.SaveChanges();
                //    }
                //    catch { }
                //}
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
                if (update?.Message?.Text != null || update?.Message?.Caption is not null)
                {
                    var mes = update.Message.Caption ?? update.Message.Text;
                    //опыт
                    user.CurXp += mes.Length;
                    //повышение репутации
                    if (update.Message.ReplyToMessage != null && (!update.Message.ReplyToMessage.From.IsBot || update.Message.ReplyToMessage.From.Id == BotID)) await ChatHandlers.ReputationUp(botClient, update, db, mes, cancellationToken);
                    //запрос к chatgdp
                    Match match = Regex.Match(mes, @"^.*?([\w/]+)");
                    if (!_commands.ContainsKey(match.Value) && ((update?.Message?.ReplyToMessage != null && update?.Message?.ReplyToMessage.From.Id == BotID) || (mes.Contains("@XpAndRepBot") && !mes.Contains('/')) || id == MID || id == IID))
                    {
                        ChatHandlers.RequestChatGPT(botClient, update, mes, cancellationToken);
                    }
                    //создание и заполнение таблицы
                    await ChatHandlers.CreateAndFillTable(user, mes);
                    //повышение уровня
                    if (update.Message?.Chat?.Id != MID) await ChatHandlers.LvlUp(botClient, update, db, user, cancellationToken);
                    //команды
                    if (_commands.ContainsKey(match.Value))
                    {
                        var command = _commands[match.Value];
                        command.ExecuteAsync(botClient, update, cancellationToken);
                    }
                    var users = db.TableUsers.Where(x => x.Roles.StartsWith(mes.Substring(1)) || x.Roles.Contains(", " + mes.Substring(1))).ToList();
                    if (mes[0] == '@' && users.Count > 0) botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: $"{user.Name} призывает {ChatHandlers.Mention(users)}", parseMode: ParseMode.Html, cancellationToken: cancellationToken);
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
                        if (containsWord) botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: ChatHandlers.Nfc(user, db), cancellationToken: cancellationToken);
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
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("", "")
                }
            });
            var inlineKeyboardMW = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "backmw"),
                    InlineKeyboardButton.WithCallbackData("Вперёд", "nextmw"),
                }
            });
            var inlineKeyboardTW = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "backtw"),
                    InlineKeyboardButton.WithCallbackData("Вперёд", "nexttw"),
                }
            });
            var inlineKeyboardTL = new InlineKeyboardMarkup(new[]
           {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "backtl"),
                    InlineKeyboardButton.WithCallbackData("Вперёд", "nexttl"),
                }
            });
            var inlineKeyboardTR = new InlineKeyboardMarkup(new[]
           {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "backtr"),
                    InlineKeyboardButton.WithCallbackData("Вперёд", "nexttr"),
                }
            });
            var inlineKeyboardL = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "backl"),
                    InlineKeyboardButton.WithCallbackData("Вперёд", "nextl"),
                }
            });
            var userId = callbackQuery.Message.ReplyToMessage.From.Id;
            var option = callbackQuery.Data;
            var messageId = callbackQuery.Message.MessageId;
            var chatId = callbackQuery.Message.Chat.Id;
            string newText = "";
            int number;
            Match match;
            try
            {
                switch (option)
                {
                    case "backmw":
                        match = Regex.Match(callbackQuery.Message.Text, @"^\d+");
                        if (match.Success)
                        {
                            number = int.Parse(match.Value);
                            if (number > 10) newText = await ResponseHandlers.MeWords(callbackQuery, -10);
                        }
                        inlineKeyboard = inlineKeyboardMW;
                        break;
                    case "nextmw":
                        newText = await ResponseHandlers.MeWords(callbackQuery, 10);
                        inlineKeyboard = inlineKeyboardMW;
                        break;
                    case "backtw":
                        match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                        number = int.Parse(match.Value);
                        if (number > 50) newText = await ResponseHandlers.TopWords(number - 51);
                        inlineKeyboard = inlineKeyboardTW;
                        break;
                    case "nexttw":
                        match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                        newText = await ResponseHandlers.TopWords(int.Parse(match.Value) + 49);
                        inlineKeyboard = inlineKeyboardTW;
                        break;
                    case "backtl":
                        match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                        number = int.Parse(match.Value);
                        if (number > 50) newText = ResponseHandlers.TopLvl(number - 51);
                        inlineKeyboard = inlineKeyboardTL;
                        break;
                    case "nexttl":
                        match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                        newText = ResponseHandlers.TopLvl(int.Parse(match.Value) + 49);
                        inlineKeyboard = inlineKeyboardTL;
                        break;
                    case "backtr":
                        match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                        number = int.Parse(match.Value);
                        if (number > 50) newText = ResponseHandlers.TopRep(number - 51);
                        inlineKeyboard = inlineKeyboardTR;
                        break;
                    case "nexttr":
                        match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                        newText = ResponseHandlers.TopRep(int.Parse(match.Value) + 49);
                        inlineKeyboard = inlineKeyboardTR;
                        break;
                    case "backl":
                        match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                        number = int.Parse(match.Value);
                        if (number > 50) newText = await ResponseHandlers.TopLexicon(number - 51);
                        inlineKeyboard = inlineKeyboardL;
                        break;
                    case "nextl":
                        match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                        newText = await ResponseHandlers.TopLexicon(int.Parse(match.Value) + 49);
                        inlineKeyboard = inlineKeyboardL;
                        break;
                    default:
                        inlineKeyboard = await ResponseHandlers.VoteBan(inlineKeyboard, callbackQuery, option, chatId, botClient, cancellationToken, userId);
                        newText = callbackQuery.Message.Text;
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

