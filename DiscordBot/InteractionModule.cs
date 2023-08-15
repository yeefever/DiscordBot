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

namespace DiscordBot
{

    public class InteractionModule : ApplicationCommandModule
    {
        [SlashCommand("Channelcheck", "Channel")]
        public async Task MyCommand(InteractionContext ctx)
        {
            // Get the channel that calls the Slash Command
            DiscordChannel channel = ctx.Channel;

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"This command was called in channel {channel.Mention}"));
        }

        [SlashCommand("rat", "receive ping")]
        //[Option("string", "Type something")] string text
        public async Task HandlePing(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync("RAT");
        }

        [SlashCommand("create", "Create an event.")]
        public async Task CreateEvent(InteractionContext ctx, [Option("InviteDudes", "ping the dudes please :)")] string input)
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
            var filePath = "temp.json";
            File.WriteAllText(filePath, json);

            var mb = new DiscordInteractionResponseBuilder()
            .WithTitle("Create An Event")
            .WithCustomId("create_event")

            .AddComponents(new TextInputComponent("Event id", "id", "No spaces please :)", null, true, DSharpPlus.TextInputStyle.Short, 1, 20))
            .AddComponents(new TextInputComponent("Name", "name", "Mangos", null, true, DSharpPlus.TextInputStyle.Short, 1, 20))
            .AddComponents(new TextInputComponent("Event Description", "desc", "in my pants.", null, true, DSharpPlus.TextInputStyle.Paragraph, 1, 200))
            .AddComponents(new TextInputComponent("Week of? (Mon)", "start_mon", "ie. 8/14", null, true, DSharpPlus.TextInputStyle.Short, 1, 20));

            await ctx.CreateResponseAsync(InteractionResponseType.Modal, mb);
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


            var response = new DiscordInteractionResponseBuilder()
            .AddEmbed(embed);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
        }

        [SlashCommand("avail", "Show availability for given event")]
        public async Task DisplayAvail(InteractionContext ctx, [ChoiceProvider(typeof(InviteDudesChoiceProvider))][Option("event_id", "event id in question")] string option)
        {
            //Choice provider for number of events. 

            //Call a read on the nubmer of files and grab their file names and descriptions briefly. 


            await ctx.CreateResponseAsync("bonk");

            var channel = ctx.Interaction.Channel;
            Console.WriteLine("CHANNEL ID: " + channel.Id.ToString());

            Dictionary<String, ulong[]> map = null;
            foreach (KneeEvent ev in Program.events)
            {
                Console.WriteLine(ev.event_id + ":" + option + ".");
                if (ev.event_id == option)
                {
                    map = ev.userIdMessageId;
                }
            }

            DataColumn[] dataColumns = new DataColumn[map.Count];
            var i = 0;
            foreach (var kvp in map)
            {
                await Program.getReactions(kvp.Value[1], kvp.Value[2]);
                dataColumns[i] = new DataColumn();
                dataColumns[i].username = kvp.Key;
                for (var j = 0; j < Program.temp_table.Count(); j++)
                {
                    string emoji = Program.temp_table[j] ? "✅" : "🟡";
                    switch (j)
                    {
                        case 0:
                            dataColumns[i].mon = emoji; break;
                        case 1:
                            dataColumns[i].tue = emoji; break;
                        case 2:
                            dataColumns[i].wed = emoji; break;
                        case 3:
                            dataColumns[i].thu = emoji; break;
                        case 4:
                            dataColumns[i].fri = emoji; break;
                        case 5:
                            dataColumns[i].sat = emoji; break;
                        case 6:
                            dataColumns[i].sun = emoji; break;
                    }
                }
                i += 1;
            }


            AvailabilityData availabilityData = new AvailabilityData
            {
                title = "Availability",
                columns = new List<Column>
                    {
                        new Column { width = 150, title = "Username", dataIndex = "username", align = "right" },
                        new Column { width = 50, title = "Mon", dataIndex = "mon", align = "right" },
                        new Column { width = 50, title = "Tue", dataIndex = "tue", align = "right" },
                        new Column { width = 50, title = "Wed", dataIndex = "wed", align = "right" },
                        new Column { width = 50, title = "Thu", dataIndex = "thu", align = "right" },
                        new Column { width = 50, title = "Fri", dataIndex = "fri", align = "right" },
                        new Column { width = 50, title = "Sat", dataIndex = "sat", align = "right" },
                        new Column { width = 50, title = "Sun", dataIndex = "sun", align = "right" }
                     },
                dataSource = new List<object>
                    {
                        '-',
                    }
            };

            foreach (DataColumn d in dataColumns)
            {
                availabilityData.dataSource.Add(d);
            }

            string json = JsonConvert.SerializeObject(availabilityData, Formatting.Indented);

            Console.WriteLine(json);

            using (HttpClient client = new HttpClient())
            {
                string apiUrl = $"https://api.quickchart.io/v1/table?data={json}";

                var response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var imageStream = await response.Content.ReadAsStreamAsync();
                    var imagePath = "C:\\Users\\kliu3\\source\\repos\\DiscordBot\\DiscordBot\\image.png";

                    using (var fileStream = File.Create(imagePath))
                    {
                        await imageStream.CopyToAsync(fileStream);
                    }

                    FileStream file = new FileStream(imagePath, FileMode.Open);
                    DiscordMessageBuilder messagefile = new DiscordMessageBuilder();
                    messagefile.AddFile(file);
                    await channel.SendMessageAsync(messagefile);
                    file.Close();

                }
            }
        }

        [SlashCommand("delete", "Delete an event")]
        public async Task DeleteEvent(InteractionContext ctx, [ChoiceProvider(typeof(InviteDudesChoiceProvider))][Option("event_id", "event id in question")] string option)
        {
            //Check if the dude is the original creator. 
            KneeEvent cur = null;
            foreach(var e in Program.events)
            {
                if (option == e.event_id)
                    cur = e;
            }
            if(cur.creator_id != ctx.User.Id)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Not the event creator, dawg"));
                return;
            }

            string filePath = "C:\\Users\\kliu3\\source\\repos\\DiscordBot\\DiscordBot\\events\\";
            filePath += $"{option}.json";

            Console.WriteLine(filePath);

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine("File deleted successfully.");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Event Deleted Successfully"));
                    Program.deleteEvent(option);
                }
                else
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Event Doesn't Exist, Clown"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Something broke. All good though."));
            }          
        }

        /*
            [SlashCommand("invite", "Posthumous Invite")]
            public async Task Invite (InteractionContext ctx, )
            {
                //          /invite <event id> <@username> 
            }*/


            /*
             * public async Task ChoiceProviderCommand(InteractionContext ctx,
        [ChoiceProvider(typeof(TestChoiceProvider))]
        [Option("option", "option")] string option)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(option));
    }*/



    }
}

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
