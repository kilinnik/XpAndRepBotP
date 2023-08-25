using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Handlers;

namespace XpAndRepBot.Commands;

public class WordCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message == null) return;

        var mes = update.Message.Text;
        if (mes == "/w")
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Вы не написали слово", cancellationToken);
            return;
        }

        mes = mes?.Replace("/w ", "");
        string responseText;

        if (update.Message.ReplyToMessage is { From.IsBot: false } && update.Message.ReplyToMessage.From.Id != 777000)
        {
            responseText =
                await PersonalWordHandler.PersonalWord(update.Message.ReplyToMessage.From.Id, mes,
                    update.Message.Chat.Id);
        }
        else
        {
            responseText = await WordHandler.Word(mes, update.Message.Chat.Id);
        }

        try
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id,responseText,cancellationToken,
                update.Message.MessageId);
        }
        catch
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, responseText, cancellationToken);
        }
    }
}