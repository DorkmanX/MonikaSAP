using Microsoft.AspNetCore.Mvc;
using MonikaSAP.Services;
using MonikaSAP.Services.Interfaces;
using System.Text.RegularExpressions;

namespace MonikaSAP.Controllers
{
    [ApiController]
    [Route("api")]
    public class CalculatingController : ControllerBase
    {
        private readonly ILogger<CalculatingController> _logger;
        private readonly ICalculatingService _calculatingService;

        public CalculatingController(ILogger<CalculatingController> logger, ICalculatingService calculatingService)
        {
            _logger = logger;
            _calculatingService = calculatingService;
        }

        [HttpGet]
        [Route("processSelectedFile")]
        public IActionResult ImportDataFromFileToDB(string fileName)
        {
            var response = _calculatingService.CalculateRawMaterialCost(fileName);

            return Ok(response);
        }
    }
}
