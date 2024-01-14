using Bll.Interfaces;
using Domain.Common;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bll.Commands.StatsAndRankingsCommands;

public class TopWordsCommand(IUserLexiconService userLexiconService, ILogger<TopWordsCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            var words = await userLexiconService.GetTopWordsAsync(message.Chat.Id, 0, 50, token);

            var response = Utils.FormatTopWords(words,0);

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "backtw|0"),
                InlineKeyboardButton.WithCallbackData("Вперёд", "nexttw|0")
            });

            return new CommandResult(message.Chat.Id, new List<string> { response }, message.MessageId, new List<InlineKeyboardMarkup> { inlineKeyboard });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in TopWordsCommand");
            return null;
        }
    }
}