using DAL;
using Model;

namespace BLL
{
    public class EventCategoryBLL
    {
        private readonly EventCategoryDAL _eventCategoryDAL;

        public EventCategoryBLL(EventCategoryDAL eventCategoryDAL)
        {
            _eventCategoryDAL = eventCategoryDAL;
        }

        // Lấy tất cả
        public List<EventCategory> GetAll()
        {
            return _eventCategoryDAL.GetAll();
        }

        // Lấy theo ID
        public EventCategory? GetById(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "CategoryId không hợp lệ."
                );
            }

            return _eventCategoryDAL.GetById(id);
        }

        // Thêm
        public int Create(EventCategory model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new ArgumentException(
                    "Tên loại sự kiện không được để trống."
                );
            }

            return _eventCategoryDAL.Create(model);
        }

        // Cập nhật
        public bool Update(EventCategory model)
        {
            if (model.Id <= 0)
            {
                throw new ArgumentException(
                    "CategoryId không hợp lệ."
                );
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new ArgumentException(
                    "Tên loại sự kiện không được để trống."
                );
            }

            return _eventCategoryDAL.Update(model);
        }

        // Xóa mềm
        public bool Delete(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "CategoryId không hợp lệ."
                );
            }

            return _eventCategoryDAL.Delete(id);
        }
    }
}