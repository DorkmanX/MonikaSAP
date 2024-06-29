using System.Text.RegularExpressions;

namespace MonikaSAP.Utilities
{
    public static class TextFileReader
    {
        public static List<string> ReadTextFile(string filePath)
        {
            List<string> fileLines = new List<string>();

            using (StreamReader file = new StreamReader(filePath))
            {
                int counter = 0;
                string textLine;

                while ((textLine = file.ReadLine()) != null)
                {
                    fileLines.Add(textLine);
                }

                file.Close();
                Console.WriteLine($"File has {counter} lines.");
            }

            return fileLines;
        }
    }
}
