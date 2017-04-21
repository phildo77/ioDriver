using System.Collections.Generic;

public static partial class ioDriver
{
    public static class Profile
    {
        private static Stack<Tuple<string,long>> m_ProStack = new Stack<Tuple<string, long>>();
 
        public static void Begin(string _label)
        {
            m_ProStack.Push(new Tuple<string, long>(_label,System.DateTime.Now.Ticks));
        }

        public static void End()
        {
            var last = m_ProStack.Pop();
            var deltaTicks = System.DateTime.Now.Ticks - last.Second;
            var deltaMillisecs = (double)deltaTicks/System.TimeSpan.TicksPerMillisecond;

            Log.Info(last.First + " : " + deltaMillisecs + " ms");
        }
    }
}
