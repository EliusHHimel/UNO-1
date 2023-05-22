using System.Reflection;
using Discord;
using DiscordBotsList.Api;
using DiscordBotsList.Api.Objects;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;

namespace UNO
{
    class Program
    {
        static void Main(string[] args) =>
            new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordShardedClient _client;
        private InteractionService _commands;
        private IServiceProvider _services;
        private IDblSelfBot _dblApi;
        private HttpListener _httpListener;

        public async Task MainAsync()
        {
            _services = ConfigureServices();

            _client = _services.GetRequiredService<DiscordShardedClient>();

            _commands = _services.GetRequiredService<InteractionService>();

            // Initialize the Top.gg (Discord Bots List api)
            if (Config.USE_TOP_GG_API)
            {
                var discordBotList = new AuthDiscordBotListApi(Config.BOT_CLIENT_ID, System.Environment.GetEnvironmentVariable("UnoDblToken"));
                _dblApi = await discordBotList.GetMeAsync();
            }

            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _client.Log += LogAsync;
            _commands.Log += LogAsync;

            await _client.LoginAsync(TokenType.Bot, System.Environment.GetEnvironmentVariable("UnoToken"));
            await _client.StartAsync();

            _client.InteractionCreated += OnInteractionCreated;
            _client.ShardReady += OnReady;
            _client.JoinedGuild += OnJoinedGuild;
            _client.LeftGuild += OnLeftGuild;

            StartWebServer(); // Start the web server

            await Task.Delay(Timeout.Infinite);
        }

        private async Task UpdateBotStatus()
        {
            Console.WriteLine("Updating guild count...");

            var totalGuilds = _client.Shards.Sum(x => x.Guilds.Count);

            // Update Bot Status
            await _client.SetGameAsync($"/uno on {totalGuilds} servers");

            // Update Top.gg Server Count
            if (Config.USE_TOP_GG_API && !Config.IS_DEBUG)
                await _dblApi.UpdateStatsAsync(totalGuilds);
        }

        private async Task OnLeftGuild(SocketGuild arg) =>
            await UpdateBotStatus();

        private async Task OnJoinedGuild(SocketGuild arg) =>
            await UpdateBotStatus();

        private async Task OnReady(DiscordSocketClient arg)
        {
            // Uncomment this to register commands (should only be run once, not every time it starts)
            // Comment it out again after registering the commands
            await _commands.RegisterCommandsGloballyAsync();
            if (Config.IS_DEBUG)
            {
                Console.WriteLine("\n\nWARNING: Running in debug mode.\n\n");
                await _commands.RegisterCommandsToGuildAsync(Config.DEBUG_GUILD_ID);
            }
        }

        private async Task OnInteractionCreated(SocketInteraction interaction)
        {
            try
            {
                // Create an execution context that matches the generic type parameter of
                // the InteractionModuleBase<T> module
                var ctx = new ShardedInteractionContext(_client, interaction);
                await _commands.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordShardedClient>(x =>
                    new DiscordShardedClient(new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Info,
                        GatewayIntents = GatewayIntents.Guilds,
                        TotalShards = Config.DISCORD_SHARD_COUNT
                    }))
                .AddSingleton(x =>
                    new InteractionService(x.GetRequiredService<DiscordShardedClient>()))
                .AddSingleton<GameManager>()
                .BuildServiceProvider();
        }

        private void StartWebServer()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add("http://localhost:8080/"); // Set the desired server address

            try
            {
                _httpListener.Start();
                Console.WriteLine("Web server started. Listening for incoming requests...");

                Task.Run(() => ListenForRequests());
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while starting the web server: " + ex.Message);
            }
        }

