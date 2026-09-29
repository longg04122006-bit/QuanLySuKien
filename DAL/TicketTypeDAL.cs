using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Model;

namespace DAL
{
    public class TicketTypeDAL
    {
        private readonly string _connectionString;

        public TicketTypeDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Không tìm thấy chuỗi kết nối Database.");
        }

        // GET ALL
        public List<TicketType> GetAll()
        {
            List<TicketType> ticketTypes = new List<TicketType>();

            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"
                SELECT
                    TicketTypeId,
                    EventId,
                    TicketName,
                    Price,
                    Quantity,
                    SoldQuantity,
                    SaleStart,
                    SaleEnd,
                    IsDeleted,
                    CreatedAt
                FROM TicketTypes
                WHERE IsDeleted = 0
                ORDER BY TicketTypeId DESC";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                ticketTypes.Add(new TicketType
                {
                    TicketTypeId = Convert.ToInt32(reader["TicketTypeId"]),
                    EventId = Convert.ToInt32(reader["EventId"]),
                    TicketName = reader["TicketName"]?.ToString(),
                    Price = Convert.ToDecimal(reader["Price"]),
                    Quantity = Convert.ToInt32(reader["Quantity"]),
                    SoldQuantity = Convert.ToInt32(reader["SoldQuantity"]),

                    SaleStart = reader["SaleStart"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["SaleStart"]),

                    SaleEnd = reader["SaleEnd"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["SaleEnd"]),

                    IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),

                    CreatedAt = reader["CreatedAt"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["CreatedAt"])
                });
            }

            return ticketTypes;
        }

        // GET BY ID
        public TicketType? GetById(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"
                SELECT
                    TicketTypeId,
                    EventId,
                    TicketName,
                    Price,
                    Quantity,
                    SoldQuantity,
                    SaleStart,
                    SaleEnd,
                    IsDeleted,
                    CreatedAt
                FROM TicketTypes
                WHERE TicketTypeId = @TicketTypeId
                  AND IsDeleted = 0";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@TicketTypeId", id);

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new TicketType
                {
                    TicketTypeId = Convert.ToInt32(reader["TicketTypeId"]),
                    EventId = Convert.ToInt32(reader["EventId"]),
                    TicketName = reader["TicketName"]?.ToString(),
                    Price = Convert.ToDecimal(reader["Price"]),
                    Quantity = Convert.ToInt32(reader["Quantity"]),
                    SoldQuantity = Convert.ToInt32(reader["SoldQuantity"]),

                    SaleStart = reader["SaleStart"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["SaleStart"]),

                    SaleEnd = reader["SaleEnd"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["SaleEnd"]),

                    IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),

                    CreatedAt = reader["CreatedAt"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["CreatedAt"])
                };
            }

            return null;
        }

        // POST
        public int Create(TicketType ticketType)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"
                INSERT INTO TicketTypes
                (
                    EventId,
                    TicketName,
                    Price,
                    Quantity,
                    SoldQuantity,
                    SaleStart,
                    SaleEnd,
                    IsDeleted,
                    CreatedAt
                )
                VALUES
                (
                    @EventId,
                    @TicketName,
                    @Price,
                    @Quantity,
                    @SoldQuantity,
                    @SaleStart,
                    @SaleEnd,
                    0,
                    GETDATE()
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@EventId",
                ticketType.EventId);

            cmd.Parameters.AddWithValue(
                "@TicketName",
                (object?)ticketType.TicketName ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Price",
                ticketType.Price);

            cmd.Parameters.AddWithValue(
                "@Quantity",
                ticketType.Quantity);

            cmd.Parameters.AddWithValue(
                "@SoldQuantity",
                ticketType.SoldQuantity);

            cmd.Parameters.AddWithValue(
                "@SaleStart",
                (object?)ticketType.SaleStart ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@SaleEnd",
                (object?)ticketType.SaleEnd ?? DBNull.Value);

            conn.Open();

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // PUT
        public bool Update(TicketType ticketType)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE TicketTypes
                SET
                    EventId = @EventId,
                    TicketName = @TicketName,
                    Price = @Price,
                    Quantity = @Quantity,
                    SoldQuantity = @SoldQuantity,
                    SaleStart = @SaleStart,
                    SaleEnd = @SaleEnd
                WHERE TicketTypeId = @TicketTypeId
                  AND IsDeleted = 0";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@TicketTypeId",
                ticketType.TicketTypeId);

            cmd.Parameters.AddWithValue(
                "@EventId",
                ticketType.EventId);

            cmd.Parameters.AddWithValue(
                "@TicketName",
                (object?)ticketType.TicketName ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Price",
                ticketType.Price);

            cmd.Parameters.AddWithValue(
                "@Quantity",
                ticketType.Quantity);

            cmd.Parameters.AddWithValue(
                "@SoldQuantity",
                ticketType.SoldQuantity);

            cmd.Parameters.AddWithValue(
                "@SaleStart",
                (object?)ticketType.SaleStart ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@SaleEnd",
                (object?)ticketType.SaleEnd ?? DBNull.Value);

            conn.Open();

            return cmd.ExecuteNonQuery() > 0;
        }

        // DELETE MỀM
        public bool Delete(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE TicketTypes
                SET IsDeleted = 1
                WHERE TicketTypeId = @TicketTypeId
                  AND IsDeleted = 0";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@TicketTypeId",
                id);

            conn.Open();

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}