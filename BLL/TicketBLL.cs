using DAL;
using Model;

namespace BLL
{
    public class TicketBLL
    {
        private readonly TicketDAL _ticketDAL;

        public TicketBLL(TicketDAL ticketDAL)
        {
            _ticketDAL = ticketDAL;
        }

        // GET ALL
        public List<Ticket> GetAll()
        {
            return _ticketDAL.GetAll();
        }

        // GET BY ID
        public Ticket? GetById(int id)
        {
            return _ticketDAL.GetById(id);
        }

        // POST
        public int Create(Ticket ticket)
        {
            if (ticket.BookingDetailId <= 0)
                throw new Exception("BookingDetailId không hợp lệ.");

            if (ticket.EventId <= 0)
                throw new Exception("EventId không hợp lệ.");

            if (ticket.TicketTypeId <= 0)
                throw new Exception("TicketTypeId không hợp lệ.");

            return _ticketDAL.Create(ticket);
        }

        // PUT
        public bool Update(Ticket ticket)
        {
            if (ticket.TicketId <= 0)
                throw new Exception("TicketId không hợp lệ.");

            if (ticket.BookingDetailId <= 0)
                throw new Exception("BookingDetailId không hợp lệ.");

            if (ticket.EventId <= 0)
                throw new Exception("EventId không hợp lệ.");

            if (ticket.TicketTypeId <= 0)
                throw new Exception("TicketTypeId không hợp lệ.");

            return _ticketDAL.Update(ticket);
        }

        // DELETE
        public bool Delete(int id)
        {
            if (id <= 0)
                throw new Exception("TicketId không hợp lệ.");

            return _ticketDAL.Delete(id);
        }
    }
}