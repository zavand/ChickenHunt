using System;
using MySql.Data.MySqlClient;

namespace ChickenHunt.Website.DataLayer
{
    public static class MySqlExtensions
    {
        private const int MaxDeadlockAttempts = 100;

        public static int ExecuteNonQuery(this MySqlConnection c, MySqlTransaction t, string query, params object[] pp)
        {
            var attempt = 0;
            while (attempt < MaxDeadlockAttempts)
            {
                attempt++;
                try
                {
                    var cmd = new MySqlCommand(query, c, t);
                    for (var i = 0; i < pp.Length; i++)
                    {
                        cmd.Parameters.AddWithValue((string)pp[i], pp[i + 1]);
                        i++;
                    }

                    return cmd.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    if (ex.Message.ToLower().Contains("deadlock"))
                    {
                        continue;
                    }
                    throw;
                }
            }
            throw new MaxDeadlockAttemptsReached();
        }

        public static int ExecuteNonQuery(this MySqlConnection c, string query, params object[] pp)
        {
            return c.ExecuteNonQuery(null, query, pp);
        }

        public static T ExecuteScalar<T>(this MySqlConnection c, MySqlTransaction t, string query, params object[] pp)
        {
            var attempt = 0;
            while (attempt < MaxDeadlockAttempts)
            {
                attempt++;
                try
                {
                    var cmd = new MySqlCommand(query, c, t);

                    for (var i = 0; i < pp.Length; i++)
                    {
                        cmd.Parameters.AddWithValue((string)pp[i], pp[i + 1]);
                        i++;
                    }
                    var o = cmd.ExecuteScalar();
                    return (T)o;
                }
                catch (MySqlException ex)
                {
                    if (ex.Message.ToLower().Contains("deadlock"))
                    {
                        continue;
                    }
                    throw;
                }
            }
            throw new MaxDeadlockAttemptsReached();
        }

        public static T ExecuteScalar<T>(this MySqlConnection c, string query, params object[] pp)
        {
            return c.ExecuteScalar<T>(null, query, pp);
        }

        public static void ExecuteReader(this MySqlConnection c, MySqlTransaction t, Action<MySqlDataReader> action, string query, params object[] pp)
        {
            var attempt = 0;
            while (attempt < MaxDeadlockAttempts)
            {
                attempt++;
                try
                {
                    var cmd = new MySqlCommand(query, c, t);

                    for (var i = 0; i < pp.Length; i++)
                    {
                        cmd.Parameters.AddWithValue((string)pp[i], pp[i + 1]);
                        i++;
                    }

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            action(r);
                        }
                    }
                    return;
                }
                catch (MySqlException ex)
                {
                    if (ex.Message.ToLower().Contains("deadlock"))
                    {
                        continue;
                    }
                    throw;
                }
            }
            throw new MaxDeadlockAttemptsReached();
        }

        public static void ExecuteReader(this MySqlConnection c, Action<MySqlDataReader> action, string query, params object[] pp)
        {
            c.ExecuteReader(null, action, query, pp);
        }

        public static void ExecuteReaders(this MySqlConnection c, MySqlTransaction t, Action<MySqlDataReader, int> action, string query, params object[] pp)
        {
            var attempt = 0;
            while (attempt < MaxDeadlockAttempts)
            {
                attempt++;
                try
                {
                    var cmd = new MySqlCommand(query, c, t);

                    for (var i = 0; i < pp.Length; i++)
                    {
                        cmd.Parameters.AddWithValue((string)pp[i], pp[i + 1]);
                        i++;
                    }

                    using (var r = cmd.ExecuteReader())
                    {
                        var resultset = 0;
                        while (r.Read())
                        {
                            action(r, resultset);
                        }
                        while (r.NextResult())
                        {
                            resultset++;
                            while (r.Read())
                            {
                                action(r, resultset);
                            }
                        }
                    }
                    return;
                }
                catch (MySqlException ex)
                {
                    if (ex.Message.ToLower().Contains("deadlock"))
                    {
                        continue;
                    }
                    throw;
                }
            }
            throw new MaxDeadlockAttemptsReached();
        }

        public static void ExecuteReaders(this MySqlConnection c, Action<MySqlDataReader, int> action, string query, params object[] pp)
        {
            c.ExecuteReaders(null, action, query, pp);
        }
    }
}