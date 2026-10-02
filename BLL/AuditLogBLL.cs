using DAL;
using Model;

namespace BLL
{
    public class AuditLogBLL
    {
        private readonly AuditLogDAL _auditLogDAL;

        public AuditLogBLL(AuditLogDAL auditLogDAL)
        {
            _auditLogDAL = auditLogDAL;
        }

        public List<AuditLog> GetAll()
        {
            return _auditLogDAL.GetAll();
        }

        public AuditLog? GetById(long id)
        {
            if (id <= 0)
                throw new Exception("AuditLogId phải lớn hơn 0");

            return _auditLogDAL.GetById(id);
        }

        public long Create(AuditLog auditLog)
        {
            Validate(auditLog);

            return _auditLogDAL.Create(auditLog);
        }

        public bool Delete(long id)
        {
            if (id <= 0)
                throw new Exception("AuditLogId phải lớn hơn 0");

            return _auditLogDAL.Delete(id);
        }

        private void Validate(AuditLog auditLog)
        {
            if (auditLog == null)
                throw new Exception("Dữ liệu AuditLog không hợp lệ");

            if (string.IsNullOrWhiteSpace(auditLog.Action))
                throw new Exception("Action không được để trống");

            if (auditLog.Action.Length > 100)
                throw new Exception("Action không được vượt quá 100 ký tự");

            if (auditLog.TableName?.Length > 100)
                throw new Exception("TableName không được vượt quá 100 ký tự");

            if (auditLog.IPAddress?.Length > 50)
                throw new Exception("IPAddress không được vượt quá 50 ký tự");
        }
    }
}