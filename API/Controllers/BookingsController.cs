using BLL;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly BookingBLL _bookingBLL;

        public BookingsController(BookingBLL bookingBLL)
        {
            _bookingBLL = bookingBLL;
        }

        // GET: api/Bookings
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var result = _bookingBLL.GetAll();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // GET: api/Bookings/1
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var result = _bookingBLL.GetById(id);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy Booking"
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // POST: api/Bookings
        [HttpPost]
        public IActionResult Create([FromBody] Booking booking)
        {
            try
            {
                int id = _bookingBLL.Create(booking);

                booking.BookingId = id;

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = id },
                    booking
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

        // PUT: api/Bookings/1
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Booking booking)
        {
            try
            {
                if (id != booking.BookingId)
                {
                    return BadRequest(new
                    {
                        message = "BookingId không khớp"
                    });
                }

                bool result = _bookingBLL.Update(booking);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy Booking"
                    });
                }

                return Ok(new
                {
                    message = "Cập nhật Booking thành công"
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

        // DELETE: api/Bookings/1
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                bool result = _bookingBLL.Delete(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy Booking"
                    });
                }

                return Ok(new
                {
                    message = "Xóa Booking thành công"
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