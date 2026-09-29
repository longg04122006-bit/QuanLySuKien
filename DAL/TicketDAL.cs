using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Model;

namespace DAL
{
    public class TicketDAL
    {
        private readonly string _connectionString;

        public TicketDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Không tìm thấy chuỗi kết nối Database.");
        }

        // GET ALL
        public List<Ticket> GetAll()
        {
            List<Ticket> tickets = new List<Ticket>();

            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"
                SELECT
                    TicketId,
                    TicketCode,
                    BookingDetailId,
                    EventId,
                    TicketTypeId,
                    AttendeeName,
                    AttendeeEmail,
                    QRCode,
                    Status,
                    CheckedIn,
                    CheckedInAt,
                    IsDeleted,
                    CreatedAt
                FROM Tickets
                WHERE IsDeleted = 0
                ORDER BY TicketId DESC";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                tickets.Add(new Ticket
                {
                    TicketId = Convert.ToInt32(reader["TicketId"]),
                    TicketCode = reader["TicketCode"]?.ToString(),
                    BookingDetailId = Convert.ToInt32(reader["BookingDetailId"]),
                    EventId = Convert.ToInt32(reader["EventId"]),
                    TicketTypeId = Convert.ToInt32(reader["TicketTypeId"]),
                    AttendeeName = reader["AttendeeName"]?.ToString(),
                    AttendeeEmail = reader["AttendeeEmail"]?.ToString(),
                    QRCode = reader["QRCode"]?.ToString(),
                    Status = reader["Status"]?.ToString(),
                    CheckedIn = Convert.ToBoolean(reader["CheckedIn"]),
                    CheckedInAt = reader["CheckedInAt"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["CheckedInAt"]),
                    IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                    CreatedAt = reader["CreatedAt"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["CreatedAt"])
                });
            }

            return tickets;
        }

        // GET BY ID
        public Ticket? GetById(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"
                SELECT
                    TicketId,
                    TicketCode,
                    BookingDetailId,
                    EventId,
                    TicketTypeId,
                    AttendeeName,
                    AttendeeEmail,
                    QRCode,
                    Status,
                    CheckedIn,
                    CheckedInAt,
                    IsDeleted,
                    CreatedAt
                FROM Tickets
                WHERE TicketId = @TicketId
                  AND IsDeleted = 0";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@TicketId", id);

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new Ticket
                {
                    TicketId = Convert.ToInt32(reader["TicketId"]),
                    TicketCode = reader["TicketCode"]?.ToString(),
                    BookingDetailId = Convert.ToInt32(reader["BookingDetailId"]),
                    EventId = Convert.ToInt32(reader["EventId"]),
                    TicketTypeId = Convert.ToInt32(reader["TicketTypeId"]),
                    AttendeeName = reader["AttendeeName"]?.ToString(),
                    AttendeeEmail = reader["AttendeeEmail"]?.ToString(),
                    QRCode = reader["QRCode"]?.ToString(),
                    Status = reader["Status"]?.ToString(),
                    CheckedIn = Convert.ToBoolean(reader["CheckedIn"]),
                    CheckedInAt = reader["CheckedInAt"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["CheckedInAt"]),
                    IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                    CreatedAt = reader["CreatedAt"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["CreatedAt"])
                };
            }

            return null;
        }

        // POST
        public int Create(Ticket ticket)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"
                INSERT INTO Tickets
                (
                    TicketCode,
                    BookingDetailId,
                    EventId,
                    TicketTypeId,
                    AttendeeName,
                    AttendeeEmail,
                    QRCode,
                    Status,
                    CheckedIn,
                    CheckedInAt,
                    IsDeleted,
                    CreatedAt
                )
                VALUES
                (
                    @TicketCode,
                    @BookingDetailId,
                    @EventId,
                    @TicketTypeId,
                    @AttendeeName,
                    @AttendeeEmail,
                    @QRCode,
                    @Status,
                    @CheckedIn,
                    @CheckedInAt,
                    0,
                    GETDATE()
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@TicketCode",
                (object?)ticket.TicketCode ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@BookingDetailId",
                ticket.BookingDetailId);

            cmd.Parameters.AddWithValue("@EventId",
                ticket.EventId);

            cmd.Parameters.AddWithValue("@TicketTypeId",
                ticket.TicketTypeId);

            cmd.Parameters.AddWithValue("@AttendeeName",
                (object?)ticket.AttendeeName ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@AttendeeEmail",
                (object?)ticket.AttendeeEmail ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@QRCode",
                (object?)ticket.QRCode ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@Status",
                (object?)ticket.Status ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@CheckedIn",
                ticket.CheckedIn);

            cmd.Parameters.AddWithValue("@CheckedInAt",
                (object?)ticket.CheckedInAt ?? DBNull.Value);

            conn.Open();

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // PUT
        public bool Update(Ticket ticket)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Tickets
                SET
                    TicketCode = @TicketCode,
                    BookingDetailId = @BookingDetailId,
                    EventId = @EventId,
                    TicketTypeId = @TicketTypeId,
                    AttendeeName = @AttendeeName,
                    AttendeeEmail = @AttendeeEmail,
                    QRCode = @QRCode,
                    Status = @Status,
                    CheckedIn = @CheckedIn,
                    CheckedInAt = @CheckedInAt
                WHERE TicketId = @TicketId
                  AND IsDeleted = 0";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@TicketId",
                ticket.TicketId);

            cmd.Parameters.AddWithValue("@TicketCode",
                (object?)ticket.TicketCode ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@BookingDetailId",
                ticket.BookingDetailId);

            cmd.Parameters.AddWithValue("@EventId",
                ticket.EventId);

            cmd.Parameters.AddWithValue("@TicketTypeId",
                ticket.TicketTypeId);

            cmd.Parameters.AddWithValue("@AttendeeName",
                (object?)ticket.AttendeeName ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@AttendeeEmail",
                (object?)ticket.AttendeeEmail ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@QRCode",
                (object?)ticket.QRCode ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@Status",
                (object?)ticket.Status ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@CheckedIn",
                ticket.CheckedIn);

            cmd.Parameters.AddWithValue("@CheckedInAt",
                (object?)ticket.CheckedInAt ?? DBNull.Value);

            conn.Open();

            return cmd.ExecuteNonQuery() > 0;
        }

        // DELETE MỀM
        public bool Delete(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Tickets
                SET IsDeleted = 1
                WHERE TicketId = @TicketId
                  AND IsDeleted = 0";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@TicketId", id);

            conn.Open();

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}