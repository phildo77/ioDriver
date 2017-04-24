using System.Collections.Generic;
using System.Diagnostics;

public static partial class ioDriver
{
    public static class Profile
    {
        private static Stack<Tuple<string, Stopwatch>> m_ProStack = new Stack<Tuple<string, Stopwatch>>();
 
        public static void Begin(string _label)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            m_ProStack.Push(new Tuple<string, Stopwatch>(_label, watch));
        }

        public static void End()
        {
            var last = m_ProStack.Pop();
            last.Second.Stop();
            var ticks = last.Second.ElapsedTicks;
            var freq = Stopwatch.Frequency;
            var isHR = Stopwatch.IsHighResolution;

            var ms = (decimal)ticks/freq * 1000;

            Log.Info(last.First + " : " + ms.ToString("0.######") + " ms --- Ticks: " + ticks + "  ---- isHR : " + isHR);
        }
    }
}

