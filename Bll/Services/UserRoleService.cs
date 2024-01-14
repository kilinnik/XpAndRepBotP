using System.Text;
using Bll.Interfaces;
using Domain.DTO;
using Domain.Models;
using Infrastructure.Interfaces;
using Telegram.Bot.Types;

namespace Bll.Services;

public class UserRoleService(IUserRoleRepository userRoleRepository) : IUserRoleService
{
    public async Task<UserRole?> GetUserRoleAsync(long userId, long chatId, CancellationToken token)
    {
        return await userRoleRepository.GetUserRoleAsync(userId, chatId, token);
    }

    public async Task<IEnumerable<RoleDto>> GetRolesListAsync(
        long chatId,
        int skip,
        int take,
        CancellationToken token
    )
    {
        return await userRoleRepository.GetRolesListAsync(chatId, skip, take, token);
    }

    
    public async Task<string> UpdateUserRoleAsync(
        long userId,
        long chatId,
        string firstName,
        string role,
        CancellationToken token
    )
    {
        return await userRoleRepository.UpdateUserRoleAsync(userId, chatId, firstName, role, token);
    }
    
    public async Task<string> RemoveAllRolesAsync(long userId, long chatId, CancellationToken token)
    {
        return await userRoleRepository.RemoveAllRolesAsync(userId, chatId, token);
    }

    public async Task<string> RemoveRoleAsync(
        long userId,
        long chatId,
        string role,
        CancellationToken token
    )
    {
        return await userRoleRepository.RemoveRoleAsync(userId, chatId, role, token);
    }

    public async Task<CommandResult> HandleMentionsAsync(Message message, CancellationToken token)
    {
        var messageText = message?.Text ?? message?.Caption;
        if (
            message == null
            || string.IsNullOrWhiteSpace(messageText)
            || !messageText.StartsWith('@')
        )
        {
            return new CommandResult(message.Chat.Id, new List<string>());
        }

        var mentionedRole = messageText.Split(' ').FirstOrDefault()?[1..];
        if (string.IsNullOrWhiteSpace(mentionedRole))
        {
            return new CommandResult(message.Chat.Id, new List<string>());
        }

        var users = await userRoleRepository.GetUsersByRoleAsync(
            message.Chat.Id,
            mentionedRole,
            token
        );
        
        if (users.Count == 0)
        {
            return new CommandResult(message.Chat.Id, new List<string>());
        }

        var responseMessages = BuildMentionMessages(message.From.FirstName, users);
        return new CommandResult(message.Chat.Id, responseMessages);
    }

    private static List<string> BuildMentionMessages(string userName, IEnumerable<UserRole> users)
    {
        const int maxUsersPerMessage = 5;
        var messages = new List<string>();

        var userGroups = users
            .Select((user, index) => new { user, index })
            .GroupBy(x => x.index / maxUsersPerMessage)
            .Select(group => group.Select(x => x.user));

        foreach (var group in userGroups)
        {
            var messageBuilder = new StringBuilder();
            foreach (var user in group)
            {
                messageBuilder.Append($"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, ");
            }

            messageBuilder.Length -= 2;

            if (messageBuilder.Length > 0)
            {
                messages.Add($"{userName} призывает " + messageBuilder);
            }
        }

        return messages;
    }
}
