using DAL;
using Model;

namespace BLL
{
    public class UserBLL
    {
        private readonly UserDAL _userDAL;

        public UserBLL(UserDAL userDAL)
        {
            _userDAL = userDAL;
        }

        // =========================
        // LOGIN
        // =========================
        public UserLoginModel? Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new Exception("Username không được để trống");

            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("Password không được để trống");

            return _userDAL.Login(username, password);
        }

        // =========================
        // GET ALL
        // =========================
        public List<User> GetAll()
        {
            return _userDAL.GetAll();
        }

        // =========================
        // GET BY ID
        // =========================
        public User? GetById(int id)
        {
            if (id <= 0)
                throw new Exception("UserId phải lớn hơn 0");

            return _userDAL.GetById(id);
        }

        // =========================
        // CREATE
        // =========================
        public int Create(User user)
        {
            Validate(user);

            return _userDAL.Create(user);
        }

        // =========================
        // UPDATE
        // =========================
        public bool Update(User user)
        {
            if (user == null)
                throw new Exception("Dữ liệu User không hợp lệ");

            if (user.UserId <= 0)
                throw new Exception("UserId phải lớn hơn 0");

            Validate(user);

            return _userDAL.Update(user);
        }

        // =========================
        // DELETE
        // =========================
        public bool Delete(int id)
        {
            if (id <= 0)
                throw new Exception("UserId phải lớn hơn 0");

            return _userDAL.Delete(id);
        }

        // =========================
        // VALIDATE
        // =========================
        private void Validate(User user)
        {
            if (user == null)
                throw new Exception("Dữ liệu User không hợp lệ");

            if (string.IsNullOrWhiteSpace(user.Username))
                throw new Exception("Username không được để trống");

            if (user.Username.Length > 50)
                throw new Exception("Username không được vượt quá 50 ký tự");

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                throw new Exception("Password không được để trống");

            if (user.PasswordHash.Length > 255)
                throw new Exception("Password không được vượt quá 255 ký tự");

            if (string.IsNullOrWhiteSpace(user.FullName))
                throw new Exception("FullName không được để trống");

            if (user.FullName.Length > 100)
                throw new Exception("FullName không được vượt quá 100 ký tự");

            if (string.IsNullOrWhiteSpace(user.Email))
                throw new Exception("Email không được để trống");

            if (user.Email.Length > 100)
                throw new Exception("Email không được vượt quá 100 ký tự");

            if (user.Phone?.Length > 20)
                throw new Exception("Phone không được vượt quá 20 ký tự");

            if (user.RoleId <= 0)
                throw new Exception("RoleId phải lớn hơn 0");
        }
    }
}