using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttendeesController : ControllerBase
    {
        private readonly AttendeeBLL _attendeeBLL;

        public AttendeesController(AttendeeBLL attendeeBLL)
        {
            _attendeeBLL = attendeeBLL;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_attendeeBLL.GetAll());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var attendee = _attendeeBLL.GetById(id);

            if (attendee == null)
                return NotFound(new { message = "Không tìm thấy người tham dự." });

            return Ok(attendee);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Attendee attendee)
        {
            try
            {
                var id = _attendeeBLL.Create(attendee);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = id },
                    new
                    {
                        attendeeId = id,
                        attendee.TicketId,
                        attendee.FullName,
                        attendee.Email,
                        attendee.Phone
                    });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(
            int id,
            [FromBody] Attendee attendee)
        {
            try
            {
                attendee.AttendeeId = id;

                _attendeeBLL.Update(attendee);

                return Ok(new
                {
                    message = "Cập nhật người tham dự thành công."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _attendeeBLL.Delete(id);

                return Ok(new
                {
                    message = "Xóa người tham dự thành công."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}