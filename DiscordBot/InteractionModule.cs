using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using System.Runtime.InteropServices;
using SlashCommandAttribute = DSharpPlus.SlashCommands.SlashCommandAttribute;
using InteractionContext = DSharpPlus.SlashCommands.InteractionContext;
using TextInputStyle = Discord.TextInputStyle;
using TextInputComponent = DSharpPlus.Entities.TextInputComponent;
using InteractionResponseType = DSharpPlus.InteractionResponseType;
using System.Text.RegularExpressions;

namespace DiscordBot
{

    public class InteractionModule: ApplicationCommandModule
    {
        [SlashCommand("rat", "receive ping")]
        //[Option("string", "Type something")] string text
        public async Task HandlePing(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync("RAT");
        }

        [SlashCommand("create", "Create an event.")]
        //public async Task CreateEvent(InteractionContext ctx)
        public async Task CreateEvent(InteractionContext ctx, [Option("string", "ping the dudes please :)")] string input)
        {

            Console.WriteLine(input);

            //parse members 

            string pattern = @"<@(\d+)>";

            MatchCollection matches = Regex.Matches(input, pattern);
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    string userId = match.Groups[1].Value;
                    Console.WriteLine(userId);
                }
            }

            //Send invitations to all these dudes with the info :)



            var mb = new DiscordInteractionResponseBuilder()
            .WithTitle("Create An Event")
            .WithCustomId("create_event")

            .AddComponents(new TextInputComponent("Name", "name", "Mangos", null, true, DSharpPlus.TextInputStyle.Short, 1, 20))
            .AddComponents(new TextInputComponent("Event Description", "desc", "in my pants.", null, true, DSharpPlus.TextInputStyle.Paragraph, 1, 200))
            .AddComponents(new TextInputComponent("Week of? (Mon)", "start_mon", "ie. 8/14", null, true, DSharpPlus.TextInputStyle.Short, 1, 20));

            await ctx.CreateResponseAsync(InteractionResponseType.Modal, mb);
        }
    }
}