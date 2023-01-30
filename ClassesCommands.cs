using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

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
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: ResponseHandlers.Me(update), cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: ResponseHandlers.Me(update), cancellationToken: cancellationToken);
            }
        }
    }

    public class TopLvlCommand: ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: ResponseHandlers.TopLvl(), cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: ResponseHandlers.TopLvl(), cancellationToken: cancellationToken);
            }
        }
    }

    public class TopReputationCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: ResponseHandlers.TopRep(), cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: ResponseHandlers.TopRep(), cancellationToken: cancellationToken);
            }
        }
    }

    public class RulesCommand: ICommand
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

    public class MessagesReputationCommand: ICommand
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

    public class GamesCommand: ICommand
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

    public class TopWordsCommand: ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: ResponseHandlers.TopWords(), cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: ResponseHandlers.TopWords(), cancellationToken: cancellationToken);
            }
        }
    }

    public class RoflCommand: ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyToMessageId: update.Message.MessageId, text: $"🖕 {update.Message.From.FirstName} {update.Message.From.LastName} иди на хуй 🖕", cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: $"🖕 {update.Message.From.FirstName} {update.Message.From.LastName} иди на хуй 🖕", cancellationToken: cancellationToken);
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
}
