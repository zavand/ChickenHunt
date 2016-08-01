using System;
using MySql.Data.MySqlClient;

namespace ChickenHunt.Website.DataLayer
{
    public class MySqlDataStorage
    {
        public string InitialConnectionString { get; set; }

        public MySqlDataStorage(string initialConnectionString, string databaseName)
        {
            ConnectionString = InitialConnectionString = initialConnectionString;
            DatabaseName = databaseName;
        }

        public string ConnectionString { get; set; }

        public string DatabaseName
        {
            get
            {
                return new MySqlConnectionStringBuilder(ConnectionString).Database;
            }
            set
            {
                SetDatabase(value);
            }
        }

        protected virtual void SetDatabase(string databaseName)
        {
            var b = new MySqlConnectionStringBuilder(ConnectionString);
            b.Database = databaseName;
            ConnectionString = b.GetConnectionString(true);
        }

        public virtual void CreateTables()
        {

        }

        public void Init()
        {
            CreateDatabase();
            CreateTables();
            ApplyPatches();
        }

        protected virtual void ApplyPatches()
        {

        }

        public virtual void CreateDatabase()
        {
            var b = new MySqlConnectionStringBuilder(InitialConnectionString);
            if (String.IsNullOrEmpty(b.Database))
            {
                using (var c = new MySqlConnection(InitialConnectionString))
                {
                    c.Open();

                    // Create database
                    c.ExecuteNonQuery($"create database if not exists {DatabaseName}");
                }
            }
        }
    }
}