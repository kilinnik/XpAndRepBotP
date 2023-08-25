using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace XpAndRepBot.Handlers;

public static class DefaultCallbackQueryHandler
{
    public static async Task HandleDefaultCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery,
        CancellationToken cancellationToken)
    {
        if (callbackQuery.Message?.ReplyToMessage?.From == null) return;

        var chatId = callbackQuery.Message.Chat.Id;
        var messageId = callbackQuery.Message.MessageId;
        var userId = callbackQuery.Message.ReplyToMessage.From.Id;

        var inlineKeyboard = callbackQuery.Message.ReplyMarkup;

        if (callbackQuery.Data != null && callbackQuery.Data[0] == 'm')
        {
            inlineKeyboard = await AcceptMariageHandler.AcceptMariage(callbackQuery.Data, callbackQuery, botClient,
                chatId, userId, cancellationToken);
        }
        else
        {
            var flag = await GetChatPermissionsHandler.GetChatPermissions(callbackQuery.Data, callbackQuery, botClient,
                chatId, cancellationToken);
            if (flag)
            {
                try
                {
                    await botClient.DeleteMessageAsync(callbackQuery.Message, cancellationToken);
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        await botClient.EditMessageTextAsync(callbackQuery.Message, callbackQuery.Message.Text, inlineKeyboard,
            cancellationToken: cancellationToken);
    }
}