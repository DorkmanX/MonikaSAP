using MonikaSAP.Services.Interfaces;
using MonikaSAP.Utilities;

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

            foreach(var entry in mainTable ) 
            {
                if(entry.HierachyLevel == (short)HierarchyLevel.Level1)
                {

                }
            }

            double result = 0.0;
            return result;
        }
    }
}
