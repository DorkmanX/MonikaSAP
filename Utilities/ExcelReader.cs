using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Charts;
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
                    IEnumerable<StringValue?> cellReferences = row.Descendants<Cell>().Select(c => c.CellReference);
                    List<string> cellReferencesConverted = new List<string>();

                    foreach (var item in cellReferences)
                    {
                        cellReferencesConverted.Add(Convert.ToString(item));
                    }
                    // Loop through expected cell positions (based on column count)
                    int cellCount = row.Descendants<Cell>().Count();
                    for (int colIndex = 1; colIndex <= cellCount; colIndex++)
                    {
                        string cellReference = $"A{row.RowIndex}"; // Adjust base column letter if needed
                        cellReference = cellReference.Replace("A", GetColumnName(colIndex)); // Convert to column letter

                        // Check if cell exists in the row
                        if (cellReferencesConverted.Contains(cellReference))
                        {
                            Cell cell = row.Elements<Cell>().Where(c => c.CellReference == cellReference).FirstOrDefault();
                            rowData.Add(GetCellValueAndType(spreadsheetDocument, cell));
                        }
                        else
                        {
                            // Add empty string for missing cell
                            rowData.Add(new KeyValuePair<string, int>($"empty{row.RowIndex}{colIndex}", -1));
                        }
                    }
                    if(rowData.Count >= 12)
                        unformattedDataTable.Add(rowData);
                }
            }

            return unformattedDataTable;
        }

        private static KeyValuePair<string, int> GetCellValueAndType(SpreadsheetDocument document, Cell cell)
        {
            // Handle different cell value types
            if (cell.CellValue == null)
                return new KeyValuePair<string, int>("empty", -1);

            SharedStringTablePart stringTablePart = document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

            if ((cell?.DataType ?? CellValues.Number) == CellValues.SharedString)
                return new KeyValuePair<string, int>(stringTablePart.SharedStringTable?.ElementAt(int.Parse(cell.CellValue.InnerText)).InnerText ?? "", (int)DataType.String);
            

            return cell.CellValue.InnerText.Any(character => character == ',') ? new KeyValuePair<string, int>(cell.CellValue.InnerText, (int)DataType.Double) : new KeyValuePair<string, int>(cell.CellValue.InnerText, (int)DataType.Int);
        }

        private static string GetColumnName(int columnIndex)
        {
            int remainder;
            string columnName = "";

            while (columnIndex > 0)
            {
                remainder = (columnIndex - 1) % 26;
                columnName = (char)(65 + remainder) + columnName; // Convert to A-Z
                columnIndex = (columnIndex - 1) / 26;
            }

            return columnName;
        }
    }
}
