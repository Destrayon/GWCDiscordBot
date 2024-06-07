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
    public class UserChecker
    {
        private readonly SocketGuild _guild;

        private readonly SocketTextChannel _channel;

        private readonly IConfiguration _config;

        public UserChecker(SocketGuild guild, SocketTextChannel channel, IConfiguration config)
        {
            _guild = guild;
            _channel = channel;
            _config = config;
        }

        private async Task GetUsersJoined()
        {
            int userFilterInSeconds = int.Parse(_config["DiscordSettings:NewestUsersFilterInSeconds"] ?? "-1");

            if (userFilterInSeconds <= 0)
            {
                return;
            }

            // Get the users who joined in the last week
            IEnumerable<IGuildUser> usersJoinedLastWeek = await _guild.GetUsersAsync().FlattenAsync();

            DateTime joinedAtFilter = DateTime.Now - TimeSpan.FromSeconds(userFilterInSeconds);

            var filteredUsers = usersJoinedLastWeek.Where(u => u.JoinedAt.HasValue && u.JoinedAt.Value >= joinedAtFilter);

            // Print the usernames of the filtered users
            foreach (var user in filteredUsers)
            {
                Console.WriteLine($"User: {user.Username}#{user.Discriminator} joined on {user.JoinedAt.Value}");
            }

            await Task.Delay(-1);
        }
    }
}
