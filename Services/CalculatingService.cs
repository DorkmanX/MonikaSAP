using MonikaSAP.Models;
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

        private double CalculateRawMaterialRatio(double suborderQuantity,double allQuantity) => suborderQuantity / allQuantity;
        private double CalculateSuborderRawMaterial(string subBatchNumber,double rawMaterialRatio,List<ExcelTableRow> excelTable)
        {
            double result = 0;
            return result;
        }

        public double CalculateRawMaterialCost(string fileName)
        {
            List<Hierarchy> mainTable = _preprocessingService.PreprocessHierarchyTable(fileName);
            List<ExcelTableRow> excelTable = _preprocessingService.PreprocessExcelTable(fileName);
            double result = 0.0;
            double allQuantity = 0.0;

            foreach (var entry in mainTable ) 
            {
                if(entry.HierachyLevel == (short)HierarchyLevel.Level2 && entry.HierarchyType == (short)HierarchyType.Batch)
                {
                    List<ExcelTableRow> mainProductEntries = excelTable.Where(x => x.BatchNumber == entry.Number).ToList();
                    foreach(var productEntry in mainProductEntries)
                    {
                        if (productEntry.IndicatorWnMa == 'S' || (productEntry.IndicatorWnMa == 'H' && !productEntry.NumberOrder.Contains("empty")))
                        {
                            if(productEntry.IndicatorWnMa == 'S')
                                allQuantity = Math.Abs(productEntry.Quantity);
                            continue;
                        }
                        result += productEntry.Cost;
                    }
                }
                if (entry.HierachyLevel == (short)HierarchyLevel.Level4 && entry.HierarchyType == (short)HierarchyType.Batch)
                {
                    List<ExcelTableRow> mainProductEntries = excelTable.Where(x => x.BatchNumber == entry.Number).ToList();
                    foreach (var productEntry in mainProductEntries)
                    {
                        if (productEntry.IndicatorWnMa == 'S')
                            continue;
                        if (productEntry.IndicatorWnMa == 'H' && !productEntry.NumberOrder.Contains("empty"))
                        {
                            result += CalculateSuborderRawMaterial(productEntry.BatchNumber,CalculateRawMaterialRatio(productEntry.Quantity, allQuantity),excelTable);
                            continue;
                        }
                        result += productEntry.Cost;
                    }
                }
            }

            return result;
        }
    }
}
