using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly EventBLL _eventBLL;

        public EventsController(EventBLL eventBLL)
        {
            _eventBLL = eventBLL;
        }

        // =========================================
        // GET: api/Events
        // Ai cũng có thể xem
        // =========================================

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _eventBLL.GetAll();

            return Ok(result);
        }

        // =========================================
        // GET: api/Events/1
        // =========================================

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var result = _eventBLL.GetById(id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy sự kiện."
                });
            }

            return Ok(result);
        }

        // =========================================
        // POST: api/Events
        // Admin + Manager
        // =========================================

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public IActionResult Create(EventModel model)
        {
            try
            {
                int eventId = _eventBLL.Create(model);

                var newEvent = _eventBLL.GetById(eventId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = eventId },
                    newEvent
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // =========================================
        // PUT: api/Events/1
        // Admin + Manager
        // =========================================

        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id}")]
        public IActionResult Update(
            int id,
            EventModel model)
        {
            try
            {
                if (id != model.EventId)
                {
                    return BadRequest(new
                    {
                        message = "EventId không khớp."
                    });
                }

                bool result = _eventBLL.Update(model);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy sự kiện."
                    });
                }

                return Ok(new
                {
                    message = "Cập nhật sự kiện thành công."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // =========================================
        // DELETE: api/Events/1
        // Admin
        // =========================================

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                bool result = _eventBLL.Delete(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy sự kiện."
                    });
                }

                return Ok(new
                {
                    message = "Xóa sự kiện thành công."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}