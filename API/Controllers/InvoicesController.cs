using BLL;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly InvoiceBLL _invoiceBLL;

        public InvoicesController(InvoiceBLL invoiceBLL)
        {
            _invoiceBLL = invoiceBLL;
        }

        // GET: api/Invoices
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var result = _invoiceBLL.GetAll();
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

        // GET: api/Invoices/1
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var result = _invoiceBLL.GetById(id);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy Invoice"
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

        // POST: api/Invoices
        [HttpPost]
        public IActionResult Create([FromBody] Invoice invoice)
        {
            try
            {
                int id = _invoiceBLL.Create(invoice);

                invoice.InvoiceId = id;

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = id },
                    invoice
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

        // PUT: api/Invoices/1
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Invoice invoice)
        {
            try
            {
                if (id != invoice.InvoiceId)
                {
                    return BadRequest(new
                    {
                        message = "InvoiceId không khớp"
                    });
                }

                bool result = _invoiceBLL.Update(invoice);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy Invoice"
                    });
                }

                return Ok(new
                {
                    message = "Cập nhật Invoice thành công"
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

        // DELETE: api/Invoices/1
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                bool result = _invoiceBLL.Delete(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy Invoice"
                    });
                }

                return Ok(new
                {
                    message = "Xóa Invoice thành công"
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