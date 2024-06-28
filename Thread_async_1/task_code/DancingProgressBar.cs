using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MultiTaskApp
{
    public class DancingProgressBar
    {
        public async Task StartDancingBars(int barCount, StackPanel panel)
        {
            Random random = new Random();

            for (int i = 0; i < barCount; i++)
            {
                ProgressBar progressBar = new ProgressBar();
                progressBar.Maximum = 100;
                panel.Children.Add(progressBar);

                await Task.Run(() =>
                {
                    for (int j = 0; j <= 100; j++)
                    {
                        progressBar.Dispatcher.Invoke(() =>
                        {
                            progressBar.Value = j;
                            progressBar.Foreground = new SolidColorBrush(Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)));
                        });
                        Task.Delay(50).Wait();
                    }
                    panel.Children.Remove(progressBar);
                });
            }
        }
    }
}