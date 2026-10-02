using BLL;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly RoleBLL _roleBLL;

        public RolesController(RoleBLL roleBLL)
        {
            _roleBLL = roleBLL;
        }

        // GET: api/Roles
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                return Ok(_roleBLL.GetAll());
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Roles/1
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var role = _roleBLL.GetById(id);

                if (role == null)
                    return NotFound(new
                    {
                        message = "Không tìm thấy Role"
                    });

                return Ok(role);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/Roles
        [HttpPost]
        public IActionResult Create([FromBody] Role role)
        {
            try
            {
                int id = _roleBLL.Create(role);

                return Ok(new
                {
                    message = "Thêm Role thành công",
                    roleId = id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Roles/1
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Role role)
        {
            try
            {
                role.RoleId = id;

                bool result = _roleBLL.Update(role);

                if (!result)
                    return NotFound(new
                    {
                        message = "Không tìm thấy Role để cập nhật"
                    });

                return Ok(new
                {
                    message = "Cập nhật Role thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/Roles/1
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            try
            {
                bool result = _roleBLL.Delete(id);

                if (!result)
                    return NotFound(new
                    {
                        message = "Không tìm thấy Role để xóa"
                    });

                return Ok(new
                {
                    message = "Xóa Role thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}