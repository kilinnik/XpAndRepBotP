using System;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Mirror.ChatGpt.Models.ChatGpt;
using System.Collections.Generic;

namespace XpAndRepBot
{
    public static class Program
    {
        public static Dictionary<int, MessageEntry[]> Context = new();
        public static List<List<int>> ListBranches = new();

        private static async Task Main()
        {
            var botClient = new TelegramBotClient(Consts.TgApiKey);
            
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, _) => cts.Cancel();

            var handler = new UpdateHandler();
            var receiverOptions = new ReceiverOptions();
            botClient.StartReceiving(handler, receiverOptions, cancellationToken: cts.Token);

            Console.WriteLine("Bot started. Press ^C to stop");
            await Task.Delay(-1, cancellationToken: cts.Token);
            Console.WriteLine("Bot stopped");
        }
    }
}
