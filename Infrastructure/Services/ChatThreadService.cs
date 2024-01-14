using System.Collections.Concurrent;
using Infrastructure.Interfaces;
using Junaid.GoogleGemini.Net.Models.Requests;
using Telegram.Bot.Types;

namespace Infrastructure.Services;

public class ChatThreadService : IChatThreadService
{
    private readonly ConcurrentDictionary<string, List<string>> _threads = new();
    private readonly ConcurrentDictionary<long, HashSet<int>> _botMessageIds = new();
    private readonly ConcurrentDictionary<string, List<int>> _threadMessageIds = new();

    public bool IsUserMessageReplyToBot(long chatId, int messageId)
    {
        return _botMessageIds.TryGetValue(chatId, out var messageIds)
            && messageIds.Contains(messageId);
    }

    public void AddMessageAndIdToThread(
        string threadKey,
        string message,
        int messageId,
        bool isBotResponse = false
    )
    {
        AddMessageToThread(threadKey, message, messageId, isBotResponse);
        AddMessageIdToThread(threadKey, messageId);
    }

    public string GetThreadKey(Message message)
    {
        var chatId = message.Chat.Id;
        if (message.ReplyToMessage != null)
        {
            var replyToMessageId = message.ReplyToMessage.MessageId;

            foreach (var threadPair in _threadMessageIds)
            {
                if (threadPair.Value.Contains(replyToMessageId))
                {
                    return $"{chatId}:{threadPair.Value.First()}";
                }
            }
        }

        return $"{chatId}:{message.MessageId}";
    }

    public MessageObject[] GetThreadHistory(string threadKey)
    {
        if (_threads.TryGetValue(threadKey, out var thread))
        {
            return thread
                .Select(msg =>
                {
                    var role = msg.StartsWith("[Bot]:") ? "model" : "user";
                    var text = msg.Replace("[Bot]:", "").Trim();
                    return new MessageObject(role, text);
                })
                .ToArray();
        }

        return Array.Empty<MessageObject>();
    }

    private void AddMessageToThread(
        string threadKey,
        string message,
        int messageId,
        bool isBotResponse = false
    )
    {
        if (!_threads.TryGetValue(threadKey, out var thread))
        {
            thread = [];
            _threads[threadKey] = thread;
        }

        var formattedMessage = isBotResponse ? $"[Bot]: {message}" : message;
        thread.Add(formattedMessage);

        if (!isBotResponse)
            return;
        var chatId = ExtractChatIdFromThreadKey(threadKey);
        if (!_botMessageIds.TryGetValue(chatId, out var botMessageIds))
        {
            botMessageIds = [];
            _botMessageIds[chatId] = botMessageIds;
        }

        botMessageIds.Add(messageId);
    }

    private void AddMessageIdToThread(string threadKey, int messageId)
    {
        if (!_threadMessageIds.TryGetValue(threadKey, out var messageIds))
        {
            messageIds = [];
            _threadMessageIds[threadKey] = messageIds;
        }

        messageIds.Add(messageId);
    }

    private static long ExtractChatIdFromThreadKey(string threadKey)
    {
        var parts = threadKey.Split(':');
        return long.Parse(parts[0]);
    }
}
