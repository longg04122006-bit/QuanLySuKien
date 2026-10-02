
using DAL;
using Model;

namespace BLL
{
    public class NotificationBLL
    {
        private readonly NotificationDAL _notificationDAL;

        public NotificationBLL(NotificationDAL notificationDAL)
        {
            _notificationDAL = notificationDAL;
        }

        public List<Notification> GetAll()
        {
            return _notificationDAL.GetAll();
        }

        public Notification? GetById(int id)
        {
            if (id <= 0)
                throw new Exception("NotificationId phải lớn hơn 0");

            return _notificationDAL.GetById(id);
        }

        public int Create(Notification notification)
        {
            Validate(notification);

            if (notification.UserId <= 0)
                throw new Exception("UserId phải lớn hơn 0");

            notification.CreatedAt = DateTime.Now;

            return _notificationDAL.Create(notification);
        }

        public bool Update(Notification notification)
        {
            if (notification.NotificationId <= 0)
                throw new Exception("NotificationId phải lớn hơn 0");

            Validate(notification);

            if (notification.UserId <= 0)
                throw new Exception("UserId phải lớn hơn 0");

            return _notificationDAL.Update(notification);
        }

        public bool MarkAsRead(int id)
        {
            if (id <= 0)
                throw new Exception("NotificationId phải lớn hơn 0");

            return _notificationDAL.MarkAsRead(id);
        }

        public bool Delete(int id)
        {
            if (id <= 0)
                throw new Exception("NotificationId phải lớn hơn 0");

            return _notificationDAL.Delete(id);
        }

        private void Validate(Notification notification)
        {
            if (notification == null)
                throw new Exception("Dữ liệu thông báo không hợp lệ");

            if (string.IsNullOrWhiteSpace(notification.Title))
                throw new Exception("Title không được để trống");

            if (notification.Title.Length > 200)
                throw new Exception("Title không được vượt quá 200 ký tự");

            if (string.IsNullOrWhiteSpace(notification.Message))
                throw new Exception("Message không được để trống");

            if (notification.Message.Length > 1000)
                throw new Exception("Message không được vượt quá 1000 ký tự");

            if (notification.NotificationType?.Length > 50)
                throw new Exception("NotificationType không được vượt quá 50 ký tự");
        }
    }
}