using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Model;
using System.Data;

namespace DAL
{
    public class VenueDAL
    {
        private readonly string _connectionString;

        public VenueDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Không tìm thấy ConnectionString");
        }

        public List<Venue> GetAll()
        {
            var list = new List<Venue>();

            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"SELECT VenueId, VenueName, Address, City,
                                  Capacity, Description, IsDeleted,
                                  CreatedAt, UpdatedAt
                           FROM Venues
                           WHERE IsDeleted = 0
                           ORDER BY VenueId DESC";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapVenue(reader));
            }

            return list;
        }

        public Venue? GetById(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"SELECT VenueId, VenueName, Address, City,
                                  Capacity, Description, IsDeleted,
                                  CreatedAt, UpdatedAt
                           FROM Venues
                           WHERE VenueId = @VenueId
                           AND IsDeleted = 0";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@VenueId", SqlDbType.Int).Value = id;

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return MapVenue(reader);
            }

            return null;
        }

        public int Create(Venue venue)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Venues
                           (
                               VenueName,
                               Address,
                               City,
                               Capacity,
                               Description,
                               IsDeleted,
                               CreatedAt
                           )
                           VALUES
                           (
                               @VenueName,
                               @Address,
                               @City,
                               @Capacity,
                               @Description,
                               0,
                               GETDATE()
                           );

                           SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@VenueName", SqlDbType.NVarChar, 150)
                .Value = venue.VenueName;

            cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 255)
                .Value = venue.Address;

            cmd.Parameters.Add("@City", SqlDbType.NVarChar, 100)
                .Value = venue.City;

            cmd.Parameters.Add("@Capacity", SqlDbType.Int)
                .Value = venue.Capacity;

            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500)
                .Value = (object?)venue.Description ?? DBNull.Value;

            conn.Open();

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool Update(Venue venue)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"UPDATE Venues
                           SET VenueName = @VenueName,
                               Address = @Address,
                               City = @City,
                               Capacity = @Capacity,
                               Description = @Description,
                               UpdatedAt = GETDATE()
                           WHERE VenueId = @VenueId
                           AND IsDeleted = 0";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@VenueId", SqlDbType.Int)
                .Value = venue.VenueId;

            cmd.Parameters.Add("@VenueName", SqlDbType.NVarChar, 150)
                .Value = venue.VenueName;

            cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 255)
                .Value = venue.Address;

            cmd.Parameters.Add("@City", SqlDbType.NVarChar, 100)
                .Value = venue.City;

            cmd.Parameters.Add("@Capacity", SqlDbType.Int)
                .Value = venue.Capacity;

            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500)
                .Value = (object?)venue.Description ?? DBNull.Value;

            conn.Open();

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"UPDATE Venues
                           SET IsDeleted = 1,
                               UpdatedAt = GETDATE()
                           WHERE VenueId = @VenueId
                           AND IsDeleted = 0";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@VenueId", SqlDbType.Int).Value = id;

            conn.Open();

            return cmd.ExecuteNonQuery() > 0;
        }

        private Venue MapVenue(SqlDataReader reader)
        {
            return new Venue
            {
                VenueId = reader.GetInt32(0),
                VenueName = reader.GetString(1),
                Address = reader.GetString(2),
                City = reader.GetString(3),
                Capacity = reader.GetInt32(4),
                Description = reader.IsDBNull(5)
                    ? null
                    : reader.GetString(5),
                IsDeleted = reader.GetBoolean(6),
                CreatedAt = reader.GetDateTime(7),
                UpdatedAt = reader.IsDBNull(8)
                    ? null
                    : reader.GetDateTime(8)
            };
        }
    }
}