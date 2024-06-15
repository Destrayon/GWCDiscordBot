using Discord;
using Discord.WebSocket;
using GWCDiscordBot.Database;
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
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _config;
        private readonly PingUsers _pingUsers;

        public RunBotChecker(DiscordSocketClient client, IConfiguration config, PingUsers pingUsers) 
        { 
            _client = client;
            _config = config;
            _pingUsers = pingUsers;
        }
        public async Task StartAsync()
        {
            DatabaseManager.InitializeDatabase("dbcreation.sql");

            await _pingUsers.SendPings();
        }
    }
}
