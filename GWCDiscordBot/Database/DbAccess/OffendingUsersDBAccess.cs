using GWCDiscordBot.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
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
                INSERT INTO OffendingUsers (DiscordId, PingAmount)
                VALUES (@DiscordId, @PingAmount)
                ON CONFLICT(DiscordId) DO UPDATE SET
                    PingAmount = PingAmount + @PingAmount;
            ";

            List<SqliteParameter> parameters = new List<SqliteParameter>
            {
                new SqliteParameter("@DiscordId", offendingUser.DiscordId),
                new SqliteParameter("@PingAmount", offendingUser.PingAmount)
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
    }
}
