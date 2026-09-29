using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Model;

namespace DAL
{
    public class PaymentDAL
    {
        private readonly string _connectionString;

        public PaymentDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Không tìm thấy ConnectionString");
        }

        // GET ALL
        public List<Payment> GetAll()
        {
            var list = new List<Payment>();

            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string sql = @"
                SELECT
                    PaymentId,
                    BookingId,
                    PaymentCode,
                    Amount,
                    PaymentMethod,
                    PaymentDate,
                    Status,
                    TransactionCode,
                    CreatedAt
                FROM Payments
                ORDER BY PaymentId DESC";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Payment
                {
                    PaymentId = Convert.ToInt32(reader["PaymentId"]),
                    BookingId = Convert.ToInt32(reader["BookingId"]),
                    PaymentCode = reader["PaymentCode"]?.ToString(),
                    Amount = Convert.ToDecimal(reader["Amount"]),
                    PaymentMethod = reader["PaymentMethod"]?.ToString(),

                    PaymentDate = reader["PaymentDate"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["PaymentDate"]),

                    Status = reader["Status"]?.ToString(),

                    TransactionCode = reader["TransactionCode"] == DBNull.Value
                        ? null
                        : reader["TransactionCode"]?.ToString(),

                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                });
            }

            return list;
        }

        // GET BY ID
        public Payment? GetById(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string sql = @"
                SELECT
                    PaymentId,
                    BookingId,
                    PaymentCode,
                    Amount,
                    PaymentMethod,
                    PaymentDate,
                    Status,
                    TransactionCode,
                    CreatedAt
                FROM Payments
                WHERE PaymentId = @PaymentId";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PaymentId", id);

            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new Payment
                {
                    PaymentId = Convert.ToInt32(reader["PaymentId"]),
                    BookingId = Convert.ToInt32(reader["BookingId"]),
                    PaymentCode = reader["PaymentCode"]?.ToString(),
                    Amount = Convert.ToDecimal(reader["Amount"]),
                    PaymentMethod = reader["PaymentMethod"]?.ToString(),

                    PaymentDate = reader["PaymentDate"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["PaymentDate"]),

                    Status = reader["Status"]?.ToString(),

                    TransactionCode = reader["TransactionCode"] == DBNull.Value
                        ? null
                        : reader["TransactionCode"]?.ToString(),

                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                };
            }

            return null;
        }

        // CREATE
        public int Create(Payment payment)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string sql = @"
                INSERT INTO Payments
                (
                    BookingId,
                    PaymentCode,
                    Amount,
                    PaymentMethod,
                    PaymentDate,
                    Status,
                    TransactionCode,
                    CreatedAt
                )
                VALUES
                (
                    @BookingId,
                    @PaymentCode,
                    @Amount,
                    @PaymentMethod,
                    @PaymentDate,
                    @Status,
                    @TransactionCode,
                    GETDATE()
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@BookingId", payment.BookingId);
            cmd.Parameters.AddWithValue("@PaymentCode", payment.PaymentCode);
            cmd.Parameters.AddWithValue("@Amount", payment.Amount);
            cmd.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);

            cmd.Parameters.AddWithValue(
                "@PaymentDate",
                (object?)payment.PaymentDate ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue("@Status", payment.Status);

            cmd.Parameters.AddWithValue(
                "@TransactionCode",
                (object?)payment.TransactionCode ?? DBNull.Value
            );

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // UPDATE
        public bool Update(Payment payment)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string sql = @"
                UPDATE Payments
                SET
                    BookingId = @BookingId,
                    PaymentCode = @PaymentCode,
                    Amount = @Amount,
                    PaymentMethod = @PaymentMethod,
                    PaymentDate = @PaymentDate,
                    Status = @Status,
                    TransactionCode = @TransactionCode
                WHERE PaymentId = @PaymentId";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@PaymentId", payment.PaymentId);
            cmd.Parameters.AddWithValue("@BookingId", payment.BookingId);
            cmd.Parameters.AddWithValue("@PaymentCode", payment.PaymentCode);
            cmd.Parameters.AddWithValue("@Amount", payment.Amount);
            cmd.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);

            cmd.Parameters.AddWithValue(
                "@PaymentDate",
                (object?)payment.PaymentDate ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue("@Status", payment.Status);

            cmd.Parameters.AddWithValue(
                "@TransactionCode",
                (object?)payment.TransactionCode ?? DBNull.Value
            );

            return cmd.ExecuteNonQuery() > 0;
        }

        // DELETE
        public bool Delete(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string sql = @"
                DELETE FROM Payments
                WHERE PaymentId = @PaymentId";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@PaymentId", id);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}