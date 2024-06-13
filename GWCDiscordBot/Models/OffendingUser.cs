using GWCDiscordBot.Database;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWCDiscordBot.Models
{
    public class OffendingUser
    {
        public ulong? Id { get; init; }

        public required ulong DiscordId { get; init; }

        public required int PingAmount { get; set; }

        public required bool HasEscalated { get; set; }
    }
}
