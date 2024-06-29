using MonikaSAP.Models;

namespace MonikaSAP.Services.Interfaces
{
    public interface IPreprocessingService
    {
        public List<Hierarchy> PreprocessHierarchyTable(string fileName);
        public List<ExcelTableRow> PreprocessExcelTable(string fileName);
    }
}
