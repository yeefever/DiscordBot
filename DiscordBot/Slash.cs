using Discord.Interactions;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace DiscordBot
{
    public class Slash : InteractionModuleBase<SocketInteractionContext>
    {
        [Command("pig")]
        [Description("Respond with 'pig'.")]
        public async Task PigCommand(CommandContext ctx)
        {
            await ctx.RespondAsync("pig");
        }
    }
}