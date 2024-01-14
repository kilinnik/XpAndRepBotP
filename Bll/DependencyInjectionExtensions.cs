using Bll.Commands.AdminCommands;
using Bll.Commands.EntertainmentAndGamesCommands;
using Bll.Commands.HelpAndSupportCommands;
using Bll.Commands.RelationshipCommands;
using Bll.Commands.RulesAndRolesCommands;
using Bll.Commands.StatsAndRankingsCommands;
using Bll.Commands.UserCommands;
using Bll.Factories;
using Bll.Handlers;
using Bll.Interfaces;
using Bll.Services;
using Bll.Strategies;
using Microsoft.Extensions.DependencyInjection;

namespace Bll;

public static class DependencyInjectionExtensions
{
    public static async Task AddBllServices(this IServiceCollection services)
    {
        var wordListService = await WordListService.CreateAsync();

        services
            // Admin commands
            .AddScoped<AssignRoleCommand>()
            .AddScoped<BanUserCommand>()
            .AddScoped<MuteUserCommand>()
            .AddScoped<RemoveAllRolesCommand>()
            .AddScoped<RemoveRoleCommand>()
            .AddScoped<UnbanUserCommand>()
            .AddScoped<UnmuteUserCommand>()
            .AddScoped<UnwarnUserCommand>()
            .AddScoped<WarnUserCommand>()
            // Entertainment and games commands
            .AddScoped<GenerateImageCommand>()
            .AddScoped<NoFuckChallengeCommand>()
            .AddScoped<WordUsageCountCommand>()
            // Help and support commands
            .AddScoped<AiInteractionHelpCommand>()
            .AddScoped<HelpCommand>()
            .AddScoped<LinksCommand>()
            // Relationship commands
            .AddScoped<DivorceCommand>()
            .AddScoped<MarriageCommand>()
            .AddScoped<MarriageListCommand>()
            .AddScoped<MarriageStatusCommand>()
            // Rules and roles commands
            .AddScoped<ChatRolesCommand>()
            .AddScoped<ChatRulesCommand>()
            .AddScoped<LevelRewardsCommand>()
            .AddScoped<PositiveFeedbackListCommand>()
            // Stats and rankings commands
            .AddScoped<TopLevelCommand>()
            .AddScoped<TopLexiconCommand>()
            .AddScoped<TopReputationCommand>()
            .AddScoped<TopWordsCommand>()
            // User commands
            .AddScoped<ComplaintsListCommand>()
            .AddScoped<MeCommand>()
            .AddScoped<ReportUserCommand>()
            // Services
            .AddScoped<IUpdateService, UpdateService>()
            .AddScoped<ICommandService, CommandService>()
            .AddScoped<IWelcomeService, WelcomeService>()
            .AddScoped<IUserRoleService, UserRoleService>()
            .AddScoped<IUserMarriageService, UserMarriageService>()
            .AddScoped<IUserLevelService, UserLevelService>()
            .AddScoped<IUserLexiconService, UserLexiconService>()
            .AddScoped<IUserProfileService, UserProfileService>()
            .AddScoped<IUserComplaintService, UserComplaintService>()
            .AddScoped<IUserReputationService, UserReputationService>()
            .AddScoped<IUserModerationService, UserModerationService>()
            .AddScoped<ITelegramMessageService, TelegramMessageService>()
            .AddScoped<IUserNfcService, UserNfcService>()
            .AddScoped<IRequestProcessingService, RequestProcessingService>()
            .AddScoped<IUserMessageProcessingService, UserMessageProcessingService>()
            .AddScoped<IUserMarriageService, UserMarriageService>()
            .AddSingleton(wordListService)
            // Strategies
            .AddScoped<IUserMessageStrategy, UserLevelStrategy>()
            .AddScoped<IUserMessageStrategy, AiResponseStrategy>()
            .AddScoped<IUserMessageStrategy, UserLexiconStrategy>()
            .AddScoped<IUserMessageStrategy, LinkMessageStrategy>()
            .AddScoped<IUserMessageStrategy, ChatPermissionStrategy>()
            .AddScoped<IUserMessageStrategy, UserModerationStrategy>()
            .AddScoped<IUserMessageStrategy, UserReputationStrategy>()
            .AddScoped<IUserMessageStrategy, RepeatedMessageStrategy>()
            .AddScoped<IUserMessageStrategy, NoFuckChallengeStrategy>()
            .AddScoped<IUserMessageStrategy, UserRoleMentionsStrategy>()
            .AddScoped<IUserMessageStrategy, ContentModerationStrategy>()
            // Factories
            .AddScoped<ICommandFactory, CommandFactory>()
            // Handlers
            .AddScoped<ICallbackQueryHandler, CallbackQueryHandler>();
    }
}