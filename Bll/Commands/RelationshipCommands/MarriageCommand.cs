using Bll.Interfaces;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bll.Commands.RelationshipCommands;

public class MarriageCommand(IUserMarriageService userMarriageService, ILogger<MarriageCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            if (message.ReplyToMessage == null || message.ReplyToMessage.From.IsBot)
            {
                return new CommandResult(message.Chat.Id, new List<string> { "Ответьте на предложение" }, message.MessageId);
            }

            var proposerId = message.From.Id;
            var proposeeId = message.ReplyToMessage.From.Id;

            var statusMessage =
                await userMarriageService.CheckMarriageStatus(proposerId, proposeeId, message.Chat.Id, token);
            if (!string.IsNullOrEmpty(statusMessage))
            {
                return new CommandResult(message.Chat.Id, new List<string> { statusMessage }, message.MessageId);
            }

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("Да", $"marry_yes|{proposerId}|{proposeeId}"),
                InlineKeyboardButton.WithCallbackData("Нет", $"marry_no|{proposerId}|{proposeeId}")
            });

            var responseMessage =
                $"💖 {message.From.FirstName} делает вам предложение. Согласны ли вы вступить в брак?";

            return new CommandResult(message.Chat.Id, new List<string> { responseMessage },
                message.ReplyToMessage.MessageId, new List<InlineKeyboardMarkup> { inlineKeyboard });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in MarriageCommand");
            return null;
        }
    }
}