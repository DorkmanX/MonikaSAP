using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Hosting;
using MonikaSAP.Models;
using MonikaSAP.Services.Interfaces;
using MonikaSAP.Utilities;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MonikaSAP.Services
{
    public class PreprocessingService : IPreprocessingService
    {

        private List<KeyValuePair<string, short>> PrepareHierarchyFromTextFile(string fileName)
        {
            List<KeyValuePair<string, short>> hierarchy = new List<KeyValuePair<string, short>>();

            List<string> rawFile = TextFileReader.ReadTextFile(fileName);
            rawFile = FilterFile(rawFile);

            short lastHierarchyLevel = 0;
            short decreaseHierarchyLevel = 0;
            bool decreaseAllSubOrders = false;
            bool substituteOneLevel = false;

            foreach (string line in rawFile)
            {
                short hierarchyLevel = CountWhitespacesAtBegin(line);
                string operationNumber = CutAfterInitialWhitespaceAndDigits(line);
                if (decreaseHierarchyLevel >= hierarchyLevel)
                {
                    substituteOneLevel = false;
                    decreaseAllSubOrders = false;
                }

                if (hierarchyLevel > lastHierarchyLevel && hierarchyLevel - lastHierarchyLevel > 1 && !decreaseAllSubOrders)
                {
                    decreaseAllSubOrders = true;
                    substituteOneLevel = true;
                    decreaseHierarchyLevel = hierarchyLevel;
                }

                if(substituteOneLevel == true)
                    hierarchyLevel -= 1;

                hierarchy.Add(new KeyValuePair<string, short>(operationNumber, hierarchyLevel));
                lastHierarchyLevel = hierarchyLevel;
            }

            return hierarchy;
        }

        private List<string> FilterFile(List<string> rawFile)
        {
            List<string> filteredFile = rawFile.Skip(11).ToList();
            filteredFile = filteredFile.Where(text => text.Length > 1).ToList();
            return filteredFile;
        }

        private short CountWhitespacesAtBegin(string text)
        {
            Match match = Regex.Match(text, @"^\s*(\w|\d)");
            return (short) (match.Success ? match.Groups[0].Value.Count(c => char.IsWhiteSpace(c)) : 0);
        }

        private string CutAfterInitialWhitespaceAndDigits(string text)
        {
            Match match = Regex.Match(text, @"^\s*(\w+)\s*");
            return match.Success ? match.Groups[1].Value : "";
        }

        public List<ExcelTableRow> PreprocessExcelTable(string fileName)
        {
            List<List<KeyValuePair<string, int>>> excelWorksheetWithDataType = ExcelReader.ReadExcelFileAsNestedTable(fileName);
            List<ExcelTableRow> excelTableRows = new List<ExcelTableRow>();

            int counter = 0;
            for(int i = 1; i < excelWorksheetWithDataType.Count; i++)
            {
                excelTableRows.Add(new ExcelTableRow()
                {
                    Id = ++counter,
                    BatchNumber = excelWorksheetWithDataType[i][0].Key,
                    Material = excelWorksheetWithDataType[i][1].Key,
                    NumberOrder = excelWorksheetWithDataType[i][8].Key,
                    IndicatorWnMa = excelWorksheetWithDataType[i][9].Key[0],
                    Cost = Convert.ToDouble(excelWorksheetWithDataType[i][10].Key),
                    Quantity = Convert.ToDouble(excelWorksheetWithDataType[i][12].Key)
                });
            }
            

            return excelTableRows;
        }

        public List<Hierarchy> PreprocessHierarchyTable(string fileName)
        {
            List<Hierarchy> mainTable = new List<Hierarchy>();
            int counter = 0;

            List<KeyValuePair<string, short>> hierarchyFromTxtFile = PrepareHierarchyFromTextFile(fileName);

            foreach (var hierarchy in hierarchyFromTxtFile) 
            {
                var newHierarchy = new Hierarchy()
                {
                    Id = ++counter,
                    Number = hierarchy.Key,
                    HierarchyType = hierarchy.Value % 2 == 0 ? (short)HierarchyType.Batch : (short)HierarchyType.Order, 
                    HierachyLevel = hierarchy.Value,
                    ReferenceBatchNumber = null,
                    OverlordSuborderNumber = null
                };

                //find if batch quantity is done for the first full order, check reference overlord batch
                if(newHierarchy.HierarchyType == (short)HierarchyType.Batch && hierarchy.Value > (short)HierarchyLevel.Level6)
                    newHierarchy.ReferenceBatchNumber = mainTable.LastOrDefault(x => x.HierachyLevel == (hierarchy.Value - 2)).Number;
                
                //find the whole order number to speed later percentages calculations
                if(newHierarchy.HierachyLevel > (short)HierarchyLevel.Level5)
                    newHierarchy.OverlordSuborderNumber = mainTable.LastOrDefault(x => x.HierachyLevel == (short)HierarchyLevel.Level5 
                    && x.HierarchyType == (short)HierarchyType.Order).Number;

                mainTable.Add(newHierarchy);
            }

            return mainTable;
        }
    }
}
