using Bll.Interfaces;
using Domain.Common;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bll.Commands.StatsAndRankingsCommands;

public class TopLexiconCommand(IUserLexiconService userLexiconService, ILogger<TopLexiconCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            var users = await userLexiconService.GetTopUsersByLexiconAsync(message.Chat.Id, 0, 50, token);

            var response = Utils.FormatTopUsersByLexicon(users, 0);

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "backl|0"),
                InlineKeyboardButton.WithCallbackData("Вперёд", "nextl|0")
            });

            return new CommandResult(message.Chat.Id, new List<string> { response }, message.MessageId,
                new List<InlineKeyboardMarkup> { inlineKeyboard });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in TopLexiconCommand");
            return null;
        }
    }
}