using Discord;
using Discord.WebSocket;
using GWCDiscordBot.Database;
using GWCDiscordBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWCDiscordBot
{
    public class PingUsers
    {
        private readonly UserChecker _userChecker;
        private readonly ITextChannel _adminChannel;
        private readonly string _adminRoleName;
        private readonly int _numberOfPingsBeforeEscalation;
        private readonly string _messageToSendOffendingUser;
        private readonly string _messageToSendAdminUsers;
        private readonly List<OffendingUser> _listOfNotifyAdmins;
        private readonly IGuild _guild;

        public PingUsers(UserChecker userChecker, [FromKeyedServices("AdminChannel")] ITextChannel adminChannel, IConfiguration configuration, IGuild guild) 
        {
            _userChecker = userChecker;
            _adminChannel = adminChannel;
            _adminRoleName = configuration["DiscordSettings:AdminRoleName"] ?? "";
            _numberOfPingsBeforeEscalation = int.Parse(configuration["DiscordSettings:NumberOfPingsBeforeEscalation"] ?? "0");
            _messageToSendOffendingUser = configuration["DiscordSettings:MessageToSendOffendingUser"] ?? "";
            _messageToSendAdminUsers = configuration["DiscordSettings:MessageToSendAdminUsers"] ?? "";
            _listOfNotifyAdmins = new List<OffendingUser>();
            _guild = guild;
        }

        public async Task SendPings()
        {
            List<ulong> offendingUserIds = (await _userChecker.GetOffendingUsers()).ToList();

            List<OffendingUser> offendingUsers  = OffendingUsersDBAccess.GetOffendingUsersByListOfDiscordId(offendingUserIds).ToList();

            offendingUserIds = offendingUserIds.Where(x => !offendingUsers.Select(x => x.DiscordId).Contains(x)).ToList();

            foreach (ulong discordId in offendingUserIds)
            {
                await SendPingToUser(new OffendingUser
                {
                    DiscordId = discordId,
                    PingAmount = 0,
                    HasEscalated = false
                });
            }

            foreach (OffendingUser offendingUser in offendingUsers)
            {
                await SendPingToUser(offendingUser);
            }

            await EscalateToAdmin();

            DatabaseManager.SaveObjectsInTransaction();
        }

        private async Task SendPingToUser(OffendingUser offendingUser)
        {
            if (offendingUser.PingAmount < _numberOfPingsBeforeEscalation)
            {
                IGuildUser guildUser = await _guild.GetUserAsync(offendingUser.DiscordId);

                await guildUser.SendMessageAsync(_messageToSendOffendingUser);

                offendingUser.PingAmount += 1;

                offendingUser.AddObjectInTransaction();

                return;
            }

            if (offendingUser.HasEscalated) 
            {
                return;
            }

            _listOfNotifyAdmins.Add(offendingUser);
        }

        private async Task EscalateToAdmin()
        {
            if (_listOfNotifyAdmins.Count == 0)
            {
                return;
            }

            string userMentions = $"@{_adminRoleName} {_messageToSendAdminUsers} ";

            foreach(OffendingUser user in  _listOfNotifyAdmins) 
            {
                userMentions += $"<@{user.DiscordId}> ";
                user.HasEscalated = true;

                user.AddObjectInTransaction();
            }

            userMentions.TrimEnd(' ');

            await _adminChannel.SendMessageAsync(userMentions);

            _listOfNotifyAdmins.Clear();
        }
    }
}
