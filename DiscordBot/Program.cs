using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Reactive.Joins;
using System.Text.RegularExpressions;

namespace TutorialBot
{
    class Program
    {
        //variables
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        public CommandService _commands;
        private IServiceProvider _services;

        private SocketGuild guild;

        //log channel info
        private ulong LogChannelID;
        private SocketTextChannel LogChannel;

        //run bot connection
        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All | GatewayIntents.MessageContent
            };

            _services = new ServiceCollection()
                    .AddSingleton(_client)
                    .AddSingleton(_commands)
                    .BuildServiceProvider();

            string token = "MTEzNzgxNTcxNjU1MzMxMDIxOA.GiEMYT.NfkWUGGZit-PfEMpJLqeO_3BkBMoPz5ClYPF3k";

            _client.Log += _client_Log;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, token);

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
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        // Read Input and get Output ( RECEIVE AND DO SOMETHING )
        public async Task HandleCommandAsync(SocketMessage message)
        {
            // Check if the message is from a user and not a system message
            if (message.Author.IsBot || message is not IUserMessage userMessage)
                return;

            // Get the ITextChannel from which the message was sent
            var textChannel = message.Channel as ITextChannel;

            // Fetch the message again using the ITextChannel and message's ID
            // This ensures we can access the content correctly
            var fetchedMessage = await textChannel.GetMessageAsync(message.Id) as IUserMessage;

            // Check if the fetchedMessage is null (in case the message is not found)
            if (fetchedMessage == null)
                return;

            // Get the content of the message as a string
            string messageContent = fetchedMessage.Content;

            // Now you can use the message content as needed
            // For example, you can print it or process it further
            Console.WriteLine("Received message: " + messageContent);

            if(messageContent.StartsWith("!help"))
            {
                await userMessage.Channel.SendMessageAsync("Bruh.");
            }

            if(messageContent == "!event")
            {

            }

            if (messageContent.StartsWith("!dm"))
            {
                // Check if the user sending the command has permission to use it.
                if (!(userMessage.Author is SocketGuildUser guildUser) || !guildUser.GuildPermissions.Administrator)
                {
                    await userMessage.Channel.SendMessageAsync("You don't have permission to use this command.");
                    return;
                }

                // Split the message content into parts: !dm @username YourMessageHere
                var parts = messageContent.Split(" ");
                string userId;
                if (parts.Length >= 3)
                {
                    Regex regex = new Regex(@"<@(\d+)>");
                    Match match = regex.Match(parts[1]);
                    string contents = string.Join(" ", parts, 2, parts.Length - 2);

                    // Try to find the user based on the provided username or ID.

                    if (match.Success)
                    {
                        userId = match.Groups[1].Value;
                        Console.WriteLine($"Extracted content: {userId}");
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("Invalid user ID format.");
                        return;
                    }

                    ulong userIdAsUlong;
                    Console.WriteLine(parts[1].Substring(1));
                    if (ulong.TryParse(userId, out userIdAsUlong))
                    {
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("Invalid user ID format.");
                    }

                    var user = _client.GetUser(userIdAsUlong);
                    if (user == null)
                    {
                        await userMessage.Channel.SendMessageAsync("User not found.");
                        return;
                    }

                    // Send the direct message to the selected user.
                    try
                    {
                        await user.SendMessageAsync(contents);
                        await userMessage.Channel.SendMessageAsync($"Message sent to {user.Username}#{user.Discriminator}: {messageContent}");
                    }
                    catch (Exception ex)
                    {
                        await userMessage.Channel.SendMessageAsync($"Failed to send the message to {user.Username}#{user.Discriminator}: {ex.Message}");
                    }
                }
                else
                {
                    await userMessage.Channel.SendMessageAsync("Invalid command format. Usage: !dm @username YourMessageHere");
                }
            }

        } 
    }
}