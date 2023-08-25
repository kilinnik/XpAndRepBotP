using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace XpAndRepBot.Commands;

public interface ICommand
{
    public Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
}