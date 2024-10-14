using Microsoft.AspNetCore.Mvc;
using MyDocDAL.Repositories;
using MyDocDAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyDocDAL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MyDocController(ItMyDocRepository repository) : ControllerBase
    {
        [HttpGet]
        public async Task<IEnumerable<MyDoc>> GetAsync()
        {
            return await repository.GetAllAsync();
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(MyDoc item)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                return BadRequest(new { message = "Task name cannot be empty." });
            }
            await repository.AddAsync(item);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(int id, MyDoc item)
        {
            var existingItem = await repository.GetByIdAsync(id);
            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.Name = item.Name;
            existingItem.IsComplete = item.IsComplete;
            await repository.UpdateAsync(existingItem);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var item = await repository.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            await repository.DeleteAsync(id);
            return NoContent();
        }
    }
}