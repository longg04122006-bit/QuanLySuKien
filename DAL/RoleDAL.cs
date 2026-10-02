using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Model;
using System.Data;

namespace DAL
{
    public class RoleDAL
    {
        private readonly string _connectionString;

        public RoleDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Không tìm thấy ConnectionString");
        }

        // Lấy tất cả Role
        public List<Role> GetAll()
        {
            var list = new List<Role>();

            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"SELECT RoleId, RoleName, Description
                           FROM Roles
                           ORDER BY RoleId DESC";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Role
                {
                    RoleId = reader.GetInt32(0),
                    RoleName = reader.GetString(1),
                    Description = reader.IsDBNull(2)
                        ? null
                        : reader.GetString(2)
                });
            }

            return list;
        }

        // Lấy Role theo ID
        public Role? GetById(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"SELECT RoleId, RoleName, Description
                           FROM Roles
                           WHERE RoleId = @RoleId";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@RoleId", SqlDbType.Int).Value = id;

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return new Role
            {
                RoleId = reader.GetInt32(0),
                RoleName = reader.GetString(1),
                Description = reader.IsDBNull(2)
                    ? null
                    : reader.GetString(2)
            };
        }

        // Thêm Role
        public int Create(Role role)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Roles
                           (
                               RoleName,
                               Description
                           )
                           VALUES
                           (
                               @RoleName,
                               @Description
                           );

                           SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@RoleName", SqlDbType.NVarChar, 50)
                .Value = role.RoleName;

            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 255)
                .Value = (object?)role.Description ?? DBNull.Value;

            conn.Open();

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // Cập nhật Role
        public bool Update(Role role)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"UPDATE Roles
                           SET RoleName = @RoleName,
                               Description = @Description
                           WHERE RoleId = @RoleId";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@RoleId", SqlDbType.Int)
                .Value = role.RoleId;

            cmd.Parameters.Add("@RoleName", SqlDbType.NVarChar, 50)
                .Value = role.RoleName;

            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 255)
                .Value = (object?)role.Description ?? DBNull.Value;

            conn.Open();

            return cmd.ExecuteNonQuery() > 0;
        }

        // Xóa Role
        public bool Delete(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"DELETE FROM Roles
                           WHERE RoleId = @RoleId";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@RoleId", SqlDbType.Int).Value = id;

            conn.Open();

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}