using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Mirror.ChatGpt.Models.ChatGpt;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Commands;

namespace XpAndRepBot.Handlers;

public static class ChatGptHandler
{
    public static async Task HandleChatGpt(ITelegramBotClient botClient, Update update, string messageText,
        Dictionary<string, ICommand> commands, CancellationToken cancellationToken)
    {
        if (update.Message != null)
        {
            var chatId = update.Message.Chat.Id;
            if (!commands.Keys.Any(messageText.StartsWith) &&
                (update.Message?.ReplyToMessage is { From.Id: Constants.BotId } ||
                 messageText.Contains("@XpAndRepBot") || chatId == Constants.Mid || chatId == Constants.Iid))
            {
                await RequestChatGpt(botClient, update, messageText, cancellationToken);
            }
        }
    }

    private static async Task RequestChatGpt(ITelegramBotClient botClient, Update update, string mes,
        CancellationToken cancellationToken)
    {
        if (ShouldProcessMessage(update, mes))
        {
            ProcessMessageContext(update);
        }

        mes = mes.Replace("@XpAndRepBot", "");
        if (IsValidMessage(mes) && update.Message != null)
        {
            var id = GetMessageId(update);
            var subArray = GetSubArray(id, mes);

            if (subArray[0] != null)
            {
                try
                {
                    await SendMessage(botClient, update, id, subArray, cancellationToken);
                }
                catch
                {
                    await HandleError(botClient, update, cancellationToken);
                }
            }
        }
    }

    private static bool ShouldProcessMessage(Update update, string mes)
    {
        return mes.Contains("@XpAndRepBot") ||
               (update.Message?.Chat.Id == Constants.Mid && update.Message.ReplyToMessage == null) ||
               (update.Message?.Chat.Id == Constants.Iid && update.Message.ReplyToMessage == null);
    }

    private static void ProcessMessageContext(Update update)
    {
        if (update.Message == null) return;
        Program.Context.Add(update.Message.MessageId, new MessageEntry[100]);
        Program.ListBranches.Add(new List<int> { update.Message.MessageId });
    }

    private static bool IsValidMessage(string mes)
    {
        return mes.Any(char.IsLetter) || mes.Any(char.IsNumber) || mes.Any(char.IsPunctuation);
    }

    private static int GetMessageId(Update update)
    {
        if (update.Message == null) return 0;
        var id = update.Message.MessageId;
        if (update.Message.ReplyToMessage?.Text == null) return id;
        var match = Regex.Match(update.Message.ReplyToMessage.Text, @"\b\d+\b$");
        if (!match.Success) return id;
        var number = int.Parse(match.Value);
        var targetList = Program.ListBranches.Find(list => list.Contains(number));
        targetList?.Add(update.Message.MessageId);
        if (targetList is { Count: > 0 })
        {
            id = targetList[0];
        }

        return id;
    }

    private static MessageEntry[] GetSubArray(int id, string mes)
    {
        if (!Program.Context.TryGetValue(id, out var array)) return Array.Empty<MessageEntry>();

        var index = Array.IndexOf(array, null);

        if (index == 0 && mes.Contains('\n'))
        {
            var splitMessage = mes.Split(new[] { '\n' }, 2);
            array[index] = new MessageEntry { Role = Roles.System, Content = splitMessage[0] };
            if (splitMessage.Length > 1)
            {
                index++;
                if (index < array.Length)
                {
                    array[index] = new MessageEntry { Role = Roles.User, Content = splitMessage[1] };
                }
                else
                {
                    Array.Resize(ref array, array.Length + 1);
                    array[index] = new MessageEntry { Role = Roles.User, Content = splitMessage[1] };
                }
            }
        }
        else
        {
            array[index] = new MessageEntry { Role = Roles.User, Content = mes };
        }

        Program.Context[id] = array;
        return new ArraySegment<MessageEntry>(array, 0, index + 1).ToArray();
    }

    private static async Task SendMessage(ITelegramBotClient botClient, Update update, int id,
        MessageEntry[] subArray, CancellationToken cancellationToken)
    {
        if (update.Message != null)
        {
            try
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, await RequestChatGpt(id, subArray),
                    cancellationToken: cancellationToken, update.Message.MessageId);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, await RequestChatGpt(id, subArray),
                    cancellationToken: cancellationToken);
            }
        }
    }

    private static async Task<string> RequestChatGpt(int id, MessageEntry[] messages)
    {
        var res = await Program.Service.ChatAsync(new ChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = messages
        }, default);

        if (!Program.Context.TryGetValue(id, out var array))
            return res.Choices[0].Message.Content + "\n" + id;

        var index = Array.IndexOf(array, null);
        array[index] = new MessageEntry
            { Role = res.Choices[0].Message.Role, Content = res.Choices[0].Message.Content };
        Program.Context[id] = array;

        return res.Choices[0].Message.Content + "\n" + id;
    }

    private static async Task HandleError(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        const string errorMessage =
            "Произошла ошибка. Попробуйте повторить запрос. Чтобы начать новую ветку, упомяните бота. " +
            "Чтобы продолжить ветку, ответьте на сообщение от бота, где есть число в конце";
        if (update.Message != null)
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, errorMessage,
                cancellationToken: cancellationToken);
        }
    }
}