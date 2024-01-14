using Infrastructure.Configuration;
using Infrastructure.Interfaces;
using Infrastructure.Options;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Junaid.GoogleGemini.Net.Infrastructure;
using Junaid.GoogleGemini.Net.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Uthef.FusionBrain;
using Uthef.FusionBrain.Types;

namespace Infrastructure;

public static class DependencyInjectionExtensions
{
    public static void AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = configuration
            .GetSection(ConnectionStrings.Section)
            .Get<ConnectionStrings>();
        services.AddDbContext<XpAndRepBotDbContext>(
            options => options.UseSqlServer(connectionString.XpAndRepBot)
        );
        services.AddDbContextFactory<XpAndRepBotDbContext>(
            options => options.UseSqlServer(connectionString.XpAndRepBot)
        );
        //    .LogTo(Console.WriteLine, LogLevel.Information));

        services
            .AddSingleton<IChatThreadService, ChatThreadService>()
            .AddScoped<IAiResponseService, AiResponseService>()
            .AddScoped<IImageGenerationService, ImageGenerationService>()
            .AddScoped<IChatRepository, ChatRepository>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IUserRoleRepository, UserRoleRepository>()
            .AddScoped<IUserLevelRepository, UserLevelRepository>()
            .AddScoped<IUserLexiconRepository, UserLexiconRepository>()
            .AddScoped<IUserReputationRepository, UserReputationRepository>()
            .AddScoped<IUserComplaintsRepository, UserComplaintsRepository>()
            .AddScoped<IDeletableMessageRepository, DeletableMessageRepository>()
            .AddScoped<IUserMarriageRepository, UserMarriageRepository>()
            .AddScoped<IUserModerationRepository, UserModerationRepository>()
            .AddScoped<IUserNfcRepository, UserNfcRepository>()
            .AddScoped<ChatConfigurationManager>();

        services.Configure<AiServiceOptions>(configuration.GetSection(AiServiceOptions.Section));

        services.AddSingleton<ChatService>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<AiServiceOptions>>().Value;
            var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl) };
            httpClient.DefaultRequestHeaders.Add("X-Goog-Api-Key", new[] { options.ApiKey });

            return new ChatService(new GeminiClient(httpClient));
        });

        services.AddSingleton<VisionService>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<AiServiceOptions>>().Value;
            var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl) };
            httpClient.DefaultRequestHeaders.Add("X-Goog-Api-Key", new[] { options.ApiKey });

            return new VisionService(new GeminiClient(httpClient));
        });

        services
            .AddHttpClient()
            .AddSingleton<FusionBrainApi>(provider =>
            {
                var fusionBrainConfig = configuration
                    .GetSection("FusionBrainApi")
                    .Get<AuthCredentials>();
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

                return new FusionBrainApi(
                    new AuthCredentials(fusionBrainConfig.ApiKey, fusionBrainConfig.SecretKey),
                    httpClientFactory.CreateClient()
                );
            });
    }
}
