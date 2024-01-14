using Bll.Commands.AdminCommands;
using Bll.Commands.EntertainmentAndGamesCommands;
using Bll.Commands.HelpAndSupportCommands;
using Bll.Commands.RelationshipCommands;
using Bll.Commands.RulesAndRolesCommands;
using Bll.Commands.StatsAndRankingsCommands;
using Bll.Commands.UserCommands;
using Bll.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Bll.Factories;

public class CommandFactory(IServiceProvider serviceProvider) : ICommandFactory
{
    private readonly Dictionary<string, Type> _commandTypes =
        new()
        {
            // Admin Commands
            { "/role", typeof(AssignRoleCommand) },
            { "/ban", typeof(BanUserCommand) },
            { "/mute", typeof(MuteUserCommand) },
            { "/unroleall", typeof(RemoveAllRolesCommand) },
            { "/unrall", typeof(RemoveAllRolesCommand) },
            { "/unrole", typeof(RemoveRoleCommand) },
            { "/unr", typeof(RemoveRoleCommand) },
            { "/unban", typeof(UnbanUserCommand) },
            { "/unb", typeof(UnbanUserCommand) },
            { "/unmute", typeof(UnmuteUserCommand) },
            { "/unm", typeof(UnmuteUserCommand) },
            { "/unwarn", typeof(UnwarnUserCommand) },
            { "/unw", typeof(UnwarnUserCommand) },
            { "/warn", typeof(WarnUserCommand) },
            // Entertainment and Games Commands
            { "/genimg", typeof(GenerateImageCommand) },
            { "/im", typeof(GenerateImageCommand) },
            { "/nofuckchallenge", typeof(NoFuckChallengeCommand) },
            { "/nfc", typeof(NoFuckChallengeCommand) },
            { "/wordusage", typeof(WordUsageCountCommand) },
            { "/word", typeof(WordUsageCountCommand) },
            { "/w", typeof(WordUsageCountCommand) },
            // Help and Support Commands
            { "/aihelp", typeof(AiInteractionHelpCommand) },
            { "/aih", typeof(AiInteractionHelpCommand) },
            { "/help", typeof(HelpCommand) },
            { "/h", typeof(HelpCommand) },
            { "/link", typeof(LinksCommand) },
            // Relationship Commands
            { "divorce", typeof(DivorceCommand) },
            { "marriage", typeof(MarriageCommand) },
            { "marriages", typeof(MarriageListCommand) },
            { "status", typeof(MarriageStatusCommand) },
            // Rules and Roles Commands
            { "/roles", typeof(ChatRolesCommand) },
            { "/rules", typeof(ChatRulesCommand) },
            { "/rule", typeof(ChatRulesCommand) },
            { "/r", typeof(ChatRulesCommand) },
            { "/lvlrewards", typeof(LevelRewardsCommand) },
            { "/lvlrew", typeof(LevelRewardsCommand) },
            { "/rew", typeof(LevelRewardsCommand) },
            { "/repup", typeof(PositiveFeedbackListCommand) },
            { "/repmsg", typeof(PositiveFeedbackListCommand) },
            { "/rm", typeof(PositiveFeedbackListCommand) },
            // Stats and Rankings Commands
            { "/toplevel", typeof(TopLevelCommand) },
            { "/toplvl", typeof(TopLevelCommand) },
            { "/tl", typeof(TopLevelCommand) },
            { "/toplex", typeof(TopLexiconCommand) },
            { "/tlex", typeof(TopLexiconCommand) },
            { "/l", typeof(TopLexiconCommand) },
            { "/toprep", typeof(TopReputationCommand) },
            { "/tr", typeof(TopReputationCommand) },
            { "/topwords", typeof(TopWordsCommand) },
            { "/tw", typeof(TopWordsCommand) },
            // User Commands
            { "/complaints", typeof(ComplaintsListCommand) },
            { "/c", typeof(ComplaintsListCommand) },
            { "/me", typeof(MeCommand) },
            { "/m", typeof(MeCommand) },
            { "/report", typeof(ReportUserCommand) }
        };

    public ICommand? CreateCommand(string messageText)
    {
        var parts = messageText.Split(' ');
        var commandWithBotNick = parts[0].ToLower();

        var commandKey = commandWithBotNick.Split('@')[0];

        if (!_commandTypes.TryGetValue(commandKey, out var commandType))
        {
            return null;
        }

        return (ICommand)serviceProvider.GetRequiredService(commandType);
    }
}
