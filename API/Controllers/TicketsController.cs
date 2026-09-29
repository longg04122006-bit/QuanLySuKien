using BLL;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly TicketBLL _ticketBLL;

        public TicketsController(TicketBLL ticketBLL)
        {
            _ticketBLL = ticketBLL;
        }

        // GET: api/Tickets
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var tickets = _ticketBLL.GetAll();

                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // GET: api/Tickets/1
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var ticket = _ticketBLL.GetById(id);

                if (ticket == null)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy vé."
                    });
                }

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // POST: api/Tickets
        [HttpPost]
        public IActionResult Create([FromBody] Ticket ticket)
        {
            try
            {
                int id = _ticketBLL.Create(ticket);

                ticket.TicketId = id;

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = id },
                    ticket
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // PUT: api/Tickets/1
        [HttpPut("{id}")]
        public IActionResult Update(
            int id,
            [FromBody] Ticket ticket)
        {
            try
            {
                if (id != ticket.TicketId)
                {
                    return BadRequest(new
                    {
                        message = "TicketId không khớp."
                    });
                }

                bool result = _ticketBLL.Update(ticket);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy vé để cập nhật."
                    });
                }

                return Ok(new
                {
                    message = "Cập nhật vé thành công."
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

        // DELETE: api/Tickets/1
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                bool result = _ticketBLL.Delete(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy vé để xóa."
                    });
                }

                return Ok(new
                {
                    message = "Xóa vé thành công."
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