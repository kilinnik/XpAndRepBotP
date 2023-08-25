using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XpAndRepBot.Commands;
using XpAndRepBot.Database;
using XpAndRepBot.Database.Models;

namespace XpAndRepBot.Handlers;

public static class CommandsHandler
{
    public static async Task HandleCommands(ITelegramBotClient botClient, Update update,
        Dictionary<string, ICommand> commands,
        CancellationToken cancellationToken)
    {
        var messageText = Utilities.GetMessageText(update);
        var message = update.Message;

        try
        {
            if (message is { From: not null })
            {
                var chatMember = await botClient.GetChatMemberAsync(message.Chat.Id,
                    message.From.Id, cancellationToken);


                await using var db = new DbUsersContext();
                if (message.From is { Id: 777000 } && message.Chat.Id == Constants.IgruhaChatId)
                {
                    db.MessageIdsForDeletion.Add(new MessageIdsForDelete(message.MessageId,
                        message.MessageId.ToString(), message.Chat.Id));
                    await db.SaveChangesAsync(cancellationToken);
                }

                var flagForDelete = message.ReplyToMessage != null &&
                                    Utilities.ContainsMessageId(db, message.ReplyToMessage.MessageId);
                var match = Regex.Match(messageText, @"^.*?([\w/]+)");
                if (commands.ContainsKey(match.Value.ToLower()))
                {
                    if (chatMember.Status is not (ChatMemberStatus.Administrator or ChatMemberStatus.Creator) &&
                        flagForDelete)
                    {
                        await botClient.DeleteMessageAsync(message, cancellationToken);
                    }
                    else
                    {
                        var command = commands[match.Value.ToLower()];
                        await command.ExecuteAsync(botClient, update, cancellationToken);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}