using BLL;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketTypesController : ControllerBase
    {
        private readonly TicketTypeBLL _ticketTypeBLL;

        public TicketTypesController(TicketTypeBLL ticketTypeBLL)
        {
            _ticketTypeBLL = ticketTypeBLL;
        }

        // GET: api/TicketTypes
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var ticketTypes = _ticketTypeBLL.GetAll();

                return Ok(ticketTypes);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // GET: api/TicketTypes/1
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var ticketType = _ticketTypeBLL.GetById(id);

                if (ticketType == null)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy loại vé."
                    });
                }

                return Ok(ticketType);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // POST: api/TicketTypes
        [HttpPost]
        public IActionResult Create(
            [FromBody] TicketType ticketType)
        {
            try
            {
                int id = _ticketTypeBLL.Create(ticketType);

                ticketType.TicketTypeId = id;

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = id },
                    ticketType);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // PUT: api/TicketTypes/1
        [HttpPut("{id}")]
        public IActionResult Update(
            int id,
            [FromBody] TicketType ticketType)
        {
            try
            {
                if (id != ticketType.TicketTypeId)
                {
                    return BadRequest(new
                    {
                        message = "TicketTypeId không khớp."
                    });
                }

                bool result = _ticketTypeBLL.Update(ticketType);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy loại vé để cập nhật."
                    });
                }

                return Ok(new
                {
                    message = "Cập nhật loại vé thành công."
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

        // DELETE: api/TicketTypes/1
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                bool result = _ticketTypeBLL.Delete(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy loại vé để xóa."
                    });
                }

                return Ok(new
                {
                    message = "Xóa loại vé thành công."
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