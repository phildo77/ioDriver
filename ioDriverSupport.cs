﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;

public static partial class ioDriver
{
    /// <summary>
    /// Basic profiler functionality (time of execution)
    /// </summary>
    public static class Profile
    {
        private static Dictionary<string, Tuple<string, Stopwatch>> m_ProfilerData =
            new Dictionary<string, Tuple<string, Stopwatch>>();
        /// <summary>
        /// Begin profile timer with specified label, or resume if it already exists.
        /// </summary>
        /// <param name="_label">Profiler Identifier</param>
        public static void Begin(string _label)
        {
            if (m_ProfilerData.ContainsKey(_label))
            {
                m_ProfilerData[_label].Second.Start();
                return;
            }
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            m_ProfilerData.Add(_label, new Tuple<string, Stopwatch>(_label, watch));
        }

        /// <summary>
        /// End profiling with specified label and report result via <see cref="Log.Info"/>.
        /// </summary>
        /// <param name="_label">Profiler Identifier</param>
        public static void End(string _label)
        {
            if (!m_ProfilerData.ContainsKey(_label))
            {
                Log.Err("Profiler does not contain key: " + _label);
                return;
            }

            var data = m_ProfilerData[_label];
            m_ProfilerData.Remove(_label);


            data.Second.Stop();
            var ticks = data.Second.ElapsedTicks;
            var freq = Stopwatch.Frequency;
            var isHR = Stopwatch.IsHighResolution;

            var ms = (decimal)ticks / freq * 1000;

            Log.Info(data.First + " : " + ms.ToString("0.######") + " ms --- Ticks: " + ticks + "  ---- isHR : " + isHR);
        }

        public static void Cumulative(string _label)
        {
            if (!m_ProfilerData.ContainsKey(_label))
            {
                Log.Err("Profiler does not contain key: " + _label);
                return;
            }

            m_ProfilerData[_label].Second.Stop();

        }
    }

    /// <summary>
    /// Class that allows chaining and branching of multiple drivers.
    /// </summary>
    public class DChain
    {
        private readonly List<ioDriver.DBase> m_Chain = new List<ioDriver.DBase>();
        private readonly Dictionary<ioDriver.DBase, List<DChain>> m_Branches = new Dictionary<ioDriver.DBase, List<DChain>>();

        /// <summary>
        /// Create empty driver chain.
        /// </summary>
        public DChain() { }

        /// <summary>
        /// Create driver chain with one driver.
        /// </summary>
        /// <param name="_driver">Driver at start of chain</param>
        public DChain(ioDriver.DBase _driver) { m_Chain.Add(_driver); }

        /// <summary>
        /// Create driver chain series from list of drivers.
        /// </summary>
        /// <param name="_chain">Enumerated drivers in chain order</param>
        public DChain(IEnumerable<ioDriver.DBase> _chain) { m_Chain = _chain.ToList(); }

        /// <summary>
        /// Create driver chain series from params
        /// </summary>
        /// <param name="_chain">Enumerated drivers in chain order</param>
        public DChain(params ioDriver.DBase[] _chain) { m_Chain = ToList(_chain); }

        /// <summary>
        /// Add driver to end of chain
        /// </summary>
        /// <param name="_driver">Driver to add</param>
        public void Add(ioDriver.DBase _driver) { m_Chain.Add(_driver); }

        /// <summary>
        /// Add this chain as branch to OnFinish of driver at index of specified chain.
        /// </summary>
        /// <param name="_index">Index of driver to add this chain as branch</param>
        /// <param name="_chain">Chain to index and add to</param>
        public void AddChainTo(int _index, DChain _chain)
        {
            var selDriver = m_Chain[_index];
            if (!m_Branches.ContainsKey(selDriver))
                m_Branches.Add(selDriver, new List<DChain>());
            m_Branches[selDriver].Add(_chain);
        }

        /// <summary>
        /// Add this chain as branch to OnFinish of specified driver in specified chain.
        /// </summary>
        /// <param name="_driver">Driver to add to</param>
        /// <param name="_chain">Chain to add to</param>
        public void AddChainTo(ioDriver.DBase _driver, DChain _chain)
        {
            for (int idx = 0; idx < m_Chain.Count; ++idx)
            {
                if (m_Chain[idx] != _driver) continue;
                AddChainTo(idx, _chain);
                return;
            }
            Log.Err("ioDriver:Chain:AddChainTo - Driver does not exist in chain!  Driver '" + _driver.Name + "' requested.");
        }

        /// <summary>
        /// Start this chain
        /// </summary>
        public void Go()
        {
            if (m_Chain.Count == 0)
            {
                Log.Warn("ioDriver:Chain:Go() - No drivers in chain!");
                return;
            }
            //Link up drivers and fire branch chains
            for (int idx = 0; idx < m_Chain.Count - 1; ++idx)
            {
                var curDrv = m_Chain[idx];
                var nextDrv = m_Chain[idx + 1];
                curDrv.SetAddEvent(Events.OnFinish, _drv => nextDrv.Go());
                if (!m_Branches.ContainsKey(curDrv)) continue;
                foreach (var chain in m_Branches[curDrv])
                {
                    var thisChain = chain;
                    curDrv.SetAddEvent(Events.OnFinish, _drv => thisChain.Go());
                }
            }
            m_Chain[0].Go();
        }
    }

