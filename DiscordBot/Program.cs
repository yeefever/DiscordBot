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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Runtime.ConstrainedExecution;
using System.Formats.Asn1;

namespace DiscordBot
{
    class Program : InteractionModuleBase<SocketInteractionContext>
    {
        //variables

        static void Main(string[] args) => new Program().RunMainAsync().GetAwaiter().GetResult();

        private static DiscordClient _client { get; set; }
        public static KneeEvent cur_event;
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
        private static SlashCommandsExtension slashCommandsConfig;
        private string filePath;
        public static bool[] isCommandLocked = {false, false, false};
        private static readonly SemaphoreSlim createLock = new SemaphoreSlim(2);
        private static readonly SemaphoreSlim deleteLock = new SemaphoreSlim(3);


        //TO DO: FIX TYPING LATER. MAP <event_id> -> event object

        public static List<KneeEvent> events = new List<KneeEvent>();

        public  List<LogMessage> debugLog = new List<LogMessage>();
        public static Boolean[] temp_table = new bool[10];
        public static string path = null;
        private static Timer _timer;

        public Program()
        {
        }

        public async Task RunMainAsync()
        {

            var configFileContent = File.ReadAllText("C:\\Users\\kliu3\\source\\repos\\DiscordBot\\DiscordBot\\config_file.json");
            JsonDocument configDocument = JsonDocument.Parse(configFileContent);
            JsonElement root = configDocument.RootElement;
            string config_token = root.GetProperty("token").GetString();
            path = root.GetProperty("path").ToString();
           
            var config = new DiscordConfiguration()
            {
                Token = config_token,
                //MinimumLogLevel = LogLevel.Trace,
                TokenType = TokenType.Bot,
                AutoReconnect = true
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

            for (var i = 0; i < 10; i++)
            {
                temp_table[i] = false;
            }

            readAll();

           slashCommandsConfig = _client.UseSlashCommands();
            slashCommandsConfig.RegisterCommands<InteractionModule>(1137843173050302564);
            slashCommandsConfig.RegisterCommands<InteractionModule>(1016229761195974698);
           // slashCommandsConfig.RegisterCommands<InteractionModule>();

            //Connect to the Client and get the Bot online
            _client.ConnectAsync().GetAwaiter().GetResult();
            Task.Delay(-1).GetAwaiter().GetResult();
        }

        public static async void Reconnect()
        {
           await _client.DisconnectAsync();
           await _client.ConnectAsync();
        }

        private Task _client_ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs args)
        {
            throw new NotImplementedException();
        }

        public static void readAll()
        {
            //Read all the event_ids 
            string directoryPath = $"{path}events";

            events.Clear();
            if (Directory.Exists(directoryPath))
            {
                string[] fileNames = Directory.GetFiles(directoryPath);

                foreach (string fileName in fileNames)
                {
                    if (fileName == $"{path}events\\temp.json") continue;
                    string json = File.ReadAllText(fileName);
                    events.Add(JsonConvert.DeserializeObject<KneeEvent>(json));
                }
            }
            else
            {
                Console.WriteLine("Directory does not exist.");
            }
        }
        public static async Task LockCommand(int i)
        {
            switch (i)
            {
                case 1: await deleteLock.WaitAsync();
                    break;
                case 2: await createLock.WaitAsync();
                    break;
            }
            isCommandLocked[i] = true;
        }

