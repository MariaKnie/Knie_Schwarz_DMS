using Microsoft.AspNetCore.Mvc;
using WebApplicationTest1;

namespace ASP_Api_Demo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodoController : ControllerBase
    {

        private static List<TodoItem> _todoItems = new List<TodoItem>
        {
            new TodoItem { Id = 1, Name = "Task1", IsComplete = false },
            new TodoItem { Id = 2, Name = "Task2", IsComplete = true }
        };

        /// <summary>
        /// Gibt eine Liste von Todoitems zurück. Optional können Name und/oder Erledigungsstatus gefiltert werden
        /// </summary>
        /// <param name="name">Der Name des TodoItems, nach dem gefiltert werden kann (string, optional)</param>
        /// <param name="isComplete">Der Status, ob das TodoItem abgschlossen ist (bool, optional)</param>
        /// <returns>IEnumearble<TodoItem></TodoItem></returns>
        [HttpGet]
        public IEnumerable<TodoItem> Get([FromQuery] string? name, [FromQuery] bool? isComplete)
        {
            var items = _todoItems.AsEnumerable();

            //Nach Name filterm wenn ein Name übergeben wurde
            if (!string.IsNullOrWhiteSpace(name))
            {
                items = items.Where(t => t.Name.Contains(name));
            }

            //Nach Erledigungsstatus filter, wenn einer übergeben wurde
            if (isComplete.HasValue)
            {
                items = items.Where(t => t.IsComplete == isComplete.Value);
            }

            return items;
        }

        [HttpPost]
        public ActionResult<TodoItem> PostTodoItem(TodoItem item)
        {
            // Überprüfung, ob der Task-Name leer oder null ist
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                return BadRequest(new { message = "Task name cannot be empty." });
            }

            // Neue ID generieren: Falls die Liste leer ist, starte mit ID 1
            if (_todoItems.Any())
            {
                item.Id = _todoItems.Max(t => t.Id) + 1; // Generiere die nächste ID basierend auf dem Maximalwert
            } else
            {
                item.Id = 1; // Falls die Liste leer ist, starte mit ID 1
            }

            _todoItems.Add(item);

            // Erfolgreich erstellt zurückgeben
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }


        [HttpPut("{id}")]
        public IActionResult PutTodoItem(int id, TodoItem item)
        {
            var existingItem = _todoItems.FirstOrDefault(t => t.Id == id);
            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.Name = item.Name;
            existingItem.IsComplete = item.IsComplete;
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteTodoItem(int id)
        {
            var item = _todoItems.FirstOrDefault(t => t.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            _todoItems.Remove(item);
            return NoContent();
        }
    }
}
