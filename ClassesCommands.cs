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
            if (update?.Message?.ReplyToMessage != null && !update.Message.ReplyToMessage.From.IsBot)
            {
                try
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard, replyToMessageId: update.Message.MessageId, text: await ResponseHandlers.Me(update.Message.ReplyToMessage.From.Id), cancellationToken: cancellationToken);
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

    public class ImageCommand : ICommand
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

    public class NfcCommand : ICommand
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
}
