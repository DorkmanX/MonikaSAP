using Microsoft.AspNetCore.Mvc;
using MonikaSAP.Services;
using MonikaSAP.Services.Interfaces;
using System.Text.RegularExpressions;

namespace MonikaSAP.Controllers
{
    [ApiController]
    [Route("api")]
    public class ImportController : ControllerBase
    {
        private readonly ILogger<ImportController> _logger;
        private readonly IPreprocessingService _preprocessingService;

        public ImportController(ILogger<ImportController> logger,IPreprocessingService service)
        {
            _logger = logger;
            _preprocessingService = service;
        }

        [HttpGet]
        [Route("processSelectedFile")]
        public IActionResult ImportDataFromFileToDB(string fileName = null)
        {
            var mainTable = _preprocessingService.PreprocessHierarchyTable(fileName);
            var excelTable = _preprocessingService.PreprocessExcelTable(fileName);

            return Ok();
        }
    }
}
