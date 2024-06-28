using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MultiTaskApp
{
    public class DirectoryWordSearch
    {
        public async Task<List<SearchResult>> SearchInDirectory(string directoryPath, string word)
        {
            List<SearchResult> results = new List<SearchResult>();

            await Task.Run(() =>
            {
                var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    string content = File.ReadAllText(file);
                    int count = CountOccurrences(content, word);
                    results.Add(new SearchResult
                    {
                        FileName = Path.GetFileName(file),
                        FilePath = file,
                        Count = count
                    });
                }
            });

            return results;
        }

        private int CountOccurrences(string text, string word)
        {
            return Regex.Matches(text, "\\b" + word + "\\b").Count;
        }
    }

    public class SearchResult
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int Count { get; set; }
    }
}