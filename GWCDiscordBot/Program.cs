using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using GWCDiscordBot;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;

ManualResetEvent readyEvent = new ManualResetEvent(false);

IConfigurationRoot config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

ulong guildId = ulong.Parse(config["DiscordSettings:GuildId"] ?? "0");
ulong playerInfoChannelId = ulong.Parse(config["DiscordSettings:PlayerInfoChannelId"] ?? "0");
ulong adminChannelId = ulong.Parse(config["DiscordSettings:AdminChannelId"] ?? "0");
string botToken = config["DiscordSettings:BotToken"] ?? "";

DiscordSocketClient client = new();
client.Log += Log;
client.Ready += OnReady;

await client.LoginAsync(TokenType.Bot, botToken);
await client.StartAsync();

readyEvent.WaitOne();

await client.StopAsync();

Environment.Exit(0);

async Task OnReady()
{
    ServiceCollection services = new ServiceCollection();

    SocketGuild guild = client.GetGuild(guildId);
    SocketTextChannel playerInfoChannel = (SocketTextChannel)guild.GetChannel(playerInfoChannelId);
    SocketTextChannel adminChannel = (SocketTextChannel)guild.GetChannel(adminChannelId);

    services.AddSingleton(client)
            .AddSingleton<IGuild>(guild)
            .AddKeyedSingleton<ITextChannel>("PlayerInfoChannel", playerInfoChannel)
            .AddKeyedSingleton<ITextChannel>("AdminChannel", adminChannel)
            .AddSingleton<IConfiguration>(config)
            .AddSingleton<UserChecker>()
            .AddSingleton<RunBotChecker>()
            .AddSingleton<PingUsers>();

    DefaultServiceProviderFactory serviceProviderFactory = new DefaultServiceProviderFactory();

    IServiceProvider serviceProvider = serviceProviderFactory.CreateServiceProvider(services);

    await serviceProvider.GetRequiredService<RunBotChecker>().StartAsync();

    readyEvent.Set();
}

Task Log(LogMessage msg)
{
    Console.WriteLine(msg.ToString());
    return Task.CompletedTask;
}

