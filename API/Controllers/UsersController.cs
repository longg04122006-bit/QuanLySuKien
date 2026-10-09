using BLL;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserBLL _userBLL;

        public UsersController(UserBLL userBLL)
        {
            _userBLL = userBLL;
        }

        // GET: api/Users
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                return Ok(_userBLL.GetAll());
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Users/1
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var user = _userBLL.GetById(id);

                if (user == null)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy User"
                    });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/Users
        [HttpPost]
        public IActionResult Create([FromBody] User user)
        {
            try
            {
                int id = _userBLL.Create(user);

                return Ok(new
                {
                    message = "Thêm User thành công",
                    userId = id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Users/1
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] User user)
        {
            try
            {
                user.UserId = id;

                bool result = _userBLL.Update(user);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy User để cập nhật"
                    });
                }

                return Ok(new
                {
                    message = "Cập nhật User thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/Users/1
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            try
            {
                bool result = _userBLL.Delete(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy User để xóa"
                    });
                }

                return Ok(new
                {
                    message = "Xóa User thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}