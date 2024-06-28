using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace MultiThreadingApp
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource fileCopyCancellationTokenSource;
        private CancellationTokenSource directoryCopyCancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartTask1_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => GenerateNumbers());
            Task.Run(() => GenerateLetters());
            Task.Run(() => GenerateSymbols());
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

        private void BrowseSourceFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                SourceFilePathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void BrowseDestinationFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                DestinationFilePathTextBox.Text = saveFileDialog.FileName;
            }
        }

        private async void StartFileCopy_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(FileCopyThreadCountTextBox.Text, out int threadCount) &&
                !string.IsNullOrWhiteSpace(SourceFilePathTextBox.Text) &&
                !string.IsNullOrWhiteSpace(DestinationFilePathTextBox.Text))
            {
                fileCopyCancellationTokenSource = new CancellationTokenSource();
                var progress = new Progress<int>(value => FileCopyProgressBar.Value = value);
                await CopyFileAsync(SourceFilePathTextBox.Text, DestinationFilePathTextBox.Text, threadCount, progress, fileCopyCancellationTokenSource.Token);
            }
        }

        private void PauseFileCopy_Click(object sender, RoutedEventArgs e)
        {
            fileCopyCancellationTokenSource?.Cancel();
        }

        private void StopFileCopy_Click(object sender, RoutedEventArgs e)
        {
            fileCopyCancellationTokenSource?.Cancel();
        }

        private Task CopyFileAsync(string sourcePath, string destinationPath, int threadCount, IProgress<int> progress, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                const int bufferSize = 8192;
                long totalBytes = new FileInfo(sourcePath).Length;
                long totalBytesCopied = 0;

                byte[] buffer = new byte[bufferSize];
                List<Task> tasks = new List<Task>();

                using (FileStream sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
                using (FileStream destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                {
                    for (int i = 0; i < threadCount; i++)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            int bytesRead;
                            while ((bytesRead = sourceStream.Read(buffer, 0, bufferSize)) > 0 && !cancellationToken.IsCancellationRequested)
                            {
                                destinationStream.Write(buffer, 0, bytesRead);
                                Interlocked.Add(ref totalBytesCopied, bytesRead);
                                progress.Report((int)((totalBytesCopied * 100) / totalBytes));
                            }
                        }));
                    }
                    Task.WhenAll(tasks).Wait();
                }
            }, cancellationToken);
        }

        private void BrowseSourceDirectory_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.ValidateNames = false;
            openFileDialog.CheckFileExists = false;
            openFileDialog.CheckPathExists = true;
            openFileDialog.FileName = "Select a folder"; 
            openFileDialog.Filter = "Folder|*.none";
            if (openFileDialog.ShowDialog() == true)
            {
                SourceDirectoryPathTextBox.Text = Path.GetDirectoryName(openFileDialog.FileName);
            }
        }

        private void BrowseDestinationDirectory_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.ValidateNames = false;
            saveFileDialog.CheckFileExists = false;
            saveFileDialog.CheckPathExists = true;
            saveFileDialog.FileName = "Select a folder";
            saveFileDialog.Filter = "Folder|*.none";
            if (saveFileDialog.ShowDialog() == true)
            {
                DestinationDirectoryPathTextBox.Text = Path.GetDirectoryName(saveFileDialog.FileName);
            }
        }

        private async void StartDirectoryCopy_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(DirectoryCopyThreadCountTextBox.Text, out int threadCount) &&
                !string.IsNullOrWhiteSpace(SourceDirectoryPathTextBox.Text) &&
                !string.IsNullOrWhiteSpace(DestinationDirectoryPathTextBox.Text))
            {
                directoryCopyCancellationTokenSource = new CancellationTokenSource();
                var progress = new Progress<int>(value => DirectoryCopyProgressBar.Value = value);
                await CopyDirectoryAsync(SourceDirectoryPathTextBox.Text, DestinationDirectoryPathTextBox.Text, threadCount, progress, directoryCopyCancellationTokenSource.Token);
            }
        }

        private void PauseDirectoryCopy_Click(object sender, RoutedEventArgs e)
        {
            directoryCopyCancellationTokenSource?.Cancel();
        }

        private void StopDirectoryCopy_Click(object sender, RoutedEventArgs e)
        {
            directoryCopyCancellationTokenSource?.Cancel();
        }

        private Task CopyDirectoryAsync(string sourceDir, string destinationDir, int threadCount, IProgress<int> progress, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                Directory.CreateDirectory(destinationDir);
                string[] files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
                long totalBytes = files.Sum(file => new FileInfo(file).Length);
                long totalBytesCopied = 0;

                List<Task> tasks = new List<Task>();

                foreach (string file in files)
                {
                    string dest = file.Replace(sourceDir, destinationDir);
                    Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    tasks.Add(Task.Run(() =>
                    {
                        byte[] buffer = new byte[8192];
                        using (FileStream sourceStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                        using (FileStream destinationStream = new FileStream(dest, FileMode.Create, FileAccess.Write))
                        {
                            int bytesRead;
                            while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0 && !cancellationToken.IsCancellationRequested)
                            {
                                destinationStream.Write(buffer, 0, bytesRead);
                                Interlocked.Add(ref totalBytesCopied, bytesRead);
                                progress.Report((int)((totalBytesCopied * 100) / totalBytes));
                            }
                        }
                    }));
                }

                Task.WhenAll(tasks).Wait();
            }, cancellationToken);
        }

        private async void CalculateFactorial_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(FactorialNumberTextBox.Text, out int number))
            {
                long result = await Task.Run(() => CalculateFactorial(number));
                FactorialResultTextBlock.Text = $"Factorial of {number} is {result}";
            }
        }

        private long CalculateFactorial(int number)
        {
            if (number == 0) return 1;
            return number * CalculateFactorial(number - 1);
        }

        private async void CalculatePower_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(PowerBaseTextBox.Text, out int baseNumber) &&
                int.TryParse(PowerExponentTextBox.Text, out int exponent))
            {
                long result = await Task.Run(() => CalculatePower(baseNumber, exponent));
                PowerResultTextBlock.Text = $"{baseNumber}^{exponent} = {result}";
            }
        }

        private long CalculatePower(int baseNumber, int exponent)
        {
            return (long)Math.Pow(baseNumber, exponent);
        }

        private async void CountTextElements_Click(object sender, RoutedEventArgs e)
        {
            string input = InputTextBox.Text;
            var result = await Task.Run(() => CountElements(input));
            TextCountResultTextBlock.Text = $"Vowels: {result.vowels}, Consonants: {result.consonants}, Total: {result.total}";
        }

        private (int vowels, int consonants, int total) CountElements(string text)
        {
            int vowels = 0, consonants = 0, total = 0;
            foreach (char c in text)
            {
                if ("aeiouAEIOU".Contains(c))
                {
                    vowels++;
                }
                else if (char.IsLetter(c))
                {
                    consonants++;
                }
                total++;
            }
            return (vowels, consonants, total);
        }
    }
}