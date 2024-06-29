using MonikaSAP.Services.Interfaces;

namespace MonikaSAP.Services
{
    public class CalculatingService : ICalculatingService
    {
        private readonly IPreprocessingService _preprocessingService;
        public CalculatingService(IPreprocessingService preprocessingService) 
        {
            _preprocessingService = preprocessingService;
        }
        public double CalculateRawMaterialCost(string fileName)
        {
            var mainTable = _preprocessingService.PreprocessHierarchyTable(fileName);
            var excelTable = _preprocessingService.PreprocessExcelTable(fileName);

            double result = 0.0;
            return result;
        }
    }
}
