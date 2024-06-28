using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MultiTaskApp
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource cancellationTokenSource;
        private readonly Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
        }

        // Task 1: Generate Numbers, Letters, and Symbols
        private async void StartTask1_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => GenerateNumbers());
            await Task.Run(() => GenerateLetters());
            await Task.Run(() => GenerateSymbols());
        }

        private void GenerateNumbers()
        {
            for (int i = 0; i <= 50; i++)
            {
                Dispatcher.Invoke(() => OutputTask1TextBox.AppendText(i + " "));
                Thread.Sleep(50);
            }
        }

        private void GenerateLetters()
        {
            for (char c = 'A'; c <= 'Z'; c++)
            {
                Dispatcher.Invoke(() => OutputTask1TextBox.AppendText(c + " "));
                Thread.Sleep(50);
            }
        }

        private void GenerateSymbols()
        {
            for (char c = '!'; c <= '/'; c++)
            {
                Dispatcher.Invoke(() => OutputTask1TextBox.AppendText(c + " "));
                Thread.Sleep(50);
            }
        }

        // Task 2: Horse Race Simulation
        private async void StartHorseRace_Click(object sender, RoutedEventArgs e)
        {
            await RunHorseRaceAsync();
        }

        private async Task RunHorseRaceAsync()
        {
            var tasks = new List<Task>();
            var horses = new List<Horse>();

            for (int i = 1; i <= 5; i++)
            {
                var horse = new Horse(i);
                horses.Add(horse);
                tasks.Add(Task.Run(() => horse.RunRace(random)));
            }

            await Task.WhenAll(tasks);

            // Display race results
            HorseRaceResultsTextBlock.Text = "Race Results:\n";
            foreach (var horse in horses.OrderBy(h => h.FinishTime))
            {
                HorseRaceResultsTextBlock.Text += $"Horse {horse.Number}: {horse.FinishTime} seconds\n";
            }
        }

        // Horse class for Task 2
        private class Horse
        {
            public int Number { get; }
            public int FinishTime { get; private set; }

            public Horse(int number)
            {
                Number = number;
            }

            public void RunRace(Random random)
            {
                FinishTime = random.Next(10, 20); // Simulate random race time
                Thread.Sleep(FinishTime * 100); // Simulate race duration
            }
        }

        // Task 3: Fibonacci Calculation
        private async void CalculateFibonacci_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(FibonacciLimitTextBox.Text, out int limit))
            {
                await CalculateFibonacciAsync(limit);
            }
        }

        private async Task CalculateFibonacciAsync(int limit)
        {
            await Task.Run(() =>
            {
                long a = 0, b = 1;
                Dispatcher.Invoke(() => FibonacciResultTextBlock.Text = $"Fibonacci Series up to {limit}:\n");

                for (int i = 0; i < limit; i++)
                {
                    long temp = a;
                    a = b;
                    b = temp + b;
                    if (a > limit) break;
                    Dispatcher.Invoke(() => FibonacciResultTextBlock.Text += $"{a} ");
                    Thread.Sleep(100); // Delay for visualization
                }
            });
        }

        // Task 4: Word Search in File
        private async void SearchWordInFile_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FilePathTextBox.Text) && !string.IsNullOrWhiteSpace(FileSearchWordTextBox.Text))
            {
                await SearchWordInFileAsync(FilePathTextBox.Text, FileSearchWordTextBox.Text);
            }
        }

        private async Task SearchWordInFileAsync(string filePath, string word)
        {
            await Task.Run(() =>
            {
                try
                {
                    string text = File.ReadAllText(filePath);
                    int count = CountOccurrences(text, word);
                    Dispatcher.Invoke(() => FileSearchResultTextBlock.Text = $"Occurrences of '{word}' in file: {count}");
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => FileSearchResultTextBlock.Text = $"Error: {ex.Message}");
                }
            });
        }

        private int CountOccurrences(string text, string word)
        {
            int count = 0, index = 0;
            while ((index = text.IndexOf(word, index, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                index += word.Length;
                count++;
            }
            return count;
        }

        // Task 5: Word Search in Directory
        private async void SearchWordInDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(DirectoryPathTextBox.Text) && !string.IsNullOrWhiteSpace(DirectorySearchWordTextBox.Text))
            {
                await SearchWordInDirectoryAsync(DirectoryPathTextBox.Text, DirectorySearchWordTextBox.Text);
            }
        }

        private async Task SearchWordInDirectoryAsync(string directoryPath, string word)
        {
            await Task.Run(() =>
            {
                try
                {
                    var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
                    var results = new List<SearchResult>();

                    foreach (var file in files)
                    {
                        string text = File.ReadAllText(file);
                        int count = CountOccurrences(text, word);
                        results.Add(new SearchResult { FileName = Path.GetFileName(file), FilePath = file, Count = count });
                    }

                    Dispatcher.Invoke(() =>
                    {
                        SearchResultsListBox.ItemsSource = results;
                        SearchResultsListBox.DisplayMemberPath = "FileName";
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => SearchResultsListBox.ItemsSource = new List<string> { $"Error: {ex.Message}" });
                }
            });
        }

        private class SearchResult
        {
            public string FileName { get; set; }
            public string FilePath { get; set; }
            public int Count { get; set; }
        }

        // Utility method to browse file path
        private void BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = openFileDialog.FileName;
            }
        }

        // Utility method to browse directory path
        private void BrowseDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                DirectoryPathTextBox.Text = dialog.FileName;
            }
        }
    }
}