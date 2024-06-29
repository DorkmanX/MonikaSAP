using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MonikaSAP.Services.Interfaces;
using MonikaSAP.Utilities;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MonikaSAP.Services
{
    public class PreprocessingService : IPreprocessingService
    {
        public double CalculateRawMaterialCost(string fileName)
        {
            List<KeyValuePair<string,int>> hierarchyFromTxtFile = PrepareHierarchyFromTextFile(fileName);
            List<List<KeyValuePair<string, int>>> excelWorksheetWithDataType = ExcelReader.ReadExcelFileAsNestedTable(fileName);

            double rawMaterialCost = 0.0;

            return rawMaterialCost;
        }

        private List<KeyValuePair<string, int>> PrepareHierarchyFromTextFile(string fileName)
        {
            List<KeyValuePair<string,int>> hierarchy = new List<KeyValuePair<string,int>>();

            List<string> rawFile = TextFileReader.ReadTextFile(fileName);
            rawFile = FilterFile(rawFile);

            foreach (string line in rawFile)
            {
                int hierarchyLevel = CountWhitespacesAtBegin(line);
                string operationNumber = CutAfterInitialWhitespaceAndDigits(line);
                hierarchy.Add(new KeyValuePair<string,int>(operationNumber, hierarchyLevel));
            }

            return hierarchy;
        }

        private List<string> FilterFile(List<string> rawFile)
        {
            List<string> filteredFile = rawFile.Skip(11).ToList();
            filteredFile = filteredFile.Where(text => text.Length > 1).ToList();
            return filteredFile;
        }

        private int CountWhitespacesAtBegin(string text)
        {
            Match match = Regex.Match(text, @"^\s*(\w|\d)");
            return match.Success ? match.Groups[0].Value.Count(c => char.IsWhiteSpace(c)) : 0;
        }

        private string CutAfterInitialWhitespaceAndDigits(string text)
        {
            Match match = Regex.Match(text, @"^\s*(\w+)\s*");
            return match.Success ? match.Groups[1].Value : "";
        }
    }
}
