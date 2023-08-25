using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XpAndRepBot.Database;
using XpAndRepBot.Database.Models;
using Chat = XpAndRepBot.Database.Models.Chat;

namespace XpAndRepBot.Commands;

public class HelpAdminCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message?.From == null) return;

        var chatMember =
            await botClient.GetChatMemberAsync(update.Message.Chat.Id, update.Message.From.Id, cancellationToken);
        await using var db = new DbUsersContext();
        var user = db.Users.FirstOrDefault(x =>
            x.UserId == update.Message.From.Id && x.ChatId == update.Message.Chat.Id && x.Roles.Contains("модер"));
        var chat = db.Chats.FirstOrDefault(x => x.ChatId == update.Message.Chat.Id);

        if (IsUserAdminOrModer(user, chatMember, update.Message.Chat.Id))
        {
            await SendHelpAdminCommands(botClient, update, chat, cancellationToken);
        }
        else
        {
            await botClient.DeleteMessageAsync(update.Message, cancellationToken);
        }
    }

    private static bool IsUserAdminOrModer(Users user, ChatMember chatMember, long chatId)
    {
        return user != null || (chatMember.Status is ChatMemberStatus.Administrator or ChatMemberStatus.Creator &&
                                chatId == Constants.NitokinChatId);
    }

    private static async Task SendHelpAdminCommands(ITelegramBotClient botClient, Update update, Chat chat,
        CancellationToken cancellationToken)
    {
        if (chat == null) return;

        if (update.Message != null)
        {
            try
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, chat.HelpAdminCommands, cancellationToken,
                    update.Message.MessageId);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, chat.HelpAdminCommands, cancellationToken);
            }
        }
    }
}