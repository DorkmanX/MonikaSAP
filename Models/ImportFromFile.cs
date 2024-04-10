using System.Text.RegularExpressions;

namespace MonikaSAP.Models
{
    public class ImportFromFile
    {
        private static readonly Regex trimmer = new Regex(@"\s\s+", RegexOptions.Compiled);

        public List<string> ImportFromTextFile(string textFilePath)
        {
            string s = "aa aaa    aaa";
            s = trimmer.Replace(s, " ");

            return new List<string>();
        }
    }
}
