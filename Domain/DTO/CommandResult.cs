using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Domain.DTO;

public record CommandResult(
    long ChatId,
    List<string>? Texts,
    int? ReplyToMessageId = null,
    List<InlineKeyboardMarkup>? InlineKeyboards = null,
    InputFile? Photo = null,
    ParseMode? ParseMode = ParseMode.Html
);
