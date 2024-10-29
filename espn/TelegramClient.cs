using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NBAFantasy
{
    public static class TelegramClient
    {
        const string ApiToken = "7761372749:AAHBrDM3IxWBnvsDoybvNVuj0Q5chCoO0Ws";
        const string ChatId = "-4548336952";

        private static readonly TelegramBotClient BotClient;

        static TelegramClient()
        {
            BotClient = new TelegramBotClient(ApiToken);
        }

        public static async Task SendMsg(string msg)
        {
            await BotClient.SendTextMessageAsync(ChatId, msg);
        }

        public static async Task SendImage(Bitmap bitmap, string caption = "")
        {
            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            memoryStream.Seek(0, SeekOrigin.Begin);
            await BotClient.SendPhotoAsync(ChatId, new InputFileStream(memoryStream), caption: caption);
        }
    }
}
