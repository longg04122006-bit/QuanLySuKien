using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace DAL.Helper
{
    public class DatabaseHelper
    {
        private readonly string connectionString;

        public DatabaseHelper(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found.");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public DataTable ExecuteQuery(
            string sql,
            SqlParameter[]? parameters = null)
        {
            using SqlConnection conn = GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);

            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }

            using SqlDataAdapter adapter = new SqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            adapter.Fill(dt);

            return dt;
        }

        public int ExecuteNonQuery(
            string sql,
            SqlParameter[]? parameters = null)
        {
            using SqlConnection conn = GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);

            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }

            conn.Open();

            return cmd.ExecuteNonQuery();
        }
    }
}