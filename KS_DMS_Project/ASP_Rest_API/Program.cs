using FluentValidation.AspNetCore;
using FluentValidation;
using ASP_Rest_API.Validators;
using ASP_Rest_API.Mappings;
using ASP_Rest_API.Services;
using log4net.Config;
using log4net;
using ASP_Rest_API.SearchItems;
using Minio;
using Elastic.Clients.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));


// Optional: Configure built-in logging (if needed)
builder.Logging.ClearProviders();
builder.Logging.AddLog4Net(); // You may need to add a reference for this


// Add services to the container.
builder.Services.AddControllers();

//Mapping
builder.Services.AddAutoMapper(typeof(MappingProfile));

//Add AutoValidator
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<MyDocDtoValidator>();

// CORS konfigurieren, um Anfragen von localhost:80 (WebUI) zuzulassen
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebUI",
        policy =>
        {
            policy.WithOrigins("http://localhost") // Die URL deiner Web-UI
                .AllowAnyHeader()
                .AllowAnyOrigin()
                .AllowAnyMethod();
        });
});

//Used for testing -- could maybe be removed
builder.Services.AddSingleton<IMinioClient>(provider =>
{
    // Configure the MinioClient here
    return new MinioClient()
        .WithEndpoint("minio", 9000)  // Use Docker service name instead of localhost
        .WithCredentials("minioadmin", "minioadmin")
        .WithSSL(false)  // Disable SSL to use HTTP
        .Build();
});

var elasticUri = builder.Configuration.GetConnectionString("ElasticSearch") ?? "http://localhost:9200";
builder.Services.AddSingleton(new ElasticsearchClient(new Uri(elasticUri)));

//builder.Services.Configure<ElasticSettings>(builder.Configuration.GetSection("ElasticSettings"));
//builder.Services.AddSingleton<IElasticService, ElasticService>();


builder.Services.AddControllers();
builder.Services.AddSingleton<IMessageQueueService, MessageQueueService>();
builder.Services.AddHostedService<RabbitMqListenerService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    c.IncludeXmlComments(xmlPath);
});


// Registriere HttpClient für den TodoController
builder.Services.AddHttpClient("MyDocDAL", client =>
{
    client.BaseAddress = new Uri("http://mydocdal:8082"); // URL des DAL Services in Docker
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    //swagger unter http://localhost:8081/swagger/index.html fixieren (sichert gegen Konflikte durch nginx oder browser-cache oder Konfigurationsprobleme)
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    c.RoutePrefix = "swagger";
});

// Verwende die CORS-Policy
app.UseCors("AllowWebUI");

//app.UseHttpsRedirection();

// Explicitly listen to HTTP only
app.Urls.Add("http://*:8081"); // Stelle sicher, dass die App nur HTTP verwendet
app.UseAuthorization();

app.MapControllers();

app.Run();
