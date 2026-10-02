using DAL;
using Model;

namespace BLL
{
    public class CheckInBLL
    {
        private readonly CheckInDAL _checkInDAL;

        public CheckInBLL(CheckInDAL checkInDAL)
        {
            _checkInDAL = checkInDAL;
        }

        public List<CheckIn> GetAll()
        {
            return _checkInDAL.GetAll();
        }

        public CheckIn? GetById(int id)
        {
            if (id <= 0)
                throw new Exception("CheckInId phải lớn hơn 0");

            return _checkInDAL.GetById(id);
        }

        public int Create(CheckIn checkIn)
        {
            if (checkIn.TicketId <= 0)
                throw new Exception("TicketId phải lớn hơn 0");

            if (checkIn.CheckedInBy <= 0)
                throw new Exception("CheckedInBy phải lớn hơn 0");

            if (string.IsNullOrWhiteSpace(checkIn.Status))
                throw new Exception("Status không được để trống");

            if (checkIn.Status.Length > 30)
                throw new Exception("Status không được vượt quá 30 ký tự");

            if (checkIn.CheckInTime == default)
                checkIn.CheckInTime = DateTime.Now;

            return _checkInDAL.Create(checkIn);
        }

        public bool Update(CheckIn checkIn)
        {
            if (checkIn.CheckInId <= 0)
                throw new Exception("CheckInId phải lớn hơn 0");

            if (checkIn.TicketId <= 0)
                throw new Exception("TicketId phải lớn hơn 0");

            if (checkIn.CheckedInBy <= 0)
                throw new Exception("CheckedInBy phải lớn hơn 0");

            if (string.IsNullOrWhiteSpace(checkIn.Status))
                throw new Exception("Status không được để trống");

            if (checkIn.Status.Length > 30)
                throw new Exception("Status không được vượt quá 30 ký tự");

            if (checkIn.CheckInTime == default)
                checkIn.CheckInTime = DateTime.Now;

            return _checkInDAL.Update(checkIn);
        }

        public bool Delete(int id)
        {
            if (id <= 0)
                throw new Exception("CheckInId phải lớn hơn 0");

            return _checkInDAL.Delete(id);
        }
    }
}