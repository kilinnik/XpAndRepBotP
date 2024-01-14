using Bll.Interfaces;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bll.Commands.HelpAndSupportCommands;

public class HelpCommand(ILogger<HelpCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            var inlineKeyboard = new InlineKeyboardMarkup(
                new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(
                            "🏳‍🌈 Профиль и Обратная Связь",
                            "help_profile"
                        )
                    },

                    [
                        InlineKeyboardButton.WithCallbackData(
                            "🏆 Статистика и Рейтинги",
                            "help_stats"
                        )
                    ],
                    [InlineKeyboardButton.WithCallbackData("📜 Правила и роли", "help_rules")],
                    [InlineKeyboardButton.WithCallbackData("🎮 Развлечения", "help_entertainment")],
                    [InlineKeyboardButton.WithCallbackData("💖 Отношения", "help_relationships")],
                    [InlineKeyboardButton.WithCallbackData("👮 Модерация", "help_moderation")],
                    [InlineKeyboardButton.WithCallbackData("🆘 Помощь", "help_help")],
                    [InlineKeyboardButton.WithCallbackData("Скрыть", "link_hide")]
                }
            );

            return new CommandResult(
                message.Chat.Id,
                new List<string> { "Выберите категорию команд:" },
                message.MessageId,
                new List<InlineKeyboardMarkup> { inlineKeyboard }
            );
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in HelpCommand");
            return null;
        }
    }
}
