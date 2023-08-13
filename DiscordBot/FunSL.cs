using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
//using DSharpPlus.Entities;
using System.Reflection;
using System.Runtime.InteropServices;
using SlashCommandAttribute = DSharpPlus.SlashCommands.SlashCommandAttribute;
using InteractionContext = DSharpPlus.SlashCommands.InteractionContext;
using TextInputStyle = Discord.TextInputStyle;
using TextInputComponent = DSharpPlus.Entities.TextInputComponent;
using InteractionResponseType = DSharpPlus.InteractionResponseType;

namespace DiscordBot
{

    public class FunSL: ApplicationCommandModule
    { 
        [SlashCommand("rat", "receive ping")]
        //[Option("string", "Type something")] string text
        public async Task HandlePing(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync("RAT");
        }

        [SlashCommand("create", "Create an event.")]
        public async Task CreateEvent(InteractionContext ctx)
        {
            var modal = new DiscordInteractionResponseBuilder()
                 .WithTitle("Test Modal")
                 .WithCustomId("modal")
                 .AddComponents(new TextInputComponent("Random", "randomTextBox", "Type something here"));

            await ctx.CreateResponseAsync(InteractionResponseType.Modal, modal);
        }
    }
}