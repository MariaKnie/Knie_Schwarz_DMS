using MyDocDAL.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using AutoMapper;
using ASP_Rest_API.DTO;

namespace ASP_Api_Demo.Controllers
{
    [ApiController]
    [Route("mydoc")]
    // [Route("[controller]")] -> /MyDoc

    public class MyDocController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;


        public MyDocController(IHttpClientFactory httpClientFactory, IMapper mapper)
        {
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;
        }

        //private static List<MyDoc> documents = new List<MyDoc>
        //{
        //   new MyDoc(1, "whale" , DateTime.Now, DateTime.Now, "Ann", "me"),
        //   new MyDoc(2, "fish" , DateTime.Now, DateTime.Now, "Bob", "ma"),
        //   new MyDoc(3, "deer" , DateTime.Now, DateTime.Now, "Cindy", "mu")
        //};

        /// <summary>
        /// Gibt eine Liste von MyDocs zurück. Optional können Titel und/oder Author gefiltert werden
        /// </summary>
        /// <param name="author">Der Author des MyDocs, nach dem gefiltert werden kann (string, optional)</param>
        /// <param name="title">Der Title des MyDocs, (string, optional)</param>
        /// <returns>IEnumearble<MyDoc></MyDoc></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var client = _httpClientFactory.CreateClient("MyDocDAL");
            var response = await client.GetAsync("/api/mydoc"); // Endpunkt des DAL

            if (response.IsSuccessStatusCode)
            {
                var items = await response.Content.ReadFromJsonAsync<IEnumerable<MyDoc>>();
                var dtoItems = _mapper.Map<IEnumerable<MyDocDTO>>(items);
                return Ok(dtoItems);
            }

            return StatusCode((int)response.StatusCode, "Error retrieving MyDoc items from DAL");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var client = _httpClientFactory.CreateClient("MyDocDAL");
            var response = await client.GetAsync($"/api/mydoc/{id}");

            if (response.IsSuccessStatusCode)
            {
                var item = await response.Content.ReadFromJsonAsync<MyDoc>();
                if (item == null)
                {
                    return NotFound();
                }
                var dtoItem = _mapper.Map<MyDocDTO>(item);
                return Ok(dtoItem);
            }

            // Log the response status code for debugging
            Console.WriteLine($"Received response with status code: {response.StatusCode}");
            return StatusCode((int)response.StatusCode, "Error retrieving MyDoc item from DAL");
        }


        [HttpPost]
        public async Task<IActionResult> Create(MyDocDTO itemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = _httpClientFactory.CreateClient("MyDocDAL");
            var item = _mapper.Map<MyDoc>(itemDto);
            var response = await client.PostAsJsonAsync("/api/mydoc", item);

            if (response.IsSuccessStatusCode)
            {
                return CreatedAtAction(nameof(GetById), new { id = item.id }, itemDto);
            }

            return StatusCode((int)response.StatusCode, "Error creating MyDoc item in DAL");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MyDocDTO itemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);

            }
            if (id != itemDto.id)
            {
                return BadRequest("ID mismatch");
            }

            itemDto.editeddate = DateTime.Now.ToUniversalTime();

            var client = _httpClientFactory.CreateClient("MyDocDAL");
            var item = _mapper.Map<MyDoc>(itemDto);
            var response = await client.PutAsJsonAsync($"/api/mydoc/{id}", item);

            if (response.IsSuccessStatusCode)
            {
                return NoContent();
            }

            return StatusCode((int)response.StatusCode, "Error updating MyDoc item in DAL");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("MyDocDAL");
            var response = await client.DeleteAsync($"/api/mydoc/{id}");

            if (response.IsSuccessStatusCode)
            {
                return NoContent();
            }

            return StatusCode((int)response.StatusCode, "Error deleting MyDoc item from DAL");
        }

        //public IEnumerable<MyDoc> Get([FromQuery] string? author, [FromQuery] string? title)
        //{



        //    var items = documents.AsEnumerable();

        //    //Nach Name filterm wenn ein Name übergeben wurde
        //    if (!string.IsNullOrWhiteSpace(author))
        //    {
        //        items = items.Where(t => t.Author.Contains(author));
        //    }

        //    //Nach Name filterm wenn ein Name übergeben wurde
        //    if (!string.IsNullOrWhiteSpace(title))
        //    {
        //        items = items.Where(t => t.Title.Contains(title));
        //    }

        //    return items;
        //}

        //[HttpPost]
        //public ActionResult<MyDoc> PostMyDocItem(MyDoc item)
        //{
        //    // Überprüfung, ob der Title-Name leer oder null ist
        //    if (string.IsNullOrWhiteSpace(item.Title))
        //    {
        //        return BadRequest(new { message = "Doc Title cannot be empty." });
        //    }

        //    // Neue ID generieren: Falls die Liste leer ist, starte mit ID 1
        //    if (documents.Any())
        //    {
        //        item.Id = documents.Max(t => t.Id) + 1; // Generiere die nächste ID basierend auf dem Maximalwert
        //    } else
        //    {
        //        item.Id = 1; // Falls die Liste leer ist, starte mit ID 1
        //    }

        //    documents.Add(item);

        //    // Erfolgreich erstellt zurückgeben
        //    return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        //}


        //[HttpPut("{id}")]
        //public IActionResult PutMyDocItem(int id, MyDoc item)
        //{
        //    var existingItem = documents.FirstOrDefault(t => t.Id == id);
        //    if (existingItem == null)
        //    {
        //        return NotFound();
        //    }

        //    existingItem.Title = item.Title;
        //    existingItem.Author = item.Author;
        //    existingItem.EditedDate = DateTime.Now;
        //    existingItem.TextField = item.TextField;
        //    return NoContent();
        //}

        //[HttpDelete("{id}")]
        //public IActionResult DeleteMyDocItem(int id)
        //{
        //    var item = documents.FirstOrDefault(t => t.Id == id);
        //    if (item == null)
        //    {
        //        return NotFound();
        //    }

        //    documents.Remove(item);
        //    return NoContent();
        //}
    }
}
