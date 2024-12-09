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

        private double CalculateSuborderRawMaterial(string subOrderNumber,double allQuantity,List<Hierarchy> hierarchyTable,List<ExcelTableRow> excelTable)
        {
            var orderEntryFromExcel = excelTable.FirstOrDefault(x => x.NumberOrder == subOrderNumber && x.IndicatorWnMa == 'H');
            var ratio = CalculateRawMaterialRatio(orderEntryFromExcel.Quantity, allQuantity);

            var subOrderCost = 0.0;
            var alreadyDoneQtyOnBatches = 0.0;
            var allSubOrderEntries = hierarchyTable.Where(x => x.OverlordSuborderNumber == subOrderNumber && x.HierarchyType == (short)HierarchyType.Batch);

            foreach(var batchHierachyEntry in allSubOrderEntries)
            {
                var excelDataForBatch = excelTable.Where(x => x.NumberOrder == batchHierachyEntry.Number);
                if(!excelDataForBatch.Any())
                    continue;

                if(batchHierachyEntry.ReferenceBatchNumber != null)
                {
                    var quantityOnOverlordBatch = Math.Abs(excelDataForBatch.FirstOrDefault(x => x.IndicatorWnMa == 'S' && x.BatchNumber == batchHierachyEntry.ReferenceBatchNumber).Quantity);
                    var quantityDone = Math.Abs(excelDataForBatch.FirstOrDefault(x => x.IndicatorWnMa == 'S' && x.BatchNumber == batchHierachyEntry.Number).Quantity);
                    if(alreadyDoneQtyOnBatches + quantityDone >= quantityOnOverlordBatch)
                        continue;
                    else
                        alreadyDoneQtyOnBatches += quantityDone;
                }
                foreach(var excelEntry in excelDataForBatch)
                {
                    if (excelEntry.IndicatorWnMa == 'H' && string.IsNullOrEmpty(excelEntry.NumberOrder))
                        subOrderCost += excelEntry.Cost * ratio;
                    if (excelEntry.IndicatorWnMa == 'H' && !string.IsNullOrEmpty(excelEntry.NumberOrder) && 
                        (excelTable.Count(x => x.NumberOrder == excelEntry.BatchNumber) > 1 && excelTable.Count(x => x.Material == excelEntry.Material) > 1))
                        subOrderCost += excelEntry.Cost * ratio;
                }
            }
            return subOrderCost;
        }

        public double CalculateRawMaterialCost(string fileName)
        {
            List<Hierarchy> mainTable = _preprocessingService.PreprocessHierarchyTable(fileName);
            List<ExcelTableRow> excelTable = _preprocessingService.PreprocessExcelTable(fileName);
            double result = 0.0;
            double allQuantity = 0.0;

            foreach (var entry in mainTable) 
            {
                //DONE
                if(entry.HierachyLevel == (short)HierarchyLevel.Level2 && entry.HierarchyType == (short)HierarchyType.Batch)
                {
                    List<ExcelTableRow> mainProductEntries = excelTable.Where(x => x.BatchNumber == entry.Number).ToList();
                    foreach(var productEntry in mainProductEntries)
                    {
                        if (productEntry.IndicatorWnMa == 'S' || (productEntry.IndicatorWnMa == 'H' && !string.IsNullOrEmpty(productEntry.NumberOrder)))
                        {
                            allQuantity = allQuantity <= Math.Abs(productEntry.Quantity) ?  Math.Abs(productEntry.Quantity) : allQuantity;
                            continue;
                        }
                        result += productEntry.Cost;
                    }
                } // DONE
                else if (entry.HierachyLevel == (short)HierarchyLevel.Level4 && entry.HierarchyType == (short)HierarchyType.Batch)
                {
                    List<ExcelTableRow> mainProductEntries = excelTable.Where(x => x.BatchNumber == entry.Number).ToList();
                    foreach (var productEntry in mainProductEntries)
                    {
                        if (productEntry.IndicatorWnMa == 'H' && string.IsNullOrEmpty(productEntry.NumberOrder))
                            result += productEntry.Cost;
                    }
                }
            }
            //CALCULATE SUBORDERS BASED ON RATIO
            var mainSubordersNumbers = mainTable.Where(x => x.HierachyLevel == (short)HierarchyLevel.Level5 && x.HierarchyType == (short)HierarchyType.Order);
            foreach(var subOrderNumber in mainSubordersNumbers)
            {
                result += CalculateSuborderRawMaterial(subOrderNumber.Number, allQuantity, mainTable, excelTable);
            }

            return result;
        }
    }
}
