using Junaid.GoogleGemini.Net.Models.Requests;
using Telegram.Bot.Types;

namespace Infrastructure.Interfaces;

public interface IChatThreadService
{
    bool IsUserMessageReplyToBot(long chatId, int messageId);

    void AddMessageAndIdToThread(
        string threadKey,
        string message,
        int messageId,
        bool isBotResponse = false
    );

    string GetThreadKey(Message message);

    MessageObject[] GetThreadHistory(string threadKey);
}
