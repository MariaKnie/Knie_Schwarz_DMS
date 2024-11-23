using MyDocDAL.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using AutoMapper;
using ASP_Rest_API.DTO;
using RabbitMQ.Client;
using System.Text;
using log4net;
using ASP_Rest_API.Services;
using System.Formats.Tar;
using Minio;
using Minio.DataModel.Args;
using System.Reactive.Linq;

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
        private readonly IMinioClient _minioClient;
        private const string BucketName = "uploads";

        private static readonly ILog log = LogManager.GetLogger(typeof(MyDocController));

        private readonly IMessageQueueService _messageQueueService;
        public MyDocController(IHttpClientFactory httpClientFactory, IMapper mapper, IMessageQueueService messageQueueService)
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
           // _channel.QueueDeclare(queue: "file_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _messageQueueService = messageQueueService;

            _minioClient = new MinioClient()
                .WithEndpoint("minio", 9000)
                .WithCredentials("minioadmin", "minioadmin")
                .WithSSL(false)
                .Build();
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

            Console.WriteLine($@"[PUT] Eingehender OcrText: {itemDto.ocrtext}");

            itemDto.editeddate = DateTime.Now.ToUniversalTime();

            var client = _httpClientFactory.CreateClient("MyDocDAL");
            var item = _mapper.Map<MyDoc>(itemDto);
            var response = await client.PutAsJsonAsync($"/api/mydoc/{id}", item);
            Console.WriteLine($@"[PUT] Gemappter OcrText: {item.ocrtext}");

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

            // Minio stuff
            await EnsureBucketExists();
            var fileName = docFile.FileName;
            await using var fileStream = docFile.OpenReadStream();

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(BucketName)
                .WithObject(fileName)
                .WithStreamData(fileStream)
                .WithObjectSize(docFile.Length));

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
               await SendToMessageQueue(id, docFile);
            }
            catch (Exception ex)
            {
                log.Error("Fehler beim Senden der Nachricht an RabbitMQ:" + ex.Message);
                return StatusCode(500, $"Fehler beim Senden der Nachricht an RabbitMQ: {ex.Message}");
            }

            return Ok(new { message = $"Dateiname {docFile.FileName} für Doc {id} erfolgreich gespeichert." });
        }


        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var memoryStream = new MemoryStream();

            await _minioClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(BucketName)
                .WithObject(fileName)
                .WithCallbackStream(stream =>
                {
                    stream.CopyTo(memoryStream);
                }));

            memoryStream.Position = 0;
            return File(memoryStream, "application/octet-stream", fileName);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            
            var client = _httpClientFactory.CreateClient("MyDocDAL");

            var response1 = await client.GetAsync($"/api/mydoc/{id}");

            var item = await response1.Content.ReadFromJsonAsync<MyDoc>();
            if (item != null)
            {
                var dtoItem = _mapper.Map<MyDocDTO>(item);
                if(dtoItem.filename != null)
                {
                    try
                    {
                        await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                            .WithBucket(BucketName)
                            .WithObject(dtoItem.filename));

                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new { message = $"Fehler beim Löschen der Datei: {ex.Message}" });
                    }
                }
            }
            
            var response = await client.DeleteAsync($"/api/mydoc/{id}");

            if (response.IsSuccessStatusCode)
            {
                return NoContent();
            }

            log.Error("Error deleting MyDoc item from DAL in Delete ID:" + id);
            return StatusCode((int)response.StatusCode, "Error deleting MyDoc item from DAL");
        }
        // Delete from Minio
        [HttpDelete("delete/{fileName}")]
        public async Task<IActionResult> DeleteFileFromMinio(string fileName)
        {
            try
            {
                await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                    .WithBucket(BucketName)
                    .WithObject(fileName));


                return Ok(new { message = $"Datei '{fileName}' erfolgreich gelöscht." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Fehler beim Löschen der Datei: {ex.Message}" });
            }
        }

        // Delete File from DAL
        [HttpDelete("{id}/File")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var client = _httpClientFactory.CreateClient("MyDocDAL");
            var response = await client.DeleteAsync($"/api/mydoc/{id}/File");

            if (response.IsSuccessStatusCode)
            {
                return Ok(new { message = $"Datei erfolgreich gelöscht." });
            }

            log.Error("Error deleting MyDoc item from DAL in Delete FILE ID:" + id);
            return StatusCode((int)response.StatusCode, "Error deleting MyDoc item File from DAL");
        }

        private async Task EnsureBucketExists()
        {
            bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(BucketName));
            if (!found)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(BucketName));
            }
        }

        private async Task SendToMessageQueue(int id, IFormFile? taskFile)
        {
            // Sende die Nachricht in den RabbitMQ channel/queue
            var body = Encoding.UTF8.GetBytes(taskFile.FileName);
           // _channel.BasicPublish(exchange: "", routingKey: "file_queue", basicProperties: null, body: body);


            // Datei speichern (lokal im Container)
            var filePath = Path.Combine("/app/uploads", taskFile.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!); // Erstelle das Verzeichnis, falls es nicht existiert
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await taskFile.CopyToAsync(stream);
            }

            Console.WriteLine($"{filePath} - should be derectory");

            // Nachricht an RabbitMQ
            try
            {
                _messageQueueService.SendToQueue($"{id}|{filePath}");
                Console.WriteLine($@"File Path {filePath} an RabbitMQ Queue gesendet.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Senden der Nachricht an RabbitMQ: {ex.Message}");
            }

            Console.WriteLine($@"[x] Sent {taskFile.FileName}");
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }

    }
}
