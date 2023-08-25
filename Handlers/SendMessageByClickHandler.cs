using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace XpAndRepBot.Handlers;

public static class SendMessageByClickHandler
{
    public static async Task SendMessageByClick(ITelegramBotClient botClient, long chatId, string option,
        int messageId, CancellationToken cancellationToken)
    {
        var messageText = option switch
        {
            "link_repack" => "Скачать репак — https://t.me/+TOl5zAKElRU4ODhi",
            "link_chat" => "Чат в ТГ — https://t.me/+xVNP9XrdFdFkNGFi",
            "link_youtube" => "YouTube — https://www.youtube.com/@RGnitokin",
            "link_discord" => "Discord — https://discord.gg/VC3sXPebtE",
            "link_vk" => "ВКонтакте — https://vk.com/nitokin",
            "link_vk_reserve" => "ВКонтакте (запасной паблик) — https://vk.com/nito_kin",
            "link_tiktok" => "ТикТок — https://www.tiktok.com/@r.g.nitokin?_t=8d4tu93wEvx&_r=1",
            "link_soft" => "NITOKIN SOFT — https://t.me/nito_kin_soft",
            "link_apps" => "Nitokin - apps — https://t.me/apk_andro",
            _ => null
        };

        try
        {
            await botClient.SendTextMessageAsync(chatId, messageText, cancellationToken, replyToMessageId: messageId);
        }
        catch
        {
            await botClient.SendTextMessageAsync(chatId, messageText, cancellationToken);
        }
    }
}