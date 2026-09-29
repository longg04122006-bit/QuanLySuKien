using DAL.Helper;
using Microsoft.Data.SqlClient;
using Model;

namespace DAL
{
    public class EventDAL
    {
        private readonly DatabaseHelper _databaseHelper;

        public EventDAL(DatabaseHelper databaseHelper)
        {
            _databaseHelper = databaseHelper;
        }

        // =========================================
        // LẤY TẤT CẢ SỰ KIỆN
        // =========================================
        public List<EventModel> GetAll()
        {
            var events = new List<EventModel>();

            using var connection = _databaseHelper.GetConnection();

            string sql = @"
                SELECT
                    EventId,
                    EventName,
                    CategoryId,
                    VenueId,
                    Description,
                    StartDate,
                    EndDate,
                    OrganizerId,
                    Status,
                    ImageUrl,
                    IsDeleted,
                    CreatedAt,
                    UpdatedAt
                FROM Events
                WHERE IsDeleted = 0
                ORDER BY EventId DESC";

            using var command = new SqlCommand(sql, connection);

            connection.Open();

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                events.Add(MapEvent(reader));
            }

            return events;
        }

        // =========================================
        // LẤY SỰ KIỆN THEO ID
        // =========================================
        public EventModel? GetById(int id)
        {
            using var connection = _databaseHelper.GetConnection();

            string sql = @"
                SELECT
                    EventId,
                    EventName,
                    CategoryId,
                    VenueId,
                    Description,
                    StartDate,
                    EndDate,
                    OrganizerId,
                    Status,
                    ImageUrl,
                    IsDeleted,
                    CreatedAt,
                    UpdatedAt
                FROM Events
                WHERE EventId = @EventId
                  AND IsDeleted = 0";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@EventId", id);

            connection.Open();

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return MapEvent(reader);
            }

            return null;
        }

        // =========================================
        // THÊM SỰ KIỆN
        // =========================================
        public int Create(EventModel model)
        {
            using var connection = _databaseHelper.GetConnection();

            string sql = @"
                INSERT INTO Events
                (
                    EventName,
                    CategoryId,
                    VenueId,
                    Description,
                    StartDate,
                    EndDate,
                    OrganizerId,
                    Status,
                    ImageUrl,
                    IsDeleted,
                    CreatedAt
                )
                VALUES
                (
                    @EventName,
                    @CategoryId,
                    @VenueId,
                    @Description,
                    @StartDate,
                    @EndDate,
                    @OrganizerId,
                    @Status,
                    @ImageUrl,
                    0,
                    GETDATE()
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@EventName", model.EventName);
            command.Parameters.AddWithValue("@CategoryId", model.CategoryId);
            command.Parameters.AddWithValue("@VenueId", model.VenueId);
            command.Parameters.AddWithValue(
                "@Description",
                (object?)model.Description ?? DBNull.Value
            );
            command.Parameters.AddWithValue("@StartDate", model.StartDate);
            command.Parameters.AddWithValue("@EndDate", model.EndDate);
            command.Parameters.AddWithValue("@OrganizerId", model.OrganizerId);
            command.Parameters.AddWithValue("@Status", model.Status);
            command.Parameters.AddWithValue(
                "@ImageUrl",
                (object?)model.ImageUrl ?? DBNull.Value
            );

            connection.Open();

            return Convert.ToInt32(command.ExecuteScalar());
        }

        // =========================================
        // CẬP NHẬT SỰ KIỆN
        // =========================================
        public bool Update(EventModel model)
        {
            using var connection = _databaseHelper.GetConnection();

            string sql = @"
                UPDATE Events
                SET
                    EventName = @EventName,
                    CategoryId = @CategoryId,
                    VenueId = @VenueId,
                    Description = @Description,
                    StartDate = @StartDate,
                    EndDate = @EndDate,
                    OrganizerId = @OrganizerId,
                    Status = @Status,
                    ImageUrl = @ImageUrl,
                    UpdatedAt = GETDATE()
                WHERE EventId = @EventId
                  AND IsDeleted = 0";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@EventId", model.EventId);
            command.Parameters.AddWithValue("@EventName", model.EventName);
            command.Parameters.AddWithValue("@CategoryId", model.CategoryId);
            command.Parameters.AddWithValue("@VenueId", model.VenueId);
            command.Parameters.AddWithValue(
                "@Description",
                (object?)model.Description ?? DBNull.Value
            );
            command.Parameters.AddWithValue("@StartDate", model.StartDate);
            command.Parameters.AddWithValue("@EndDate", model.EndDate);
            command.Parameters.AddWithValue("@OrganizerId", model.OrganizerId);
            command.Parameters.AddWithValue("@Status", model.Status);
            command.Parameters.AddWithValue(
                "@ImageUrl",
                (object?)model.ImageUrl ?? DBNull.Value
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        // =========================================
        // XÓA MỀM
        // =========================================
        public bool Delete(int id)
        {
            using var connection = _databaseHelper.GetConnection();

            string sql = @"
                UPDATE Events
                SET
                    IsDeleted = 1,
                    UpdatedAt = GETDATE()
                WHERE EventId = @EventId
                  AND IsDeleted = 0";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@EventId", id);

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        // =========================================
        // MAP DATABASE → MODEL
        // =========================================
        private EventModel MapEvent(SqlDataReader reader)
        {
            return new EventModel
            {
                EventId = Convert.ToInt32(reader["EventId"]),

                EventName = reader["EventName"].ToString() ?? "",

                CategoryId = Convert.ToInt32(reader["CategoryId"]),

                VenueId = Convert.ToInt32(reader["VenueId"]),

                Description = reader["Description"] == DBNull.Value
                    ? null
                    : reader["Description"].ToString(),

                StartDate = Convert.ToDateTime(reader["StartDate"]),

                EndDate = Convert.ToDateTime(reader["EndDate"]),

                OrganizerId = Convert.ToInt32(reader["OrganizerId"]),

                Status = reader["Status"].ToString() ?? "",

                ImageUrl = reader["ImageUrl"] == DBNull.Value
                    ? null
                    : reader["ImageUrl"].ToString(),

                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),

                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),

                UpdatedAt = reader["UpdatedAt"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["UpdatedAt"])
            };
        }
    }
}