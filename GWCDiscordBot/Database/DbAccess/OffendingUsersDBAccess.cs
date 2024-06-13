using GWCDiscordBot.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWCDiscordBot.Database
{
    public static class OffendingUsersDBAccess
    {
        public static void AddObjectInTransaction(this OffendingUser offendingUser)
        {
            string sqlStatement = @"
                INSERT INTO OffendingUsers (DiscordId, PingAmount, HasEscalated)
                VALUES (@DiscordId, @PingAmount, @HasEscalated)
                ON CONFLICT(DiscordId) DO UPDATE SET
                    PingAmount = @PingAmount,
                    HasEscalated = @HasEscalated;
            ";

            List<SqliteParameter> parameters = new List<SqliteParameter>
            {
                new SqliteParameter("@DiscordId", offendingUser.DiscordId),
                new SqliteParameter("@PingAmount", offendingUser.PingAmount),
                new SqliteParameter("@HasEscalated", offendingUser.HasEscalated ? 1 : 0)
            };

            DatabaseManager.AddSQLStatementIntoTransaction(sqlStatement, parameters);
        }

        public static void RemoveObjectOnNextTransaction(this OffendingUser offendingUser)
        {
            string sqlStatement = @"
                DELETE OffendingUsers
                WHERE Id = @Id
                AND DiscordId = @DiscordId;
            ";

            List<SqliteParameter> parameters = new List<SqliteParameter>
            {
                new SqliteParameter("@DiscordId", offendingUser.DiscordId),
                new SqliteParameter("@Id", offendingUser.Id)
            };

            DatabaseManager.AddSQLStatementIntoTransaction(sqlStatement, parameters);
        }

        public static IEnumerable<OffendingUser> GetOffendingUsersByListOfDiscordId(IList<ulong> discordIds)
        {
            if (!discordIds.Any())
            {
                return [];
            }

            string sqlStatement = @"
                SELECT * FROM OffendingUsers
                WHERE DiscordId = @Id0
            ";

            List<SqliteParameter> parameters = new List<SqliteParameter>
            {
                new SqliteParameter("@Id0", discordIds[0])
            };

            for (int i = 1; i < discordIds.Count; i++)
            {
                sqlStatement += $"\nOR DiscordId = @Id{i}";

                parameters.Add(new SqliteParameter($"@Id{i}", discordIds[i]));
            }

            DataTable table = DatabaseManager.ExecuteQuery(sqlStatement, parameters);

            List<OffendingUser> offendingUsers = new List<OffendingUser>();

            foreach (DataRow dataRow in table.Rows) 
            {
                OffendingUser? offendingUser = GetOffendingUserFromDataRow(dataRow);

                if (offendingUser != null)
                {
                    offendingUsers.Add(offendingUser);
                }
            }

            return offendingUsers;
        }

        private static OffendingUser? GetOffendingUserFromDataRow(DataRow dataRow)
        {

            string? id = dataRow["Id"].ToString();

            string? discordId = dataRow["DiscordId"].ToString();

            string? pingAmount = dataRow["PingAmount"].ToString();

            string? hasEscalated = dataRow["HasEscalated"].ToString();

            if (id == null || discordId == null || pingAmount == null || hasEscalated == null)
            {
                return null;
            }

            return new OffendingUser
            {
                Id = ulong.Parse(id),
                DiscordId = ulong.Parse(discordId),
                PingAmount = int.Parse(pingAmount),
                HasEscalated = DatabaseManager.ConvertSqliteStringToBool(hasEscalated)
            };
        }
    }
}
