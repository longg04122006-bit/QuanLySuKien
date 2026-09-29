using DAL;
using Model;

namespace BLL
{
    public class PaymentBLL
    {
        private readonly PaymentDAL _paymentDAL;

        public PaymentBLL(PaymentDAL paymentDAL)
        {
            _paymentDAL = paymentDAL;
        }

        public List<Payment> GetAll()
        {
            return _paymentDAL.GetAll();
        }

        public Payment? GetById(int id)
        {
            if (id <= 0)
                throw new Exception("PaymentId không hợp lệ");

            return _paymentDAL.GetById(id);
        }

        public int Create(Payment payment)
        {
            if (payment.BookingId <= 0)
                throw new Exception("BookingId không hợp lệ");

            if (payment.Amount < 0)
                throw new Exception("Amount không được âm");

            if (string.IsNullOrWhiteSpace(payment.PaymentCode))
                throw new Exception("PaymentCode không được để trống");

            if (string.IsNullOrWhiteSpace(payment.PaymentMethod))
                throw new Exception("PaymentMethod không được để trống");

            if (string.IsNullOrWhiteSpace(payment.Status))
                payment.Status = "Pending";

            if (payment.PaymentDate == null)
                payment.PaymentDate = DateTime.Now;

            return _paymentDAL.Create(payment);
        }

        public bool Update(Payment payment)
        {
            if (payment.PaymentId <= 0)
                throw new Exception("PaymentId không hợp lệ");

            if (payment.BookingId <= 0)
                throw new Exception("BookingId không hợp lệ");

            if (payment.Amount < 0)
                throw new Exception("Amount không được âm");

            if (string.IsNullOrWhiteSpace(payment.PaymentCode))
                throw new Exception("PaymentCode không được để trống");

            if (string.IsNullOrWhiteSpace(payment.PaymentMethod))
                throw new Exception("PaymentMethod không được để trống");

            if (string.IsNullOrWhiteSpace(payment.Status))
                payment.Status = "Pending";

            return _paymentDAL.Update(payment);
        }

        public bool Delete(int id)
        {
            if (id <= 0)
                throw new Exception("PaymentId không hợp lệ");

            return _paymentDAL.Delete(id);
        }
    }
}