using System.Text.RegularExpressions;

namespace MonikaSAP.Models
{
    public class ExcelTableRow
    {
        public int Id { get; set; }
        public string NumberOrder { get; set; }
        public string Material {  get; set; }
        public string BatchNumber { get; set; }
        public char IndicatorWnMa { get; set; }
        public double Cost { get; set; }
        public double Quantity { get; set; }
        public double Percentage { get; set; }
    }
}
