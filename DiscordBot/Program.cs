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

namespace DiscordBot
{
    class Program : InteractionModuleBase<SocketInteractionContext>
    {
        //variables
        static void Main(string[] args) => new Program().RunMainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        public CommandService _commands;
        private IServiceProvider _services;

        private IUser user;
        private List<string> emojis = new List<string>();
        private List<IEmote> emotes = new List<IEmote>();
        private List<ULongPair> messages = new List<ULongPair>();

        private SocketGuild guild;
             
        private ulong LogChannelID;
        private SocketTextChannel LogChannel;
        private Dictionary<ulong, ULongPair> pmap;
        private List<ulong> users;
        private string filePath;

        private Boolean[] temp_table = new bool[7];

        public Program()
        {
        }

        public async Task RunMainAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _client.Log += _client_Log;
            _client.ReactionAdded += OnReactionAdded;
            _client.MessageReceived += OnMessageReceived;

            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
            services
            // Add the configuration to the registered services
            // Add the DiscordSocketClient, along with specifying the GatewayIntents and user caching
            .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = Discord.GatewayIntents.AllUnprivileged,
                AlwaysDownloadUsers = true,
                LogLevel = Discord.LogSeverity.Debug
            }))
            // Adding console logging
            // Used for slash commands and their registration with Discord
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            // Required to subscribe to the various client events used in conjunction with Interactions
            .AddSingleton<InteractionHandler>())
            .Build();

            emojis.Add("\U0001F1E6");
            emojis.Add("\U0001F1E7");
            emojis.Add("\U0001F1E8");
            emojis.Add("\U0001F1E9");
            emojis.Add("\U0001F1EA");
            emojis.Add("\U0001F1EB");
            emojis.Add("\U0001F1EC");

            foreach (string s in emojis)
            {
                IEmote emote = new Emoji(s);
                emotes.Add(emote);
            }

            for (var i = 0; i < 7; i++)
            {
                temp_table[i] = false;
            }

            filePath = "C:\\Users\\kliu3\\source\\repos\\DiscordBot\\DiscordBot\\message_ids.txt";
            user = null;
            users = new List<ulong>();
            pmap = new Dictionary<ulong, ULongPair>();

            await RunBotAsync(host);

        }

        //run bot connection
        public async Task RunBotAsync(IHost host)
        {

            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            var sCommands = provider.GetRequiredService<InteractionService>();
            _client = provider.GetRequiredService<DiscordSocketClient>();
            await provider.GetRequiredService<InteractionHandler>().InitializeAsync();


            var config = new DiscordSocketConfig
             {
                 GatewayIntents = GatewayIntents.All | GatewayIntents.MessageContent
             };


            _services = new ServiceCollection()
                    .AddSingleton(_client)
                    .AddSingleton(_commands)
                    .BuildServiceProvider();


            string token = "MTEzNzgxNTcxNjU1MzMxMDIxOA.GiEMYT.NfkWUGGZit-PfEMpJLqeO_3BkBMoPz5ClYPF3k";

            await RegisterCommandsAsync();

            _client.Ready += async () =>
            {
                Console.WriteLine("BOT READY");
                await sCommands.RegisterCommandsToGuildAsync(1016229761195974698);
                await sCommands.RegisterCommandsToGuildAsync(1137843173050302564);
            }; 

            await _client.LoginAsync(Discord.TokenType.Bot, token);

            await _client.StartAsync();

            await Task.Delay(-1);
            //now the bot is online
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

                        ulong ulongValue = 0;
                        try
                        {
                            ulongValue = ulong.Parse(userId);
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Invalid input format. Cannot convert to ulong.");
                        }
                        catch (OverflowException)
                        {
                            Console.WriteLine("Value is outside the range of ulong.");
                        }

                        await GetUserById(ulongValue);
                        await userMessage.Channel.SendMessageAsync($"User Id: {user.Id}.");
                        await userMessage.Channel.SendMessageAsync($"User Username: {user.Username}");
                        await userMessage.Channel.SendMessageAsync($"User Discriminator: {user.Discriminator}");
                        if (user == null || ulongValue == 0)
                        {
                            await userMessage.Channel.SendMessageAsync("User not found.");
                            return;
                        }

                        try
                        {
                            await userMessage.Channel.SendMessageAsync($"Sending DM invitation to : {user.Username}.");
                            await SendScheduleMessage(user);
                        }
                        catch (Exception ex)
                        {
                            await userMessage.Channel.SendMessageAsync($"Failed to send the message to {user.Username}#{user.Discriminator}: {ex.Message}");
                        }
                    }
                }
            }

            if(messageContent == "!reactions")
            {
                //await CheckReactionsOfMessage(2323, 23232);
                await CheckReactionsOfMessage(23);
            }

            if(messageContent == "!table")
            {
                setupRead();

                List<string> usernames = new List<string>();

                foreach (ulong u in users)
                {
                    var temp = await _client.GetUserAsync(u);
                    usernames.Add(temp.Username);
                }

                DataColumn[] dataColumns = new DataColumn[users.Count];
                for (var i = 0; i < users.Count; i++)
                {
                    await CheckReactionsOfMessage(users[i]);

                    dataColumns[i] = new DataColumn() ;
                    dataColumns[i].username = usernames[i];
                    for (var j =0; j < temp_table.Count(); j++)
                    {
                        string emoji = temp_table[j] ? "✅" : "🟡";
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

                    try
                    {
                        byte[] imageBytes = await client.GetByteArrayAsync(apiUrl);
                        await userMessage.Channel.SendFileAsync(new MemoryStream(imageBytes), "image.png");
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine($"HTTP request error: {ex.Message}");
                    }
                }
            }
            Console.WriteLine("message finished received\n");
        }

        private void setupRead()
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] strings = line.Split(',');
                        List<ulong> l = new List<ulong>();
                        foreach (string s in strings)
                        {
                            l.Add(ulong.Parse(s));
                            Console.WriteLine(s);
                        }
                        //Console.WriteLine(l[0].ToString());
                        if (!users.Contains(l[0]))
                        {
                            users.Add(l[0]);
                            pmap[l[0]] = new ULongPair(l[1], l[2]);
                            Console.WriteLine(line);
                        }                        
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
        }

        public async Task CheckReactionsOfMessage(ulong userId)
        {
            ULongPair p = pmap[userId];

            for (int i = 0; i < 7; i++)
            {
                temp_table[i] = false;
            }

            IDMChannel channel = (IDMChannel) await _client.GetDMChannelAsync(p.First);
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
            }


        }





        private async Task GetUserById(ulong userId)
        {
            Console.WriteLine(userId.ToString());
            user = await _client.GetUserAsync(userId);

            if (user != null)
            {
                /*Console.WriteLine($"User ID: {user.Id}");
                Console.WriteLine($"User Username: {user.Username}");
                Console.WriteLine($"User Discriminator: {user.Discriminator}");*/
            }
            else
            {
                Console.WriteLine("User not found.");
            }
        }


        private async Task OnMessageReceived(SocketMessage message)
        {
            if (message.Author is SocketGuildUser user)
            {
                var currentGuild = user.Guild;

                if (currentGuild != null)
                {
                    Console.WriteLine($"Current Guild Name: {currentGuild.Name}");
                    Console.WriteLine($"Current Guild ID: {currentGuild.Id}");
                }
            }
        }


        /* private async Task CheckUserReaction(ulong userId, ulong messageId)
         {
             // Get the channel where the message was sent
             var channel = _client.GetChannel(YOUR_CHANNEL_ID) as SocketTextChannel;

             // Get the message using the message ID
             var message = await channel.GetMessageAsync(messageId) as IUserMessage;

             // Check if the user's ID is among the list of users who have reacted to the message
             var userReaction = message.Reactions.FirstOrDefault(reaction => reaction.Value.ReactionUsers.Any(user => user.Id == userId));

             if (userReaction.Key != null)
             {
                 // User has reacted to the message
                 Console.WriteLine($"User with ID {userId} has reacted to the message.");
             }
             else
             {
                 // User has not reacted to the message
                 Console.WriteLine($"User with ID {userId} has not reacted to the message.");
             }
         }*/


        private Task OnReactionAdded(Cacheable<IUserMessage, ulong> cacheable1, Cacheable<IMessageChannel, ulong> cacheable2, SocketReaction reaction)
        {
            // Ignore reactions from bots
            if (reaction.User.IsSpecified && reaction.User.Value.IsBot)
                return Task.CompletedTask;

            //int selectedDay = GetSelectedDay(reaction.Emote.Name);

            //PrintNumberAsync(selectedDay, cacheable2.Value);
            return Task.CompletedTask;
        }

        private async Task SendScheduleMessage(IUser user)
        {
            IUserMessage tempMessage = await user.SendMessageAsync($"Hey {user.Username}, please select a day:");

            Console.WriteLine("FIRST CHANNEL: " +  tempMessage.Channel.Id.ToString());

            messages.Add(new ULongPair(tempMessage.Channel.Id, tempMessage.Id));

            try
            {
                File.AppendAllText(filePath, $"{user.Id},{tempMessage.Channel.Id},{tempMessage.Id}\n");
                Console.WriteLine("Message ID has been written to the file.");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            for (var i = 0; i < 7; i++)
            {
                await tempMessage.AddReactionAsync(new Emoji(emojis[i]));
            }

            //_currentSchedule = message;
        }

        private IUserMessage _currentSchedule;

        /*[ModalInteraction("create_modal_temp")]
        public async Task HandleModalInput(CreateModal modal)
        {
            string input = modal.Description;
            await RespondAsync(input);
        }


        internal class CreateModal : IModal
        {
            public string Title => "Create Event";
            [InputLabel("Describe your event:")]
            [ModalTextInput("event_desc", Discord.TextInputStyle.Short, placeholder: "Date, Location, Time, Cost, etc. ", maxLength: 50)]
            public string Description { get; set; }
        }*/
    }
}
