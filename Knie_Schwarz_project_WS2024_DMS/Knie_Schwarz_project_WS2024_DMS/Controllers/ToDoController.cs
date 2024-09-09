using Microsoft.AspNetCore.Mvc;

namespace Knie_Schwarz_project_WS2024_DMS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodoController : ControllerBase
    {
        private Document first;

        public TodoController()
        {
            first = new Document();
        }

        [HttpGet(Name = "GetTodo")]
        public string Get()
        {
            return first.Title;
        }

        [HttpDelete("{id}")]
        public string DeleteId()
        {

            return first.Id.ToString();
        }
    }
}
