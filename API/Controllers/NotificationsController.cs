
using BLL;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationBLL _notificationBLL;

        public NotificationsController(NotificationBLL notificationBLL)
        {
            _notificationBLL = notificationBLL;
        }

        // GET: api/Notifications
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                return Ok(_notificationBLL.GetAll());
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Notifications/1
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var notification = _notificationBLL.GetById(id);

                if (notification == null)
                    return NotFound(new { message = "Không tìm thấy thông báo" });

                return Ok(notification);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/Notifications
        [HttpPost]
        public IActionResult Create([FromBody] Notification notification)
        {
            try
            {
                int id = _notificationBLL.Create(notification);

                return Ok(new
                {
                    message = "Thêm thông báo thành công",
                    notificationId = id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Notifications/1
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Notification notification)
        {
            try
            {
                notification.NotificationId = id;

                bool result = _notificationBLL.Update(notification);

                if (!result)
                    return NotFound(new { message = "Không tìm thấy thông báo" });

                return Ok(new { message = "Cập nhật thông báo thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Notifications/1/read
        [HttpPut("{id:int}/read")]
        public IActionResult MarkAsRead(int id)
        {
            try
            {
                bool result = _notificationBLL.MarkAsRead(id);

                if (!result)
                    return NotFound(new { message = "Không tìm thấy thông báo" });

                return Ok(new { message = "Đã đánh dấu thông báo là đã đọc" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/Notifications/1
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            try
            {
                bool result = _notificationBLL.Delete(id);

                if (!result)
                    return NotFound(new { message = "Không tìm thấy thông báo" });

                return Ok(new { message = "Xóa thông báo thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}