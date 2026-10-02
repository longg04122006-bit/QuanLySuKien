using DAL;
using Model;

namespace BLL
{
    public class RoleBLL
    {
        private readonly RoleDAL _roleDAL;

        public RoleBLL(RoleDAL roleDAL)
        {
            _roleDAL = roleDAL;
        }

        public List<Role> GetAll()
        {
            return _roleDAL.GetAll();
        }

        public Role? GetById(int id)
        {
            if (id <= 0)
                throw new Exception("RoleId phải lớn hơn 0");

            return _roleDAL.GetById(id);
        }

        public int Create(Role role)
        {
            Validate(role);

            return _roleDAL.Create(role);
        }

        public bool Update(Role role)
        {
            if (role.RoleId <= 0)
                throw new Exception("RoleId phải lớn hơn 0");

            Validate(role);

            return _roleDAL.Update(role);
        }

        public bool Delete(int id)
        {
            if (id <= 0)
                throw new Exception("RoleId phải lớn hơn 0");

            return _roleDAL.Delete(id);
        }

        private void Validate(Role role)
        {
            if (role == null)
                throw new Exception("Dữ liệu Role không hợp lệ");

            if (string.IsNullOrWhiteSpace(role.RoleName))
                throw new Exception("RoleName không được để trống");

            if (role.RoleName.Length > 50)
                throw new Exception("RoleName không được vượt quá 50 ký tự");

            if (role.Description?.Length > 255)
                throw new Exception("Description không được vượt quá 255 ký tự");
        }
    }
}