using DocumentFormat.OpenXml.Vml.Office;
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

        private double CalculateSuborderRawMaterial(string subOrderNumber,double allQuantity,List<Hierarchy> hierarchyTable,List<ExcelTableRow> excelTable,List<History> history)
        {
            var orderEntryFromExcel = excelTable.FirstOrDefault(x => x.NumberOrder == subOrderNumber && x.IndicatorWnMa == 'H');
            var ratio = CalculateRawMaterialRatio(orderEntryFromExcel.Quantity, allQuantity);

            history.Add(new History()
            {
                Number = subOrderNumber,
                Formula = $"{orderEntryFromExcel.Quantity} szt / {allQuantity} szt",
                Result = ratio * 100,
                Reason = "Procent do kontynuowania przy segregacji RM w kolejnych podzleceniach"
            });

            var subOrderCost = 0.0;
            var alreadyDoneQtyOnBatches = 0.0;
            var allSubOrderEntries = hierarchyTable.Where(x => x.OverlordSuborderNumber == subOrderNumber && x.HierarchyType == (short)HierarchyType.Batch);

            foreach(var batchHierachyEntry in allSubOrderEntries)
            {
                var excelDataForBatch = excelTable.Where(x => x.NumberOrder == batchHierachyEntry.Number);
                if (!excelDataForBatch.Any())
                {
                    history.Add(new History()
                    {
                        Number = batchHierachyEntry.Number,
                        Formula = $"",
                        Result = 0,
                        Reason = "Zlecenie przerwane ? brak wpisów dla nr zlecenia w pliku excel"
                    });
                    continue;
                }

                if(batchHierachyEntry.ReferenceBatchNumber != null)
                {
                    var quantityOnOverlordBatch = Math.Abs(excelDataForBatch.FirstOrDefault(x => x.IndicatorWnMa == 'S' && x.BatchNumber == batchHierachyEntry.ReferenceBatchNumber).Quantity);
                    var quantityDone = Math.Abs(excelDataForBatch.FirstOrDefault(x => x.IndicatorWnMa == 'S' && x.BatchNumber == batchHierachyEntry.Number).Quantity);
                    if (alreadyDoneQtyOnBatches + quantityDone >= quantityOnOverlordBatch)
                    {
                        history.Add(new History()
                        {
                            Number = batchHierachyEntry.Number,
                            Formula = $"{alreadyDoneQtyOnBatches} szt + {quantityDone} szt >= {quantityOnOverlordBatch} szt",
                            Result = 0,
                            Reason = $"Cała ilość wykorzystana w zleceniu {batchHierachyEntry.ReferenceBatchNumber} => partia testowa"
                        });
                        continue;
                    }
                    else
                        alreadyDoneQtyOnBatches += quantityDone;
                }
                foreach(var excelEntry in excelDataForBatch)
                {
                    if (excelEntry.IndicatorWnMa == 'H' && string.IsNullOrEmpty(excelEntry.NumberOrder))
                    {
                        subOrderCost += excelEntry.Cost * ratio;
                        history.Add(new History()
                        {
                            Number = batchHierachyEntry.Number,
                            Formula = $"{excelEntry.Cost}zł * {ratio * 100} %",
                            Result = excelEntry.Cost * ratio,
                            Reason = "Kopiujemy z S dla tego zlecenia % dla wszystkich surówców"
                        });
                    }
                    if (excelEntry.IndicatorWnMa == 'H' && !string.IsNullOrEmpty(excelEntry.NumberOrder))
                    {
                        subOrderCost += excelEntry.Cost * ratio;
                        if ((excelTable.Count(x => x.NumberOrder == excelEntry.BatchNumber) > 1 && excelTable.Count(x => x.Material == excelEntry.Material) > 1))
                        {
                            history.Add(new History()
                            {
                                Number = batchHierachyEntry.Number,
                                Formula = $"{excelEntry.Cost}zł * {ratio * 100} %",
                                Result = excelEntry.Cost * ratio,
                                Reason = "H i nr partii szukamy czy występuje nr partii i nr materiału - tutaj jest"
                            });
                        }
                        else
                        {
                            history.Add(new History()
                            {
                                Number = batchHierachyEntry.Number,
                                Formula = $"{excelEntry.Cost}zł * {ratio * 100} %",
                                Result = excelEntry.Cost * ratio,
                                Reason = "H i nr partii szukamy czy występuje nr partii i nr materiału - tutaj nie ma ale doliczam bo kazałaś"
                            });
                        }
                    }
                }
            }
            return subOrderCost;
        }

        public Response CalculateRawMaterialCost(string fileName)
        {
            List<Hierarchy> mainTable = _preprocessingService.PreprocessHierarchyTable(fileName);
            List<ExcelTableRow> excelTable = _preprocessingService.PreprocessExcelTable(fileName);
            List<History> history = new List<History>();

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
                        history.Add(new History()
                        {
                            Number = entry.Number,
                            Formula = $"{productEntry.Cost}zł * 100%",
                            Result = productEntry.Cost,
                            Reason = "100% wykorzystane w FG"
                        });
                    }
                } // DONE
                else if (entry.HierachyLevel == (short)HierarchyLevel.Level4 && entry.HierarchyType == (short)HierarchyType.Batch)
                {
                    List<ExcelTableRow> mainProductEntries = excelTable.Where(x => x.BatchNumber == entry.Number).ToList();
                    foreach (var productEntry in mainProductEntries)
                    {
                        if (productEntry.IndicatorWnMa == 'H' && string.IsNullOrEmpty(productEntry.NumberOrder))
                        {
                            result += productEntry.Cost;
                            history.Add(new History()
                            {
                                Number = entry.Number,
                                Formula = $"{productEntry.Cost}zł * 100%",
                                Result = productEntry.Cost,
                                Reason = "100% wykorzystane w FG"
                            });
                        }
                    }
                }
            }
            //CALCULATE SUBORDERS BASED ON RATIO
            var mainSubordersNumbers = mainTable.Where(x => x.HierachyLevel == (short)HierarchyLevel.Level5 && x.HierarchyType == (short)HierarchyType.Order);
            foreach(var subOrderNumber in mainSubordersNumbers)
            {
                result += CalculateSuborderRawMaterial(subOrderNumber.Number, allQuantity, mainTable, excelTable,history);
            }

            return new Response() { History = history, Cost = result };
        }
    }
}
