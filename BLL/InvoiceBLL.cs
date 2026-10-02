using DAL;
using Model;

namespace BLL
{
    public class InvoiceBLL
    {
        private readonly InvoiceDAL _invoiceDAL;

        public InvoiceBLL(InvoiceDAL invoiceDAL)
        {
            _invoiceDAL = invoiceDAL;
        }

        public List<Invoice> GetAll()
        {
            return _invoiceDAL.GetAll();
        }

        public Invoice? GetById(int id)
        {
            if (id <= 0)
                throw new Exception("InvoiceId không hợp lệ");

            return _invoiceDAL.GetById(id);
        }

        public int Create(Invoice invoice)
        {
            if (invoice.BookingId <= 0)
                throw new Exception("BookingId không hợp lệ");

            if (string.IsNullOrWhiteSpace(invoice.InvoiceCode))
                throw new Exception("InvoiceCode không được để trống");

            if (string.IsNullOrWhiteSpace(invoice.CustomerName))
                throw new Exception("CustomerName không được để trống");

            if (invoice.TotalAmount < 0)
                throw new Exception("TotalAmount không được âm");

            if (invoice.InvoiceDate == default)
                invoice.InvoiceDate = DateTime.Now;

            return _invoiceDAL.Create(invoice);
        }

        public bool Update(Invoice invoice)
        {
            if (invoice.InvoiceId <= 0)
                throw new Exception("InvoiceId không hợp lệ");

            if (invoice.BookingId <= 0)
                throw new Exception("BookingId không hợp lệ");

            if (string.IsNullOrWhiteSpace(invoice.InvoiceCode))
                throw new Exception("InvoiceCode không được để trống");

            if (string.IsNullOrWhiteSpace(invoice.CustomerName))
                throw new Exception("CustomerName không được để trống");

            if (invoice.TotalAmount < 0)
                throw new Exception("TotalAmount không được âm");

            return _invoiceDAL.Update(invoice);
        }

        public bool Delete(int id)
        {
            if (id <= 0)
                throw new Exception("InvoiceId không hợp lệ");

            return _invoiceDAL.Delete(id);
        }
    }
}