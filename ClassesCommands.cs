using OpenAI.GPT3.Managers;
using OpenAI.GPT3;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text.RegularExpressions;
using Telegram.Bot.Types.InputFiles;
using System.Linq;
using OpenAI.GPT3.Interfaces;

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
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: Consts.HelpText, cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: Consts.HelpText, cancellationToken: cancellationToken);
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
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard, replyToMessageId: update.Message.MessageId, text: ResponseHandlers.Me(update), cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: ResponseHandlers.Me(update), cancellationToken: cancellationToken);
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
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: Consts.RulesText, cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: Consts.RulesText, cancellationToken: cancellationToken);
            }
        }
    }

    public class MessagesReputationCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: Consts.MesRepText, cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: Consts.MesRepText, cancellationToken: cancellationToken);
            }
        }
    }

    public class GamesCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: Consts.GamesText, cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: Consts.GamesText, cancellationToken: cancellationToken);
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
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard, replyToMessageId: update.Message.MessageId, text: ResponseHandlers.TopWords(0), cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard, text: ResponseHandlers.TopWords(0), cancellationToken: cancellationToken);
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
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard, replyToMessageId: update.Message.MessageId, text: ResponseHandlers.TopLexicon(0), cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard, text: ResponseHandlers.TopLexicon(0), cancellationToken: cancellationToken);
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
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: Consts.HelpGPTText, cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: Consts.HelpGPTText, cancellationToken: cancellationToken);
            }
        }
    }

    public class ImageCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var openAiService = new OpenAIService(new OpenAiOptions() { ApiKey = Consts.SSHKey });
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
            if (update.Message.From.Id == 1882185833)
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

    public class UnwarnCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message.From.Id == 1882185833)
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
