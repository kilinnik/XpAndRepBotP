using System;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Mirror.ChatGpt.Models.ChatGpt;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Mirror.ChatGpt;

namespace XpAndRepBot;

public static class Program
{
    public static Dictionary<int, MessageEntry[]> Context = new();
    public static List<List<int>> ListBranches = new();
    public static ChatGptClient Service;
        
    private static CancellationTokenSource _cts;
        
    private static async Task Main()
    {
        var services = new ServiceCollection();
        services.AddChatGptClient(new ChatGptClientOptions { ApiKey = Constants.SshKey });
        var app = services.BuildServiceProvider();
        Service = app.GetRequiredService<ChatGptClient>();
            
        var botClient = new TelegramBotClient(Constants.TgApiKey);
    
        _cts = new CancellationTokenSource();
        Console.CancelKeyPress += OnCancelKeyPress;

        var handler = new UpdateHandler();
        var receiverOptions = new ReceiverOptions();
        botClient.StartReceiving(handler, receiverOptions, cancellationToken: _cts.Token);

        Console.WriteLine("Bot started. Press ^C to stop");
        await Task.Delay(-1, cancellationToken: _cts.Token);
        Console.WriteLine("Bot stopped");
    }

    private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        // Handle the cancel key press event here
        _cts.Cancel();
    }
}