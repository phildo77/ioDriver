using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

public static partial class ioDriver
{

    /// <summary>
    /// Class that allows chaining and branching of multiple drivers.
    /// </summary>
    public class DChain
    {
        private readonly List<ioDriver.DBase> m_Chain = new List<ioDriver.DBase>();
        private readonly Dictionary<ioDriver.DBase, List<DChain>> m_Branches = new Dictionary<ioDriver.DBase, List<DChain>>();

        /// Create empty driver chain.
        public DChain() { }

        /// Create driver chain with one driver.
        public DChain(ioDriver.DBase _driver) { m_Chain.Add(_driver); }

        /// Create driver chain series from list of drivers.
        public DChain(List<ioDriver.DBase> _chain) { m_Chain = _chain; }

        /// Create driver chain series from params
        public DChain(params ioDriver.DBase[] _chain) { m_Chain = ToList(_chain); }

        /// Add driver to end of chain
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

        /// Start this chain
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
    /// Returns radians equivalent of _degrees.
    public static float ToRadians(float _degrees) { return (_degrees / 360f) * 2 * (float)Math.PI; }
    /// Returns degrees equivalent of _radians.
    public static float ToDegrees(float _radians) { return _radians * 360 / (2 * (float)Math.PI); }

    /// Floating point equality check with precision.
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

    public static float Lerp(float _a, float _b, float _pct)
    {
        return _a + (_b - _a) * _pct;
    }

}

