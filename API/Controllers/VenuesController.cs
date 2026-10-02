using BLL;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VenuesController : ControllerBase
    {
        private readonly VenueBLL _venueBLL;

        public VenuesController(VenueBLL venueBLL)
        {
            _venueBLL = venueBLL;
        }

        // GET: api/Venues
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                return Ok(_venueBLL.GetAll());
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Venues/1
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var venue = _venueBLL.GetById(id);

                if (venue == null)
                    return NotFound(new { message = "Không tìm thấy địa điểm" });

                return Ok(venue);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/Venues
        [HttpPost]
        public IActionResult Create([FromBody] Venue venue)
        {
            try
            {
                int id = _venueBLL.Create(venue);

                return Ok(new
                {
                    message = "Thêm địa điểm thành công",
                    venueId = id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Venues/1
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Venue venue)
        {
            try
            {
                venue.VenueId = id;

                bool result = _venueBLL.Update(venue);

                if (!result)
                    return NotFound(new
                    {
                        message = "Không tìm thấy địa điểm để cập nhật"
                    });

                return Ok(new
                {
                    message = "Cập nhật địa điểm thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/Venues/1
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            try
            {
                bool result = _venueBLL.Delete(id);

                if (!result)
                    return NotFound(new
                    {
                        message = "Không tìm thấy địa điểm để xóa"
                    });

                return Ok(new
                {
                    message = "Xóa địa điểm thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}