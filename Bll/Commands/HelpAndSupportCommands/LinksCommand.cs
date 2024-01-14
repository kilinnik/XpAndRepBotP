using Bll.Interfaces;
using Domain.DTO;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bll.Commands.HelpAndSupportCommands;

public class LinksCommand(IChatRepository chatRepository, ILogger<LinksCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            var chatSettings = await chatRepository.GetChatByIdAsync(message.Chat.Id, token);
            if (chatSettings == null)
                return null;

            var buttonNames = chatSettings.LinkButtonNames?.Split('\n') ?? Array.Empty<string>();
            var urls = chatSettings.LinkUrls?.Split('\n') ?? Array.Empty<string>();

            if (buttonNames.Length != urls.Length)
            {
                logger.LogError("Mismatch in button names and URL counts");
                return null;
            }

            var totalButtons = buttonNames.Length + 1;  
            var inlineKeyboardButtons = new List<InlineKeyboardButton[]>();

            if (totalButtons <= 11)
            {
                inlineKeyboardButtons.AddRange(
                    buttonNames.Zip(urls, (name, url) => new[] { InlineKeyboardButton.WithUrl(name, url) }));
                inlineKeyboardButtons.Add([InlineKeyboardButton.WithCallbackData("Скрыть", "link_hide")]);
            }
            else
            {
                var half = (totalButtons - 1) / 2;
                for (var i = 0; i < half; i++)
                {
                    inlineKeyboardButtons.Add(
                        new[]
                        {
                            InlineKeyboardButton.WithUrl(buttonNames[i], urls[i]),
                            InlineKeyboardButton.WithUrl(buttonNames[i + half], urls[i + half])
                        }
                    );
                }

                if (totalButtons % 2 == 0)
                {
                    inlineKeyboardButtons.Add(
                        new[]
                        {
                            InlineKeyboardButton.WithUrl(buttonNames[^1], urls[^1]),
                            InlineKeyboardButton.WithCallbackData("Скрыть", "link_hide")
                        }
                    );
                }
                else
                {
                    inlineKeyboardButtons.Add(
                        [InlineKeyboardButton.WithUrl(buttonNames[^1], urls[^1])]
                    );
                    inlineKeyboardButtons.Add(
                        [InlineKeyboardButton.WithCallbackData("Скрыть", "link_hide")]
                    );
                }
            }

            var inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboardButtons);

            return new CommandResult(
                message.Chat.Id,
                new List<string> { chatSettings.LinkMessageText ?? string.Empty },
                message.MessageId,
                new List<InlineKeyboardMarkup> { inlineKeyboard }
            );
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in ChatGameLinksCommand");
            return null;
        }
    }
}
