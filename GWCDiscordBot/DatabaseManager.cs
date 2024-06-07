using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWCDiscordBot
{
    public static class DatabaseManager
    {
        public static void InitializeDatabase(string dbCreationSQLFilePath)
        {
            string databasePath = "database.db";

            if (!File.Exists(databasePath))
            {
                using (SqliteConnection conn = new SqliteConnection($"Data Source={databasePath}")) 
                {
                    conn.Open();

                    string sqlQuery = File.ReadAllText(databasePath);
                    SqliteCommand cmd = conn.CreateCommand();

                    cmd.CommandText = sqlQuery;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
