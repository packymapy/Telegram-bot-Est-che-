using System;
using System.Data;
using System.Configuration;
using Npgsql;
using Newtonsoft.Json;

namespace AdminApp.Database
{
    public class AdminInfo
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public dynamic Permissions { get; set; }
        public DateTime? LastLogin { get; set; }

        public bool HasPermission(string permission)
        {
            if (Permissions == null) return false;
            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, bool>>(
                    Permissions.ToString()
                );
                return dict.ContainsKey(permission) && dict[permission];
            }
            catch
            {
                return false;
            }
        }

        public bool IsAdmin()
        {
            return HasPermission("can_manage_admins") &&
                   HasPermission("can_manage_users") &&
                   HasPermission("can_manage_products");
        }

        public bool IsSeller()
        {
            return HasPermission("can_view_products") &&
                   !HasPermission("can_manage_admins");
        }
    }

    public static class DatabaseHelper
    {
        private static string _connectionString;

        static DatabaseHelper()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public static AdminInfo AuthenticateAdmin(string login, string password)
        {
            string query = "SELECT * FROM authenticate_admin(@login, @password)";
            
            using (var conn = GetConnection())
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", password);
                
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new AdminInfo
                        {
                            Id = reader.GetInt32(0),
                            FullName = reader.GetString(1),
                            Permissions = JsonConvert.DeserializeObject(reader.GetString(2)),
                            IsActive = reader.GetBoolean(3),
                            IsLocked = reader.GetBoolean(4)
                        };
                    }
                }
            }
            return null;
        }

        public static DataTable ExecuteQuery(string query, NpgsqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    var dt = new DataTable();
                    dt.Load(reader);
                    return dt;
                }
            }
        }

        public static int ExecuteNonQuery(string query, NpgsqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string query, NpgsqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                conn.Open();
                return cmd.ExecuteScalar();
            }
        }
    }
}
