using OpenAI.GPT3.Managers;
using OpenAI.GPT3;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text.RegularExpressions;
using System.Linq;
using static XpAndRepBot.Consts;
using Telegram.Bot.Types.Enums;
using System;
using StableDiffusionClient;

namespace XpAndRepBot
{
    public interface ICommand
    {
        public Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    }

    public class HelpCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: HelpText, cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: HelpText, cancellationToken: cancellationToken);
            }
        }
    }

    public class MeCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "backmw"),
                    InlineKeyboardButton.WithCallbackData("Вперёд", "nextmw"),
                }
            });
            if (update?.Message?.ReplyToMessage != null && !update.Message.ReplyToMessage.From.IsBot && update.Message.ReplyToMessage.From.Id != 777000)
            {
                try
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard, replyToMessageId: update.Message.ReplyToMessage.MessageId, text: await ResponseHandlers.Me(update.Message.ReplyToMessage.From.Id), cancellationToken: cancellationToken);
                }
                catch
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: await ResponseHandlers.Me(update.Message.ReplyToMessage.From.Id), cancellationToken: cancellationToken);
                }
            }
            else
            {
                try
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard, replyToMessageId: update.Message.MessageId, text: await ResponseHandlers.Me(update.Message.From.Id), cancellationToken: cancellationToken);
                }
                catch
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: await ResponseHandlers.Me(update.Message.From.Id), cancellationToken: cancellationToken);
                }
            }
        }
    }

    public class TopLvlCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
          {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "backtl"),
                    InlineKeyboardButton.WithCallbackData("Вперёд", "nexttl"),
                }
            });
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard, replyToMessageId: update.Message.MessageId, text: ResponseHandlers.TopLvl(0), cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard, text: ResponseHandlers.TopLvl(0), cancellationToken: cancellationToken);
            }
        }
    }

    public class TopReputationCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
           {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "backtr"),
                    InlineKeyboardButton.WithCallbackData("Вперёд", "nexttr"),
                }
            });
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard, replyToMessageId: update.Message.MessageId, text: ResponseHandlers.TopRep(0), cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard, text: ResponseHandlers.TopRep(0), cancellationToken: cancellationToken);
            }
        }
    }

    public class RulesCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: RulesText, cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: RulesText, cancellationToken: cancellationToken);
            }
        }
    }

    public class MessagesReputationCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: MesRepText, cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: MesRepText, cancellationToken: cancellationToken);
            }
        }
    }

    public class GamesCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: GamesText, cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: GamesText, cancellationToken: cancellationToken);
            }
        }
    }

    public class TopWordsCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "backtw"),
                    InlineKeyboardButton.WithCallbackData("Вперёд", "nexttw"),
                }
            });
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard, replyToMessageId: update.Message.MessageId, text: await ResponseHandlers.TopWords(0), cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard, text: await ResponseHandlers.TopWords(0), cancellationToken: cancellationToken);
            }
        }
    }

    public class TopLexiconCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "backl"),
                    InlineKeyboardButton.WithCallbackData("Вперёд", "nextl"),
                }
            });
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard, replyToMessageId: update.Message.MessageId, text: await ResponseHandlers.TopLexicon(0), cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard, text: await ResponseHandlers.TopLexicon(0), cancellationToken: cancellationToken);
            }
        }
    }

    public class RoflCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            using var db = new InfoContext();
            var user = db.TableUsers.FirstOrDefault(x => x.Id == update.Message.From.Id);
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: $"🖕 {user.Name} иди на хуй 🖕", cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: $"🖕 {user.Name} иди на хуй 🖕", cancellationToken: cancellationToken);
            }
        }
    }

    public class HelpChatGPTCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: HelpGPTText, cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: HelpGPTText, cancellationToken: cancellationToken);
            }
        }
    }

    public class ImageDalleCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var openAiService = new OpenAIService(new OpenAiOptions() { ApiKey = SSHKey });
            var mes = update.Message.Caption ?? update.Message.Text;
            var matches = Regex.Match(mes, @"(?<=\s)\w[\w\s]*");
            try
            {
                try
                {
                    await botClient.SendPhotoAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, photo: await ResponseHandlers.GenerateImage(openAiService, matches.Value), cancellationToken: cancellationToken);
                }
                catch
                {
                    await botClient.SendPhotoAsync(chatId: update.Message.Chat.Id, photo: await ResponseHandlers.GenerateImage(openAiService, matches.Value), cancellationToken: cancellationToken);
                }
            }
            catch { await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: "Произошла ошибка. Возможно вы не ввели текст/ввели некорректный запрос ", cancellationToken: cancellationToken); }
        }
    }

    public class WarnCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message.From.Id == IID)
            {
                try
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.ReplyToMessage.MessageId, text: ResponseHandlers.Warn(update), cancellationToken: cancellationToken);
                }
                catch
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: ResponseHandlers.Warn(update), cancellationToken: cancellationToken);
                }
            }
        }
    }

    public class UnwarnCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message.From.Id == IID)
            {
                try
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.ReplyToMessage.MessageId, text: ResponseHandlers.Unwarn(update), cancellationToken: cancellationToken);
                }
                catch
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: ResponseHandlers.Unwarn(update), cancellationToken: cancellationToken);
                }
            }
        }
    }

    public class TgEmpressCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: TgEmpress, cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: TgEmpress, cancellationToken: cancellationToken);
            }
        }
    }

    public class RoleCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message.From.Id == IID)
            {
                try
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.ReplyToMessage.MessageId, text: ResponseHandlers.GiveRole(update.Message.ReplyToMessage.From.Id, update.Message.Text[6..]), cancellationToken: cancellationToken);
                }
                catch
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: ResponseHandlers.GiveRole(update.Message.ReplyToMessage.From.Id, update.Message.Text[6..]), cancellationToken: cancellationToken);
                }
            }
        }
    }

    public class RolesCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: ResponseHandlers.GetRoles(), parseMode: ParseMode.Html, cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: ResponseHandlers.GetRoles(), parseMode: ParseMode.Html, cancellationToken: cancellationToken);
            }
        }
    }

    public class NoFuckChallengeCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            using var db = new InfoContext();
            var user = db.TableUsers.First(x => x.Id == update.Message.From.Id);
            if (user.Nfc == true)
            {
                try
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: ResponseHandlers.PrintNfc(), cancellationToken: cancellationToken);
                }
                catch
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: ResponseHandlers.PrintNfc(), cancellationToken: cancellationToken);
                }
            }
            else
            {
                user.Nfc = true;
                DateTime date = new(2009, 8, 1, 12, 0, 0);
                int result = DateTime.Compare(user.StartNfc, date);
                string bestTime = "";
                if (user.BestTime > 0)
                {
                    var ts = TimeSpan.FromTicks(user.BestTime);
                    bestTime = string.Format("\nВаш лучший результат: {0} d, {1} h, {2} m.", ts.Days, ts.Hours, ts.Minutes);
                }
                user.StartNfc = DateTime.Now;
                db.SaveChanges();
                try
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: $"Вы начали новую серию без мата 👮‍♂️{bestTime}\nУдачи 😉", cancellationToken: cancellationToken);
                }
                catch
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: $"Вы начали новую серию без мата 👮‍♂️\nУдачи 😉", cancellationToken: cancellationToken);
                }
            }
        }
    }

    public class UnRoleCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message.From.Id == IID)
            {
                try
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: ResponseHandlers.DelRole(update.Message.ReplyToMessage.From.Id, update.Message.Text[5..]), cancellationToken: cancellationToken);
                }
                catch
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: ResponseHandlers.DelRole(update.Message.ReplyToMessage.From.Id, update.Message.Text[5..]), parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                }
            }
        }
    }

    public class BalabobaCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: await ResponseHandlers.RequestBalaboba(update.Message.Text[3..]), cancellationToken: cancellationToken);
            }
            catch
            {
                _ = await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: await ResponseHandlers.RequestBalaboba(update.Message.Text[3..]), cancellationToken: cancellationToken);
            }
        }
    }

    public class VoteBanCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update?.Message?.ReplyToMessage != null && !update.Message.ReplyToMessage.From.IsBot && update.Message.ReplyToMessage.From.Id != 777000)
            {
                var db = new InfoContext();
                long userId1 = update.Message.ReplyToMessage.From.Id;
                var user1 = db.TableUsers.First(x => x.Id == userId1);
                long userId2 = update.Message.From.Id;
                var user2 = db.TableUsers.First(x => x.Id == userId2);
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Да ✅ - 0", $"y0_{userId1}"),
                        InlineKeyboardButton.WithCallbackData("Нет ❌ - 0", $"n0_{userId1}"),
                    }
                });
                var chatMember = await botClient.GetChatMemberAsync(update.Message.Chat.Id, userId1, cancellationToken: cancellationToken);
                if (chatMember.Status != ChatMemberStatus.Administrator && chatMember.Status != ChatMemberStatus.Creator)
                {
                    try
                    {
                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.ReplyToMessage.MessageId, text: $"{user2.Name} начал голосование за бан {user1.Name}", replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
                    }
                    catch { }
                }
            }
            else
            {
                try
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: "Кого баним? Ответьте на сообщение пользователя. Воутбан можно начинать строго по правилам /r", cancellationToken: cancellationToken);
                }
                catch
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: "Кого баним? Ответьте на сообщение пользователя. Воутбан можно начинать строго по правилам /r", cancellationToken: cancellationToken);
                }
            }
        }
    }

    public class BanCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message.From.Id == IID)
            {
                try
                {

                    await botClient.BanChatMemberAsync(chatId: update.Message.Chat.Id, userId: update.Message.ReplyToMessage.From.Id, cancellationToken: cancellationToken);
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.ReplyToMessage.MessageId, text: $"Пользователь забанен", cancellationToken: cancellationToken);
                }
                catch { }
            }
        }
    }

    public class StatusCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            using var db = new InfoContext();
            string status = "";
            if (update?.Message?.ReplyToMessage != null && !update.Message.ReplyToMessage.From.IsBot && update.Message.ReplyToMessage.From.Id != 777000)
            {
                var user1 = db.TableUsers.First(x => x.Id == update.Message.ReplyToMessage.From.Id);
                if (user1.Mariage == 0) status = $"{user1.Name} не состоит в браке";
                else
                {
                    var user2 = db.TableUsers.First(x => x.Id == user1.Mariage);
                    TimeSpan ts = DateTime.Now - user2.DateMariage;
                    status = $"🤵🏿 🤵🏿 {user1.Name} состоит в браке с {user2.Name} {ts.Days} d, {ts.Hours} h, {ts.Minutes} m. Дата регистрации {user1.DateMariage:yy/MM/dd HH:mm:ss}";
                }
            }
            else
            {
                var user1 = db.TableUsers.First(x => x.Id == update.Message.From.Id);
                if (user1.Mariage == 0) status = $"Вы не состоите в браке";
                else
                {
                    var user2 = db.TableUsers.First(x => x.Id == user1.Mariage);
                    TimeSpan ts = DateTime.Now - user2.DateMariage;
                    status = $"🤵🏿 🤵🏿 {user1.Name} состоит в браке с {user2.Name} {ts.Days} d, {ts.Hours} h, {ts.Minutes} m. Дата регистрации {user1.DateMariage:yy/MM/dd HH:mm:ss}";
                }
            }
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: status, cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: status, cancellationToken: cancellationToken);
            }
        }
    }

    public class MariageCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update?.Message?.ReplyToMessage != null && !update.Message.ReplyToMessage.From.IsBot && update.Message.ReplyToMessage.From.Id != 777000)
            {
                using var db = new InfoContext();
                var user1 = db.TableUsers.First(x => x.Id == update.Message.From.Id);
                var user2 = db.TableUsers.First(x => x.Id == update.Message.ReplyToMessage.From.Id);
                if (user1.Mariage != 0) await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: "Вы уже в браке", cancellationToken: cancellationToken);
                else if (user2.Mariage != 0) await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: $"{user2.Name} уже в браке", cancellationToken: cancellationToken);
                else
                {
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Нет", $"mny{user1.Id}"),
                            InlineKeyboardButton.WithCallbackData("Да", $"my{user1.Id}"),
                        }
                    });
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard, replyToMessageId: update.Message.ReplyToMessage.MessageId, text: $"💖 {user1.Name} делает вам предложение руки и сердца. Согласны ли вы вступить в брак с {user1.Name}?", cancellationToken: cancellationToken);
                }
            }
            else await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: "Ответьте на сообщение того, с кем хотите заключить брак", cancellationToken: cancellationToken);
        }
    }

    public class DivorceCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            using var db = new InfoContext();
            var user1 = db.TableUsers.First(x => x.Id == update.Message.From.Id);
            if (user1.Mariage == 0) await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: "Вы не состоите в браке", cancellationToken: cancellationToken);
            else
            {
                var user2 = db.TableUsers.First(x => x.Id == user1.Mariage);
                user1.Mariage = 0;
                user2.Mariage = 0;
                TimeSpan ts = DateTime.Now - user2.DateMariage;
                db.SaveChanges();
                string mes = $"💔 {user2.Name} сожалеем, но {user1.Name} подал на развод. Ваш брак был зарегистрирован {user1.DateMariage:yy/MM/dd HH:mm:ss} и просуществовал {ts.Days} d, {ts.Hours} h, {ts.Minutes} m";
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: mes, cancellationToken: cancellationToken);
            }
        }
    }

    public class MariagesCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: ResponseHandlers.Mariages(), cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: ResponseHandlers.Mariages(), cancellationToken: cancellationToken);
            }
        }
    }
}
