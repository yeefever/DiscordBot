using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Text.Json;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Reactive.Joins;
using System.Text.RegularExpressions;
using Discord.Interactions;
using Discord.Net;
using Newtonsoft.Json;
using System.Diagnostics;
using OfficeOpenXml;
using System.Numerics;
using System.Net.Http;
using System.Threading.Channels;
using System.Drawing.Printing;
using Microsoft.Extensions.Primitives;
using Discord.Rest;
using System.Text.Json;
using System.Diagnostics.Metrics;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Newtonsoft.Json.Serialization;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System.Collections;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using TokenType = DSharpPlus.TokenType;
using Newtonsoft.Json.Linq;
using DSharpPlus.EventArgs;
using System.Security.Cryptography.X509Certificates;
using DSharpPlus.Entities;
using InteractionType = DSharpPlus.InteractionType;
using InteractionResponseType = DSharpPlus.InteractionResponseType;
using static OfficeOpenXml.ExcelErrorValue;
using ActivityType = Discord.ActivityType;
using UserStatus = Discord.UserStatus;
using System.Text.Json.Nodes;
using static Microsoft.IO.RecyclableMemoryStreamManager;
using System.Diagnostics.Contracts;

namespace DiscordBot
{
    class Program : InteractionModuleBase<SocketInteractionContext>
    {
        //variables
        static void Main(string[] args) => new Program().RunMainAsync().GetAwaiter().GetResult();

        private static DiscordClient _client { get; set; }
        private static KneeEvent cur_event;
        private static CommandsNextExtension Commands { get; set; }
        public CommandService _commands;
        private IServiceProvider _services;

        private IUser user;
        private List<ULongPair> messages = new List<ULongPair>();

        private SocketGuild guild;
             
        private ulong LogChannelID;
        private SocketTextChannel LogChannel;
        private Dictionary<ulong, ULongPair> pmap;
        private List<ulong> users;
        private string filePath;

        public static List<KneeEvent> events = new List<KneeEvent>();


        public static Boolean[] temp_table = new bool[7];

        public Program()
        {
        }

        public async Task RunMainAsync()
        {

            var config = new DiscordConfiguration()
            {
                Token = "MTEzNzgxNTcxNjU1MzMxMDIxOA.GNH7W0.qTeSrd8ke1NoFP3oIv_acZU9pxmcfB0xEYebqE", // Event v1 bot
                TokenType = TokenType.Bot
            };

            //Initializing the client with this config
            _client = new DiscordClient(config);

            //Setting our default timeout for Interactivity based commands
            _client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(2)
            });

            //EVENT HANDLERS
            _client.Ready += OnClientReady;
            //_client.ComponentInteractionCreated += InteractionEventHandler;
            _client.ModalSubmitted += ModalEventHandler;
           /* _client.MessageCreated += MessageSendHandler;
            _client.ModalSubmitted += ModalEventHandler;
            _client.VoiceStateUpdated += VoiceChannelHandler;
            _client.GuildMemberAdded += UserJoinHandler;*/

            var commandsConfig = new CommandsNextConfiguration()
            {
                EnableDms = true,
                EnableDefaultHelp = false,
            };

            Commands = _client.UseCommandsNext(commandsConfig);
            var slashCommandsConfig = _client.UseSlashCommands();

            for (var i = 0; i < 7; i++)
            {
                temp_table[i] = false;
            }

            filePath = "C:\\Users\\kliu3\\source\\repos\\DiscordBot\\DiscordBot\\message_ids.txt";
            user = null;
            users = new List<ulong>();
            pmap = new Dictionary<ulong, ULongPair>();


            //Read all the event_ids 
            string directoryPath = "C:\\Users\\kliu3\\source\\repos\\DiscordBot\\DiscordBot\\events";

            if (Directory.Exists(directoryPath))
            {
                string[] fileNames = Directory.GetFiles(directoryPath);

                foreach (string fileName in fileNames)
                {
                    string json = File.ReadAllText(fileName);
                    events.Add(JsonConvert.DeserializeObject<KneeEvent>(json));
                }
            }
            else
            {
                Console.WriteLine("Directory does not exist.");
            }


            slashCommandsConfig.RegisterCommands<InteractionModule>(1137843173050302564);
            slashCommandsConfig.RegisterCommands<InteractionModule>(1016229761195974698);

            //slashCommandsConfig.RegisterCommands<InteractionModule>();

