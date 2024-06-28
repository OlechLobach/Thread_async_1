using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MultiTaskApp
{
    public class WordSearch
    {
        public async Task<int> CountWordOccurrences(string filePath, string word)
        {
            int count = 0;
            await Task.Run(() =>
            {
                string content = File.ReadAllText(filePath);
                count = Regex.Matches(content, "\\b" + word + "\\b").Count;
            });
            return count;
        }
    }
}