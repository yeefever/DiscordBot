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
using Newtonsoft.Json;
using Discord.Rest;
using DSharpPlus.CommandsNext;
using DiscordBot;
using System.Net.Http;
using System.Threading.Channels;
using Discord.Rest;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System.Reflection.Metadata.Ecma335;
using AutocompleteAttribute = DSharpPlus.SlashCommands.AutocompleteAttribute;
using DSharpPlus.CommandsNext.Attributes;

namespace DiscordBot
{

    public class InteractionModule : ApplicationCommandModule
    {

        [SlashCommand("ping", "secret message. very cool!")]
        //[Option("string", "Type something")] string text
        public async Task HandlePing(InteractionContext ctx)
        {
            var responseBuilder = new DiscordInteractionResponseBuilder()
             .WithContent("You are a rat.")
             .AsEphemeral(true);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder);
        }

        [SlashCommand("create", "Create an event.")]
        public async Task CreateEvent(InteractionContext ctx, [Option("InviteDudes", "ping the dudes please :)")] string input)
        {

            if (Program.isCommandLocked[2])
            {
                var responseBuilder = new DiscordInteractionResponseBuilder()
                    .WithContent("Command on cooldown.")
                    .AsEphemeral(true);

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder);
                return;
            }

            await Program.LockCommand(2);

            try
            {

                Console.WriteLine(input);

                //parse members 

                string pattern = @"<@(\d+)>";
                var userIds = new List<string>();
                MatchCollection matches = Regex.Matches(input, pattern);
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                    {
                        string userId = match.Groups[1].Value;
                        Console.WriteLine(userId);
                        userIds.Add(userId);
                    }
                }

                var jsonObject = new
                {
                    users = userIds
                };



                string json = JsonConvert.SerializeObject(jsonObject);
                var filePath = $"{Program.path}events\\temp.json";
                File.WriteAllText(filePath, json);

                var mb = new DiscordInteractionResponseBuilder()
                .WithTitle("Create An Event")
                .WithCustomId("create_event")

