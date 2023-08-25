using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Handlers;

namespace XpAndRepBot.Commands;

public class ImageDalleCommand : ICommand
{
    [Obsolete("Obsolete")]
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message == null) return;

        var mes = update.Message.Caption ?? update.Message.Text;
        if (string.IsNullOrWhiteSpace(mes) || mes.Length <= 3) return;

        var matches = Regex.Match(mes, @"(?<=\s)\w[\w\s]*");
        if (!matches.Success) return;

        var openAiService = new OpenAIService(new OpenAiOptions { ApiKey = Constants.SshKey });

        try
        {
            var photoToSend = await DalleHandler.GenerateImage(openAiService, matches.Value);
            if (photoToSend != null)
            {
                await botClient.SendPhotoAsync(
                    chatId: update.Message.Chat.Id,
                    replyToMessageId: update.Message.MessageId,
                    photo: photoToSend,
                    cancellationToken: cancellationToken);
            }
        }
        catch
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                "Произошла ошибка. Возможно вы не ввели текст/ввели некорректный запрос.", cancellationToken);
        }
    }
}