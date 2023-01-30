using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using System.Linq;
using System.Collections.Generic;

namespace XpAndRepBot
{
    class UpdateHandler : IUpdateHandler
    {
        static readonly Dictionary<string, ICommand> _commands = new()
        {
            {"/m", new MeCommand() },
            {"/m@XpAndRepBot", new MeCommand() },
            {"/tl", new TopLvlCommand() },
            {"/tl@XpAndRepBot", new TopLvlCommand() },
            {"/tr", new TopReputationCommand() },
            {"/tr@XpAndRepBot", new TopReputationCommand() },
            {"/r", new RulesCommand() },
            {"/r@XpAndRepBot", new RulesCommand() },
            {"/h", new HelpCommand() },
            {"/h@XpAndRepBot", new HelpCommand() },
            {"/help@XpAndRepBot", new HelpCommand() },
            {"/mr", new MessagesReputationCommand() },
            {"/mr@XpAndRepBot", new MessagesReputationCommand() },
            {"/g", new GamesCommand() },
            {"/g@XpAndRepBot", new GamesCommand() },
            {"/tw", new TopWordsCommand() },
            {"/tw@XpAndRepBot", new TopWordsCommand() },
            {"/porno", new RoflCommand() },
            {"/hc", new HelpChatGPTCommand() },
            {"/hc@XpAndRepBot", new HelpChatGPTCommand() }
        };

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Debug.WriteLine(JsonSerializer.Serialize(update)); //1813723228 1882185833
            using var db = new InfoContext();
            DataClasses1DataContext dbl = new(Consts.ConStrindDbLexicon);
            if ((update.Message?.Chat?.Id is long id && (id == -1001412057284 || id == -1001489033044 || id == 1813723228 || id == 1882185833)) && !update.Message.From.IsBot)
            {
                var idUser = update.Message.From.Id;
                var user = db.TableUsers.FirstOrDefault(x => x.Id == idUser);
                if (user == null)
                {
                    user = new Users(idUser, update.Message.From.FirstName + " " + update.Message.From.LastName, 0, 0, 0);
                    db.TableUsers.Add(user);
                }
                db.SaveChanges();
                //удаление гифок, стикеров и опросов
                await ChatHandlers.Delete(botClient, update, user, cancellationToken);
                if (update?.Message?.Text != null || update?.Message?.Caption is not null)
                {
                    var mes = update.Message.Caption ?? update.Message.Text;
                    //опыт
                    user.CurXp += mes.Length;
                    //повышение репутации
                    if (update.Message.ReplyToMessage != null && !update.Message.ReplyToMessage.From.IsBot) await ChatHandlers.ReputationUp(botClient, update, db, mes, cancellationToken);
                    //запрос к chatgdp
                    if ((update?.Message?.ReplyToMessage != null && update?.Message?.ReplyToMessage.From.Id == 5759112130) || (mes.Contains("@XpAndRepBot") && !mes.Contains('/')) || update.Message?.Chat?.Id == 1813723228 || id == 1882185833)
                    {
#pragma warning disable CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до тех пор, пока вызов не будет завершен
                        ChatHandlers.RequestChatGPT(botClient, update, mes, cancellationToken);
#pragma warning restore CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до тех пор, пока вызов не будет завершен
                    }
                    //создание и заполнение таблицы
                    ChatHandlers.CreateAndFillTable(user, mes, dbl);
                    //повышение уровня
                    if (update.Message?.Chat?.Id != 1813723228) await ChatHandlers.LvlUp(botClient, update, db, user, cancellationToken);
                    //команды
                    if (_commands.ContainsKey(mes))
                    {
                        var command = _commands[update.Message.Text];
                        await command.ExecuteAsync(botClient, update, cancellationToken);
                    }
                }        
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.Error.WriteLine(exception);
            return Task.CompletedTask;
        }
    }
}

