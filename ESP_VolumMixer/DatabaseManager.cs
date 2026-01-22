using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SQLite;

namespace ESP_VolumMixer
{
    public class DatabaseManager
    {
        private const string ConnectionString = "Data Source={0};Version=3;";
        private string databasePath;

        public DatabaseManager()
        {
            databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"myapp.db");
        }

        private SQLiteConnection GetConnection()
        {
            string connectionString = string.Format(ConnectionString, databasePath);
            return new SQLiteConnection(connectionString);
        }

        public void InitializeDatabase()
        {
            string createProfilesTable = @"CREATE TABLE IF NOT EXISTS Profiles (Id INTEGER PRIMARY KEY AUTOINCREMENT,Name TEXT NOT NULL,ProcessIds TEXT)";
            string createProcessesTable = @"CREATE TABLE IF NOT EXISTS Processes (Id INTEGER PRIMARY KEY AUTOINCREMENT,ProcessName TEXT NOT NULL)";

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new SQLiteCommand(createProfilesTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(createProcessesTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                if (IsTableEmpty(connection, "Processes"))
                {
                    AddDefaultProcesses(connection);
                }

                if (IsTableEmpty(connection, "Profiles"))
                {
                    AddDefaultProfiles(connection);
                }
            }
        }

        private bool IsTableEmpty(SQLiteConnection connection, string tableName)
        {
            string sql = $"SELECT COUNT(*) FROM {tableName}";

            using (var command = new SQLiteCommand(sql, connection))
            {
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count == 0;
            }
        }

        private void AddDefaultProfiles(SQLiteConnection connection)
        {
            var defaultProfiles = new List<string>
            {
                "Profile 1",
                "Profile 2",
                "Profile 3",
                "Profile 4",
                "Profile 5"
            };

            var defaultProcessIds = new List<string>
            {
                "1, 1, 1, 1, 1, 1, 1, 1, 1",
                "1, 1, 1, 1, 1, 1, 1, 1, 1",
                "1, 1, 1, 1, 1, 1, 1, 1, 1",
                "1, 1, 1, 1, 1, 1, 1, 1, 1",
                "1, 1, 1, 1, 1, 1, 1, 1, 1"
            };

            string sql = "INSERT INTO Profiles (Name, ProcessIds) VALUES (@name, @ids)";

            using (var command = new SQLiteCommand(sql, connection))
            {
                for (int Profilesnum = 0; Profilesnum < defaultProfiles.Count; Profilesnum++)
                {
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@name", defaultProfiles[Profilesnum]);
                    command.Parameters.AddWithValue("@ids", defaultProcessIds[Profilesnum]);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void AddDefaultProcesses(SQLiteConnection connection)
        {
            var defaultProcesses = new List<string>
            {
                "chrome.exe",
                "firefox.exe",
                "msedge.exe",
                "opera.exe",
                "discord.exe",
                "teams.exe",
                "zoom.exe"
            };

            string sql = "INSERT OR IGNORE INTO Processes (ProcessName) VALUES (@name)";

            using (var command = new SQLiteCommand(sql, connection))
            {
                foreach (string processName in defaultProcesses)
                {
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@name", processName);
                    command.ExecuteNonQuery();
                }
            }
        }

        private bool ProcessExists(SQLiteConnection connection, string processName)
        {
            string sql = "SELECT COUNT(*) FROM Processes WHERE ProcessName = @name";

            using (var command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@name", processName);
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }

        public void AddProfile(Profile profile)
        {
            string sql = @"INSERT INTO Profiles (Name, ProcessIds) VALUES (@name, @processIds)";

            using (var connection = GetConnection())
            {
                connection.Open();

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name", profile.Name);
                    command.Parameters.AddWithValue("@processIds", profile.ProcessIds ?? "");
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Profile> GetAllProfiles()
        {
            var profiles = new List<Profile>();
            string sql = "SELECT * FROM Profiles";

            using (var connection = GetConnection())
            {
                connection.Open();

                using (var command = new SQLiteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var profile = new Profile
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            ProcessIds = reader["ProcessIds"].ToString()
                        };
                        profiles.Add(profile);
                    }
                }
            }

            return profiles;
        }

        public void AddProcess(Process process)
        {
            string sql = @"INSERT INTO Processes (ProcessName) VALUES (@processName)";

            using (var connection = GetConnection())
            {
                connection.Open();

                using (var command = new SQLiteCommand(sql, connection))
                {
                    if (!ProcessExists(connection, process.NameProcess))
                    {
                        command.Parameters.AddWithValue("@processName", process.NameProcess);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public List<Process> GetAllProcesses()
        {
            var processes = new List<Process>();
            string sql = "SELECT * FROM Process";
            
            using (var connection = GetConnection())
            {
                connection.Open();

                using (var comand = new SQLiteCommand(sql, connection))
                using (var reader = comand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var process = new Process
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            NameProcess = reader["Name"].ToString()
                        };
                        processes.Add(process);
                    }
                }
            }
            return processes;
        }

    }
}
