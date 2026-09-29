using DAL;
using Model;

namespace BLL
{
    public class BookingBLL
    {
        private readonly BookingDAL _bookingDAL;

        public BookingBLL(BookingDAL bookingDAL)
        {
            _bookingDAL = bookingDAL;
        }

        public List<Booking> GetAll()
        {
            return _bookingDAL.GetAll();
        }

        public Booking? GetById(int id)
        {
            if (id <= 0)
                throw new Exception("BookingId không hợp lệ");

            return _bookingDAL.GetById(id);
        }

        public int Create(Booking booking)
        {
            if (booking.UserId <= 0)
                throw new Exception("UserId không hợp lệ");

            if (booking.TotalAmount < 0)
                throw new Exception("TotalAmount không được âm");

            if (string.IsNullOrWhiteSpace(booking.BookingCode))
                throw new Exception("BookingCode không được để trống");

            if (string.IsNullOrWhiteSpace(booking.Status))
                booking.Status = "Pending";

            if (booking.BookingDate == default)
                booking.BookingDate = DateTime.Now;

            return _bookingDAL.Create(booking);
        }

        public bool Update(Booking booking)
        {
            if (booking.BookingId <= 0)
                throw new Exception("BookingId không hợp lệ");

            if (booking.UserId <= 0)
                throw new Exception("UserId không hợp lệ");

            if (booking.TotalAmount < 0)
                throw new Exception("TotalAmount không được âm");

            if (string.IsNullOrWhiteSpace(booking.BookingCode))
                throw new Exception("BookingCode không được để trống");

            if (string.IsNullOrWhiteSpace(booking.Status))
                booking.Status = "Pending";

            return _bookingDAL.Update(booking);
        }

        public bool Delete(int id)
        {
            if (id <= 0)
                throw new Exception("BookingId không hợp lệ");

            return _bookingDAL.Delete(id);
        }
    }
}