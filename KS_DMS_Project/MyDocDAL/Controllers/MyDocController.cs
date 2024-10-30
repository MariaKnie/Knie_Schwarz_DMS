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
            if (string.IsNullOrWhiteSpace(item.author))
            {
                return BadRequest(new { message = "Task Author cannot be empty." });
            }
            if (string.IsNullOrWhiteSpace(item.title))
            {
                return BadRequest(new { message = "Task Titel cannot be empty." });
            }

            item.author = item.author;
            item.createddate = DateTime.Now.ToUniversalTime();
            item.editeddate = DateTime.Now.ToUniversalTime();

            await repository.AddAsync(item);
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            var existingItem = await repository.GetByIdAsync(id);
            if (existingItem == null)
            {
                return NotFound(); // Return 404 if the item is not found
            }

            return Ok(existingItem); // Return 200 OK with the item
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(int id, MyDoc item)
        {
            var existingItem = await repository.GetByIdAsync(id);
            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.title = item.title;
            existingItem.author = item.author;
            existingItem.textfield = item.textfield;
            existingItem.createddate = existingItem.createddate.Value.ToUniversalTime();
            existingItem.editeddate = DateTime.Now.ToUniversalTime();
            if (item.filename != null)
            {
                existingItem.filename = item.filename;
            }
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



        [HttpDelete("{id}/File")]
        public async Task<IActionResult> DelteFileAsync(int id)
        {
            var existingItem = await repository.GetByIdAsync(id);
            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.createddate = existingItem.createddate.Value.ToUniversalTime();
            existingItem.editeddate = DateTime.Now.ToUniversalTime();
            existingItem.filename = "";
            
            await repository.UpdateAsync(existingItem);
            return NoContent();
        }
    }
}