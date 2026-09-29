using DAL.Helper;
using Microsoft.Data.SqlClient;
using Model;

namespace DAL
{
    public class EventCategoryDAL
    {
        private readonly DatabaseHelper _databaseHelper;

        public EventCategoryDAL(DatabaseHelper databaseHelper)
        {
            _databaseHelper = databaseHelper;
        }

        // =========================================
        // LẤY TẤT CẢ
        // =========================================
        public List<EventCategory> GetAll()
        {
            var categories = new List<EventCategory>();

            using var connection = _databaseHelper.GetConnection();

            string sql = @"
                SELECT
                    CategoryId,
                    CategoryName,
                    Description,
                    IsDeleted,
                    CreatedAt
                FROM EventCategories
                WHERE IsDeleted = 0
                ORDER BY CategoryId ASC";

            using var command = new SqlCommand(sql, connection);

            connection.Open();

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                categories.Add(MapCategory(reader));
            }

            return categories;
        }

        // =========================================
        // LẤY THEO ID
        // =========================================
        public EventCategory? GetById(int id)
        {
            using var connection = _databaseHelper.GetConnection();

            string sql = @"
                SELECT
                    CategoryId,
                    CategoryName,
                    Description,
                    IsDeleted,
                    CreatedAt
                FROM EventCategories
                WHERE CategoryId = @CategoryId
                  AND IsDeleted = 0";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@CategoryId", id);

            connection.Open();

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return MapCategory(reader);
            }

            return null;
        }

        // =========================================
        // THÊM
        // =========================================
        public int Create(EventCategory model)
        {
            using var connection = _databaseHelper.GetConnection();

            string sql = @"
                INSERT INTO EventCategories
                (
                    CategoryName,
                    Description,
                    IsDeleted,
                    CreatedAt
                )
                VALUES
                (
                    @CategoryName,
                    @Description,
                    0,
                    GETDATE()
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@CategoryName",
                model.Name
            );

            command.Parameters.AddWithValue(
                "@Description",
                model.Description
            );

            connection.Open();

            return Convert.ToInt32(command.ExecuteScalar());
        }

        // =========================================
        // CẬP NHẬT
        // =========================================
        public bool Update(EventCategory model)
        {
            using var connection = _databaseHelper.GetConnection();

            string sql = @"
                UPDATE EventCategories
                SET
                    CategoryName = @CategoryName,
                    Description = @Description
                WHERE CategoryId = @CategoryId
                  AND IsDeleted = 0";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@CategoryId",
                model.Id
            );

            command.Parameters.AddWithValue(
                "@CategoryName",
                model.Name
            );

            command.Parameters.AddWithValue(
                "@Description",
                model.Description
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
                UPDATE EventCategories
                SET IsDeleted = 1
                WHERE CategoryId = @CategoryId
                  AND IsDeleted = 0";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@CategoryId",
                id
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        // =========================================
        // MAP DATABASE → MODEL
        // =========================================
        private EventCategory MapCategory(SqlDataReader reader)
        {
            return new EventCategory
            {
                Id = Convert.ToInt32(
                    reader["CategoryId"]
                ),

                Name = reader["CategoryName"]
                    .ToString() ?? "",

                Description = reader["Description"]
                    .ToString() ?? "",

                IsDeleted = Convert.ToBoolean(
                    reader["IsDeleted"]
                ),

                CreatedAt = Convert.ToDateTime(
                    reader["CreatedAt"]
                )
            };
        }
    }
}