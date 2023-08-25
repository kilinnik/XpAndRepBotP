using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XpAndRepBot.Commands;
using XpAndRepBot.Database;
using XpAndRepBot.Database.Models;
using XpAndRepBot.Handlers;
using static XpAndRepBot.Constants;

namespace XpAndRepBot;

internal class UpdateHandler : IUpdateHandler
{
    private static readonly Dictionary<string, ICommand> Commands = new()
    {
        { "/m", new MeCommand() },
        { "/tl", new TopLvlCommand() },
        { "/tr", new TopReputationCommand() },
        { "/r", new RulesCommand() },
        { "/h", new HelpCommand() },
        { "/help", new HelpCommand() },
        { "/mr", new MessagesReputationCommand() },
        { "/g", new GamesCommand() },
        { "/tw", new TopWordsCommand() },
        { "/porno", new RoflCommand() },
        { "/hc", new HelpChatGptCommand() },
        { "/im", new ImageDalleCommand() },
        { "/l", new TopLexiconCommand() },
        { "/role", new RoleCommand() },
        { "/roles", new RolesCommand() },
        { "/nfc", new NoFuckChallengeCommand() },
        { "/unr", new UnRoleCommand() },
        { "/unrall", new UnRoleAllCommand() },
        { "/w", new WordCommand() },
        { "marriage", new MarriageCommand() },
        { "marriages", new MarriagesCommand() },
        { "status", new StatusCommand() },
        { "divorce", new DivorceCommand() },
        { "/ha", new HelpAdminCommand() },
        { "/report", new ReportCommand() },
        { "/c", new ComplaintsCommand() },
        { "/ban", new BanCommand() },
        { "/unb", new UnBanCommand() },
        { "/warn", new WarnCommand() },
        { "/unwarn", new UnwarnCommand() },
        { "/unw", new UnwarnCommand() },
        { "/mute", new MuteCommand() },
        { "/rew", new HelpRewardsCommand() },
        { "/link", new LinkCommand() }
    };

    public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        Debug.WriteLine(JsonSerializer.Serialize(update));

        //handle callback
        return Task.FromResult(update.Type == UpdateType.CallbackQuery
            ? Task.Run(() => HandleCallbackQuery(botClient, update.CallbackQuery, cancellationToken))
            : Task.Run(() => HandleNonCallbackUpdate(botClient, update, cancellationToken)));
    }

    private static async Task HandleNonCallbackUpdate(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        await using var db = new DbUsersContext();
        if (update.Message?.Chat.Id is IgruhaChatId or MyChatId or Mid or Iid or NitokinChatId)
        {
            if (update.Message.From != null)
            {
                var user = await GetOrCreateUserHandler.GetOrCreateUser(db, update);

                if (user.ChatId == IgruhaChatId)
                {
                    if (user.UserId == 777000)
                    {
                        await SetChatPermissionsHandler.SetChatPermissionsAsync(botClient, user.ChatId);
                    }

                    await DeleteHandler.DeleteUnwantedMessages(botClient, update, user, cancellationToken);
                }

                await NewMemberHandler.NewMember(botClient, update, user, cancellationToken);
                await UpdateUserLastMessageTimeHandler.UpdateUserLastMessageTime(db, user, update, cancellationToken);
                await UserWarnsHandler.HandleUserWarns(botClient, db, user, update, cancellationToken);
                await ProcessMessageContent(botClient, db, update, user, cancellationToken);
            }
        }
    }

    private static async Task ProcessMessageContent(ITelegramBotClient botClient, DbUsersContext db, Update update,
        Users user, CancellationToken cancellationToken)
    {
        if (ShouldProcessMessage(update))
        {
            var mes = Utilities.GetMessageText(update);

            user.CurXp += mes.Length;

            await ReputationUpHandler.HandleReputationUp(botClient, update, db, mes, cancellationToken);
            await AddWordsToLexiconHandler.AddWordsToLexicon(user, mes);
            await LvlUpHandler.HandleLevelUp(botClient, update, db, user, cancellationToken);
            await CommandsHandler.HandleCommands(botClient, update, Commands, cancellationToken);
            await MentionHandler.HandleMentions(db, user, update, botClient, cancellationToken);
            await NfcHandler.HandleNfc(botClient, update, user, cancellationToken);
            if (user.ChatId == IgruhaChatId)
            {
                await RepeatedMessagesHandler.HandleRepeatedMessages(botClient, update, user, cancellationToken);
            }

            await ChatGptHandler.HandleChatGpt(botClient, update, mes, Commands, cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static bool ShouldProcessMessage(Update update)
    {
        return update.Message?.Text != null || update.Message?.Caption != null;
    }


    private static async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery,
        CancellationToken cancellationToken)
    {
        if (callbackQuery.Message == null) return;

        var option = callbackQuery.Data;
        try
        {
            switch (option)
            {
                case "link_repack":
                case "link_chat":
                case "link_youtube":
                case "link_discord":
                case "link_vk":
                case "link_vk_reserve":
                case "link_tiktok":
                case "link_soft":
                case "link_apps":
                    await SendMessageByClickHandler.SendMessageByClick(botClient, callbackQuery.Message.Chat.Id, option,
                        callbackQuery.Message.MessageId, cancellationToken);
                    break;
                case "link_hide":
                    await botClient.DeleteMessageAsync(callbackQuery.Message, cancellationToken);
                    break;
                case "backmw":
                case "nextmw":
                    await MeWordsHandler.HandleMeWordsCallbackQuery(botClient, callbackQuery, cancellationToken);
                    break;
                case "backtw":
                case "nexttw":
                    await TopWordsHandler.HandleTopWordsCallbackQuery(botClient, callbackQuery, cancellationToken);
                    break;
                case "backtl":
                case "nexttl":
                    await TopLvlHandler.HandleTopLvlCallbackQuery(botClient, callbackQuery, cancellationToken);
                    break;
                case "backtr":
                case "nexttr":
                    await TopRepHandler.HandleTopRepCallbackQuery(botClient, callbackQuery, cancellationToken);
                    break;
                case "backl":
                case "nextl":
                    await TopLexiconHandler.HandleTopLexiconCallbackQuery(botClient, callbackQuery, cancellationToken);
                    break;
                case "backr":
                case "nextr":
                    await RolesHandler.HandleRolesCallbackQuery(botClient, callbackQuery, cancellationToken);
                    break;
                default:
                    await DefaultCallbackQueryHandler.HandleDefaultCallbackQuery(botClient, callbackQuery,
                        cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        Console.Error.WriteLine(exception);
        return Task.CompletedTask;
    }
}