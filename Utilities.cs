using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using XpAndRepBot.Database;
using XpAndRepBot.Database.Models;

namespace XpAndRepBot;

public static class Utilities
{
    public static async Task DeleteMessageAsync(this ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        if (message == null) return;

        try
        {
            await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting message: {ex.Message}");
        }
    }

    public static async Task SendTextMessageAsync(this ITelegramBotClient botClient, long chatId, string messageText,
        CancellationToken cancellationToken, int? replyToMessageId = null, ParseMode? parseMode = null,
        IReplyMarkup markup = null)
    {
        try
        {
            await botClient.SendTextMessageAsync(
                chatId, 
                messageText,
                parseMode: parseMode, 
                replyMarkup: markup,
                replyToMessageId: replyToMessageId, 
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    public static async Task EditMessageTextAsync(this ITelegramBotClient botClient, Message message, string text,
        InlineKeyboardMarkup markup, ParseMode? parseMode = null, CancellationToken cancellationToken = default)
    {
        if (message == null) return;

        if (ShouldEditMessage(text, message, markup))
        {
            await botClient.EditMessageTextAsync(
                chatId: message.Chat.Id,
                replyMarkup: markup,
                messageId: message.MessageId,
                text: text,
                parseMode: parseMode,
                cancellationToken: cancellationToken);
        }
    }

    private static bool ShouldEditMessage(string newText, Message message,
        InlineKeyboardMarkup inlineKeyboard)
    {
        return message != null && newText.Length > 0 &&
               (newText != message.Text || AreInlineKeyboardsEqual(inlineKeyboard, message.ReplyMarkup));
    }

    public static bool ContainsMessageId(DbUsersContext db, long targetMessageId)
    {
        return db.MessageIdsForDeletion.Any(row => row.MessageIds.Contains(targetMessageId.ToString()));
    }

    public static string GetMessageText(Update update)
    {
        if (update.Message != null) return update.Message.Caption ?? update.Message.Text;
        return null;
    }

    public static InlineKeyboardMarkup CreateInlineKeyboard(string backOption, string nextOption)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", backOption),
                InlineKeyboardButton.WithCallbackData("Вперёд", nextOption),
            }
        });
    }

    private static bool AreInlineKeyboardsEqual(InlineKeyboardMarkup keyboard1, InlineKeyboardMarkup keyboard2)
    {
        if (keyboard1 == null) return true;
        var buttons1 = keyboard1.InlineKeyboard.SelectMany(x => x).ToList();
        var buttons2 = keyboard2.InlineKeyboard.SelectMany(x => x).ToList();

        if (buttons1.Count != buttons2.Count)
        {
            return false;
        }

        return !buttons1.Where((t, i) => t.CallbackData != buttons2[i].CallbackData).Any();
    }

    public static int EmojiToNumber(string emoji)
    {
        var emojiDigits = new List<string>();
        for (var i = 0; i < emoji.Length; i += 3)
        {
            emojiDigits.Add(emoji.Substring(i, 3));
        }

        var digits = emojiDigits.Select(e => Constants.KeycodeKeymaps.IndexOf(e)).ToList();
        return int.Parse(string.Join("", digits));
    }

    public static Match GetMatchFromMessage(Message message, string pattern,
        RegexOptions options = RegexOptions.None)
    {
        return message.Text != null ? Regex.Match(message.Text, pattern, options) : null;
    }

    public static string NumberToEmoji(int number)
    {
        if (number <= 10)
        {
            return Constants.KeycodeKeymaps[number];
        }

        var digits = number.ToString().Select(d => int.Parse(d.ToString())).ToList();
        return string.Join("", digits.Select(d => Constants.KeycodeKeymaps[d]));
    }

    private static int PlaceByCriteria(long idUser, IQueryable<Users> tableUsers, long chatId,
        Func<Users, int> criteria)
    {
        var users = tableUsers.Where(x => x.ChatId == chatId).AsEnumerable().OrderByDescending(criteria).ToList();
        return users.IndexOf(users.FirstOrDefault(x => x.UserId == idUser)) + 1;
    }

    public static int PlaceLvl(long idUser, DbSet<Users> tableUsers, long chatId)
    {
        return PlaceByCriteria(idUser, tableUsers, chatId, u => u.Lvl);
    }

    public static int PlaceRep(long idUser, DbSet<Users> tableUsers, long chatId)
    {
        return PlaceByCriteria(idUser, tableUsers, chatId, u => u.Rep);
    }

    public static async Task<int> PlaceLexicon(Users user, long chatId)
    {
        await using SqlConnection connection = new(Constants.ConnectionString);
        await connection.OpenAsync();
        SqlCommand command = new(
            "SELECT RowNumber from (SELECT ROW_NUMBER() OVER (ORDER BY Count(*) DESC) AS RowNumber, UserID, " +
            "COUNT(*) AS UserCount FROM dbo.UserLexicons WHERE [ChatId] = @chatId GROUP BY UserID) AS T WHERE UserID = @userId",
            connection);

        command.Parameters.AddWithValue("@chatId", chatId);
        command.Parameters.AddWithValue("@userId", user.UserId);

        var reader = await command.ExecuteReaderAsync();
        var position = 1;
        while (await reader.ReadAsync())
        {
            position = (int)reader.GetInt64(0);
        }

        await reader.CloseAsync();
        return position;
    }

    public static int GenerateXpForLevel(int x)
    {
        return Constants.XpForLvlUp[x];
    }
}