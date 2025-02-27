using System;
using System.Windows.Threading;

namespace XpathViewer
{
    internal class TypingTimer : DispatcherTimer
    {
        public event EventHandler TypingStop;

        public TypingTimer(double milliseconds = 300)
        {
            Interval = TimeSpan.FromMilliseconds(milliseconds);
            Tick += TypingTimer_Tick;
        }

        public void Reset()
        {
            Stop();
            Start();
        }

        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            Stop();
            TypingStop?.Invoke(this, EventArgs.Empty);
        }

    }
}
