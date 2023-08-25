using System;
using System.Linq;
using XpAndRepBot.Database;

namespace XpAndRepBot.Handlers;

public class DelRoleHandler
{
    public static string DelRole(long userId, string role, long chatId)
    {
        using var db = new DbUsersContext();
        var user = db.Users.First(x => x.UserId == userId && x.ChatId == chatId);
        var roles = user.Roles?.Split(", ") ?? Array.Empty<string>();

        if (!roles.Contains(role)) return $"{user.Name} теряет роль {role}";
        var rolesList = roles.ToList();
        rolesList.Remove(role);
        user.Roles = string.Join(", ", rolesList);
        db.SaveChanges();

        return $"{user.Name} теряет роль {role}";
    }
}