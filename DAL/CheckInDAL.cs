using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Model;
using System.Data;

namespace DAL
{
    public class CheckInDAL
    {
        private readonly string _connectionString;

        public CheckInDAL(IConfiguration configuration)
        {
            _connectionString = configuration
                .GetConnectionString("DefaultConnection")
                ?? throw new Exception("Không tìm thấy ConnectionString");
        }

        // Lấy danh sách check-in
        public List<CheckIn> GetAll()
        {
            var list = new List<CheckIn>();

            using SqlConnection conn = new SqlConnection(_connectionString);
            string sql = @"SELECT CheckInId, TicketId, CheckInTime,
                                  CheckedInBy, Status
                           FROM CheckIns
                           ORDER BY CheckInId DESC";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new CheckIn
                {
                    CheckInId = reader.GetInt32(0),
                    TicketId = reader.GetInt32(1),
                    CheckInTime = reader.GetDateTime(2),
                    CheckedInBy = reader.GetInt32(3),
                    Status = reader.GetString(4)
                });
            }

            return list;
        }

        // Lấy check-in theo ID
        public CheckIn? GetById(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"SELECT CheckInId, TicketId, CheckInTime,
                                  CheckedInBy, Status
                           FROM CheckIns
                           WHERE CheckInId = @CheckInId";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@CheckInId", SqlDbType.Int).Value = id;

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new CheckIn
                {
                    CheckInId = reader.GetInt32(0),
                    TicketId = reader.GetInt32(1),
                    CheckInTime = reader.GetDateTime(2),
                    CheckedInBy = reader.GetInt32(3),
                    Status = reader.GetString(4)
                };
            }

            return null;
        }

        // Thêm check-in
        public int Create(CheckIn checkIn)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO CheckIns
                           (TicketId, CheckInTime, CheckedInBy, Status)
                           VALUES
                           (@TicketId, @CheckInTime, @CheckedInBy, @Status);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@TicketId", SqlDbType.Int).Value =
                checkIn.TicketId;

            cmd.Parameters.Add("@CheckInTime", SqlDbType.DateTime).Value =
                checkIn.CheckInTime;

            cmd.Parameters.Add("@CheckedInBy", SqlDbType.Int).Value =
                checkIn.CheckedInBy;

            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value =
                checkIn.Status;

            conn.Open();

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // Cập nhật check-in
        public bool Update(CheckIn checkIn)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"UPDATE CheckIns
                           SET TicketId = @TicketId,
                               CheckInTime = @CheckInTime,
                               CheckedInBy = @CheckedInBy,
                               Status = @Status
                           WHERE CheckInId = @CheckInId";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@CheckInId", SqlDbType.Int).Value =
                checkIn.CheckInId;

            cmd.Parameters.Add("@TicketId", SqlDbType.Int).Value =
                checkIn.TicketId;

            cmd.Parameters.Add("@CheckInTime", SqlDbType.DateTime).Value =
                checkIn.CheckInTime;

            cmd.Parameters.Add("@CheckedInBy", SqlDbType.Int).Value =
                checkIn.CheckedInBy;

            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value =
                checkIn.Status;

            conn.Open();

            return cmd.ExecuteNonQuery() > 0;
        }

        // Xóa check-in
        public bool Delete(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = "DELETE FROM CheckIns WHERE CheckInId = @CheckInId";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@CheckInId", SqlDbType.Int).Value = id;

            conn.Open();

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}