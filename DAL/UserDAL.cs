using DAL.Helper;
using Model;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DAL
{
    public class UserDAL
    {
        private readonly DatabaseHelper _databaseHelper;

        public UserDAL(DatabaseHelper databaseHelper)
        {
            _databaseHelper = databaseHelper;
        }

        // =========================
        // LOGIN
        // =========================
        public UserLoginModel? Login(string username, string password)
        {
            using var connection = _databaseHelper.GetConnection();

            string sql = @"
                SELECT UserId, Username, FullName, Email, Phone, RoleId
                FROM Users
                WHERE Username = @Username
                  AND PasswordHash = @Password
                  AND IsActive = 1
                  AND IsDeleted = 0";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Password", password);

            connection.Open();

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new UserLoginModel
                {
                    UserId = Convert.ToInt32(reader["UserId"]),
                    Username = reader["Username"].ToString()!,
                    FullName = reader["FullName"].ToString()!,
                    Email = reader["Email"].ToString()!,
                    Phone = reader["Phone"].ToString()!,
                    RoleId = Convert.ToInt32(reader["RoleId"])
                };
            }

            return null;
        }

        // =========================
        // GET ALL
        // =========================
        public List<User> GetAll()
        {
            var list = new List<User>();

            using var connection = _databaseHelper.GetConnection();

            string sql = @"
                SELECT UserId, Username, PasswordHash,
                       FullName, Email, Phone, RoleId,
                       IsActive, IsDeleted, CreatedAt, UpdatedAt
                FROM Users
                WHERE IsDeleted = 0
                ORDER BY UserId DESC";

            using var command = new SqlCommand(sql, connection);

            connection.Open();

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapUser(reader));
            }

            return list;
        }

        // =========================
        // GET BY ID
        // =========================
        public User? GetById(int id)
        {
            using var connection = _databaseHelper.GetConnection();

            string sql = @"
                SELECT UserId, Username, PasswordHash,
                       FullName, Email, Phone, RoleId,
                       IsActive, IsDeleted, CreatedAt, UpdatedAt
                FROM Users
                WHERE UserId = @UserId
                  AND IsDeleted = 0";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@UserId", SqlDbType.Int).Value = id;

            connection.Open();

            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return null;

            return MapUser(reader);
        }

        // =========================
        // CREATE
        // =========================
        public int Create(User user)
        {
            using var connection = _databaseHelper.GetConnection();

            string sql = @"
                INSERT INTO Users
                (
                    Username,
                    PasswordHash,
                    FullName,
                    Email,
                    Phone,
                    RoleId,
                    IsActive,
                    IsDeleted,
                    CreatedAt
                )
                VALUES
                (
                    @Username,
                    @PasswordHash,
                    @FullName,
                    @Email,
                    @Phone,
                    @RoleId,
                    @IsActive,
                    0,
                    GETDATE()
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@Username", SqlDbType.VarChar, 50)
                .Value = user.Username;

            command.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 255)
                .Value = user.PasswordHash;

            command.Parameters.Add("@FullName", SqlDbType.NVarChar, 100)
                .Value = user.FullName;

            command.Parameters.Add("@Email", SqlDbType.VarChar, 100)
                .Value = user.Email;

            command.Parameters.Add("@Phone", SqlDbType.VarChar, 20)
                .Value = user.Phone;

            command.Parameters.Add("@RoleId", SqlDbType.Int)
                .Value = user.RoleId;

            command.Parameters.Add("@IsActive", SqlDbType.Bit)
                .Value = user.IsActive;

            connection.Open();

            return Convert.ToInt32(command.ExecuteScalar());
        }

        // =========================
        // UPDATE
        // =========================
        public bool Update(User user)
        {
            using var connection = _databaseHelper.GetConnection();

            string sql = @"
                UPDATE Users
                SET
                    Username = @Username,
                    PasswordHash = @PasswordHash,
                    FullName = @FullName,
                    Email = @Email,
                    Phone = @Phone,
                    RoleId = @RoleId,
                    IsActive = @IsActive,
                    UpdatedAt = GETDATE()
                WHERE UserId = @UserId
                  AND IsDeleted = 0";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@UserId", SqlDbType.Int)
                .Value = user.UserId;

            command.Parameters.Add("@Username", SqlDbType.VarChar, 50)
                .Value = user.Username;

            command.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 255)
                .Value = user.PasswordHash;

            command.Parameters.Add("@FullName", SqlDbType.NVarChar, 100)
                .Value = user.FullName;

            command.Parameters.Add("@Email", SqlDbType.VarChar, 100)
                .Value = user.Email;

            command.Parameters.Add("@Phone", SqlDbType.VarChar, 20)
                .Value = user.Phone;

            command.Parameters.Add("@RoleId", SqlDbType.Int)
                .Value = user.RoleId;

            command.Parameters.Add("@IsActive", SqlDbType.Bit)
                .Value = user.IsActive;

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        // =========================
        // DELETE - SOFT DELETE
        // =========================
        public bool Delete(int id)
        {
            using var connection = _databaseHelper.GetConnection();

            string sql = @"
                UPDATE Users
                SET
                    IsDeleted = 1,
                    UpdatedAt = GETDATE()
                WHERE UserId = @UserId
                  AND IsDeleted = 0";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@UserId", SqlDbType.Int).Value = id;

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        // =========================
        // MAP USER
        // =========================
        private User MapUser(SqlDataReader reader)
        {
            return new User
            {
                UserId = Convert.ToInt32(reader["UserId"]),
                Username = reader["Username"].ToString() ?? "",
                PasswordHash = reader["PasswordHash"].ToString() ?? "",
                FullName = reader["FullName"].ToString() ?? "",
                Email = reader["Email"].ToString() ?? "",
                Phone = reader["Phone"].ToString() ?? "",
                RoleId = Convert.ToInt32(reader["RoleId"]),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                CreatedAt = reader["CreatedAt"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["CreatedAt"]),
                UpdatedAt = reader["UpdatedAt"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["UpdatedAt"])
            };
        }
    }
}