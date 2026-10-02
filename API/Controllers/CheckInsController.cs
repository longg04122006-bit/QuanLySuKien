using BLL;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckInsController : ControllerBase
    {
        private readonly CheckInBLL _checkInBLL;

        public CheckInsController(CheckInBLL checkInBLL)
        {
            _checkInBLL = checkInBLL;
        }

        // GET: api/CheckIns
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                return Ok(_checkInBLL.GetAll());
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/CheckIns/1
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var checkIn = _checkInBLL.GetById(id);

                if (checkIn == null)
                    return NotFound(new { message = "Không tìm thấy check-in" });

                return Ok(checkIn);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/CheckIns
        [HttpPost]
        public IActionResult Create([FromBody] CheckIn checkIn)
        {
            try
            {
                int id = _checkInBLL.Create(checkIn);

                return Ok(new
                {
                    message = "Thêm check-in thành công",
                    checkInId = id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/CheckIns/1
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] CheckIn checkIn)
        {
            try
            {
                checkIn.CheckInId = id;

                bool result = _checkInBLL.Update(checkIn);

                if (!result)
                    return NotFound(new { message = "Không tìm thấy check-in để cập nhật" });

                return Ok(new { message = "Cập nhật check-in thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/CheckIns/1
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            try
            {
                bool result = _checkInBLL.Delete(id);

                if (!result)
                    return NotFound(new { message = "Không tìm thấy check-in để xóa" });

                return Ok(new { message = "Xóa check-in thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}