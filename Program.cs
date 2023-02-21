using System;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using XpAndRepBot;

var botClient = new TelegramBotClient(Consts.TgAPIKey);

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, _) => cts.Cancel(); // Чтобы отловить нажатие ctrl+C и всякие sigterm, sigkill, etc

var handler = new UpdateHandler();
var receiverOptions = new ReceiverOptions();
botClient.StartReceiving(handler, receiverOptions, cancellationToken: cts.Token);
var timer = new System.Timers.Timer
{
    Interval = 1000 // Проверяем время каждую секунду
};
timer.Elapsed += async (sender, e) => await CheckTime();
timer.Start();
Console.WriteLine("Bot started. Press ^C to stop");
await Task.Delay(-1, cancellationToken: cts.Token); // Такой вариант советуют MS: https://github.com/dotnet/runtime/issues/28510#issuecomment-458139641
Console.WriteLine("Bot stopped");

async Task CheckTime()
{
    var now = DateTime.Now;
    if (now.Hour == 2 && now.Minute == 0 && now.Second == 0) // Проверяем, что сейчас 2:00:00
    {
        await botClient.SendTextMessageAsync(-1001489033044, "Слеер душнила");
    }
}
