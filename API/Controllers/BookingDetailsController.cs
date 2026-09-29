using BLL;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingDetailsController : ControllerBase
    {
        private readonly BookingDetailBLL _bookingDetailBLL;

        public BookingDetailsController(
            BookingDetailBLL bookingDetailBLL)
        {
            _bookingDetailBLL = bookingDetailBLL;
        }

        // GET: api/BookingDetails
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var details =
                    _bookingDetailBLL.GetAll();

                return Ok(details);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // GET: api/BookingDetails/1
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var detail =
                    _bookingDetailBLL.GetById(id);

                if (detail == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Không tìm thấy chi tiết đơn đặt vé."
                    });
                }

                return Ok(detail);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // POST: api/BookingDetails
        [HttpPost]
        public IActionResult Create(
            [FromBody] BookingDetail detail)
        {
            try
            {
                int id =
                    _bookingDetailBLL.Create(detail);

                detail.BookingDetailId = id;

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = id },
                    detail);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // PUT: api/BookingDetails/1
        [HttpPut("{id}")]
        public IActionResult Update(
            int id,
            [FromBody] BookingDetail detail)
        {
            try
            {
                if (id != detail.BookingDetailId)
                {
                    return BadRequest(new
                    {
                        message =
                            "BookingDetailId không khớp."
                    });
                }

                bool result =
                    _bookingDetailBLL.Update(detail);

                if (!result)
                {
                    return NotFound(new
                    {
                        message =
                            "Không tìm thấy chi tiết đơn để cập nhật."
                    });
                }

                return Ok(new
                {
                    message =
                        "Cập nhật chi tiết đơn thành công."
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

        // DELETE: api/BookingDetails/1
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                bool result =
                    _bookingDetailBLL.Delete(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message =
                            "Không tìm thấy chi tiết đơn để xóa."
                    });
                }

                return Ok(new
                {
                    message =
                        "Xóa chi tiết đơn thành công."
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