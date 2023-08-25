using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XpAndRepBot.Database;
using XpAndRepBot.Database.Models;

namespace XpAndRepBot.Commands;

public class BanCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message?.From == null) return;

        var chatMember =
            await botClient.GetChatMemberAsync(update.Message.Chat.Id, update.Message.From.Id, cancellationToken);
        await using var db = new DbUsersContext();
        var user = db.Users.FirstOrDefault(x => x.UserId == update.Message.From.Id && x.Roles.Contains("модер"));

        if (IsValidBanRequest(update, chatMember, user))
        {
            await BanUser(botClient, update, cancellationToken);
        }
        else
        {
            await botClient.DeleteMessageAsync(update.Message, cancellationToken);
        }
    }

    private static bool IsValidBanRequest(Update update, ChatMember chatMember, Users user)
    {
        return update.Message is { From: not null, ReplyToMessage.From: not null }
               && update.Message.From.Id != update.Message.ReplyToMessage.From.Id
               && (user != null || (IsAdminOrCreator(chatMember) && update.Message.Chat.Id == Constants.NitokinChatId));
    }

    private static bool IsAdminOrCreator(ChatMember chatMember)
    {
        return chatMember.Status is ChatMemberStatus.Administrator or ChatMemberStatus.Creator;
    }

    private static async Task BanUser(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.Message is { ReplyToMessage: not null })
            {
                if (update.Message.ReplyToMessage.From != null)
                {
                    await botClient.BanChatMemberAsync(
                        chatId: update.Message.Chat.Id,
                        userId: update.Message.ReplyToMessage.From.Id,
                        cancellationToken: cancellationToken);
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                        $"{update.Message.ReplyToMessage.From.FirstName} забанен", cancellationToken,
                        update.Message.ReplyToMessage.MessageId);
                    ;
                }

                await botClient.DeleteMessageAsync(update.Message, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}