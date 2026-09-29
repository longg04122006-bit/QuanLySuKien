using DAL;
using Model;

namespace BLL
{
    public class EventBLL
    {
        private readonly EventDAL _eventDAL;

        public EventBLL(EventDAL eventDAL)
        {
            _eventDAL = eventDAL;
        }

        // Lấy tất cả
        public List<EventModel> GetAll()
        {
            return _eventDAL.GetAll();
        }

        // Lấy theo ID
        public EventModel? GetById(int id)
        {
            return _eventDAL.GetById(id);
        }

        // Thêm
        public int Create(EventModel model)
        {
            if (string.IsNullOrWhiteSpace(model.EventName))
            {
                throw new ArgumentException(
                    "Tên sự kiện không được để trống."
                );
            }

            if (model.EndDate <= model.StartDate)
            {
                throw new ArgumentException(
                    "Thời gian kết thúc phải lớn hơn thời gian bắt đầu."
                );
            }

            if (model.CategoryId <= 0)
            {
                throw new ArgumentException(
                    "CategoryId không hợp lệ."
                );
            }

            if (model.VenueId <= 0)
            {
                throw new ArgumentException(
                    "VenueId không hợp lệ."
                );
            }

            if (model.OrganizerId <= 0)
            {
                throw new ArgumentException(
                    "OrganizerId không hợp lệ."
                );
            }

            if (string.IsNullOrWhiteSpace(model.Status))
            {
                model.Status = "UPCOMING";
            }

            return _eventDAL.Create(model);
        }

        // Cập nhật
        public bool Update(EventModel model)
        {
            if (model.EventId <= 0)
            {
                throw new ArgumentException(
                    "EventId không hợp lệ."
                );
            }

            if (string.IsNullOrWhiteSpace(model.EventName))
            {
                throw new ArgumentException(
                    "Tên sự kiện không được để trống."
                );
            }

            if (model.EndDate <= model.StartDate)
            {
                throw new ArgumentException(
                    "Thời gian kết thúc phải lớn hơn thời gian bắt đầu."
                );
            }

            return _eventDAL.Update(model);
        }

        // Xóa mềm
        public bool Delete(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "EventId không hợp lệ."
                );
            }

            return _eventDAL.Delete(id);
        }
    }
}