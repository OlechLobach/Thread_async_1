using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MultiTaskApp
{
    public class HorseRace
    {
        private readonly List<Horse> horses;

        public HorseRace()
        {
            horses = new List<Horse>();
            for (int i = 1; i <= 5; i++)
            {
                horses.Add(new Horse($"Horse {i}"));
            }
        }

        public async Task StartRace(StackPanel panel, ListBox resultsListBox)
        {
            resultsListBox.Items.Clear();
            panel.Children.Clear();

            foreach (var horse in horses)
            {
                ProgressBar progressBar = new ProgressBar();
                panel.Children.Add(progressBar);
                horse.ProgressBar = progressBar;
            }

            await Task.Run(() =>
            {
                Random random = new Random();
                while (!horses.TrueForAll(h => h.IsFinished))
                {
                    foreach (var horse in horses)
                    {
                        if (!horse.IsFinished)
                        {
                            horse.Move(random.Next(1, 11));
                            horse.ProgressBar.Dispatcher.Invoke(() =>
                            {
                                horse.ProgressBar.Value = horse.DistanceTraveled;
                            });
                            if (horse.IsFinished)
                            {
                                resultsListBox.Dispatcher.Invoke(() =>
                                {
                                    resultsListBox.Items.Add($"{horse.Name} finished in {horse.TimeElapsed} seconds.");
                                });
                            }
                        }
                    }
                    Task.Delay(200).Wait();
                }
            });
        }
    }

    public class Horse
    {
        public string Name { get; }
        public int DistanceTraveled { get; private set; }
        public bool IsFinished { get; private set; }
        public int TimeElapsed { get; private set; }
        public ProgressBar ProgressBar { get; set; }

        public Horse(string name)
        {
            Name = name;
        }

        public void Move(int speed)
        {
            DistanceTraveled += speed;
            if (DistanceTraveled >= 100)
            {
                IsFinished = true;
                TimeElapsed++;
            }
            else
            {
                TimeElapsed++;
            }
        }
    }
}