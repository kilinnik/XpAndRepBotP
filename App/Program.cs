using App.Options;
using Bll;
using Bll.Interfaces;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Telegram.Bot;
using Telegram.Bot.Polling;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Warning)
    .CreateLogger();

var serviceCollection = new ServiceCollection();

serviceCollection.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

serviceCollection.AddSingleton<IConfiguration>(configuration);

var botToken = configuration.GetSection(BotConfiguration.Section).Get<BotConfiguration>();
serviceCollection.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(botToken.Token));

await serviceCollection.AddBllServices();
serviceCollection.AddInfrastructureServices(configuration);

var serviceProvider = serviceCollection.BuildServiceProvider();

var botClient = serviceProvider.GetRequiredService<ITelegramBotClient>();

using var cts = new CancellationTokenSource();
var receiverOptions = new ReceiverOptions();

var updateService = serviceProvider.GetRequiredService<IUpdateService>();

botClient.StartReceiving(
    updateHandler: (_, update, token) => updateService.HandleUpdateAsync(update, token),
    pollingErrorHandler: (_, exception, _) => updateService.HandleErrorAsync(exception),
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

Console.WriteLine(@"Start listening. Press Enter to exit.");
Console.ReadLine();
