using MyDocDAL.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using AutoMapper;
using ASP_Rest_API.DTO;
using RabbitMQ.Client;
using System.Text;
using log4net;

namespace ASP_Api_Demo.Controllers
{
    [ApiController]
    [Route("mydoc")]
    // [Route("[controller]")] -> /MyDoc

    public class MyDocController : ControllerBase, IDisposable
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        private static readonly ILog log = LogManager.GetLogger(typeof(MyDocController));

        public MyDocController(IHttpClientFactory httpClientFactory, IMapper mapper)
        {
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;

            log.Info("Creating Factory, connection and channel");
            // Stelle die Verbindung zu RabbitMQ her
            var factory = new ConnectionFactory() { HostName = "rabbitmq", UserName = "user", Password = "password" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            log.Info("Creating Queue");
            // Deklariere die Queue
            _channel.QueueDeclare(queue: "file_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

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

            log.Error("Error retrieving MyDoc items from DAL In Get");
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

            log.Error("Error retrieving MyDoc items from DAL In Get ID");
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

            log.Error("Error retrieving MyDoc items from DAL In Post");
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

            log.Error("Error retrieving MyDoc items from DAL In Put ID");
            return StatusCode((int)response.StatusCode, "Error updating MyDoc item in DAL");
        }


        [HttpPut("{id}/upload")]
        public async Task<IActionResult> UploadFile(int id, IFormFile? docFile)
        {
            if (docFile == null || docFile.Length == 0)
            {
                return BadRequest("Keine Datei hochgeladen.");
            }

            // Hole den Task vom DAL
            var client = _httpClientFactory.CreateClient("MyDocDAL");
            var response = await client.GetAsync($"/api/mydoc/{id}");
            if (!response.IsSuccessStatusCode)
            {
                log.Error("Fehler beim Abrufen des Docs mit ID " + id);
                return NotFound($"Fehler beim Abrufen des Docs mit ID {id}");
            }

            // Mappe das empfangene TodoItem auf ein TodoItemDto
            var myDoc = await response.Content.ReadFromJsonAsync<MyDocDTO>();
            if (myDoc == null)
            {
                log.Error("Task mit ID nicht gefunden: " + id);
                return NotFound($"Task mit ID {id} nicht gefunden.");
            }

            var myDocDto = _mapper.Map<MyDoc>(myDoc);

            // Setze den Dateinamen im DTO
            myDocDto.filename = docFile.FileName;

            // Aktualisiere das Item im DAL, nutze das DTO
            var updateResponse = await client.PutAsJsonAsync($"/api/mydoc/{id}", myDocDto);
            if (!updateResponse.IsSuccessStatusCode)
            {
                log.Error("Fehler beim Speichern des Dateinamens für Doc " + id);
                return StatusCode((int)updateResponse.StatusCode, $"Fehler beim Speichern des Dateinamens für Doc {id}");
            }

            // Nachricht an RabbitMQ
            try
            {
                SendToMessageQueue(docFile.FileName);
            }
            catch (Exception ex)
            {
                log.Error("Fehler beim Senden der Nachricht an RabbitMQ:" + ex.Message);
                return StatusCode(500, $"Fehler beim Senden der Nachricht an RabbitMQ: {ex.Message}");
            }

            return Ok(new { message = $"Dateiname {docFile.FileName} für Doc {id} erfolgreich gespeichert." });
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

            log.Error("Error deleting MyDoc item from DAL in Delete ID:" + id);
            return StatusCode((int)response.StatusCode, "Error deleting MyDoc item from DAL");
        }

        [HttpDelete("{id}/File")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var client = _httpClientFactory.CreateClient("MyDocDAL");
            var response = await client.DeleteAsync($"/api/mydoc/{id}/File");

            if (response.IsSuccessStatusCode)
            {
                return NoContent();
            }

            log.Error("Error deleting MyDoc item from DAL in Delete FILE ID:" + id);
            return StatusCode((int)response.StatusCode, "Error deleting MyDoc item File from DAL");
        }

        private void SendToMessageQueue(string fileName)
        {
            // Sende die Nachricht in den RabbitMQ channel/queue
            var body = Encoding.UTF8.GetBytes(fileName);
            _channel.BasicPublish(exchange: "", routingKey: "file_queue", basicProperties: null, body: body);
            Console.WriteLine($@"[x] Sent {fileName}");
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }

    }
}