        public static async void UnlockCommand(int i)
        {
            switch (i)
            {
                case 1:
                    deleteLock.Release();
                    break;
                case 2:
                    createLock.Release();
                    break;
            }
            await Task.Delay(20000);
            isCommandLocked[i] = false;
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

        private static async Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            await readAvail();

            // Schedule the async method to run every 10 minutes
            _timer = new Timer(async _ => await readAvail(), null, TimeSpan.Zero, TimeSpan.FromMinutes(10));
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

            List<string> emojis = new List<string> { ":one:", ":two:", ":three:", ":four:", ":five:", ":six:", ":seven:", ":eight:", ":nine:", ":zero:"};
            for (var i = 0; i < 10; i++) 
            {
                Console.WriteLine($"EMOJI :{emojis[i]}");
                try
                {
                    var reactions = await message.GetReactionsAsync(DiscordEmoji.FromName(_client, emojis[i]));
                    if (reactions.Count > 1)
                    {
                        temp_table[i] = true;
                    }
                    await Task.Delay(4000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("BRUH : " + ex.ToString());
                }
            }

        }

        private static async Task ModalEventHandler(DiscordClient sender, ModalSubmitEventArgs e)
        {
            try
            {
                if (e.Interaction.Type == InteractionType.ModalSubmit && e.Interaction.Data.CustomId == "create_event")
                {
                    var values = e.Values;
                    List<string> userids = null;

                    string filePath = $"{path}events\\temp.json";
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


                    cur_event = new KneeEvent()
                    {
                        event_name = values["name"],
                        event_desc = values["desc"],
                        start_date = values["start_date"],
                        creator_id = e.Interaction.User.Id,
                        event_id = values["id"],
                        userIdMessageId = new Dictionary<string, ulong[]>(),
                        lastUpdate = DateTime.Now
                    };

                    try
                    {
                        DateTime currentDate = DateTime.ParseExact(values["start_date"], "MM/dd", null);
                    }
                    catch (Exception ex)
                    {
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Invalid date format"));
                        return;
                    }

                    string jsone = JsonConvert.SerializeObject(cur_event, Formatting.Indented);
                    filePath = $"{path}events\\{values["id"]}.json";
                    try
                    {
                        File.WriteAllText(filePath, jsone);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc.ToString());
                    }

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{e.Interaction.User.Username} submitted an event : {values.Values.First()}"));

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
                            
                            await SendScheduleMessage(e.Interaction.Guild.Id, ulong.Parse(user_id), e.Interaction.Channel);

                        }
                        catch (Exception ex)
                        {
                            await e.Interaction.Channel.SendMessageAsync($"Cannot send DM to someone out of server.");
                            await e.Interaction.Channel.SendMessageAsync($"Failed to send the message to {user.Username}#{user.Discriminator}: {ex.Message}");
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("MODAL RECEIVED ERROR: " + ex.ToString());
            }
            readAll();
            Program.Reconnect();
        }

        public static async Task DeleteMessage(ulong c, ulong m)
        {
            DiscordChannel channel = await _client.GetChannelAsync(c);
            DiscordMessage message = await channel.GetMessageAsync(m);

            if (message != null)
            {
                await message.DeleteAsync();
                Console.WriteLine($"Message with ID {m} deleted.");
            }
            else
            {
                Console.WriteLine($"Message with ID {m} not found.");
            }
        }


        //client log
        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            string json = JsonConvert.SerializeObject(arg, Formatting.Indented);

            // Specify the path for the output JSON file
            string outputPath = $"{path}log.json";
            // Write the JSON to the file
            File.AppendAllText(outputPath, json);
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


        public static async Task readAvail()
        {
            //Choice provider for number of events. 
            //Call a read on the nubmer of files and grab their file names and descriptions briefly.

            DateTime currentDate = DateTime.Now;
            foreach (KneeEvent ev in Program.events)
            {
                Dictionary<String, ulong[]> map = ev.userIdMessageId;
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
                        string temp = $"day{(j % 10).ToString()}";
                        switch (j)
                        {
                            case 0:
                                dataColumns[i].day0 = emoji; break;
                            case 1:
                                dataColumns[i].day1 = emoji; break;
                            case 2:
                                dataColumns[i].day2 = emoji; break;
                            case 3:
                                dataColumns[i].day3 = emoji; break;
                            case 4:
                                dataColumns[i].day4 = emoji; break;
                            case 5:
                                dataColumns[i].day5 = emoji; break;
                            case 6:
                                dataColumns[i].day6 = emoji; break;
                            case 7:
                                dataColumns[i].day7 = emoji; break;
                            case 8:
                                dataColumns[i].day8 = emoji; break;
                            case 9:
                                dataColumns[i].day9 = emoji; break;
                        }
                    }
                    i += 1;
                }


                AvailabilityData availabilityData = new AvailabilityData
                {
                    title = "Availability",
                    columns = new List<Column>
                        {
                            new Column { width = 150, title = "Username", dataIndex = "username", align = "right" }
                         },
                    dataSource = new List<object>
                        {
                            '-',
                        }
                };

                for (var j = 0; j < 10; j++)
                {
                    availabilityData.columns.Add(new Column { width = 70, title = $"{currentDate:MM/dd}", dataIndex = $"day{j}", align = "right" });
                    currentDate = currentDate.AddDays(1);
                }

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
                        var imagePath = $"{Program.path}avail_images\\{ev.event_id}.png";

                        using (var fileStream = File.Create(imagePath))
                        {
                            await imageStream.CopyToAsync(fileStream);
                        }
                    }
                }
                ev.lastUpdate = DateTime.Now;
            }
        }



        public static async Task SendScheduleMessage(ulong guildId, ulong userId, DiscordChannel failChannel)
        {
            var guild = await _client.GetGuildAsync(guildId);
            DiscordMember member = null;
            if (guild != null)
            {
                Console.WriteLine("GUILD: " + guild.Name.ToString());
                member = await guild.GetMemberAsync(userId);
            }
            else
            {
                await failChannel.SendMessageAsync($"Failed to send invite to <@{userId}>");
                return;
            }


            var DMChannel = await member.CreateDmChannelAsync();
            Console.WriteLine(DMChannel.Id);


            var embed = new DiscordEmbedBuilder
            {
                Title = $"{cur_event.event_name} Availability Check",
                Color = new DiscordColor(0xFFC5DB)
            };

            string message = $"Hey {member.Username}, checking availability for {cur_event.event_name}\n" +
                        $"• **Description:** {cur_event.event_desc}\n" +
                        $"• **Start Date:** {cur_event.start_date}\n" +
                        $"*Each number below corresponds to a date on the table. React for availability. \n";


            DateTime currentDate = DateTime.ParseExact(cur_event.start_date, "MM/dd", null);


            message +=  ("Day   |   Date\n");
            message += ("---------------\n");

            for (int i = 1; i <= 10; i++)
            {
                if(i == 10)
                    message += ($"**0** |   {currentDate:MM/dd}\n");
                else
                    message += ($"**{i,-5}** |   {currentDate:MM/dd}\n");
                currentDate = currentDate.AddDays(1);
            }
            
            embed.Description = message;


            var tempMessage = await DMChannel.SendMessageAsync(embed: embed);

            cur_event.userIdMessageId.Add(member.Username, new ulong[] {member.Id, tempMessage.Channel.Id, tempMessage.Id } );

            //Rewrite to JSON

            string jsone = JsonConvert.SerializeObject(cur_event, Formatting.Indented);
            var filePath = $"{path}events\\{cur_event.event_id}.json";
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

            List<string> emojis = new List<string> { ":one:", ":two:", ":three:", ":four:", ":five:", ":six:", ":seven:", ":eight:", ":nine:", ":zero:"  };
            foreach (string s in emojis)
            {
                Console.WriteLine($"EMOJI :{s}");
                await tempMessage.CreateReactionAsync(DiscordEmoji.FromName(_client, s));
                //await Task.Delay(1000);
            } 
            Console.WriteLine("DONE");
        }
    }
}
