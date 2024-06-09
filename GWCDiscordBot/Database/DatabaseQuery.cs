using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWCDiscordBot.Database
{
    public record DatabaseQuery
    {
        public required string Query { get; init; }
        public List<SqliteParameter>? SqliteParameters { get; init; }
    }
}
