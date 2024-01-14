using Bll.Interfaces;
using Telegram.Bot.Types;

namespace Bll.Services;

public class CommandService(ICommandFactory commandFactory, ITelegramMessageService messageService)
    : ICommandService
{
    public async Task ExecuteCommandAsync(Message message, CancellationToken token)
    {
        if (message.Text != null)
        {
            var command = commandFactory.CreateCommand(message.Text);

            if (command != null)
            {
                var result = await command.ExecuteAsync(message, token);

                if (result != null)
                {
                    await messageService.SendMessageAsync(result, token);
                }
            }
        }
    }
}
