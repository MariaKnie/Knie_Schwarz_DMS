using Microsoft.AspNetCore.Mvc;

namespace Knie_Schwarz_project_WS2024_DMS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodoController : ControllerBase
    {
        //private static readonly string[] Summaries = new[]
        //{
        //    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        //};

        //public TodoController()
        //{
        //}

        //[HttpGet(Name = "GetTodo")]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        //        TemperatureC = Random.Shared.Next(-20, 55),
        //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}

        //[HttpDelete("{id}")]
        //public IActionResult DeleteId()
        //{
        //    var item = _todolist.FirstOrDefault(testc => t.id == id); 

        //}
    }
}