    [System.Serializable]
    private class Tuple<T1, T2>
    {
        public readonly T1 First;
        public readonly T2 Second;

        private static readonly IEqualityComparer Item1Comparer = EqualityComparer<T1>.Default;
        private static readonly IEqualityComparer Item2Comparer = EqualityComparer<T2>.Default;

        public Tuple(T1 _first, T2 _second)
        {
            this.First = _first;
            this.Second = _second;
        }

        public override string ToString()
        {
            return string.Format("<{0}, {1}>", First, Second);
        }

        public static bool operator ==(Tuple<T1, T2> _a, Tuple<T1, T2> _b)
        {
            if (Tuple<T1, T2>.IsNull(_a) && !Tuple<T1, T2>.IsNull(_b))
                return false;

            if (!Tuple<T1, T2>.IsNull(_a) && Tuple<T1, T2>.IsNull(_b))
                return false;

            if (Tuple<T1, T2>.IsNull(_a) && Tuple<T1, T2>.IsNull(_b))
                return true;

            return
                _a.First.Equals(_b.First) &&
                _a.Second.Equals(_b.Second);
        }

        public static bool operator !=(Tuple<T1, T2> _a, Tuple<T1, T2> _b)
        {
            return !(_a == _b);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + First.GetHashCode();
            hash = hash * 23 + Second.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Tuple<T1, T2>;
            if (object.ReferenceEquals(other, null))
                return false;
            else
                return Item1Comparer.Equals(First, other.First) &&
                       Item2Comparer.Equals(Second, other.Second);
        }

        private static bool IsNull(object obj)
        {
            return object.ReferenceEquals(obj, null);
        }
    }

    private static Random m_RandomIDHelper = new System.Random((int)DateTime.Now.Ticks);
    private static string GenID()
    {
        var sb = new StringBuilder();
        sb.Append(m_RandomIDHelper.Next());
        sb.Append(DateTime.Now.Ticks);
        return sb.ToString();
    }

    private static List<T> ToList<T>(IEnumerable<T> _enumerable)
    {
        return new List<T>(_enumerable);
    }

    private static bool Contains<T>(IEnumerable<T> _array, T _toCheck)
    {
        foreach (var item in _array)
            if (item.Equals(_toCheck)) return true;
        return false;
    }

    private static Type GetTypeEx(string TypeName)
    {

        // Try Type.GetType() first. This will work with types defined
        // by the Mono runtime, in the same assembly as the caller, etc.
        var type = Type.GetType(TypeName);

        // If it worked, then we're done here
        if (type != null)
            return type;

        /*
        // If the TypeName is a full name, then we can try loading the defining assembly directly
        if (TypeName.Contains("."))
        {

            // Get the name of the assembly (Assumption is that we are using 
            // fully-qualified type names)
            var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));

            // Attempt to load the indicated Assembly
            var assembly = Assembly.Load(assemblyName);
            if (assembly == null)
                return null;

            // Ask that assembly to return the proper Type
            type = assembly.GetType(TypeName);
            if (type != null)
                return type;

        }*/

        // If we still haven't found the proper type, we can enumerate all of the 
        // loaded assemblies and see if any of them define the type
        var currentAssembly = Assembly.GetExecutingAssembly();
        var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {

            if (assembly != null)
            {
                // See if that assembly defines the named type
                type = assembly.GetType(TypeName);
                if (type != null)
                    return type;
            }
        }

        // The type just couldn't be found...
        return null;

    }
}

/// <summary>
/// Math support methods.
/// </summary>
public static partial class ioMath
{
    /// <summary>
    /// Returns radians equivalent of _degrees.
    /// </summary>
    /// <param name="_degrees"></param>
    /// <returns></returns>
    public static float ToRadians(float _degrees) { return (_degrees / 360f) * 2 * (float)Math.PI; }

    /// <summary>
    /// Returns degrees equivalent of _radians.
    /// </summary>
    /// <param name="_radians"></param>
    /// <returns></returns>
    public static float ToDegrees(float _radians) { return _radians * 360 / (2 * (float)Math.PI); }

    /// <summary>
    /// Floating point equality check with precision.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="epsilon"></param>
    /// <returns></returns>
    public static bool EqualsApprox(float a, float b, float epsilon = 0.0001f)
    {
        float absA = Math.Abs(a);
        float absB = Math.Abs(b);
        float diff = Math.Abs(a - b);

        if (a == b)
            return true;
        else if (a == 0 || b == 0 || diff < float.Epsilon)
            return diff < epsilon;
        else
            return diff / (absA + absB) < epsilon;
    }

    /// <summary>
    /// Linearly interpolate line defined by specified points at specified percent.  Does not clamp.
    /// </summary>
    /// <param name="_a">Line definition point A</param>
    /// <param name="_b">Line definition point B</param>
    /// <param name="_pct">Percent target for interpolation</param>
    /// <returns>Lerp result</returns>
    public static float Lerp(float _a, float _b, float _pct)
    {
        return _a + (_b - _a) * _pct;
    }

}

