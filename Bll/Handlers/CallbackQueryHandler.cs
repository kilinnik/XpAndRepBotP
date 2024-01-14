using Bll.Interfaces;
using Domain.Common;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bll.Handlers;

public class CallbackQueryHandler(
    ITelegramBotClient botClient,
    IUserRoleService userRoleService,
    IUserMarriageService userMarriageService,
    IUserLevelService userLevelService,
    ILogger<CallbackQueryHandler> logger,
    IUserProfileService userProfileService,
    IUserLexiconService userLexiconService,
    IUserReputationService userReputationService,
    IRequestProcessingService requestProcessingService
) : ICallbackQueryHandler
{
    public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken token)
    {
        var id = callbackQuery.Message.MessageId + callbackQuery.Message.Chat.Id.ToString();

        if (requestProcessingService.StartProcessing(id))
        {
            try
            {
                var action = callbackQuery.Data.Split('|')[0];

                switch (action)
                {
                    case "help_profile":
                        await UpdateMessageAsync(
                            callbackQuery,
                            "🏳‍🌈 <u><i>Профиль и Обратная Связь:</i></u>\n\n/me · /m - Показать информацию о своём профиле\n"
                            + "/report (текст) - Отправить жалобу на пользователя\n/complaints · /c - Просмотр списка жалоб на пользователя",
                            action,
                            token
                        );
                        break;
                    case "help_stats":
                        await UpdateMessageAsync(
                            callbackQuery,
                            "🏆 <u><i>Статистика и Рейтинги:</i></u>\n\n/toplevel · /toplvl · /tl - Топ пользователей по уровню\n"
                            + "/toprep · /tr - Топ пользователей по репутации\n/topwords · /tw - Самые популярные слова в чате\n"
                            + "/toplex · /tlex · /l - Топ пользователей по лексикону",
                            action,
                            token
                        );
                        break;
                    case "help_rules":
                        await UpdateMessageAsync(
                            callbackQuery,
                            "📜 <u><i>Правила и роли:</i></u>\n\n/rules · /rule · /r - Правила чата\n"
                            + "/repup · /repmsg · /rm - Сообщения, повышающие репутацию\n/roles - Роли, доступные в чате\n"
                            + "/lvlrewards · /lvlrew · /rew - Награды за достижение определённого уровня",
                            action,
                            token
                        );
                        break;
                    case "help_entertainment":
                        await UpdateMessageAsync(
                            callbackQuery,
                            "🎮 <u><i>Развлечения:</i></u>\n\n/genimg · /im (текст) - Генерация изображений на основе текста\n"
                            + "/wordusage · /word · /w (слово) - Подсчёт использования слова в чате\n/nofuckchallenge · /nfc - No Fuck Challenge (челлендж без мата)",
                            action,
                            token
                        );
                        break;
                    case "help_relationships":
                        await UpdateMessageAsync(
                            callbackQuery,
                            "💖 <u><i>Отношения:</i></u>\n\n<code>marriage</code> - Предложение брака\n<code>marriages</code> - Список всех браков в чате\n"
                            + "<code>status</code> - Проверка статуса брака\n<code>divorce</code> - Развод с партнером",
                            action,
                            token
                        );
                        break;
                    case "help_moderation":
                        await UpdateMessageAsync(
                            callbackQuery,
                            "👮 <u><i>Модерация:</i></u>\n\n/ban - Блокировка пользователя\n"
                            + "/unban · /unb - Снятие блокировки с пользователя\n/warn (в ответ) - Выдача предупреждения пользователю\n"
                            + "/unwarn · /unw - Снятие предупреждения с пользователя\n/mute ad bh cm - Мут пользователя на a дней, b часов, c минут\n"
                            + "/role (роль) - Дать роль\n/unr (роль) - Удалить роль\n/unrall · /unroleall - Удалить все роли",
                            action,
                            token
                        );
                        break;
                    case "help_help":
                        await UpdateMessageAsync(
                            callbackQuery,
                            "🆘 <u><i>Помощь:</i></u>\n\n/aihelp · /aih - Инструкции по взаимодействию с ИИ\n/help · /h - Команды бота\n/link - Ссылки чата",
                            action,
                            token
                        );
                        break;
                    case "help_back":
                        await SendHelpCategories(callbackQuery, token);
                        break;
                    case "backtl":
                    case "nexttl":
                        await HandlePaginatedCallback(
                            callbackQuery,
                            userLevelService.GetTopUsersByLevelAsync,
                            Utils.FormatTopLevelUsers,
                            50,
                            token
                        );
                        break;
                    case "backtw":
                    case "nexttw":
                        await HandlePaginatedCallback(
                            callbackQuery,
                            userLexiconService.GetTopWordsAsync,
                            Utils.FormatTopWords,
                            50,
                            token
                        );
                        break;
                    case "backtr":
                    case "nexttr":
                        await HandlePaginatedCallback(
                            callbackQuery,
                            userReputationService.GetTopUsersByReputationAsync,
                            Utils.FormatTopUsersByReputation,
                            50,
                            token
                        );
                        break;
                    case "backl":
                    case "nextl":
                        await HandlePaginatedCallback(
                            callbackQuery,
                            userLexiconService.GetTopUsersByLexiconAsync,
                            Utils.FormatTopUsersByLexicon,
                            50,
                            token
                        );
                        break;
                    case "backr":
                    case "nextr":
                        await HandlePaginatedCallback(
                            callbackQuery,
                            userRoleService.GetRolesListAsync,
                            Utils.FormatRoles,
                            20,
                            token
                        );
                        break;
                    case "backmw":
                    case "nextmw":
                        await HandleUserInfoAsync(
                            callbackQuery,
                            callbackQuery.Data.Split('|'),
                            action,
                            token
                        );
                        break;
                    case "marry_yes":
                    case "marry_no":
                        await HandleMarriageResponse(
                            callbackQuery,
                            callbackQuery.Data.Split('|'),
                            token
                        );
                        break;
                    case "link_hide":
                        await botClient.DeleteMessageAsync(
                            callbackQuery.Message.Chat.Id,
                            callbackQuery.Message.MessageId,
                            token
                        );
                        break;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in CallbackQueryHandler");
            }
            finally
            {
                requestProcessingService.FinishProcessing(id);
            }
        }
    }

    private async Task UpdateMessageAsync(
        CallbackQuery callbackQuery,
        string text,
        string currentCategory,
        CancellationToken token
    )
    {
        var categories = new List<string>
        {
            "help_profile",
            "help_stats",
            "help_rules",
            "help_entertainment",
            "help_relationships",
            "help_moderation",
            "help_help"
        };

        var currentIndex = categories.IndexOf(currentCategory);
        var prevIndex = currentIndex - 1 >= 0 ? currentIndex - 1 : categories.Count - 1;
        var nextIndex = currentIndex + 1 < categories.Count ? currentIndex + 1 : 0;

        var inlineKeyboardButtons = new List<InlineKeyboardButton[]>
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("⬅️ Назад", categories[prevIndex]),
                InlineKeyboardButton.WithCallbackData("Вперед ➡️", categories[nextIndex])
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Меню", "help_back"),
                InlineKeyboardButton.WithCallbackData("Скрыть", "link_hide")
            }
        };

        var inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboardButtons);

        await botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: text,
            replyMarkup: inlineKeyboard,
            parseMode: ParseMode.Html,
            cancellationToken: token
        );
    }

    private async Task SendHelpCategories(CallbackQuery callbackQuery, CancellationToken token)
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
                [InlineKeyboardButton.WithCallbackData("🏆 Статистика и Рейтинги", "help_stats")],
                [InlineKeyboardButton.WithCallbackData("📜 Правила и роли", "help_rules")],
                [InlineKeyboardButton.WithCallbackData("🎮 Развлечения", "help_entertainment")],
                [InlineKeyboardButton.WithCallbackData("💖 Отношения", "help_relationships")],
                [InlineKeyboardButton.WithCallbackData("👮 Модерация", "help_moderation")],
                [InlineKeyboardButton.WithCallbackData("🆘 Помощь", "help_help")],
                [InlineKeyboardButton.WithCallbackData("Скрыть", "link_hide")]
            }
        );

        await botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "Выберите категорию команд:",
            replyMarkup: inlineKeyboard,
            cancellationToken: token
        );
    }

    private async Task HandleMarriageResponse(
        CallbackQuery callbackQuery,
        IReadOnlyList<string> parts,
        CancellationToken token
    )
    {
        var proposerId = long.Parse(parts[1]);
        var proposeeId = long.Parse(parts[2]);
        var response = parts[0] == "marry_yes";

        if (callbackQuery.From.Id != proposeeId)
        {
            return;
        }

        string responseMessage;

        if (response)
        {
            responseMessage = await userMarriageService.UpdateMarriageStatus(
                callbackQuery.Message.Chat.Id,
                proposerId,
                proposeeId,
                true,
                token
            );
        }
        else
        {
            responseMessage = "Предложение брака отклонено.";
        }

        await botClient.SendTextMessageAsync(
            callbackQuery.Message.Chat.Id,
            responseMessage,
            cancellationToken: token
        );

        await botClient.DeleteMessageAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            token
        );
    }

    private async Task HandleUserInfoAsync(
        CallbackQuery callbackQuery,
        IReadOnlyList<string> parts,
        string action,
        CancellationToken token
    )
    {
        var userId = long.Parse(parts[1]);
        var offset = int.Parse(parts[2]);

        if (action is "backmw" or "nextmw")
        {
            offset += action == "backmw" ? -10 : 10;

            if (offset < 0)
                return;

            string newMessage;
            if (offset == 0)
            {
                var userInfo = await userProfileService.GetUserInfoAsync(
                    userId,
                    callbackQuery.Message.Chat.Id,
                    token
                );
                newMessage = userInfo.ToString();
            }
            else
            {
                newMessage = await userProfileService.GetUserLexiconPageAsync(
                    userId,
                    callbackQuery.Message.Chat.Id,
                    offset,
                    token
                );
            }

            var inlineKeyboard = callbackQuery.Message.ReplyMarkup;
            if (inlineKeyboard != null)
            {
                var updatedInlineKeyboard = new InlineKeyboardMarkup(
                    inlineKeyboard
                        .InlineKeyboard.Select(
                            row =>
                                row.Select(button => UpdateButtonCallbackData(button, offset))
                                    .ToList()
                        )
                        .ToList()
                );

                await botClient.EditMessageTextAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    text: newMessage,
                    replyMarkup: updatedInlineKeyboard,
                    cancellationToken: token
                );
            }
        }
    }

    private async Task HandlePaginatedCallback<T>(
        CallbackQuery callbackQuery,
        Func<long, int, int, CancellationToken, Task<IEnumerable<T>>> fetchDataFunc,
        Func<IEnumerable<T>, int, string> formatDataFunc,
        int pageSize,
        CancellationToken token
    )
    {
        var parts = callbackQuery.Data.Split('|');
        var action = parts[0];
        var chatId = callbackQuery.Message.Chat.Id;
        var currentOffset = int.Parse(parts[1]);

        var newOffset = action.StartsWith("back")
            ? currentOffset - pageSize
            : currentOffset + pageSize;
        if (newOffset < 0)
        {
            return;
        }

        var data = await fetchDataFunc(chatId, newOffset, pageSize, token);
        var newText = formatDataFunc(data, newOffset);

        var inlineKeyboard = callbackQuery.Message.ReplyMarkup;

        var updatedInlineKeyboard = new InlineKeyboardMarkup(
            inlineKeyboard
                .InlineKeyboard.Select(
                    row =>
                        row.Select(button => UpdateButtonCallbackData(button, newOffset)).ToList()
                )
                .ToList()
        );

        await botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: newText,
            replyMarkup: updatedInlineKeyboard,
            parseMode: ParseMode.Html,
            cancellationToken: token
        );
    }

    private static InlineKeyboardButton UpdateButtonCallbackData(
        InlineKeyboardButton button,
        int newOffset
    )
    {
        var lastIndexOfPipe = button.CallbackData.LastIndexOf('|');

        var baseCallbackData = button.CallbackData[..lastIndexOfPipe];
        var newCallbackData = $"{baseCallbackData}|{newOffset}";

        return InlineKeyboardButton.WithCallbackData(button.Text, newCallbackData);
    }
}