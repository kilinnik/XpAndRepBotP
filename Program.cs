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

Console.WriteLine("Bot started. Press ^C to stop");
await Task.Delay(-1, cancellationToken: cts.Token); // Такой вариант советуют MS: https://github.com/dotnet/runtime/issues/28510#issuecomment-458139641
Console.WriteLine("Bot stopped");
