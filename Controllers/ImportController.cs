using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace MonikaSAP.Controllers
{
    [ApiController]
    [Route("api/import")]
    public class ImportController : ControllerBase
    {
        private readonly ILogger<ImportController> _logger;

        public ImportController(ILogger<ImportController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("file/txt")]
        public IActionResult ImportDataFromFileToDB()
        {
            return Ok();
        }
    }
}
