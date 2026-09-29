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
    }
}