                .AddComponents(new TextInputComponent("Event id", "id", "No spaces please :)", null, true, DSharpPlus.TextInputStyle.Short, 1, 20))
                .AddComponents(new TextInputComponent("Name", "name", "Mangos", null, true, DSharpPlus.TextInputStyle.Short, 1, 20))
                .AddComponents(new TextInputComponent("Event Description", "desc", "in my pants.", null, true, DSharpPlus.TextInputStyle.Paragraph, 1, 200))
                .AddComponents(new TextInputComponent("Start date?", "start_date", "ie. 08/14 MM/DD", null, true, DSharpPlus.TextInputStyle.Short, 1, 5));
                await ctx.CreateResponseAsync(InteractionResponseType.Modal, mb);
            }
            finally
            {
                Program.UnlockCommand(2);
            }
        }

        [SlashCommand("showevents", "Show current events.")]
        public async Task DisplayEvents(InteractionContext ctx)
        {
            String msg = "**Event_id : Event_name**\n";
            foreach (KneeEvent i in Program.events)
            {
                msg += i.event_id + ": " + i.event_name + "\n";
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = "Available Events",
                Color = new DiscordColor(0xFFC5DB)
            };

            string eventList = string.Join("\n", Program.events.Select(e => $"• **{e.event_name}** (ID: {e.event_id})"));
            embed.Description = eventList;

            if (Program.events.Count == 0)
            {
                embed.Description = "There are no available events.";
            }

            var response = new DiscordInteractionResponseBuilder()
            .AddEmbed(embed);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
        }

        [SlashCommand("avail", "Show availability for given event")]
        public async Task DisplayAvail(InteractionContext ctx, [ChoiceProvider(typeof(InviteDudesChoiceProvider))][Option("event_id", "event id in question"),] string option)
        {
            KneeEvent cur = null;
            foreach (var ev in Program.events)
            {
                if(option == ev.event_id)
                {
                    cur = ev;
                    break;
                }
            }

            if(!File.Exists($"{Program.path}avail_images\\{option}.png"))
            {
                //file no existe  :(

                var badResponseBuilder = new DiscordInteractionResponseBuilder()
                 .WithContent("File still updating. Check back in 15.")
                 .AsEphemeral(true);

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, badResponseBuilder);
                return;
            }

            // Pull from the reading 
            FileStream file = new FileStream($"{Program.path}avail_images\\{option}.png", FileMode.Open);
            DiscordMessageBuilder messagefile = new DiscordMessageBuilder();
            messagefile.AddFile(file);

            var diff = DateTime.Now - cur.lastUpdate;

            var responseBuilder = new DiscordInteractionResponseBuilder()
               .AsEphemeral(true);

            if (diff.Hours == 0)
            {
                responseBuilder.Content = $"sending image ... last updated {diff.Minutes} minutes and {diff.Seconds} seconds ago";
            }
            else
            {
                responseBuilder.Content = $"sending image ... last updated {diff.Hours} hours and {diff.Minutes} minutes ago";
            }

            await ctx.CreateResponseAsync(responseBuilder);
            await ctx.Channel.SendMessageAsync(messagefile);

            file.Close();
        }

        [SlashCommand("desc", "Event Description")]
        public async Task DescEvent(InteractionContext ctx, [ChoiceProvider(typeof(InviteDudesChoiceProvider))][Option("event_id", "event id in question")] string option)
        {
            //Check if the dude is the original creator. 
            KneeEvent cur = null;
            foreach (var e in Program.events)
            {
                if (option == e.event_id)
                    cur = e;
            }

            //create an embed.

            var embed = new DiscordEmbedBuilder
            {
                Title = cur.event_name,
                Color = new DiscordColor(0xFFC5DB)
            };


            DateTime currentDate = DateTime.ParseExact(cur.start_date, "MM/dd", null);
            DateTime futureDate = currentDate.AddDays(10);

            string desc = $"**Event Desc:** {cur.event_desc}\n *{currentDate:MM/dd} - {futureDate:MM/dd}*\n**Invited: **";
            var i = 0;
            foreach (var kvp in cur.userIdMessageId)
            {
                desc += kvp.Key;
                if (i != cur.userIdMessageId.Count - 1)
                    desc += ", ";
                i += 1;
            }
            embed.Description = desc;
            var response = new DiscordInteractionResponseBuilder()
            .AddEmbed(embed);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
        }

        [SlashCommand("delete", "Delete an event")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task DeleteEvent(InteractionContext ctx, [ChoiceProvider(typeof(InviteDudesChoiceProvider))][Option("event_id", "event id in question")] string option)
        {
            if (Program.isCommandLocked[1])
            {
                await ctx.CreateResponseAsync("Sorry, this command is currently in cooldown : ( .");
                return;
            }

            await Program.LockCommand(1);

            try
            {

                //Fix typ
                KneeEvent cur = null;
                foreach (var e in Program.events)
                {
                    if (option == e.event_id)
                        cur = e;
                }
                if (cur.creator_id != ctx.User.Id)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Not the event creator, dawg"));
                    return;
                }

                /*foreach (var kvp in cur.userIdMessageId)
                {
                    await Program.DeleteMessage(kvp.Value[1], kvp.Value[2]);
                }*/


                string filePath = $"{Program.path}events\\";
                filePath += $"{option}.json";

                Console.WriteLine(filePath);

                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);

                        //clear the png too
                        var imageFilePath = $"{Program.path}avail_images\\{option}.json";

                        if(File.Exists(imageFilePath))
                        {
                            File.Delete(imageFilePath);
                        } 

                        

                        var responseBuilder = new DiscordInteractionResponseBuilder()
                            .WithContent("Event Deleted Successfully.")
                            .AsEphemeral(true);

                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder);

                        Program.deleteEvent(option);
                    }
                    else
                    {
                        var responseBuilder = new DiscordInteractionResponseBuilder()
                            .WithContent("Event doesn't exist clown.")
                            .AsEphemeral(true);

                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    var responseBuilder = new DiscordInteractionResponseBuilder()
                            .WithContent("Something broke : ) .")
                            .AsEphemeral(true);

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder);
                }


                Program.Reconnect();
            }
            finally
            {
                Program.UnlockCommand(1);
            }
        }

        [SlashCommand("invite", "Posthumous Invite")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task Invite(InteractionContext ctx, [ChoiceProvider(typeof(InviteDudesChoiceProvider))][Option("event_id", "event id in question")] string option, [Option("Users", "ping the dudes please :)")] string users)
        {

            // Read the file associated
            string filePath = $"{Program.path}events\\";
            filePath += $"{option}.json";

            Console.WriteLine(filePath);

            try
            {
                if (File.Exists(filePath))
                {
                    KneeEvent cur = null;
                    foreach (KneeEvent e in Program.events)
                    {
                        if (e.event_id == option)
                        {
                            cur = e;
                            break;
                        }
                    }

                    string pattern = @"<@(\d+)>";
                    var userIds = new List<string>();
                    List<String> usersToInvite = new List<String>();
                    MatchCollection matches = Regex.Matches(users, pattern);
                    foreach (Match match in matches)
                    {
                        if (match.Groups.Count > 1)
                        {

                            string userId = match.Groups[1].Value;
                            usersToInvite.Add(userId);
                            bool temp_b = false;
                            foreach (var kvp in cur.userIdMessageId)
                            {
                                if (kvp.Value[0] == ulong.Parse(userId)) { temp_b = true; break; }
                            }
                            if (temp_b)
                            {
                                var responseBuilder2 = new DiscordInteractionResponseBuilder()
                                .WithContent($"User <@{userId}> already invited clown. Try again")
                                .AsEphemeral(true);

                                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder2);
                                return;
                            }
                        }
                    }

                    Program.cur_event = cur;

                    var responseBuilder = new DiscordInteractionResponseBuilder()
                                .WithContent($"Invited sending!")
                                .AsEphemeral(true);

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder);

                    string msg = "";
                    foreach (String user in usersToInvite)
                    {
                        await Program.SendScheduleMessage(ctx.Interaction.Guild.Id, ulong.Parse(user), ctx.Interaction.Channel);
                        msg += $"<@{user}>";
                    }

                }
                else
                {
                    var responseBuilder = new DiscordInteractionResponseBuilder()
                                .WithContent($"Event doesn't exist bozo")
                                .AsEphemeral(true);

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("BRUH" + ex.ToString());
            }
        }

        /*
         * public async Task ChoiceProviderCommand(InteractionContext ctx,
    [ChoiceProvider(typeof(TestChoiceProvider))]
    [Option("option", "option")] string option)
{
    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(option));
}*/ }

    public class InviteDudesChoiceProvider : IChoiceProvider
    {
        public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
        {
            DiscordApplicationCommandOptionChoice[] ret = new DiscordApplicationCommandOptionChoice[Program.events.Count];
            for (var i = 0; i < Program.events.Count; i++)
            {
                ret[i] = new DiscordApplicationCommandOptionChoice(Program.events[i].event_name + ": " + Program.events[i].event_id, Program.events[i].event_id);
            }
            return ret;
        }
    }

    public class eventProvider : IAutocompleteProvider
    {
        public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            DiscordAutoCompleteChoice[] choices = new DiscordAutoCompleteChoice[2]
            {
                new DiscordAutoCompleteChoice("Temp option", "temp descriptipion"),
                new DiscordAutoCompleteChoice("Temp option1", "temp descriptipion1")
            };

            return Task.FromResult<IEnumerable<DiscordAutoCompleteChoice>>(choices);
        }
    }

}

