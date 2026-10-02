
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Model;
using System.Data;

namespace DAL
{
    public class NotificationDAL
    {
        private readonly string _connectionString;

        public NotificationDAL(IConfiguration configuration)
        {
            _connectionString = configuration
                .GetConnectionString("DefaultConnection")
                ?? throw new Exception("Không tìm thấy ConnectionString");
        }

        // Lấy danh sách thông báo
        public List<Notification> GetAll()
        {
            var list = new List<Notification>();

            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"SELECT NotificationId, UserId, Title, Message,
                                  NotificationType, IsRead, CreatedAt
                           FROM Notifications
                           ORDER BY NotificationId DESC";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Notification
                {
                    NotificationId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    Title = reader.GetString(2),
                    Message = reader.GetString(3),
                    NotificationType = reader.IsDBNull(4)
                        ? null : reader.GetString(4),
                    IsRead = reader.GetBoolean(5),
                    CreatedAt = reader.GetDateTime(6)
                });
            }

            return list;
        }

        // Lấy thông báo theo ID
        public Notification? GetById(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"SELECT NotificationId, UserId, Title, Message,
                                  NotificationType, IsRead, CreatedAt
                           FROM Notifications
                           WHERE NotificationId = @Id";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return new Notification
            {
                NotificationId = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                Title = reader.GetString(2),
                Message = reader.GetString(3),
                NotificationType = reader.IsDBNull(4)
                    ? null : reader.GetString(4),
                IsRead = reader.GetBoolean(5),
                CreatedAt = reader.GetDateTime(6)
            };
        }

        // Thêm thông báo
        public int Create(Notification notification)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Notifications
                           (UserId, Title, Message, NotificationType,
                            IsRead, CreatedAt)
                           VALUES
                           (@UserId, @Title, @Message, @NotificationType,
                            @IsRead, GETDATE());

                           SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value =
                notification.UserId;

            cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value =
                notification.Title;

            cmd.Parameters.Add("@Message", SqlDbType.NVarChar, 1000).Value =
                notification.Message;

            cmd.Parameters.Add("@NotificationType", SqlDbType.NVarChar, 50).Value =
                (object?)notification.NotificationType ?? DBNull.Value;

            cmd.Parameters.Add("@IsRead", SqlDbType.Bit).Value =
                notification.IsRead;

            conn.Open();

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // Cập nhật thông báo
        public bool Update(Notification notification)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"UPDATE Notifications
                           SET UserId = @UserId,
                               Title = @Title,
                               Message = @Message,
                               NotificationType = @NotificationType,
                               IsRead = @IsRead
                           WHERE NotificationId = @Id";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@Id", SqlDbType.Int).Value =
                notification.NotificationId;

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value =
                notification.UserId;

            cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value =
                notification.Title;

            cmd.Parameters.Add("@Message", SqlDbType.NVarChar, 1000).Value =
                notification.Message;

            cmd.Parameters.Add("@NotificationType", SqlDbType.NVarChar, 50).Value =
                (object?)notification.NotificationType ?? DBNull.Value;

            cmd.Parameters.Add("@IsRead", SqlDbType.Bit).Value =
                notification.IsRead;

            conn.Open();

            return cmd.ExecuteNonQuery() > 0;
        }

        // Đánh dấu thông báo đã đọc
        public bool MarkAsRead(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"UPDATE Notifications
                           SET IsRead = 1
                           WHERE NotificationId = @Id";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            conn.Open();

            return cmd.ExecuteNonQuery() > 0;
        }

        // Xóa thông báo
        public bool Delete(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = "DELETE FROM Notifications WHERE NotificationId = @Id";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            conn.Open();

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}