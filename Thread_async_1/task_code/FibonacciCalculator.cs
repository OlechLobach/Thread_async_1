using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MultiTaskApp
{
    public class FibonacciCalculator
    {
        public async Task CalculateAndDisplay(int limit, TextBlock resultTextBlock)
        {
            await Task.Run(() =>
            {
                BigInteger a = 0;
                BigInteger b = 1;

                for (int i = 0; i < limit; i++)
                {
                    BigInteger temp = a;
                    a = b;
                    b = temp + b;
                }

                resultTextBlock.Dispatcher.Invoke(() =>
                {
                    resultTextBlock.Text = $"Fibonacci sequence up to {limit}:\n{a}";
                });
            });
        }
    }
}