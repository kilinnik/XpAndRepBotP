using Bll.Interfaces;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Bll.Commands.UserCommands;

public class ReportUserCommand(IUserComplaintService userComplaintService, ILogger<ReportUserCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            if (message.ReplyToMessage?.From == null)
            {
                return new CommandResult(message.Chat.Id, new List<string> { Resources.ReplyToMsg }, message.MessageId);
            }

            var reportedUserId = message.ReplyToMessage.From.Id;

            var complainantUserId = message.From.Id;
            var messageText = message.Caption ?? message.Text ?? string.Empty;
            var complaintText = messageText.Length > 8 ? messageText[8..] : string.Empty;

            var result = await userComplaintService.ReportUserAsync(complainantUserId, reportedUserId,
                complaintText,
                message.Chat.Id, token);

            var responseMessage = result
                ? "Ваша жалоба принята"
                : "Ваша жалоба отклонена, вы уже жаловались на этого пользователя";

            return new CommandResult(message.Chat.Id, new List<string> { responseMessage }, message.MessageId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in ReportUserCommand");
            return null;
        }
    }
}