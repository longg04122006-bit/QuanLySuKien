using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Model;
using System.Data;

namespace DAL
{
    public class AuditLogDAL
    {
        private readonly string _connectionString;

        public AuditLogDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Không tìm thấy ConnectionString");
        }

        // Lấy tất cả AuditLog
        public List<AuditLog> GetAll()
        {
            var list = new List<AuditLog>();

            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"SELECT AuditLogId, UserId, Action,
                                  TableName, RecordId, OldData,
                                  NewData, IPAddress, CreatedAt
                           FROM AuditLogs
                           ORDER BY AuditLogId DESC";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapAuditLog(reader));
            }

            return list;
        }

        // Lấy AuditLog theo ID
        public AuditLog? GetById(long id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"SELECT AuditLogId, UserId, Action,
                                  TableName, RecordId, OldData,
                                  NewData, IPAddress, CreatedAt
                           FROM AuditLogs
                           WHERE AuditLogId = @AuditLogId";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@AuditLogId", SqlDbType.BigInt).Value = id;

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return MapAuditLog(reader);
        }

        // Thêm AuditLog
        public long Create(AuditLog auditLog)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO AuditLogs
                           (
                               UserId,
                               Action,
                               TableName,
                               RecordId,
                               OldData,
                               NewData,
                               IPAddress,
                               CreatedAt
                           )
                           VALUES
                           (
                               @UserId,
                               @Action,
                               @TableName,
                               @RecordId,
                               @OldData,
                               @NewData,
                               @IPAddress,
                               GETDATE()
                           );

                           SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@UserId", SqlDbType.Int)
                .Value = (object?)auditLog.UserId ?? DBNull.Value;

            cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 100)
                .Value = auditLog.Action;

            cmd.Parameters.Add("@TableName", SqlDbType.NVarChar, 100)
                .Value = (object?)auditLog.TableName ?? DBNull.Value;

            cmd.Parameters.Add("@RecordId", SqlDbType.Int)
                .Value = (object?)auditLog.RecordId ?? DBNull.Value;

            cmd.Parameters.Add("@OldData", SqlDbType.NVarChar)
                .Value = (object?)auditLog.OldData ?? DBNull.Value;

            cmd.Parameters.Add("@NewData", SqlDbType.NVarChar)
                .Value = (object?)auditLog.NewData ?? DBNull.Value;

            cmd.Parameters.Add("@IPAddress", SqlDbType.VarChar, 50)
                .Value = (object?)auditLog.IPAddress ?? DBNull.Value;

            conn.Open();

            return Convert.ToInt64(cmd.ExecuteScalar());
        }

        // Xóa AuditLog
        public bool Delete(long id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"DELETE FROM AuditLogs
                           WHERE AuditLogId = @AuditLogId";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@AuditLogId", SqlDbType.BigInt).Value = id;

            conn.Open();

            return cmd.ExecuteNonQuery() > 0;
        }

        private AuditLog MapAuditLog(SqlDataReader reader)
        {
            return new AuditLog
            {
                AuditLogId = reader.GetInt64(0),

                UserId = reader.IsDBNull(1)
                    ? null
                    : reader.GetInt32(1),

                Action = reader.GetString(2),

                TableName = reader.IsDBNull(3)
                    ? null
                    : reader.GetString(3),

                RecordId = reader.IsDBNull(4)
                    ? null
                    : reader.GetInt32(4),

                OldData = reader.IsDBNull(5)
                    ? null
                    : reader.GetString(5),

                NewData = reader.IsDBNull(6)
                    ? null
                    : reader.GetString(6),

                IPAddress = reader.IsDBNull(7)
                    ? null
                    : reader.GetString(7),

                CreatedAt = reader.GetDateTime(8)
            };
        }
    }
}