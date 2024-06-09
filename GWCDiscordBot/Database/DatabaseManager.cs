using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace GWCDiscordBot.Database
{
    public static class DatabaseManager
    {
        private static string _databasePath = "";
        private static readonly List<DatabaseQuery> queries = new List<DatabaseQuery>();

        public static void InitializeDatabase(string dbCreationSQLFilePath, string databasePath = "database.db")
        {
            _databasePath = databasePath;

            if (!File.Exists(_databasePath))
            {
                using SqliteConnection conn = new SqliteConnection($"Data Source={_databasePath}");

                Console.WriteLine("Database doesn't exist, creating database.");

                conn.Open();

                string sqlQuery = File.ReadAllText(dbCreationSQLFilePath);

                using SqliteCommand cmd = conn.CreateCommand();

                cmd.CommandText = sqlQuery;
                cmd.ExecuteNonQuery();

                conn.Close();
            }
        }

        public static void AddSQLStatementIntoTransaction(string sqlStatement, List<SqliteParameter>? parameters = null)
        {
            sqlStatement = sqlStatement.Last() == ';' ? sqlStatement : sqlStatement + ';';

            queries.Add(new DatabaseQuery
            {
                Query = sqlStatement,
                SqliteParameters = parameters
            });
        }

        public static int SaveObjectsInTheTransaction()
        {
            using SqliteConnection conn = new SqliteConnection($"Data Source={_databasePath}");

            conn.Open();

            using SqliteTransaction transaction = conn.BeginTransaction();

            try
            {
                int databaseCount = 0;

                foreach (DatabaseQuery query in queries)
                {
                    using SqliteCommand cmd = new SqliteCommand(query.Query, conn, transaction);

                    if (query.SqliteParameters != null)
                    {
                        cmd.Parameters.AddRange(query.SqliteParameters);
                    }

                    databaseCount += cmd.ExecuteNonQuery();
                }

                transaction.Commit();

                return databaseCount;
            }
            catch (Exception)
            {
                transaction.Rollback();

                throw;
            }
            finally
            {
                conn.Close();

                queries.Clear();
            }
        }
    }
}
