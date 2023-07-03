using OpenAI.GPT3.Managers;
using OpenAI.GPT3;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text.RegularExpressions;
using System.Linq;
using static XpAndRepBot.Consts;
using Telegram.Bot.Types.Enums;
using System;

namespace XpAndRepBot
{
    public interface ICommand
    {
        public Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    }

    public class HelpCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        replyToMessageId: update.Message.MessageId, parseMode: ParseMode.Html, text: HelpText,
                        cancellationToken: cancellationToken);
            }
            catch
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, parseMode: ParseMode.Html,
                        text: HelpText, cancellationToken: cancellationToken);
            }
        }
    }

    public class HelpRewardsCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        replyToMessageId: update.Message.MessageId, text: Rewards,
                        cancellationToken: cancellationToken);
            }
            catch
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: Rewards,
                        cancellationToken: cancellationToken);
            }
        }
    }

    public class MeCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "backmw"),
                    InlineKeyboardButton.WithCallbackData("Вперёд", "nextmw"),
                }
            });
            long userId = 0;
            if (update.Message is { Entities: not null } && update.Message.Entities.Any(x => x.Type
                    is MessageEntityType.TextMention or MessageEntityType.Mention))
            {
                foreach (var entity in update.Message.Entities)
                {
                    switch (entity.Type)
                    {
                        case MessageEntityType.TextMention:
                            if (entity.User != null) userId = entity.User.Id;
                            break;
                        case MessageEntityType.Mention:
                        {
                            await using var db = new InfoContext();
                            var user = db.TableUsers.FirstOrDefault(x =>
                                x.Username == update.Message.Text.Substring(entity.Offset + 1, entity.Length - 1));
                            if (user != null) userId = user.Id;
                            break;
                        }
                        default:
                            break;
                    }
                }
            }
            else if (update.Message is { ReplyToMessage.From.IsBot: false } &&
                     update.Message.ReplyToMessage.From.Id != 777000)
            {
                userId = update.Message.ReplyToMessage.From.Id;
            }
            else
            {
                if (update.Message is { From: not null }) userId = update.Message.From.Id;
            }

            try
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard,
                        replyToMessageId: update.Message.MessageId, text: await ResponseHandlers.Me(userId),
                        cancellationToken: cancellationToken);
            }
            catch
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: await ResponseHandlers.Me(userId), cancellationToken: cancellationToken);
            }
        }
    }

    public class TopLvlCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "backtl"),
                    InlineKeyboardButton.WithCallbackData("Вперёд", "nexttl"),
                }
            });
            try
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard,
                        replyToMessageId: update.Message.MessageId, text: ResponseHandlers.TopLvl(0),
                        cancellationToken: cancellationToken);
            }
            catch
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard,
                        text: ResponseHandlers.TopLvl(0), cancellationToken: cancellationToken);
            }
        }
    }

    public class TopReputationCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "backtr"),
                    InlineKeyboardButton.WithCallbackData("Вперёд", "nexttr"),
                }
            });
            try
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard,
                        replyToMessageId: update.Message.MessageId, text: ResponseHandlers.TopRep(0),
                        cancellationToken: cancellationToken);
            }
            catch
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard,
                        text: ResponseHandlers.TopRep(0), cancellationToken: cancellationToken);
            }
        }
    }

    public class RulesCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        replyToMessageId: update.Message.MessageId, text: RulesText, parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken);
            }
            catch
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: RulesText,
                        parseMode: ParseMode.Html, cancellationToken: cancellationToken);
            }
        }
    }

    public class MessagesReputationCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        replyToMessageId: update.Message.MessageId, text: MesRepText,
                        cancellationToken: cancellationToken);
            }
            catch
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: MesRepText,
                        cancellationToken: cancellationToken);
            }
        }
    }

    public class GamesCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        replyToMessageId: update.Message.MessageId, text: GamesText,
                        cancellationToken: cancellationToken);
            }
            catch
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: GamesText,
                        cancellationToken: cancellationToken);
            }
        }
    }

    public class TopWordsCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "backtw"),
                    InlineKeyboardButton.WithCallbackData("Вперёд", "nexttw"),
                }
            });
            try
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard,
                        replyToMessageId: update.Message.MessageId, text: await ResponseHandlers.TopWords(0),
                        cancellationToken: cancellationToken);
            }
            catch
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard,
                        text: await ResponseHandlers.TopWords(0), cancellationToken: cancellationToken);
            }
        }
    }

    public class TopLexiconCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "backl"),
                    InlineKeyboardButton.WithCallbackData("Вперёд", "nextl"),
                }
            });
            try
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard,
                        replyToMessageId: update.Message.MessageId, text: await ResponseHandlers.TopLexicon(0),
                        cancellationToken: cancellationToken);
            }
            catch
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard,
                        text: await ResponseHandlers.TopLexicon(0), cancellationToken: cancellationToken);
            }
        }
    }

    public class RoflCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await using var db = new InfoContext();
            var user = db.TableUsers.FirstOrDefault(x => x.Id == update.Message.From.Id);
            try
            {
                if (update.Message != null)
                    if (user != null)
                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                            replyToMessageId: update.Message.MessageId, text: $"🖕, {user.Name}, иди на хуй 🖕",
                            cancellationToken: cancellationToken);
            }
            catch
            {
                if (update.Message != null)
                    if (user != null)
                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                            text: $"🖕, {user.Name}, иди на хуй 🖕", cancellationToken: cancellationToken);
            }
        }
    }

    public class HelpChatGptCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        replyToMessageId: update.Message.MessageId, text: HelpGptText,
                        cancellationToken: cancellationToken);
            }
            catch
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: HelpGptText,
                        cancellationToken: cancellationToken);
            }
        }
    }

    public class ImageDalleCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var openAiService = new OpenAIService(new OpenAiOptions() { ApiKey = SshKey });
            if (update.Message != null)
            {
                var mes = update.Message.Caption ?? update.Message.Text;
                if (mes != null)
                {
                    var matches = Regex.Match(mes, @"(?<=\s)\w[\w\s]*");
                    try
                    {
                        try
                        {
                            await botClient.SendPhotoAsync(chatId: update.Message.Chat.Id,
                                replyToMessageId: update.Message.MessageId,
                                photo: await ResponseHandlers.GenerateImage(openAiService, matches.Value),
                                cancellationToken: cancellationToken);
                        }
                        catch
                        {
                            await botClient.SendPhotoAsync(chatId: update.Message.Chat.Id,
                                photo: await ResponseHandlers.GenerateImage(openAiService, matches.Value),
                                cancellationToken: cancellationToken);
                        }
                    }
                    catch
                    {
                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                            text: "Произошла ошибка. Возможно вы не ввели текст/ввели некорректный запрос ",
                            cancellationToken: cancellationToken);
                    }
                }
            }
        }
    }

    public class TgEmpressCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        replyToMessageId: update.Message.MessageId, text: TgEmpress,
                        cancellationToken: cancellationToken);
            }
            catch
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: TgEmpress,
                        cancellationToken: cancellationToken);
            }
        }
    }

    public class RoleCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is { From.Id: Iid })
            {
                try
                {
                    if (update.Message.ReplyToMessage is { From: not null })
                        if (update.Message.Text != null)
                            await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                replyToMessageId: update.Message.ReplyToMessage.MessageId,
                                text: ResponseHandlers.GiveRole(update.Message.ReplyToMessage.From.Id,
                                    update.Message.Text[6..]), cancellationToken: cancellationToken);
                }
                catch
                {
                    if (update.Message.ReplyToMessage?.From != null)
                        if (update.Message.Text != null)
                            await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                text: ResponseHandlers.GiveRole(update.Message.ReplyToMessage.From.Id,
                                    update.Message.Text[6..]), cancellationToken: cancellationToken);
                }
            }
            else if (update.Message != null)
                await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
        }
    }

    public class RolesCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "backr"),
                    InlineKeyboardButton.WithCallbackData("Вперёд", "nextr"),
                }
            });
            try
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        replyToMessageId: update.Message.MessageId, replyMarkup: inlineKeyboard,
                        text: ResponseHandlers.GetRoles(0), parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken);
            }
            catch
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard,
                        text: ResponseHandlers.GetRoles(0), parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken);
            }
        }
    }

    public class NoFuckChallengeCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await using var db = new InfoContext();
            var user = db.TableUsers.First(x => x.Id == update.Message.From.Id);
            if (user.Nfc == true)
            {
                try
                {
                    if (update.Message != null)
                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                            replyToMessageId: update.Message.MessageId, text: ResponseHandlers.PrintNfc(),
                            cancellationToken: cancellationToken);
                }
                catch
                {
                    if (update.Message != null)
                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                            text: ResponseHandlers.PrintNfc(), cancellationToken: cancellationToken);
                }
            }
            else
            {
                user.Nfc = true;
                var bestTime = "";
                if (user.BestTime > 0)
                {
                    var ts = TimeSpan.FromTicks(user.BestTime);
                    bestTime = $"\nВаш лучший результат: {ts.Days} d, {ts.Hours} h, {ts.Minutes} m.";
                }

                user.StartNfc = DateTime.Now;
                await db.SaveChangesAsync(cancellationToken);
                try
                {
                    if (update.Message != null)
                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                            replyToMessageId: update.Message.MessageId,
                            text: $"Вы начали новую серию без мата 👮‍♂️{bestTime}\nУдачи 😉",
                            cancellationToken: cancellationToken);
                }
                catch
                {
                    if (update.Message != null)
                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                            text: $"Вы начали новую серию без мата 👮‍♂️\nУдачи 😉",
                            cancellationToken: cancellationToken);
                }
            }
        }
    }

    public class UnRoleCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is { From.Id: Iid })
            {
                try
                {
                    if (update.Message.ReplyToMessage?.From != null)
                        if (update.Message.Text != null)
                            await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                replyToMessageId: update.Message.MessageId,
                                text: ResponseHandlers.DelRole(update.Message.ReplyToMessage.From.Id,
                                    update.Message.Text[5..]), cancellationToken: cancellationToken);
                }
                catch
                {
                    if (update.Message.ReplyToMessage?.From != null)
                        if (update.Message.Text != null)
                            await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                text: ResponseHandlers.DelRole(update.Message.ReplyToMessage.From.Id,
                                    update.Message.Text[5..]), parseMode: ParseMode.Html,
                                cancellationToken: cancellationToken);
                }
            }
            else if (update.Message != null)
                await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
        }
    }

    public class BalabobaCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message?.Text != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        replyToMessageId: update.Message.MessageId,
                        text: await ResponseHandlers.RequestBalaboba(update.Message.Text[3..]),
                        cancellationToken: cancellationToken);
            }
            catch
            {
                if (update.Message?.Text != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: await ResponseHandlers.RequestBalaboba(update.Message.Text[3..]),
                        cancellationToken: cancellationToken);
            }
        }
    }

    public class HelpAdminCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await using var db = new InfoContext();
            var user = db.TableUsers.FirstOrDefault(x => x.Id == update.Message.From.Id && x.Roles.Contains("модер"));
            if (update.Message is { From: not null } && user != null)
            {
                try
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        replyToMessageId: update.Message.MessageId, parseMode: ParseMode.Html, text: HelpAdminText,
                        cancellationToken: cancellationToken);
                }
                catch
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, parseMode: ParseMode.Html,
                        text: HelpAdminText, cancellationToken: cancellationToken);
                }
            }
            else if (update.Message != null)
                await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
        }
    }

    public class BanCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await using var db = new InfoContext();
            var user = db.TableUsers.FirstOrDefault(x => x.Id == update.Message.From.Id && x.Roles.Contains("модер"));
            if (update.Message is { ReplyToMessage.From: not null } and { From: not null } && user != null 
                && update.Message.From.Id != update.Message.ReplyToMessage?.From.Id)
            {
                try
                {
                    if (update.Message.ReplyToMessage != null)
                    {
                        await botClient.BanChatMemberAsync(chatId: update.Message.Chat.Id,
                            userId: update.Message.ReplyToMessage.From.Id, cancellationToken: cancellationToken);
                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                            replyToMessageId: update.Message.ReplyToMessage.MessageId,
                            text: $"{update.Message.ReplyToMessage.From.FirstName} забанен",
                            cancellationToken: cancellationToken);
                        await botClient.DeleteMessageAsync(update.Message.Chat.Id,
                            update.Message.ReplyToMessage.MessageId,
                            cancellationToken);
                    }
                }
                catch
                {
                    // ignored
                }
            }
            else if (update.Message != null)
                await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
        }
    }

    public class UnBanCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await using var db = new InfoContext();
            var user = db.TableUsers.FirstOrDefault(x => x.Id == update.Message.From.Id && x.Roles.Contains("модер"));
            if (update.Message is { ReplyToMessage.From: not null, From: not null } && user != null && 
                update.Message.From.Id != update.Message.ReplyToMessage?.From.Id)
            {
                long userId = 0;
                var name = "";
                if (update.Message.Entities != null &&
                    update.Message.Entities.Any(x => x.Type == MessageEntityType.Mention))
                {
                    foreach (var entity in update.Message.Entities)
                    {
                        if (entity.Type != MessageEntityType.Mention) continue;
                        var userMention = db.TableUsers.FirstOrDefault(x =>
                            x.Username == update.Message.Text.Substring(entity.Offset + 1, entity.Length - 1));
                        if (userMention == null) continue;
                        userId = userMention.Id;
                        name = userMention.Name;
                    }
                }
                else if (update.Message?.ReplyToMessage != null)
                {
                    userId = update.Message.ReplyToMessage.From.Id;
                    name = update.Message.ReplyToMessage.From.FirstName;
                }

                try
                {
                    if (update.Message != null)
                    {
                        await botClient.RestrictChatMemberAsync(
                            chatId: update.Message.Chat.Id,
                            userId,
                            new ChatPermissions
                            {
                                CanSendMessages = true,
                                CanSendMediaMessages = true,
                                CanSendOtherMessages = true,
                                CanSendPolls = true,
                                CanAddWebPagePreviews = true,
                                CanChangeInfo = true,
                                CanInviteUsers = true,
                                CanPinMessages = true,
                            },
                            cancellationToken: cancellationToken
                        );
                        await botClient.SendTextMessageAsync(
                            chatId: update.Message.Chat.Id,
                            replyToMessageId: update.Message.MessageId,
                            text: $"{name} разбанен",
                            cancellationToken: cancellationToken
                        );
                    }
                }
                catch
                {
                    // ignored
                }
            }
            else if (update.Message != null)
                await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
        }
    }

    public class WarnCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await using var db = new InfoContext();
            var user = db.TableUsers.FirstOrDefault(x => x.Id == update.Message.From.Id && x.Roles.Contains("модер"));
            if (update.Message is { ReplyToMessage.From: not null, From: not null } && user != null && 
                update.Message.From.Id != update.Message.ReplyToMessage?.From.Id)
            {
                try
                {
                    if (update.Message.ReplyToMessage != null)
                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                            replyToMessageId: update.Message.ReplyToMessage.MessageId,
                            text: ResponseHandlers.Warn(update),
                            cancellationToken: cancellationToken);
                }
                catch
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: ResponseHandlers.Warn(update), cancellationToken: cancellationToken);
                }
            }
            else if (update.Message != null)
                await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
        }
    }

    public class UnwarnCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await using var db = new InfoContext();
            var user = db.TableUsers.FirstOrDefault(x => x.Id == update.Message.From.Id && x.Roles.Contains("модер"));
            if (update.Message is { ReplyToMessage.From: not null, From: not null } && user != null && 
                update.Message.From.Id != update.Message.ReplyToMessage?.From.Id)
            {
                try
                {
                    if (update.Message.ReplyToMessage != null)
                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                            replyToMessageId: update.Message.ReplyToMessage.MessageId,
                            text: ResponseHandlers.Unwarn(update), cancellationToken: cancellationToken);
                }
                catch
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: ResponseHandlers.Unwarn(update), cancellationToken: cancellationToken);
                }
            }
            else if (update.Message != null)
                await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
        }
    }

    public class MuteCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await using var db = new InfoContext();
            var user = db.TableUsers.FirstOrDefault(x => x.Id == update.Message.From.Id && x.Roles.Contains("модер"));
            if (update.Message is { ReplyToMessage.From: not null, From: not null } && user != null && 
                update.Message.From.Id != update.Message.ReplyToMessage?.From.Id)
            {
                if (update.Message.Text != null)
                {
                    var parts = update.Message.Text[6..].Split(' ');
                    int days = 0, hours = 0, minutes = 0;
                    foreach (var part in parts)
                    {
                        if (part.EndsWith("d"))
                        {
                            if (int.TryParse(part.TrimEnd('d'), out var result))
                                days = result;
                        }
                        else if (part.EndsWith("h"))
                        {
                            if (int.TryParse(part.TrimEnd('h'), out var result))
                                hours = result;
                        }
                        else if (part.EndsWith("m"))
                        {
                            if (int.TryParse(part.TrimEnd('m'), out var result))
                                minutes = result;
                        }
                    }

                    try
                    {
                        if (days != 0 || hours != 0 || minutes != 0)
                        {
                            var muteDate = DateTime.Now.AddDays(days).AddHours(hours).AddMinutes(minutes);
                            if (update.Message.ReplyToMessage != null)
                            {
                                await botClient.RestrictChatMemberAsync(
                                    chatId: update.Message.Chat.Id,
                                    userId: update.Message.ReplyToMessage.From.Id,
                                    new ChatPermissions
                                    {
                                        CanSendMessages = false,
                                        CanSendMediaMessages = false
                                    },
                                    untilDate: muteDate,
                                    cancellationToken: cancellationToken
                                );
                                var userMute =
                                    db.TableUsers.FirstOrDefault(x => x.Id == update.Message.ReplyToMessage.From.Id);
                                if (userMute != null) userMute.DateMute = muteDate;
                                await db.SaveChangesAsync(cancellationToken);
                                try
                                {
                                    await botClient.SendTextMessageAsync(
                                        chatId: update.Message.Chat.Id,
                                        replyToMessageId: update.Message.ReplyToMessage.MessageId,
                                        text:
                                        $"{update.Message.ReplyToMessage.From.FirstName} получил мут на {GetFormattedDuration(days, hours, minutes)} до {muteDate:dd.MM.yyyy HH:mm}",
                                        cancellationToken: cancellationToken
                                    );
                                }
                                catch
                                {
                                    await botClient.SendTextMessageAsync(
                                        chatId: update.Message.Chat.Id,
                                        text:
                                        $"{update.Message.ReplyToMessage.From.FirstName} получил мут на {GetFormattedDuration(days, hours, minutes)} минут до {muteDate:dd.MM.yyyy HH:mm}",
                                        cancellationToken: cancellationToken
                                    );
                                }
                            }
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: update.Message.Chat.Id,
                                text: $"Команда мута введена некорректно",
                                cancellationToken: cancellationToken
                            );
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            else if (update.Message != null)
                await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
        }

        private static string GetFormattedDuration(int days, int hours, int minutes)
        {
            var duration = "";

            if (days > 0)
            {
                var daysWord = GetNounForm(days, "день", "дня", "дней");
                duration += $"{days} {daysWord} ";
            }

            if (hours > 0)
            {
                var hoursWord = GetNounForm(hours, "час", "часа", "часов");
                duration += $"{hours} {hoursWord} ";
            }

            if (minutes > 0)
            {
                var minutesWord = GetNounForm(minutes, "минуту", "минуты", "минут");
                duration += $"{minutes} {minutesWord} ";
            }

            return duration.Trim();
        }

        private static string GetNounForm(int number, string form1, string form2, string form5)
        {
            var mod10 = number % 10;
            var mod100 = number % 100;
            if (mod100 is >= 11 and <= 19)
                return form5;
            return mod10 switch
            {
                1 => form1,
                2 or 3 or 4 => form2,
                _ => form5,
            };
        }
    }

    public class StatusCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await using var db = new InfoContext();
            string status;
            if (update.Message?.ReplyToMessage is { From.IsBot: false } &&
                update.Message.ReplyToMessage.From.Id != 777000)
            {
                var user1 = db.TableUsers.First(x => x.Id == update.Message.ReplyToMessage.From.Id);
                if (user1.Mariage == 0) status = $"{user1.Name} не состоит в браке";
                else
                {
                    var user2 = db.TableUsers.First(x => x.Id == user1.Mariage);
                    var ts = DateTime.Now - user2.DateMariage;
                    status =
                        $"🤵🏿 🤵🏿 {user1.Name} состоит в браке с {user2.Name} {ts.Days} d, {ts.Hours} h, {ts.Minutes} m. " +
                        $"Дата регистрации {user1.DateMariage:yy/MM/dd HH:mm:ss}";
                }
            }
            else
            {
                var user1 = db.TableUsers.First(x => x.Id == update.Message.From.Id);
                if (user1.Mariage == 0) status = $"Вы не состоите в браке";
                else
                {
                    var user2 = db.TableUsers.First(x => x.Id == user1.Mariage);
                    var ts = DateTime.Now - user2.DateMariage;
                    status =
                        $"🤵🏿 🤵🏿 {user1.Name} состоит в браке с {user2.Name} {ts.Days} d, {ts.Hours} h, {ts.Minutes} m. " +
                        $"Дата регистрации {user1.DateMariage:yy/MM/dd HH:mm:ss}";
                }
            }

            try
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        replyToMessageId: update.Message.MessageId, text: status, cancellationToken: cancellationToken);
            }
            catch
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: status,
                        cancellationToken: cancellationToken);
            }
        }
    }

    public class MariageCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is { ReplyToMessage.From: not null } && update.Message?.ReplyToMessage != null && 
                !update.Message.ReplyToMessage.From.IsBot && update.Message.ReplyToMessage.From.Id != 777000)
            {
                await using var db = new InfoContext();
                var user1 = db.TableUsers.First(x => x.Id == update.Message.From.Id);
                var user2 = db.TableUsers.First(x => x.Id == update.Message.ReplyToMessage.From.Id);
                if (user1.Mariage != 0)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: "Вы уже в браке",
                        cancellationToken: cancellationToken);
                else if (user2.Mariage != 0)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        replyToMessageId: update.Message.MessageId, text: $"{user2.Name} уже в браке",
                        cancellationToken: cancellationToken);
                else
                {
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Нет", $"mny{user1.Id}"),
                            InlineKeyboardButton.WithCallbackData("Да", $"my{user1.Id}"),
                        }
                    });
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, replyMarkup: inlineKeyboard,
                        replyToMessageId: update.Message.ReplyToMessage.MessageId,
                        text:
                        $"💖 {user1.Name} делает вам предложение руки и сердца. Согласны ли вы вступить в брак с {user1.Name}?",
                        cancellationToken: cancellationToken);
                }
            }
            else if (update.Message != null)
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                    replyToMessageId: update.Message.MessageId,
                    text: "Ответьте на сообщение того, с кем хотите заключить брак",
                    cancellationToken: cancellationToken);
        }
    }

    public class DivorceCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await using var db = new InfoContext();
            var user1 = db.TableUsers.First(x => x.Id == update.Message.From.Id);
            if (user1.Mariage == 0)
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: "Вы не состоите в браке",
                        cancellationToken: cancellationToken);
            }
            else
            {
                var user2 = db.TableUsers.First(x => x.Id == user1.Mariage);
                user1.Mariage = 0;
                user2.Mariage = 0;
                var ts = DateTime.Now - user2.DateMariage;
                await db.SaveChangesAsync(cancellationToken);
                var mes =
                    $"💔 {user2.Name} сожалеем, но {user1.Name} подал на развод. Ваш брак был зарегистрирован " +
                    $"{user1.DateMariage:yy/MM/dd HH:mm:ss} и просуществовал {ts.Days} d, {ts.Hours} h, {ts.Minutes} m";
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        replyToMessageId: update.Message.MessageId, text: mes, cancellationToken: cancellationToken);
            }
        }
    }

    public class MariagesCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        replyToMessageId: update.Message.MessageId, text: ResponseHandlers.Mariages(),
                        cancellationToken: cancellationToken);
            }
            catch
            {
                if (update.Message != null)
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: ResponseHandlers.Mariages(),
                        cancellationToken: cancellationToken);
            }
        }
    }

    public class WordCommand : ICommand
    {
        public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message != null)
            {
                var mes = update.Message.Text;
                if (mes == "/w")
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: "Вы не написали слово",
                        cancellationToken: cancellationToken);
                else
                {
                    if (mes != null)
                    {
                        mes = mes.Replace("/w ", "");
                        if (update.Message.ReplyToMessage is { From.IsBot: false } &&
                            update.Message.ReplyToMessage.From.Id != 777000)
                        {
                            try
                            {
                                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                    replyToMessageId: update.Message.MessageId,
                                    text: await ResponseHandlers.PersonalWord(update.Message.ReplyToMessage.From.Id,
                                        mes),
                                    cancellationToken: cancellationToken);
                            }
                            catch
                            {
                                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                    text: await ResponseHandlers.PersonalWord(update.Message.ReplyToMessage.From.Id,
                                        mes),
                                    cancellationToken: cancellationToken);
                            }
                        }
                        else
                        {
                            try
                            {
                                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                    replyToMessageId: update.Message.MessageId, text: await ResponseHandlers.Word(mes),
                                    cancellationToken: cancellationToken);
                            }
                            catch
                            {
                                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                    text: await ResponseHandlers.Word(mes), cancellationToken: cancellationToken);
                            }
                        }
                    }
                }
            }
        }
    }
}