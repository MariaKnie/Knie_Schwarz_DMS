using MyDocDAL.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using AutoMapper;
using ASP_Rest_API.DTO;
using ASP_Rest_API.SearchItems;
using RabbitMQ.Client;
using System.Text;
using log4net;
using ASP_Rest_API.Services;
using System.Formats.Tar;
using Minio;
using Minio.DataModel.Args;
using System.Reactive.Linq;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Nodes;
using Elastic.Clients.Elasticsearch.Ingest;
using Elastic.Transport;

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
        //private readonly IElasticService _elasticService;
        private readonly ElasticsearchClient _elasticClient;
        private const string BucketName = "uploads";

        private static readonly ILog log = LogManager.GetLogger(typeof(MyDocController));

        private readonly IMessageQueueService _messageQueueService;
        //public MyDocController(IHttpClientFactory httpClientFactory, IMapper mapper, IMessageQueueService messageQueueService, IElasticService elasticService)
        public MyDocController(IHttpClientFactory httpClientFactory, IMapper mapper, IMessageQueueService messageQueueService, ElasticsearchClient elasticClient)
        {
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;
            //_elasticService = elasticService;
            _elasticClient = elasticClient;

            var settings = new ElasticsearchClientSettings(new Uri("http://elasticsearch:9200"))
                .Authentication(new BasicAuthentication("", ""))
                .DefaultIndex("documents");

            _elasticClient = new ElasticsearchClient(settings);

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

        /*
        [HttpPost("create-index")]
        public async Task<IActionResult> CreateIndex(string indexName)
        {
            await _elasticService.CreateIndexIfNotExistsAsync(indexName);
            return Ok($"Index {indexName} created or exists");
        }

        [HttpPost("add-document")]
        public async Task<IActionResult> AddDocument([FromBody] Doc document)
        {
            var result = await _elasticService.AddOrUpdate(document);
            return result ? Ok($"Document added") : StatusCode(500, "Error adding ducument to elasticsearch");

        }

        [HttpGet("get-document/{key}")]
        public async Task<IActionResult> GetDocument(string document)
        {
            var result = await _elasticService.Get(document);
            return result != null ?  Ok(document) : NotFound("Document not found");

        }

        [HttpGet("get-all-documents")]
        public async Task<IActionResult> GetAllDocuments()
        {
            var results = await _elasticService.GetAll();
            return results != null ? Ok(results) : StatusCode(500, "Error retrieving documents");

        }

        [HttpDelete("delete-document/{key}")]
        public async Task<IActionResult> DeleteDocument(string key)
        {
            var result = await _elasticService.Remove(key);
            return result ? Ok("Document deleted") : StatusCode(500, "Error deleting documents");

        }
        */

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

            //used to debug
            Thread.Sleep(1000);
            // Get ocrtext
            var response2 = await client.GetAsync($"/api/mydoc/{id}");
            var myDoc2 = await response2.Content.ReadFromJsonAsync<MyDocDTO>();
            var doc2add = new Doc() { Id = id, OcrText = myDoc2.ocrtext };
            // debug

            var indexResponse = await _elasticClient.IndexAsync(doc2add, idx =>
            idx.Index("documents")
                .OpType(OpType.Index));
            if (indexResponse.IsValidResponse)
            {
                return Ok(new { message = "Document indexed successfully" });
            }
            return StatusCode(500, new { message = "Failed to index document", details = indexResponse.DebugInformation });


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
        

        // Wildcard-Search (QueryString)
        [HttpPost("search/querystring")]
        public async Task<IActionResult> SearchByQueryString([FromBody] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(new { message = "Search term cannot be empty" });
            }

            var response = await _elasticClient.SearchAsync<Doc>(s => s
                .Index("documents")
                .Query(q => q.QueryString(qs => qs.Query($"{searchTerm}"))));

            return HandleSearchResponse(response);
        }

        // Fuzzy-Search with Match(Normalisation)
        [HttpPost("search/fuzzy")]
        public async Task<IActionResult> SearchByFuzzy([FromBody] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(new { message = "Search term cannot be empty" });
            }

            var response = await _elasticClient.SearchAsync<Doc>(s => s
                .Index("documents")
                .Query(q => q.Match(m => m
                    .Field(f => f.OcrText)
                    .Query(searchTerm)
                    .Fuzziness(new Fuzziness(4)))));

            return HandleSearchResponse(response);
        }

        private IActionResult HandleSearchResponse(SearchResponse<Doc> response)
        {
            if (response.IsValidResponse)
            {
                if (response.Documents.Any())
                {
                    return Ok(response.Documents);
                }
                return NotFound(new { message = "No documents found matching the search term." });
            }

            return StatusCode(500, new { message = "Failed to search documents", details = response.DebugInformation });
        }

        
    }
}
