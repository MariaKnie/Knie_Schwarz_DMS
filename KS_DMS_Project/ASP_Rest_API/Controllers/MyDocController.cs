using ASP_Rest_API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace ASP_Api_Demo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MyDocController : ControllerBase
    {

        private static List<MyDoc> documents = new List<MyDoc>
        {
           new MyDoc(1, "whale" , DateTime.Now, DateTime.Now, "Ann", "me"),
           new MyDoc(2, "fish" , DateTime.Now, DateTime.Now, "Bob", "ma"),
           new MyDoc(3, "deer" , DateTime.Now, DateTime.Now, "Cindy", "mu")
        };

        /// <summary>
        /// Gibt eine Liste von MyDocs zurück. Optional können Titel und/oder Author gefiltert werden
        /// </summary>
        /// <param name="author">Der Author des MyDocs, nach dem gefiltert werden kann (string, optional)</param>
        /// <param name="title">Der Title des MyDocs, (string, optional)</param>
        /// <returns>IEnumearble<MyDoc></MyDoc></returns>
        [HttpGet]
        public IEnumerable<MyDoc> Get([FromQuery] string? author, [FromQuery] string? title)
        {
            var items = documents.AsEnumerable();

            //Nach Name filterm wenn ein Name übergeben wurde
            if (!string.IsNullOrWhiteSpace(author))
            {
                items = items.Where(t => t.Author.Contains(author));
            }

            //Nach Name filterm wenn ein Name übergeben wurde
            if (!string.IsNullOrWhiteSpace(title))
            {
                items = items.Where(t => t.Title.Contains(title));
            }

            return items;
        }

        [HttpPost]
        public ActionResult<MyDoc> PostMyDocItem(MyDoc item)
        {
            // Überprüfung, ob der Title-Name leer oder null ist
            if (string.IsNullOrWhiteSpace(item.Title))
            {
                return BadRequest(new { message = "Doc Title cannot be empty." });
            }

            // Neue ID generieren: Falls die Liste leer ist, starte mit ID 1
            if (documents.Any())
            {
                item.Id = documents.Max(t => t.Id) + 1; // Generiere die nächste ID basierend auf dem Maximalwert
            } else
            {
                item.Id = 1; // Falls die Liste leer ist, starte mit ID 1
            }

            documents.Add(item);

            // Erfolgreich erstellt zurückgeben
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }


        [HttpPut("{id}")]
        public IActionResult PutMyDocItem(int id, MyDoc item)
        {
            var existingItem = documents.FirstOrDefault(t => t.Id == id);
            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.Title = item.Title;
            existingItem.Author = item.Author;
            existingItem.EditedDate = DateTime.Now;
            existingItem.TextField = item.TextField;
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteMyDocItem(int id)
        {
            var item = documents.FirstOrDefault(t => t.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            documents.Remove(item);
            return NoContent();
        }
    }
}
