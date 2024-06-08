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
        private readonly IGuild _guild;

        private readonly ITextChannel _channel;

        private readonly IConfiguration _config;

        private List<ulong> _usersJoined;

        private readonly int _userFilterInSeconds;

        private HashSet<ulong> _usersMessaged;

        public UserChecker(IGuild guild, ITextChannel channel, IConfiguration config)
        {
            _guild = guild;
            _channel = channel;
            _config = config;
            _userFilterInSeconds = int.Parse(_config["DiscordSettings:NewestUsersFilterInSeconds"] ?? "-1");
            _usersJoined = new List<ulong>();
            _usersMessaged = new HashSet<ulong>();
        }

        public async Task<IEnumerable<ulong>> GetOffendingUsers()
        {
            await GetUsersJoined();
            await GetUsersMessagedInChannel();

            List<ulong> offendingUsers = new List<ulong>();

            foreach (ulong userId in _usersJoined)
            {
                if (!_usersMessaged.Contains(userId))
                {
                    offendingUsers.Add(userId);
                }
            }

            return offendingUsers;
        }

        private async Task GetUsersJoined()
        {
            if (_userFilterInSeconds <= 0)
            {
                return;
            }
            DateTime joinedAtFilter = DateTime.Now - TimeSpan.FromSeconds(_userFilterInSeconds);

            _usersJoined = (await _guild.GetUsersAsync())
                .Where(u => u.JoinedAt.HasValue && u.JoinedAt.Value >= joinedAtFilter)
                .Select(x => x.Id)
                .ToList();
        }

        private async Task GetUsersMessagedInChannel()
        {
            if (_userFilterInSeconds <= 0)
            {
                return;
            }

            _usersMessaged = new HashSet<ulong>();
            DateTime messagedAtFilter = DateTime.Now - TimeSpan.FromSeconds(_userFilterInSeconds);

            IEnumerable<IMessage> messages = await _channel.GetMessagesAsync(limit: 100).FlattenAsync();

            while (messages.Any())
            {
                foreach (IMessage message in messages)
                {
                    if (message.Timestamp <= messagedAtFilter)
                    {
                        return;
                    }

                    _usersMessaged.Add(message.Author.Id);
                }

                IMessage oldestMessage = messages.LastOrDefault();
                if (oldestMessage == null || oldestMessage.Timestamp <= messagedAtFilter)
                {
                    return;
                }

                messages = await _channel.GetMessagesAsync(oldestMessage, Direction.Before, limit: 100).FlattenAsync();
            }
        }

    }
}
