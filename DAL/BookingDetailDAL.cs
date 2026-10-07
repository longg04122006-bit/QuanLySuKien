
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Model;

namespace DAL
{
    public class BookingDetailDAL
    {
        private readonly string _connectionString;

        public BookingDetailDAL(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception(
                    "Không tìm thấy chuỗi kết nối Database.");
        }

        // GET ALL
        public List<BookingDetail> GetAll()
        {
            List<BookingDetail> bookingDetails =
                new List<BookingDetail>();

            using SqlConnection conn =
                new SqlConnection(_connectionString);

            string sql = @"
                SELECT
                    BookingDetailId,
                    BookingId,
                    TicketTypeId,
                    Quantity,
                    UnitPrice,
                    Amount
                FROM BookingDetails
                ORDER BY BookingDetailId DESC";

            using SqlCommand cmd =
                new SqlCommand(sql, conn);

            conn.Open();

            using SqlDataReader reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                bookingDetails.Add(new BookingDetail
                {
                    BookingDetailId =
                        Convert.ToInt32(reader["BookingDetailId"]),

                    BookingId =
                        Convert.ToInt32(reader["BookingId"]),

                    TicketTypeId =
                        Convert.ToInt32(reader["TicketTypeId"]),

                    Quantity =
                        Convert.ToInt32(reader["Quantity"]),

                    UnitPrice =
                        Convert.ToDecimal(reader["UnitPrice"]),

                    Amount =
                        Convert.ToDecimal(reader["Amount"])
                });
            }

            return bookingDetails;
        }

        // GET BY ID
        public BookingDetail? GetById(int id)
        {
            using SqlConnection conn =
                new SqlConnection(_connectionString);

            string sql = @"
                SELECT
                    BookingDetailId,
                    BookingId,
                    TicketTypeId,
                    Quantity,
                    UnitPrice,
                    Amount
                FROM BookingDetails
                WHERE BookingDetailId = @BookingDetailId";

            using SqlCommand cmd =
                new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@BookingDetailId",
                id);

            conn.Open();

            using SqlDataReader reader =
                cmd.ExecuteReader();

            if (reader.Read())
            {
                return new BookingDetail
                {
                    BookingDetailId =
                        Convert.ToInt32(reader["BookingDetailId"]),

                    BookingId =
                        Convert.ToInt32(reader["BookingId"]),

                    TicketTypeId =
                        Convert.ToInt32(reader["TicketTypeId"]),

                    Quantity =
                        Convert.ToInt32(reader["Quantity"]),

                    UnitPrice =
                        Convert.ToDecimal(reader["UnitPrice"]),

                    Amount =
                        Convert.ToDecimal(reader["Amount"])
                };
            }

            return null;
        }

        // POST
        public int Create(BookingDetail detail)
        {
            using SqlConnection conn =
                new SqlConnection(_connectionString);

            string sql = @"
                INSERT INTO BookingDetails
                (
                    BookingId,
                    TicketTypeId,
                    Quantity,
                    UnitPrice
                )
                VALUES
                (
                    @BookingId,
                    @TicketTypeId,
                    @Quantity,
                    @UnitPrice
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using SqlCommand cmd =
                new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@BookingId",
                detail.BookingId);

            cmd.Parameters.AddWithValue(
                "@TicketTypeId",
                detail.TicketTypeId);

            cmd.Parameters.AddWithValue(
                "@Quantity",
                detail.Quantity);

            cmd.Parameters.AddWithValue(
                "@UnitPrice",
                detail.UnitPrice);

            conn.Open();

            return Convert.ToInt32(
                cmd.ExecuteScalar());
        }

        // PUT
        public bool Update(BookingDetail detail)
        {
            using SqlConnection conn =
                new SqlConnection(_connectionString);

            string sql = @"
                UPDATE BookingDetails
                SET
                    BookingId = @BookingId,
                    TicketTypeId = @TicketTypeId,
                    Quantity = @Quantity,
                    UnitPrice = @UnitPrice
                WHERE BookingDetailId = @BookingDetailId";

            using SqlCommand cmd =
                new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@BookingDetailId",
                detail.BookingDetailId);

            cmd.Parameters.AddWithValue(
                "@BookingId",
                detail.BookingId);

            cmd.Parameters.AddWithValue(
                "@TicketTypeId",
                detail.TicketTypeId);

            cmd.Parameters.AddWithValue(
                "@Quantity",
                detail.Quantity);

            cmd.Parameters.AddWithValue(
                "@UnitPrice",
                detail.UnitPrice);

            conn.Open();

            return cmd.ExecuteNonQuery() > 0;
        }

        // DELETE
        public bool Delete(int id)
        {
            using SqlConnection conn =
                new SqlConnection(_connectionString);

            string sql = @"
                DELETE FROM BookingDetails
                WHERE BookingDetailId = @BookingDetailId";

            using SqlCommand cmd =
                new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@BookingDetailId",
                id);

            conn.Open();

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}



