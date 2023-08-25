﻿using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace XpAndRepBot.Commands;

public class GamesCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message != null)
        {
            try
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, Constants.GamesText, cancellationToken,
                    update.Message.MessageId);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, Constants.GamesText, cancellationToken);
            }
        }
    }
}