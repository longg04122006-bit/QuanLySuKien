using DAL;
using Model;

namespace BLL
{
    public class VenueBLL
    {
        private readonly VenueDAL _venueDAL;

        public VenueBLL(VenueDAL venueDAL)
        {
            _venueDAL = venueDAL;
        }

        public List<Venue> GetAll()
        {
            return _venueDAL.GetAll();
        }

        public Venue? GetById(int id)
        {
            if (id <= 0)
                throw new Exception("VenueId phải lớn hơn 0");

            return _venueDAL.GetById(id);
        }

        public int Create(Venue venue)
        {
            Validate(venue);

            return _venueDAL.Create(venue);
        }

        public bool Update(Venue venue)
        {
            if (venue.VenueId <= 0)
                throw new Exception("VenueId phải lớn hơn 0");

            Validate(venue);

            return _venueDAL.Update(venue);
        }

        public bool Delete(int id)
        {
            if (id <= 0)
                throw new Exception("VenueId phải lớn hơn 0");

            return _venueDAL.Delete(id);
        }

        private void Validate(Venue venue)
        {
            if (venue == null)
                throw new Exception("Dữ liệu địa điểm không hợp lệ");

            if (string.IsNullOrWhiteSpace(venue.VenueName))
                throw new Exception("VenueName không được để trống");

            if (venue.VenueName.Length > 150)
                throw new Exception("VenueName không được vượt quá 150 ký tự");

            if (string.IsNullOrWhiteSpace(venue.Address))
                throw new Exception("Address không được để trống");

            if (venue.Address.Length > 255)
                throw new Exception("Address không được vượt quá 255 ký tự");

            if (string.IsNullOrWhiteSpace(venue.City))
                throw new Exception("City không được để trống");

            if (venue.City.Length > 100)
                throw new Exception("City không được vượt quá 100 ký tự");

            if (venue.Capacity <= 0)
                throw new Exception("Capacity phải lớn hơn 0");

            if (venue.Description?.Length > 500)
                throw new Exception("Description không được vượt quá 500 ký tự");
        }
    }
}