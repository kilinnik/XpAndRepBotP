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
            //{"/m@XpAndRepBot", new MeCommand() },
            {"/tl", new TopLvlCommand() },
            //{"/tl@XpAndRepBot", new TopLvlCommand() },
            {"/tr", new TopReputationCommand() },
            //{"/tr@XpAndRepBot", new TopReputationCommand() },
            {"/r", new RulesCommand() },
            //{"/r@XpAndRepBot", new RulesCommand() },
            {"/h", new HelpCommand() },
            //{"/h@XpAndRepBot", new HelpCommand() },
            //{"/help@XpAndRepBot", new HelpCommand() },
            {"/help", new HelpCommand() },
            {"/mr", new MessagesReputationCommand() },
            //{"/mr@XpAndRepBot", new MessagesReputationCommand() },
            {"/g", new GamesCommand() },
            //{"/g@XpAndRepBot", new GamesCommand() },
            {"/tw", new TopWordsCommand() },
            //{"/tw@XpAndRepBot", new TopWordsCommand() },
            {"/porno", new RoflCommand() },
            {"/hc", new HelpChatGPTCommand() },
            //{"/hc@XpAndRepBot", new HelpChatGPTCommand() },
            {"/i", new ImageCommand() },
            //{"/i@XpAndRepBot", new ImageCommand() },
            {"/warn", new WarnCommand() },
            {"/unwarn", new UnwarnCommand() },
            {"/unw", new UnwarnCommand() },
            {"/l", new TopLexiconCommand() },
            {"/tge", new TgEmpressCommand() },
            {"/role", new RoleCommand() },
            {"/roles", new RolesCommand() },
             {"/nfc", new NfcCommand() }
        };

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Debug.WriteLine(JsonSerializer.Serialize(update));
            //inline buttons
            if (update.Type == UpdateType.CallbackQuery)
            {
                await HandleCallbackQuery(botClient, update.CallbackQuery);
            }
            using var db = new InfoContext();
            DataClasses1DataContext dbl = new(ConStringDbLexicon);
            if (update.Message?.Chat?.Id is long id && (id == IgruhaChatID || id == MyChatID || id == MID || id == IID) && !update.Message.From.IsBot)
            {
                var idUser = update.Message.From.Id;
                var user = db.TableUsers.FirstOrDefault(x => x.Id == idUser);
                if (user == null)
                {
                    user = new Users(idUser, update.Message.From.FirstName + " " + update.Message.From.LastName, 0, 0, 0);
                    db.TableUsers.Add(user);
                }
                if (user.Name == "0") user.Name = update.Message.From.FirstName + " " + update.Message.From.LastName;
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
                db.SaveChanges();
                //снятие варна 
                if (user.Warns > 0)
                {
                    TimeSpan difference = user.LastTime - DateTime.Now;
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
                    if (update.Message.ReplyToMessage != null && !update.Message.ReplyToMessage.From.IsBot) await ChatHandlers.ReputationUp(botClient, update, db, mes, cancellationToken);
                    //запрос к chatgdp
                    Match match = Regex.Match(mes, @"^.*?([\w/]+)");
                    if (!_commands.ContainsKey(match.Value) && ((update?.Message?.ReplyToMessage != null && update?.Message?.ReplyToMessage.From.Id == BotID) || (mes.Contains("@XpAndRepBot") && !mes.Contains('/')) || update.Message?.Chat?.Id == MID || id == IID))
                    {
                        ChatHandlers.RequestChatGPT(botClient, update, mes, cancellationToken);
                    }
                    //создание и заполнение таблицы
                    ChatHandlers.CreateAndFillTable(user, mes, dbl);
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
                    List<string> words = new List<string>();
                    if (user.Nfc == true)
                    {
                        using (StreamReader reader = new StreamReader("bw.txt"))
                        {
                            while (!reader.EndOfStream)
                            {
                                string line = reader.ReadLine();
                                words.Add(line.ToLower());
                            }
                        }
                        bool containsWord = words.Any(w => mes.ToLower() == w);
                        if (containsWord) botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: ChatHandlers.Nfc(user, db), cancellationToken: cancellationToken);
                    }
                }
            }
        }
        public static async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
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
            var option = callbackQuery.Data;
            var messageId = callbackQuery.Message.MessageId;
            var chatId = callbackQuery.Message.Chat.Id;
            string newText = "";
            int number;
            Match match;
            switch (option)
            {
                case "backmw":
                    match = Regex.Match(callbackQuery.Message.Text, @"^\d+");
                    if (match.Success)
                    {
                        number = int.Parse(match.Value);
                        if (number > 10) newText = ResponseHandlers.MeWords(callbackQuery, -10);
                    }
                    inlineKeyboard = inlineKeyboardMW;
                    break;
                case "nextmw":
                    newText = ResponseHandlers.MeWords(callbackQuery, 10);
                    inlineKeyboard = inlineKeyboardMW;
                    break;
                case "backtw":
                    match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                    number = int.Parse(match.Value);
                    if (number > 50) newText = ResponseHandlers.TopWords(number - 51);
                    inlineKeyboard = inlineKeyboardTW;
                    break;
                case "nexttw":
                    match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                    newText = ResponseHandlers.TopWords(int.Parse(match.Value) + 49);
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
                    if (number > 50) newText = ResponseHandlers.TopLexicon(number - 51);
                    inlineKeyboard = inlineKeyboardL;
                    break;
                case "nextl":
                    match = Regex.Match(callbackQuery.Message.Text, @"\n\d+", RegexOptions.Multiline);
                    newText = ResponseHandlers.TopLexicon(int.Parse(match.Value) + 49);
                    inlineKeyboard = inlineKeyboardL;
                    break;
            }
            try
            {
                await botClient.EditMessageTextAsync(chatId: chatId, replyMarkup: inlineKeyboard, messageId: messageId, text: newText);
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

