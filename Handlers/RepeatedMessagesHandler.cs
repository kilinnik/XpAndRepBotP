using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Database.Models;

namespace XpAndRepBot.Handlers;

public static class RepeatedMessagesHandler
{
    public static async Task HandleRepeatedMessages(ITelegramBotClient botClient, Update update, Users user,
        CancellationToken cancellationToken)
    {
        var messageText = Utilities.GetMessageText(update);
        if (messageText.Length > 50)
        {
            messageText = messageText[..50];
        }

        if (user.LastMessage == messageText)
        {
            user.CountRepeatMessage++;
        }
        else
        {
            user.LastMessage = messageText;
            user.CountRepeatMessage = 1;
        }

        if (user.CountRepeatMessage > 3)
        {
            if (update.Message != null)
            {
                try
                {
                    await botClient.DeleteMessageAsync(update.Message, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}