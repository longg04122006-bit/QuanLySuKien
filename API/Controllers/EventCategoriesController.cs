using BLL;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventCategoriesController : ControllerBase
    {
        private readonly EventCategoryBLL _bll;

        public EventCategoriesController(EventCategoryBLL bll)
        {
            _bll = bll;
        }

        // GET: api/EventCategories
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var data = _bll.GetAll();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        // GET: api/EventCategories/1
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var data = _bll.GetById(id);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy loại sự kiện."
                    });
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        // POST: api/EventCategories
        [HttpPost]
        public IActionResult Create([FromBody] EventCategory category)
        {
            try
            {
                int id = _bll.Create(category);

                return Ok(new
                {
                    message = "Thêm loại sự kiện thành công.",
                    id = id
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

        // PUT: api/EventCategories/1
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] EventCategory category)
        {
            try
            {
                category.Id = id;

                bool result = _bll.Update(category);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy loại sự kiện."
                    });
                }

                return Ok(new
                {
                    message = "Cập nhật loại sự kiện thành công."
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

        // DELETE: api/EventCategories/1
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                bool result = _bll.Delete(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy loại sự kiện."
                    });
                }

                return Ok(new
                {
                    message = "Xóa loại sự kiện thành công."
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