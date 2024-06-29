using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Linq;

namespace MonikaSAP.Utilities
{
    public static class ExcelReader
    {
        public static List<List<KeyValuePair<string, int>>> ReadExcelFileAsNestedTable(string fileName)
        {
            List<List<KeyValuePair<string,int>>> unformattedDataTable = new List<List<KeyValuePair<string, int>>>();
            string fullPath = fileName + " R.XLSX";

            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fullPath, false))
            {
                // Access worksheet data
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

                // Iterate through rows and cells
                foreach (Row row in sheetData.Elements<Row>())
                {
                    List<KeyValuePair<string, int>> rowData = new List<KeyValuePair<string, int>>();
                    foreach (Cell cell in row.Elements<Cell>())
                    {
                        rowData.Add(GetCellValueAndType(spreadsheetDocument, cell));
                    }
                    unformattedDataTable.Add(rowData);
                }
            }

            return unformattedDataTable;
        }

        private static KeyValuePair<string, int> GetCellValueAndType(SpreadsheetDocument document, Cell cell)
        {
            // Handle different cell value types
            if (cell.CellValue == null)
                return new KeyValuePair<string, int>(null, -1);

            SharedStringTablePart stringTablePart = document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

            if ((cell?.DataType ?? CellValues.Number) == CellValues.SharedString)
                return new KeyValuePair<string, int>(stringTablePart.SharedStringTable?.ElementAt(int.Parse(cell.CellValue.InnerText)).InnerText ?? "", (int)DataType.String);
            

            return cell.CellValue.InnerText.Any(character => character == ',') ? new KeyValuePair<string, int>(cell.CellValue.InnerText, (int)DataType.Double) : new KeyValuePair<string, int>(cell.CellValue.InnerText, (int)DataType.Int);
        }
    }
}
