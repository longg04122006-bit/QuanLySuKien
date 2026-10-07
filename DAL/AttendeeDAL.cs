using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Model;

namespace DAL
{
    public class AttendeeDAL
    {
        private readonly string _connectionString;

        public AttendeeDAL(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception(
                    "Không tìm thấy chuỗi kết nối Database.");
        }

        // GET ALL
        public List<Attendee> GetAll()
        {
            List<Attendee> list = new List<Attendee>();

            using SqlConnection conn =
                new SqlConnection(_connectionString);

            string sql = @"
                SELECT
                    AttendeeId,
                    TicketId,
                    FullName,
                    Email,
                    Phone,
                    CreatedAt
                FROM Attendees
                ORDER BY AttendeeId DESC";

            using SqlCommand cmd =
                new SqlCommand(sql, conn);

            conn.Open();

            using SqlDataReader reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapAttendee(reader));
            }

            return list;
        }

        // GET BY ID
        public Attendee? GetById(int id)
        {
            using SqlConnection conn =
                new SqlConnection(_connectionString);

            string sql = @"
                SELECT
                    AttendeeId,
                    TicketId,
                    FullName,
                    Email,
                    Phone,
                    CreatedAt
                FROM Attendees
                WHERE AttendeeId = @AttendeeId";

            using SqlCommand cmd =
                new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@AttendeeId",
                id);

            conn.Open();

            using SqlDataReader reader =
                cmd.ExecuteReader();

            if (reader.Read())
            {
                return MapAttendee(reader);
            }

            return null;
        }

        // POST
        public int Create(Attendee attendee)
        {
            using SqlConnection conn =
                new SqlConnection(_connectionString);

            string sql = @"
                INSERT INTO Attendees
                (
                    TicketId,
                    FullName,
                    Email,
                    Phone,
                    CreatedAt
                )
                VALUES
                (
                    @TicketId,
                    @FullName,
                    @Email,
                    @Phone,
                    GETDATE()
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using SqlCommand cmd =
                new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@TicketId",
                attendee.TicketId);

            cmd.Parameters.AddWithValue(
                "@FullName",
                attendee.FullName);

            cmd.Parameters.AddWithValue(
                "@Email",
                (object?)attendee.Email ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Phone",
                (object?)attendee.Phone ?? DBNull.Value);

            conn.Open();

            return Convert.ToInt32(
                cmd.ExecuteScalar());
        }

        // PUT
        public bool Update(Attendee attendee)
        {
            using SqlConnection conn =
                new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Attendees
                SET
                    TicketId = @TicketId,
                    FullName = @FullName,
                    Email = @Email,
                    Phone = @Phone
                WHERE AttendeeId = @AttendeeId";

            using SqlCommand cmd =
                new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@AttendeeId",
                attendee.AttendeeId);

            cmd.Parameters.AddWithValue(
                "@TicketId",
                attendee.TicketId);

            cmd.Parameters.AddWithValue(
                "@FullName",
                attendee.FullName);

            cmd.Parameters.AddWithValue(
                "@Email",
                (object?)attendee.Email ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Phone",
                (object?)attendee.Phone ?? DBNull.Value);

            conn.Open();

            return cmd.ExecuteNonQuery() > 0;
        }

        // DELETE
        public bool Delete(int id)
        {
            using SqlConnection conn =
                new SqlConnection(_connectionString);

            string sql = @"
                DELETE FROM Attendees
                WHERE AttendeeId = @AttendeeId";

            using SqlCommand cmd =
                new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@AttendeeId",
                id);

            conn.Open();

            return cmd.ExecuteNonQuery() > 0;
        }

        // MAP DATA
        private Attendee MapAttendee(SqlDataReader reader)
        {
            return new Attendee
            {
                AttendeeId =
                    Convert.ToInt32(reader["AttendeeId"]),

                TicketId =
                    Convert.ToInt32(reader["TicketId"]),

                FullName =
                    reader["FullName"].ToString() ?? string.Empty,

                Email =
                    reader["Email"] == DBNull.Value
                        ? null
                        : reader["Email"].ToString(),

                Phone =
                    reader["Phone"] == DBNull.Value
                        ? null
                        : reader["Phone"].ToString(),

                CreatedAt =
                    Convert.ToDateTime(reader["CreatedAt"])
            };
        }
    }
}