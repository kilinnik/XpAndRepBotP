using Bll.Interfaces;
using Domain.Models;
using Infrastructure.Interfaces;
using Serilog;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bll.Services;

public class UpdateService(
    ICommandService commandService,
    IWelcomeService welcomeService,
    ICallbackQueryHandler callbackQueryHandler,
    IUserRepository userRepository,
    IUserMessageProcessingService userMessageProcessingService
) : IUpdateService
{
    public async Task HandleUpdateAsync(Update update, CancellationToken token)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                if (
                    update.Message?.Chat.Id
                    is -1001489033044
                        or -1001412057284
                        or 1813723228
                        or 1882185833
                        or -1001182202672
                        or 859181523
                )
                {
                    await HandleMessageUpdate(update.Message, token);
                }

                break;
            case UpdateType.CallbackQuery:
                await callbackQueryHandler.HandleCallbackQueryAsync(update.CallbackQuery, token);
                break;
            case UpdateType.EditedMessage:
                break;
        }
    }

    public Task HandleErrorAsync(Exception exception)
    {
        Log.Error(exception, "An error occurred while receiving updates");
        return Task.CompletedTask;
    }

    private async Task HandleMessageUpdate(Message message, CancellationToken token)
    {
        await HandleNewChatMembersUpdate(message, token);
        var user = await GetUser(message, token);
        await commandService.ExecuteCommandAsync(message, token);
        await userMessageProcessingService.ProcessUserMessageAsync(message, user, token);
    }


    private async Task<ChatUser> GetUser(Message message, CancellationToken token)
    {
        return await userRepository.GetUserAsync(message.From.Id, message.Chat.Id, token)
               ?? await userRepository.CreateUserAsync(
                   message.From.Id,
                   message.From.FirstName,
                   message.From.Username,
                   message.Chat.Id,
                   token
               );
    }
    private async Task HandleNewChatMembersUpdate(Message message, CancellationToken token)
    {
        var newChatMembers = message.NewChatMembers;
        if (newChatMembers != null)
        {
            foreach (var newChatMember in newChatMembers)
            {
                await welcomeService.WelcomeNewMemberAsync(message.Chat.Id, newChatMember, token);
            }
        }
    }
}
