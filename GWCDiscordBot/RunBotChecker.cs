using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWCDiscordBot
{
    public class RunBotChecker
    {
        private DiscordSocketClient _client;
        private IConfiguration _config;
        public RunBotChecker(DiscordSocketClient client, IConfiguration config) 
        { 
            _client = client;
            _config = config;
        }
        public async Task StartAsync()
        {
            DatabaseManager.InitializeDatabase("dbcreation.sql");

            string botToken = _config["DiscordSettings:BotToken"] ?? "";

            await _client.LoginAsync(TokenType.Bot, botToken);
            await _client.StartAsync();
            await Task.Delay(-1);
        }
    }
}
