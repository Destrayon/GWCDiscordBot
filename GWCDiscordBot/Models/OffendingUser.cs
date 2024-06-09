using GWCDiscordBot.Database;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWCDiscordBot.Models
{
    public struct OffendingUser
    {
        public ulong Id { get; init; }

        public ulong DiscordId { get; init; }

        public int PingAmount { get; set; }
    }
}
