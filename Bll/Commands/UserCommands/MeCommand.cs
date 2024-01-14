using Bll.Interfaces;
using Domain.DTO;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bll.Commands.UserCommands;

public class MeCommand(
    IUserRepository userRepository,
    IUserProfileService userProfileService,
    ILogger<MeCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            var targetUserIds = new List<long>();

            if (message.ReplyToMessage != null)
            {
                targetUserIds.Add(message.ReplyToMessage.From.Id);
            }
            else if (message.Entities != null)
            {
                foreach (var entity in message.Entities.Where(e => e.Type == MessageEntityType.Mention))
                {
                    var username = message.Text.Substring(entity.Offset + 1, entity.Length - 1);
                    var userId = await userRepository.GetUserIdByUsernameAsync(username, message.Chat.Id, token);
                    if (userId.HasValue)
                    {
                        targetUserIds.Add(userId.Value);
                    }
                }
            }

            if (targetUserIds.Count == 0)
            {
                targetUserIds.Add(message.From.Id);
            }

            var responseMessages = new List<string>();
            var inlineKeyboards = new List<InlineKeyboardMarkup>();
            foreach (var userId in targetUserIds)
            {
                var userInfo = await userProfileService.GetUserInfoAsync(userId, message.Chat.Id, token);
                var responseMessage = userInfo.ToString();
                responseMessages.Add(responseMessage);

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", $"backmw|{userId}|0"),
                    InlineKeyboardButton.WithCallbackData("Вперёд", $"nextmw|{userId}|0")
                });
                inlineKeyboards.Add(inlineKeyboard);
            }

            return new CommandResult(message.Chat.Id, responseMessages, message.MessageId, inlineKeyboards);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in MeCommand");
            return null;
        }
    }
}