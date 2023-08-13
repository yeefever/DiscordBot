using Discord;
using Discord.Interactions;
using Discord.WebSocket;
//using DSharpPlus.Entities;
using System.Reflection;
using System.Runtime.InteropServices;
using DSharpPlus.SlashCommands;
using SlashCommandAttribute = DSharpPlus.SlashCommands.SlashCommandAttribute;

namespace DiscordBot
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("rat", "receive ping")]
        //[Option("string", "Type something")] string text
        public async Task HandlePing()
        {
            await RespondAsync("RAT"); 
        }

        [SlashCommand("create", "Create an event.")]
        public async Task CreateEvent()
        {
            var mb = new ModalBuilder()
            .WithTitle("Create An Event")
            .WithCustomId("create_event")
            .AddTextInput("Event Name", "name", placeholder: "Mangos")
            .AddTextInput("Short event description", "desc", TextInputStyle.Paragraph,
            "in my pants.")
            .AddTextInput("Week of? (Mon)", "start_mon", placeholder: "ie. 8/14");

            await Context.Interaction.RespondWithModalAsync(mb.Build());
        }


    }
}