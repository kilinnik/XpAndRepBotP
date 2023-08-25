using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using Telegram.Bot.Types.InputFiles;

namespace XpAndRepBot.Handlers;

public class DalleHandler
{
    [Obsolete("Obsolete")]
    public static async Task<InputOnlineFile> GenerateImage(IOpenAIService sdk, string prompt)
    {
        try
        {
            var imageResult = await sdk.Image.CreateImage(new ImageCreateRequest
            {
                Prompt = prompt,
                N = 1,
                Size = StaticValues.ImageStatics.Size.Size256,
                ResponseFormat = StaticValues.ImageStatics.ResponseFormat.Url,
                User = "TestUser"
            });

            if (imageResult.Successful)
            {
                var imageUrl = imageResult.Results.First().Url;
                using HttpClient httpClient = new();
                var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                return new InputOnlineFile(new MemoryStream(imageBytes), "image.png");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return null;
    }
}