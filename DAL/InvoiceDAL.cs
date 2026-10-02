using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Model;

namespace DAL
{
    public class InvoiceDAL
    {
        private readonly string _connectionString;

        public InvoiceDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Không tìm thấy ConnectionString");
        }

        // GET ALL
        public List<Invoice> GetAll()
        {
            var list = new List<Invoice>();

            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string sql = @"
                SELECT
                    InvoiceId,
                    InvoiceCode,
                    BookingId,
                    InvoiceDate,
                    CustomerName,
                    CustomerEmail,
                    TotalAmount,
                    PDFUrl
                FROM Invoices
                ORDER BY InvoiceId DESC";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Invoice
                {
                    InvoiceId = Convert.ToInt32(reader["InvoiceId"]),
                    InvoiceCode = reader["InvoiceCode"]?.ToString(),
                    BookingId = Convert.ToInt32(reader["BookingId"]),
                    InvoiceDate = Convert.ToDateTime(reader["InvoiceDate"]),
                    CustomerName = reader["CustomerName"]?.ToString(),

                    CustomerEmail = reader["CustomerEmail"] == DBNull.Value
                        ? null
                        : reader["CustomerEmail"]?.ToString(),

                    TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),

                    PDFUrl = reader["PDFUrl"] == DBNull.Value
                        ? null
                        : reader["PDFUrl"]?.ToString()
                });
            }

            return list;
        }

        // GET BY ID
        public Invoice? GetById(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string sql = @"
                SELECT
                    InvoiceId,
                    InvoiceCode,
                    BookingId,
                    InvoiceDate,
                    CustomerName,
                    CustomerEmail,
                    TotalAmount,
                    PDFUrl
                FROM Invoices
                WHERE InvoiceId = @InvoiceId";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@InvoiceId", id);

            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new Invoice
                {
                    InvoiceId = Convert.ToInt32(reader["InvoiceId"]),
                    InvoiceCode = reader["InvoiceCode"]?.ToString(),
                    BookingId = Convert.ToInt32(reader["BookingId"]),
                    InvoiceDate = Convert.ToDateTime(reader["InvoiceDate"]),
                    CustomerName = reader["CustomerName"]?.ToString(),

                    CustomerEmail = reader["CustomerEmail"] == DBNull.Value
                        ? null
                        : reader["CustomerEmail"]?.ToString(),

                    TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),

                    PDFUrl = reader["PDFUrl"] == DBNull.Value
                        ? null
                        : reader["PDFUrl"]?.ToString()
                };
            }

            return null;
        }

        // CREATE
        public int Create(Invoice invoice)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string sql = @"
                INSERT INTO Invoices
                (
                    InvoiceCode,
                    BookingId,
                    InvoiceDate,
                    CustomerName,
                    CustomerEmail,
                    TotalAmount,
                    PDFUrl
                )
                VALUES
                (
                    @InvoiceCode,
                    @BookingId,
                    @InvoiceDate,
                    @CustomerName,
                    @CustomerEmail,
                    @TotalAmount,
                    @PDFUrl
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@InvoiceCode", invoice.InvoiceCode);
            cmd.Parameters.AddWithValue("@BookingId", invoice.BookingId);
            cmd.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate);
            cmd.Parameters.AddWithValue("@CustomerName", invoice.CustomerName);

            cmd.Parameters.AddWithValue(
                "@CustomerEmail",
                (object?)invoice.CustomerEmail ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue("@TotalAmount", invoice.TotalAmount);

            cmd.Parameters.AddWithValue(
                "@PDFUrl",
                (object?)invoice.PDFUrl ?? DBNull.Value
            );

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // UPDATE
        public bool Update(Invoice invoice)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string sql = @"
                UPDATE Invoices
                SET
                    InvoiceCode = @InvoiceCode,
                    BookingId = @BookingId,
                    InvoiceDate = @InvoiceDate,
                    CustomerName = @CustomerName,
                    CustomerEmail = @CustomerEmail,
                    TotalAmount = @TotalAmount,
                    PDFUrl = @PDFUrl
                WHERE InvoiceId = @InvoiceId";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@InvoiceId", invoice.InvoiceId);
            cmd.Parameters.AddWithValue("@InvoiceCode", invoice.InvoiceCode);
            cmd.Parameters.AddWithValue("@BookingId", invoice.BookingId);
            cmd.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate);
            cmd.Parameters.AddWithValue("@CustomerName", invoice.CustomerName);

            cmd.Parameters.AddWithValue(
                "@CustomerEmail",
                (object?)invoice.CustomerEmail ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue("@TotalAmount", invoice.TotalAmount);

            cmd.Parameters.AddWithValue(
                "@PDFUrl",
                (object?)invoice.PDFUrl ?? DBNull.Value
            );

            return cmd.ExecuteNonQuery() > 0;
        }

        // DELETE
        public bool Delete(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string sql = @"
                DELETE FROM Invoices
                WHERE InvoiceId = @InvoiceId";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@InvoiceId", id);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}