using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace FileBreaker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                return;
            } else if (args[0] == "/?")
            {
                Console.WriteLine("Usage: filebreaker [path_to_file] [percent_corrupt]");
                Console.WriteLine("Example: filebreaker.exe C:\\Users\\Administrator\\Desktop\\file.txt 50");
            }
            string path = args[0];
            int corruptPercent = Convert.ToInt32(args[1]);
            byte[] file = File.ReadAllBytes(path);
            int byteNum = file.Length;
            int percent = byteNum / 100;
            int corruptBytesNum = percent * corruptPercent;
            Console.Write("Corrupting... ");
            using (var progress = new ProgressBar())
            {
                for (int i = 0; i < corruptBytesNum; i++)
                {
                    Random rand = new Random();
                    int randNum = rand.Next(0, 255);
                    byte randByte = Convert.ToByte(randNum);
                    long index = rand.Next(0, byteNum);
                    byte prevByte = file[index];
                    file[index] = randByte;
                    byte newByte = file[index];
                    progress.Report((double)i / corruptBytesNum);
                }
            }
            File.WriteAllBytes(path, file);
            Console.WriteLine("Done.");
        }
    }
    class corrupt
    {

    }
    public class ProgressBar : IDisposable, IProgress<double>
    {
        private const int blockCount = 10;
        private readonly TimeSpan animationInterval = TimeSpan.FromSeconds(1.0 / 8);
        private const string animation = @"|/-\";

        private readonly Timer timer;

        private double currentProgress = 0;
        private string currentText = string.Empty;
        private bool disposed = false;
        private int animationIndex = 0;

        public ProgressBar()
        {
            timer = new Timer(TimerHandler);

            // A progress bar is only for temporary display in a console window.
            // If the console output is redirected to a file, draw nothing.
            // Otherwise, we'll end up with a lot of garbage in the target file.
            if (!Console.IsOutputRedirected)
            {
                ResetTimer();
            }
        }

        public void Report(double value)
        {
            // Make sure value is in [0..1] range
            value = Math.Max(0, Math.Min(1, value));
            Interlocked.Exchange(ref currentProgress, value);
        }

        private void TimerHandler(object state)
        {
            lock (timer)
            {
                if (disposed) return;

                int progressBlockCount = (int)(currentProgress * blockCount);
                int percent = (int)(currentProgress * 100);
                string text = string.Format("[{0}{1}] {2,3}% {3}",
                    new string('#', progressBlockCount), new string('-', blockCount - progressBlockCount),
                    percent,
                    animation[animationIndex++ % animation.Length]);
                UpdateText(text);

                ResetTimer();
            }
        }

        private void UpdateText(string text)
        {
            // Get length of common portion
            int commonPrefixLength = 0;
            int commonLength = Math.Min(currentText.Length, text.Length);
            while (commonPrefixLength < commonLength && text[commonPrefixLength] == currentText[commonPrefixLength])
            {
                commonPrefixLength++;
            }

            // Backtrack to the first differing character
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.Append('\b', currentText.Length - commonPrefixLength);

            // Output new suffix
            outputBuilder.Append(text.Substring(commonPrefixLength));

            // If the new text is shorter than the old one: delete overlapping characters
            int overlapCount = currentText.Length - text.Length;
            if (overlapCount > 0)
            {
                outputBuilder.Append(' ', overlapCount);
                outputBuilder.Append('\b', overlapCount);
            }

            Console.Write(outputBuilder);
            currentText = text;
        }

        private void ResetTimer()
        {
            timer.Change(animationInterval, TimeSpan.FromMilliseconds(-1));
        }

        public void Dispose()
        {
            lock (timer)
            {
                disposed = true;
                UpdateText(string.Empty);
            }
        }

    }
}
