using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using XpAndRepBot.Database;
using XpAndRepBot.Database.Models;

namespace XpAndRepBot.Handlers;

public static class NewMemberHandler
{
    public static async Task NewMember(ITelegramBotClient botClient, Update update, Users user,
        CancellationToken cancellationToken)
    {
        await using var db = new DbUsersContext();
        var newMembers = update.Message?.NewChatMembers;
        if (newMembers == null) return;

        foreach (var member in newMembers)
        {
            if (user == null || member.IsBot || DateTime.Now < user.DateMute) continue;

            user.CheckEnter = false;
            await db.SaveChangesAsync(cancellationToken);

            var randomNumber = GenerateRandomNumber();
            var buttons = CreateInlineKeyboardButtons(randomNumber, member.Id);
            InlineKeyboardMarkup inlineKeyboard = new(buttons);

            await botClient.RestrictChatMemberAsync(chatId: user.ChatId, member.Id, new ChatPermissions
            {
                CanSendMessages = false,
                CanSendMediaMessages = false,
            }, cancellationToken: cancellationToken);

            await SendMessage(botClient, user, update, inlineKeyboard, randomNumber, cancellationToken);
        }
    }

    private static int GenerateRandomNumber()
    {
        Random random = new();
        return random.Next(0, 8);
    }

    private static IEnumerable<InlineKeyboardButton[]> CreateInlineKeyboardButtons(int randomNumber, long memberId)
    {
        var array = Enumerable.Range(0, 8).OrderBy(_ => Guid.NewGuid()).ToArray();
        var buttons = new InlineKeyboardButton[1][];
        buttons[0] = new InlineKeyboardButton[array.Length];

        for (var i = 0; i < array.Length; i++)
        {
            var callbackData = $"{array[i]}{(array[i] == randomNumber ? "y" : "n")}{memberId}";
            buttons[0][i] = InlineKeyboardButton.WithCallbackData(array[i].ToString(), callbackData);
        }

        return buttons;
    }

    private static async Task SendMessage(ITelegramBotClient botClient, Users user, Update update,
        IReplyMarkup inlineKeyboard, int randomNumber, CancellationToken cancellationToken)
    {
        var text =
            $"Нажми на {randomNumber}, чтобы можно было писать. Бан за неверный ответ (press {randomNumber} to be able to write. Ban for a wrong answer).";
        try
        {
            if (update.Message != null)
            {
                await botClient.SendTextMessageAsync(user.ChatId, text, cancellationToken, update.Message.MessageId,
                    markup: inlineKeyboard);
            }
        }
        catch
        {
            await botClient.SendTextMessageAsync(user.ChatId, text, cancellationToken, markup: inlineKeyboard);
        }
    }
}