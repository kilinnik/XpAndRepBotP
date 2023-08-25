using System;
using System.Linq;
using XpAndRepBot.Database;

namespace XpAndRepBot.Handlers;

public class GiveRoleHandler
{
    public static string GiveRole(long userId, string role, long chatId)
    {
        using var db = new DbUsersContext();
        var user = db.Users.First(x => x.UserId == userId && x.ChatId == chatId);
        var roles = user.Roles?.Split(", ") ?? Array.Empty<string>();

        if (roles.Contains(role)) return $"{user.Name} получает роль {role}";
        var rolesList = roles.ToList();
        rolesList.Add(role);
        user.Roles = string.Join(", ", rolesList.Take(200));
        db.SaveChanges();

        return $"{user.Name} получает роль {role}";
    }
}