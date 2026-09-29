using DAL;
using Model;

namespace BLL
{
    public class BookingDetailBLL
    {
        private readonly BookingDetailDAL _bookingDetailDAL;

        public BookingDetailBLL(
            BookingDetailDAL bookingDetailDAL)
        {
            _bookingDetailDAL = bookingDetailDAL;
        }

        // GET ALL
        public List<BookingDetail> GetAll()
        {
            return _bookingDetailDAL.GetAll();
        }

        // GET BY ID
        public BookingDetail? GetById(int id)
        {
            return _bookingDetailDAL.GetById(id);
        }

        // POST
        public int Create(BookingDetail detail)
        {
            if (detail.BookingId <= 0)
                throw new Exception(
                    "BookingId không hợp lệ.");

            if (detail.TicketTypeId <= 0)
                throw new Exception(
                    "TicketTypeId không hợp lệ.");

            if (detail.Quantity <= 0)
                throw new Exception(
                    "Quantity phải lớn hơn 0.");

            if (detail.UnitPrice < 0)
                throw new Exception(
                    "UnitPrice không được âm.");

            // Tự tính thành tiền
            detail.Amount =
                detail.Quantity * detail.UnitPrice;

            return _bookingDetailDAL.Create(detail);
        }

        // PUT
        public bool Update(BookingDetail detail)
        {
            if (detail.BookingDetailId <= 0)
                throw new Exception(
                    "BookingDetailId không hợp lệ.");

            if (detail.BookingId <= 0)
                throw new Exception(
                    "BookingId không hợp lệ.");

            if (detail.TicketTypeId <= 0)
                throw new Exception(
                    "TicketTypeId không hợp lệ.");

            if (detail.Quantity <= 0)
                throw new Exception(
                    "Quantity phải lớn hơn 0.");

            if (detail.UnitPrice < 0)
                throw new Exception(
                    "UnitPrice không được âm.");

            // Tự tính lại Amount
            detail.Amount =
                detail.Quantity * detail.UnitPrice;

            return _bookingDetailDAL.Update(detail);
        }

        // DELETE
        public bool Delete(int id)
        {
            if (id <= 0)
                throw new Exception(
                    "BookingDetailId không hợp lệ.");

            return _bookingDetailDAL.Delete(id);
        }
    }
}