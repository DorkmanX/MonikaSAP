using MonikaSAP.Services.Interfaces;
using MonikaSAP.Utilities;
using System.Text.RegularExpressions;

namespace MonikaSAP.Services
{
    public class PreprocessingService : IPreprocessingService
    {
        public List<KeyValuePair<string, int>> PrepareHierarchyFromTextFile(string fileName)
        {
            List<KeyValuePair<string,int>> hierarchy = new List<KeyValuePair<string,int>>();

            List<string> rawFile = TextFileReader.ReadTextFile(fileName);
            rawFile = rawFile.Skip(11).ToList();

            foreach(string line in rawFile)
            {
                int hierarchyLevel = CountWhitespacesAtBegin(line);
                string operationNumber = CutAfterInitialWhitespaceAndDigits(line);
                hierarchy.Add(new KeyValuePair<string,int>(operationNumber, hierarchyLevel));
            }

            return hierarchy;
        }

        private int CountWhitespacesAtBegin(string text)
        {
            Match match = Regex.Match(text, @"^\s*(\w|\d)");
            return match.Success ? match.Groups[0].Value.Count(c => char.IsWhiteSpace(c)) : 0;
        }

        private string CutAfterInitialWhitespaceAndDigits(string text)
        {
            Match match = Regex.Match(text, @"^\s*(\d+)\s*");
            return match.Success ? match.Groups[1].Value : "";
        }
    }
}