            //Connect to the Client and get the Bot online
            _client.ConnectAsync().GetAwaiter().GetResult();
            Task.Delay(-1).GetAwaiter().GetResult();
        }

        public static void deleteEvent(String id)
        {
            foreach(KneeEvent e in events)
            {
                if(e.event_id == id)
                {
                    events.Remove(e);
                    return;
                }
            }

            Console.WriteLine("Event doesn't exist bruv");
        }

        private static Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            Console.WriteLine("Bot Ready");
            return Task.CompletedTask;
        }
            
        public static async Task getReactions(ulong channelid, ulong messageid)
        {

            //get message reacitons
            var channel = await _client.GetChannelAsync(channelid);
            DiscordMessage message = null;
            var dmChannel = channel as DiscordDmChannel;
            if (dmChannel != null)
            {
                message = await dmChannel.GetMessageAsync(messageid);
                if (message != null)
                {
                    Console.WriteLine($"Message Content: {message.Content}");
                }
            }

            for (var i = 0; i < 7; i++)
            {
                temp_table[i] = false;
            }

            List<string> emojis = new List<string> { ":one:", ":two:", ":three:", ":four:", ":five:", ":six:", ":seven:" };
            for (var i = 0; i < 7; i++) 
            {
                Console.WriteLine($"EMOJI :{emojis[i]}");
                var reactions = await message.GetReactionsAsync(DiscordEmoji.FromName(_client, emojis[i]));
                if(reactions.Count > 1)
                {
                    temp_table[i] = true;
                }
                await Task.Delay(1000);
            }

        }

        private static async Task ModalEventHandler(DiscordClient sender, ModalSubmitEventArgs e)
        {
            try
            {
                Program tempP = new Program();
                if (e.Interaction.Type == InteractionType.ModalSubmit && e.Interaction.Data.CustomId == "create_event")
                {
                    string filePath = "temp.json";
                    List<string> userids = null;
                    try
                    {
                        string json = File.ReadAllText(filePath);
                        dynamic jsonObject = JsonConvert.DeserializeObject(json);
                        userids = jsonObject.users.ToObject<List<string>>();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error reading or deserializing JSON: " + ex.Message);
                    }

                    var values = e.Values;

                    //make new json object and write to actual file with name
                    cur_event = new KneeEvent()
                    {
                        event_name = values["name"],
                        event_desc = values["desc"],
                        start_date = values["start_mon"],
                        invited = userids,
                        creator_id = e.Interaction.User.Id,
                        event_id = values["id"],
                        userIdMessageId = new Dictionary<string, ulong[]>()
                    };

                    string jsone = JsonConvert.SerializeObject(cur_event, Formatting.Indented);
                    filePath = $"C:\\Users\\kliu3\\source\\repos\\DiscordBot\\DiscordBot\\events\\{values["id"]}.json";
                    try
                    {
                        File.WriteAllText(filePath, jsone);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc.ToString());
                    }

                    foreach (string user_id in userids)
                    {
                        ulong ulongValue = 0;
                        try
                        {
                            ulongValue = ulong.Parse(user_id);
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Invalid input format. Cannot convert to ulong.");
                        }
                        catch (OverflowException)
                        {
                            Console.WriteLine("Value is outside the range of ulong.");
                        }

                        var user = await _client.GetUserAsync(ulongValue);
                        if (user == null || ulongValue == 0)
                        {
                            await e.Interaction.Channel.SendMessageAsync("User not found.");
                            return;
                        }

                        try
                        {
                            var guild = await _client.GetGuildAsync(e.Interaction.Guild.Id);
                            if (guild != null)
                            {
                                Console.WriteLine("GUILD: " + guild.Name.ToString());
                                var member = await guild.GetMemberAsync(user.Id);
                                await e.Interaction.Channel.SendMessageAsync($"Sending DM invitation to : {member.Username}.");
                                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{e.Interaction.User.Username} submitted a modal with the input {values.Values.First()}"));
                                await tempP.SendScheduleMessage(member);
                            }
                            else
                            {
                                await e.Interaction.Channel.SendMessageAsync($"Cannot find the server in question you clown.");
                            }

                        }
                        catch (Exception ex)
                        {
                            await e.Interaction.Channel.SendMessageAsync($"Cannot send DM to someone out of server.");
                            await e.Interaction.Channel.SendMessageAsync($"Failed to send the message to {user.Username}#{user.Discriminator}: {ex.Message}");
                        }
                    }
                    //await Task.Delay(10000);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("MODAL RECEIVED ERROR: " + ex.ToString());
            }
        }


        //client log
        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        //Register Commands Async
        public async Task RegisterCommandsAsync()
        {
            //_client.MessageReceived += HandleCommandAsync;
            //await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            //await _commands.AddModuleAsync<Slash>(_services);
        }

        public async Task HandleCommandAsync(SocketMessage message)
        {
            if (message.Author.IsBot || message is not IUserMessage userMessage)
                return;

            var textChannel = message.Channel as ITextChannel;

            var fetchedMessage = await textChannel.GetMessageAsync(message.Id) as IUserMessage;

            if (fetchedMessage == null)
                return;

            string messageContent = fetchedMessage.Content;

            Console.WriteLine("Received message: " + messageContent);

            if (messageContent.StartsWith("!invite"))
            {
                // Check permission
                if (!(userMessage.Author is SocketGuildUser guildUser) || !guildUser.GuildPermissions.Administrator)
                {
                    await userMessage.Channel.SendMessageAsync("You don't have permission to use this command.");
                    return;
                }

                var parts = messageContent.Split(" ");
                string userId;
                if (parts.Length >= 2)
                {

                    Regex regex = new Regex(@"<@(\d+)>");
                    for (var i = 1; i < parts.Length; i++)
                    {

                        Match match = regex.Match(parts[i]);

                        // find if possible : (
                        if (match.Success)
                        {
                            userId = match.Groups[1].Value;
                        }
                        else
                        {
                            userId = parts[i];
                        }

                        //await SendDM(userId, userMessage);
                    }
                }
            }
        }

        public async Task CheckReactionsOfMessage(ulong userId)
        {
            /*ULongPair p = pmap[userId];

            for (int i = 0; i < 7; i++)
            {
                temp_table[i] = false;
            }

            IDMChannel channel = null;
                
               // (IDMChannel) await _client.GetDMChannelAsync(p.First);
            if (channel == null)
            {
                Console.WriteLine("CHANNEL NOT FOUND!!!");
                return;
            }
            else
            {
                Console.WriteLine("channel Located");
                IUserMessage message = (IUserMessage) await channel.GetMessageAsync(p.Second);
                Console.WriteLine("message located");
                for (var i = 0; i < 7; i++)
                {
                    // users
                    var users = await message.GetReactionUsersAsync(emotes[i], Int32.MaxValue).FlattenAsync();
                    Console.WriteLine("reactions located");

                    Console.WriteLine("EMOJI " + (i + 1).ToString());
                    foreach (IUser user in users)
                    {
                        if (user.Id == userId)
                        {
                            temp_table[i] = true;
                            Console.WriteLine($"User: {user.Username}#{user.Discriminator}");
                            continue;
                        }
                    }
                }
                foreach (Boolean b in temp_table)
                {
                    Console.WriteLine(b.ToString());
                }
            }*/


        }

        public async Task SendScheduleMessage(DiscordMember user)
        {
            var DMChannel = await user.CreateDmChannelAsync();
            Console.WriteLine(DMChannel.Id);


            var embed = new DiscordEmbedBuilder
            {
                Title = $"{cur_event.event_name} Availability Check",
                Color = new DiscordColor(0xFFC5DB)
            };

            string message = $"Hey {user.Username}, checking availability for {cur_event.event_name}\n" +
                        $"• **Description:** {cur_event.event_desc}\n" +
                        $"• **Start Date:** {cur_event.start_date}\n" +
                        $"*Start date is just the monday we start counting availability on. So, react with 1 if free on Monday, 2 for following tuesday and so on and so forth* \n" + 
                        "React below";

            embed.Description = message;


            var tempMessage = await DMChannel.SendMessageAsync(embed: embed);

            cur_event.userIdMessageId.Add(user.Username, new ulong[] {user.Id, tempMessage.Channel.Id, tempMessage.Id } );

            //Rewrite to JSON

            string jsone = JsonConvert.SerializeObject(cur_event, Formatting.Indented);
            filePath = $"C:\\Users\\kliu3\\source\\repos\\DiscordBot\\DiscordBot\\events\\{cur_event.event_id}.json";
            try
            {
                Console.WriteLine($"Written to {filePath}");
                Console.WriteLine(jsone);
                File.WriteAllText(filePath, jsone);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }

            List<string> emojis = new List<string> { ":one:", ":two:", ":three:", ":four:", ":five:", ":six:", ":seven:" };
            foreach (string s in emojis)
            {
                Console.WriteLine($"EMOJI :{s}");
                await tempMessage.CreateReactionAsync(DiscordEmoji.FromName(_client, s));
                await Task.Delay(1000);
            } 
            Console.WriteLine("DONE");
        }
    }
}
