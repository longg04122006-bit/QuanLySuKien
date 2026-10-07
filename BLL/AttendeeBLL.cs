using DAL;
using Model;

namespace BLL
{
    public class AttendeeBLL
    {
        private readonly AttendeeDAL _attendeeDAL;

        public AttendeeBLL(AttendeeDAL attendeeDAL)
        {
            _attendeeDAL = attendeeDAL;
        }

        // GET ALL
        public List<Attendee> GetAll()
        {
            return _attendeeDAL.GetAll();
        }

        // GET BY ID
        public Attendee? GetById(int id)
        {
            if (id <= 0)
                throw new Exception(
                    "AttendeeId phải lớn hơn 0.");

            return _attendeeDAL.GetById(id);
        }

        // POST
        public int Create(Attendee attendee)
        {
            Validate(attendee);

            return _attendeeDAL.Create(attendee);
        }

        // PUT
        public bool Update(Attendee attendee)
        {
            Validate(attendee);

            if (attendee.AttendeeId <= 0)
                throw new Exception(
                    "AttendeeId phải lớn hơn 0.");

            return _attendeeDAL.Update(attendee);
        }

        // DELETE
        public bool Delete(int id)
        {
            if (id <= 0)
                throw new Exception(
                    "AttendeeId phải lớn hơn 0.");

            return _attendeeDAL.Delete(id);
        }

        // VALIDATE
        private void Validate(Attendee attendee)
        {
            if (attendee == null)
                throw new Exception(
                    "Dữ liệu Attendee không hợp lệ.");

            if (attendee.TicketId <= 0)
                throw new Exception(
                    "TicketId phải lớn hơn 0.");

            if (string.IsNullOrWhiteSpace(attendee.FullName))
                throw new Exception(
                    "FullName không được để trống.");

            if (attendee.FullName.Length > 100)
                throw new Exception(
                    "FullName không được vượt quá 100 ký tự.");

            if (attendee.Email != null &&
                attendee.Email.Length > 100)
                throw new Exception(
                    "Email không được vượt quá 100 ký tự.");

            if (attendee.Phone != null &&
                attendee.Phone.Length > 20)
                throw new Exception(
                    "Phone không được vượt quá 20 ký tự.");
        }
    }
}