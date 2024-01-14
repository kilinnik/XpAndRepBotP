using Bll.Interfaces;
using Domain.DTO;
using Infrastructure.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bll.Strategies;

public class LinkMessageStrategy(IChatRepository chatRepository) : IUserMessageStrategy
{
    public async Task<CommandResult?> ExecuteAsync(
        Message message,
        CancellationToken token
    )
    {
        var chatSettings = await chatRepository.GetChatByIdAsync(message.Chat.Id, token);

        if (chatSettings is not { IsLinkMessageEnabled: true } || message.From.Id != 777000)
        {
            return null;
        }

        var responseTexts = new List<string> { chatSettings.InviteLinkMessageText };

        var button = InlineKeyboardButton.WithUrl(
            chatSettings.LinkButtonText,
            chatSettings.LinkUrl
        );
        var inlineKeyboards = new InlineKeyboardMarkup(button);

        return new CommandResult(
            message.Chat.Id,
            responseTexts,
            message.MessageId,
            new List<InlineKeyboardMarkup> { inlineKeyboards }
        );
    }
}
