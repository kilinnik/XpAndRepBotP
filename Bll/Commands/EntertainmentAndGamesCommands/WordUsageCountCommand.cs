using Bll.Interfaces;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Bll.Commands.EntertainmentAndGamesCommands;

public class WordUsageCountCommand(
    IUserLexiconService userLexiconService,
    ILogger<WordUsageCountCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message,
        CancellationToken token)
    {
        try
        {
            if (message.Text == "/w")
            {
                return new CommandResult(message.Chat.Id, new List<string> { "Вы не указали слово" }, message.MessageId);
            }

            var word = message.Text.Replace("/w ", "").ToLower();
            string responseText;

            if (message.ReplyToMessage?.From is { IsBot: false })
            {
                var wordDto = await userLexiconService.GetWordUsageAsync(
                    message.ReplyToMessage.From.Id, message.Chat.Id, word, token);

                responseText = wordDto != null
                    ? $"✍🏿 {message.ReplyToMessage.From.Username} употреблял слово {word} {wordDto.WordCount} раз. Оно занимает {wordDto.RowNumber} место по частоте употребления"
                    : $"Слово {word} ни разу не употреблялось пользователем {message.ReplyToMessage.From.Username}. Если что слово состоит из одного слова.";
            }
            else
            {
                var wordDto = await userLexiconService.GetSpecificWordUsageAsync(message.Chat.Id, word, token);

                responseText = wordDto != null
                    ? $"✍🏿 Слово {word} употреблялось {wordDto.WordCount} раз. Оно занимает {wordDto.RowNumber} место по частоте употребления"
                    : $"Слово {word} ни разу не употреблялось. Если что слово состоит из одного слова.";
            }

            return new CommandResult(message.Chat.Id, new List<string> { responseText }, message.MessageId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in WordUsageCountCommand");
            return null;
        }
    }
}