        private async Task ListenForRequests()
        {
            while (_httpListener.IsListening)
            {
                try
                {
                    HttpListenerContext context = await _httpListener.GetContextAsync();

                    // Handle the request
                    await HandleWebRequest(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred while processing a web request: " + ex.Message);
                }
            }
        }

        private async Task HandleWebRequest(HttpListenerContext context)
        {
            // Retrieve the request and response objects
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            // Customize your web server logic here
            // ...

            // Send a response
            string responseString = "Hello from the web server!";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentType = "text/plain";
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
}

// namespace UNO
// {
//     class Program
//     {
//         static void Main(string[] args) =>
//             new Program().MainAsync().GetAwaiter().GetResult();

//         private DiscordShardedClient _client;

//         private InteractionService _commands;

//         private IServiceProvider _services;

//         private IDblSelfBot _dblApi;

//         public async Task MainAsync()
//         {
//             _services = ConfigureServices();

//             _client = _services.GetRequiredService<DiscordShardedClient>();

//             _commands = _services.GetRequiredService<InteractionService>();

//             // Initialize the Top.gg (Discord Bots List api)
//             if (Config.USE_TOP_GG_API)
//             {
//                 var discordBotList =
//                     new AuthDiscordBotListApi(Config.BOT_CLIENT_ID,
//                         System
//                             .Environment
//                             .GetEnvironmentVariable("UnoDblToken"));
//                 _dblApi = await discordBotList.GetMeAsync();
//             }

//             // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
//             await _commands
//                 .AddModulesAsync(Assembly.GetEntryAssembly(), _services);

//             _client.Log += LogAsync;
//             _commands.Log += LogAsync;

//             await _client
//             .LoginAsync(TokenType.Bot,
//             System.Environment.GetEnvironmentVariable("UnoToken"));
//             await _client.StartAsync();

//             _client.InteractionCreated += OnInteractionCreated;
//             _client.ShardReady += OnReady;
//             _client.JoinedGuild += OnJoinedGuild;
//             _client.LeftGuild += OnLeftGuild;

//             await Task.Delay(Timeout.Infinite);
//         }

//         private async Task UpdateBotStatus()
//         {
//             Console.WriteLine("Updating guild count...");

//             var totalGuilds = _client.Shards.Sum(x => x.Guilds.Count);

//             // Update Bot Status
//             await _client.SetGameAsync($"/uno on {totalGuilds} servers");

//             // Update Top.gg Server Count
//             if (Config.USE_TOP_GG_API && !Config.IS_DEBUG)
//                 await _dblApi.UpdateStatsAsync(totalGuilds);
//         }

//         private async Task OnLeftGuild(SocketGuild arg) =>
//             await UpdateBotStatus();

//         private async Task OnJoinedGuild(SocketGuild arg) =>
//             await UpdateBotStatus();

//         private async Task OnReady(DiscordSocketClient arg)
//         {
//             // Uncomment this to register commands (should only be run once, not everytime it starts)
//             // Comment it out again after registering the commands
//             await _commands.RegisterCommandsGloballyAsync();
//             if (Config.IS_DEBUG)
//             {
//                 Console.WriteLine("\n\nWARNING: Running in debug mode.\n\n");
//                 await _commands
//                     .RegisterCommandsToGuildAsync(Config.DEBUG_GUILD_ID);
//             }
//         }

//         private async Task OnInteractionCreated(SocketInteraction interaction)
//         {
//             try
//             {
//                 // Create an execution context that matches the generic type parameter of
//                 // the InteractionModuleBase<T> module
//                 var ctx = new ShardedInteractionContext(_client, interaction);
//                 await _commands.ExecuteCommandAsync(ctx, _services);
//             }
//             catch (Exception e)
//             {
//                 Console.WriteLine(e);
//             }
//         }

//         private Task LogAsync(LogMessage log)
//         {
//             Console.WriteLine(log.ToString());
//             return Task.CompletedTask;
//         }

//         ServiceProvider ConfigureServices()
//         {
//             return new ServiceCollection()
//                 .AddSingleton<DiscordShardedClient>(x =>
//                     new DiscordShardedClient(new DiscordSocketConfig
//                     {
//                         LogLevel = LogSeverity.Info,
//                         GatewayIntents = GatewayIntents.Guilds,
//                         TotalShards = Config.DISCORD_SHARD_COUNT
//                     }))
//                 .AddSingleton(x =>
//                     new InteractionService(x
//                             .GetRequiredService<DiscordShardedClient>()))
//                 .AddSingleton<GameManager>()
//                 .BuildServiceProvider();
//         }
//     }
// }
