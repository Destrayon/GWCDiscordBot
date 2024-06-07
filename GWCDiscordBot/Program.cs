using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using GWCDiscordBot;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;


IConfigurationRoot config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

ulong guildId = ulong.Parse(config["DiscordSettings:GuildId"] ?? "0");
ulong playerInfoChannelId = ulong.Parse(config["DiscordSettings:PlayerInfoChannelId"] ?? "0");

DiscordSocketClient client = new();
client.Log += Log;

ServiceCollection services = new ServiceCollection();

services.AddSingleton(client)
        .AddSingleton(x => client.GetGuild(guildId))
        .AddSingleton(x => (SocketTextChannel)client.GetChannel(playerInfoChannelId))
        .AddSingleton<IConfiguration>(config)
        .AddSingleton<UserChecker>()
        .AddSingleton<RunBotChecker>();

DefaultServiceProviderFactory serviceProviderFactory = new DefaultServiceProviderFactory();

IServiceProvider serviceProvider = serviceProviderFactory.CreateServiceProvider(services);

await serviceProvider.GetRequiredService<RunBotChecker>().StartAsync();

Task Log(LogMessage msg)
{
    Console.WriteLine(msg.ToString());
    return Task.CompletedTask;
}

