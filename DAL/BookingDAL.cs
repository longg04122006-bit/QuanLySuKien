using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Model;

namespace DAL
{
    public class BookingDAL
    {
        private readonly string _connectionString;

        public BookingDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Không tìm thấy ConnectionString");
        }

        public List<Booking> GetAll()
        {
            var list = new List<Booking>();

            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string sql = @"
                SELECT BookingId, BookingCode, UserId,
                       BookingDate, TotalAmount, Status,
                       IsDeleted, CreatedAt
                FROM Bookings
                WHERE IsDeleted = 0
                ORDER BY BookingId DESC";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Booking
                {
                    BookingId = Convert.ToInt32(reader["BookingId"]),
                    BookingCode = reader["BookingCode"]?.ToString(),
                    UserId = Convert.ToInt32(reader["UserId"]),
                    BookingDate = Convert.ToDateTime(reader["BookingDate"]),
                    TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                    Status = reader["Status"]?.ToString(),
                    IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                });
            }

            return list;
        }

        public Booking? GetById(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string sql = @"
                SELECT BookingId, BookingCode, UserId,
                       BookingDate, TotalAmount, Status,
                       IsDeleted, CreatedAt
                FROM Bookings
                WHERE BookingId = @BookingId
                  AND IsDeleted = 0";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@BookingId", id);

            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new Booking
                {
                    BookingId = Convert.ToInt32(reader["BookingId"]),
                    BookingCode = reader["BookingCode"]?.ToString(),
                    UserId = Convert.ToInt32(reader["UserId"]),
                    BookingDate = Convert.ToDateTime(reader["BookingDate"]),
                    TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                    Status = reader["Status"]?.ToString(),
                    IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                };
            }

            return null;
        }
        // INSERT BOOKING
        public int Create(Booking booking)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string sql = @"
        INSERT INTO Bookings
        (
            BookingCode,
            UserId,
            BookingDate,
            TotalAmount,
            Status,
            IsDeleted,
            CreatedAt
        )
        VALUES
        (
            @BookingCode,
            @UserId,
            @BookingDate,
            @TotalAmount,
            @Status,
            0,
            GETDATE()
        );

        SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@BookingCode",
                (object?)booking.BookingCode ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue("@UserId", booking.UserId);
            cmd.Parameters.AddWithValue("@BookingDate", booking.BookingDate);
            cmd.Parameters.AddWithValue("@TotalAmount", booking.TotalAmount);

            cmd.Parameters.AddWithValue(
                "@Status",
                (object?)booking.Status ?? DBNull.Value
            );

            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        // UPDATE BOOKING
        public bool Update(Booking booking)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string sql = @"
        UPDATE Bookings
        SET
            BookingCode = @BookingCode,
            UserId = @UserId,
            BookingDate = @BookingDate,
            TotalAmount = @TotalAmount,
            Status = @Status
        WHERE BookingId = @BookingId
          AND IsDeleted = 0";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@BookingId", booking.BookingId);

            cmd.Parameters.AddWithValue(
                "@BookingCode",
                (object?)booking.BookingCode ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue("@UserId", booking.UserId);
            cmd.Parameters.AddWithValue("@BookingDate", booking.BookingDate);
            cmd.Parameters.AddWithValue("@TotalAmount", booking.TotalAmount);

            cmd.Parameters.AddWithValue(
                "@Status",
                (object?)booking.Status ?? DBNull.Value
            );

            return cmd.ExecuteNonQuery() > 0;
        }
        // DELETE BOOKING - SOFT DELETE
        public bool Delete(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string sql = @"
        UPDATE Bookings
        SET IsDeleted = 1
        WHERE BookingId = @BookingId
          AND IsDeleted = 0";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@BookingId", id);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}