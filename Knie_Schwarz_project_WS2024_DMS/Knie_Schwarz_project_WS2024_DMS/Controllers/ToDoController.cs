using Knie_Schwarz_project_WS2024_DMS.Models;
using Microsoft.AspNetCore.Mvc;

namespace Knie_Schwarz_project_WS2024_DMS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodoController : ControllerBase
    {
        private static List<Document> documents = new List<Document>
        {
           new Document(1, "whale" , DateTime.Now, DateTime.Now, "Ann", "me"),
           new Document(2, "fish" , DateTime.Now, DateTime.Now, "Bob", "ma"),
           new Document(3, "deer" , DateTime.Now, DateTime.Now, "Cindy", "mu")
        };

        // GET: Retrieve all documents
        [HttpGet(Name = "GetDocuments")]
        public ActionResult<IEnumerable<Document>> Get()
        {
            return Ok(documents);
        }

        // GET: Retrieve a document by ID
        [HttpGet("{id}")]
        public ActionResult<Document> GetById(int id)
        {
            var document = documents.FirstOrDefault(d => d.Id == id);
            if (document == null)
            {
                return NotFound($"Document with ID {id} not found.");
            }
            return Ok(document);
        }

        // DELETE: Delete a document by ID
        [HttpDelete("{id}")]
        public ActionResult<string> Delete(int id)
        {
            var document = documents.FirstOrDefault(d => d.Id == id);
            if (document == null)
            {
                return NotFound($"Document with ID {id} not found.");
            }
            documents.Remove(document);
            return Ok($"Document with ID {id} has been deleted.");
        }

        // PUT: Update a document by ID
        [HttpPut("{id}")]
        public ActionResult<Document> Update(int id, [FromBody] Document updatedDocument)
        {
            var document = documents.FirstOrDefault(d => d.Id == id);
            if (document == null)
            {
                return NotFound($"Document with ID {id} not found.");
            }

            // Update the document properties
            document.Title = updatedDocument.Title;
            document.EditedDate = DateTime.Now; // update EditedDate to current time
            document.Author = updatedDocument.Author;
            document.TextField = updatedDocument.TextField;

            return Ok(document);
        }
    }
}
