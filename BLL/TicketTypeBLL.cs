using DAL;
using Model;

namespace BLL
{
    public class TicketTypeBLL
    {
        private readonly TicketTypeDAL _ticketTypeDAL;

        public TicketTypeBLL(TicketTypeDAL ticketTypeDAL)
        {
            _ticketTypeDAL = ticketTypeDAL;
        }

        // GET ALL
        public List<TicketType> GetAll()
        {
            return _ticketTypeDAL.GetAll();
        }

        // GET BY ID
        public TicketType? GetById(int id)
        {
            return _ticketTypeDAL.GetById(id);
        }

        // POST
        public int Create(TicketType ticketType)
        {
            if (ticketType.EventId <= 0)
                throw new Exception("EventId không hợp lệ.");

            if (string.IsNullOrWhiteSpace(ticketType.TicketName))
                throw new Exception("Tên loại vé không được để trống.");

            if (ticketType.Price < 0)
                throw new Exception("Giá vé không được âm.");

            if (ticketType.Quantity < 0)
                throw new Exception("Số lượng vé không được âm.");

            if (ticketType.SoldQuantity < 0)
                throw new Exception("Số vé đã bán không được âm.");

            if (ticketType.SoldQuantity > ticketType.Quantity)
                throw new Exception(
                    "SoldQuantity không được lớn hơn Quantity.");

            return _ticketTypeDAL.Create(ticketType);
        }

        // PUT
        public bool Update(TicketType ticketType)
        {
            if (ticketType.TicketTypeId <= 0)
                throw new Exception("TicketTypeId không hợp lệ.");

            if (ticketType.EventId <= 0)
                throw new Exception("EventId không hợp lệ.");

            if (string.IsNullOrWhiteSpace(ticketType.TicketName))
                throw new Exception("Tên loại vé không được để trống.");

            if (ticketType.Price < 0)
                throw new Exception("Giá vé không được âm.");

            if (ticketType.Quantity < 0)
                throw new Exception("Số lượng vé không được âm.");

            if (ticketType.SoldQuantity < 0)
                throw new Exception("Số vé đã bán không được âm.");

            if (ticketType.SoldQuantity > ticketType.Quantity)
                throw new Exception(
                    "SoldQuantity không được lớn hơn Quantity.");

            return _ticketTypeDAL.Update(ticketType);
        }

        // DELETE
        public bool Delete(int id)
        {
            if (id <= 0)
                throw new Exception(
                    "TicketTypeId không hợp lệ.");

            return _ticketTypeDAL.Delete(id);
        }
    }
}