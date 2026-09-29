using BLL;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentBLL _paymentBLL;

        public PaymentsController(PaymentBLL paymentBLL)
        {
            _paymentBLL = paymentBLL;
        }

        // GET: api/Payments
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var result = _paymentBLL.GetAll();
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

        // GET: api/Payments/1
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var result = _paymentBLL.GetById(id);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy Payment"
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

        // POST: api/Payments
        [HttpPost]
        public IActionResult Create([FromBody] Payment payment)
        {
            try
            {
                int id = _paymentBLL.Create(payment);

                payment.PaymentId = id;

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = id },
                    payment
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

        // PUT: api/Payments/1
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Payment payment)
        {
            try
            {
                if (id != payment.PaymentId)
                {
                    return BadRequest(new
                    {
                        message = "PaymentId không khớp"
                    });
                }

                bool result = _paymentBLL.Update(payment);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy Payment"
                    });
                }

                return Ok(new
                {
                    message = "Cập nhật Payment thành công"
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

        // DELETE: api/Payments/1
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                bool result = _paymentBLL.Delete(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy Payment"
                    });
                }

                return Ok(new
                {
                    message = "Xóa Payment thành công"
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