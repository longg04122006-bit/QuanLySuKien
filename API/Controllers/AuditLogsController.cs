using BLL;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly AuditLogBLL _auditLogBLL;

        public AuditLogsController(AuditLogBLL auditLogBLL)
        {
            _auditLogBLL = auditLogBLL;
        }

        // GET: api/AuditLogs
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                return Ok(_auditLogBLL.GetAll());
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/AuditLogs/1
        [HttpGet("{id:long}")]
        public IActionResult GetById(long id)
        {
            try
            {
                var auditLog = _auditLogBLL.GetById(id);

                if (auditLog == null)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy AuditLog"
                    });
                }

                return Ok(auditLog);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/AuditLogs
        [HttpPost]
        public IActionResult Create([FromBody] AuditLog auditLog)
        {
            try
            {
                long id = _auditLogBLL.Create(auditLog);

                return Ok(new
                {
                    message = "Thêm AuditLog thành công",
                    auditLogId = id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/AuditLogs/1
        [HttpDelete("{id:long}")]
        public IActionResult Delete(long id)
        {
            try
            {
                bool result = _auditLogBLL.Delete(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy AuditLog để xóa"
                    });
                }

                return Ok(new
                {
                    message = "Xóa AuditLog thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}