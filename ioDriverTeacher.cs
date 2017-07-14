using System;
using System.Collections.Generic;
using System.Reflection;

public static partial class ioDriver
{
    /// <summary>
    /// Class containing data for teaching operations required for various ioDriver functionality.
    /// </summary>
    /// Automatically teaches info for built-in types float, int, double, etc.
    /// If used in Unity, also teaches Unity types, Vector2, Vector3, Vector4 and Color.
    public static class Teacher
    {
        private static bool m_InitDone = false;

        static Teacher() { ioDriver.Init(); }

        private static Dictionary<Type, HashSet<TeachType>> m_TaughtTypes;

        /// <summary>
        /// Get a dictionary containing sets of known taught functions indexed by Type.
        /// </summary>
        public static Dictionary<Type, HashSet<TeachType>> TaughtTypes
        {
            get
            {
                return m_TaughtTypes == null ? new Dictionary<Type, HashSet<TeachType>>() : 
                    new Dictionary<Type, HashSet<TeachType>>(m_TaughtTypes);
            }
        } 

        internal static void Init()
        {
            if (m_InitDone) return;

            m_TaughtTypes = new Dictionary<Type, HashSet<TeachType>>();

            Teach((byte)0,
                (_a, _b) => (byte)(_a + _b),
                (_a, _b) => Math.Abs(_b - _a),
                (_from, _to, _pct) => (byte)(_from + System.Math.Round((float)(_to - _from) * _pct)),
                (_from, _to, _val) => (float)(_val - _from) / (float)(_to - _from));
            TeachCoord<byte>(1, _val => (byte)_val[0], _val => _val);

            Teach((sbyte)0,
                (_a, _b) => (sbyte)(_a + _b),
                (_a, _b) => Math.Abs(_b - _a),
                (_from, _to, _pct) => (sbyte)(_from + System.Math.Round((float)(_to - _from) * _pct)),
                (_from, _to, _val) => (float)(_val - _from) / (float)(_to - _from));
            TeachCoord<byte>(1, _val => (byte)_val[0], _val => _val);

            Teach((short)0,
                (_a, _b) => (short)(_a + _b),
                (_a, _b) => Math.Abs(_b - _a),
                (_from, _to, _pct) => (short)(_from + System.Math.Round((float)(_to - _from) * _pct)),
                (_from, _to, _val) => (float)(_val - _from) / (float)(_to - _from));
            TeachCoord<short>(1, _val => (short)_val[0], _val => _val);

            Teach((ushort)0,
                (_a, _b) => (ushort)(_a + _b),
                (_a, _b) => Math.Abs(_b - _a),
                (_from, _to, _pct) => (ushort)(_from + System.Math.Round((float)(_to - _from) * _pct)),
                (_from, _to, _val) => (float)(_val - _from) / (float)(_to - _from));
            TeachCoord<ushort>(1, _val => (ushort)_val[0], _val => _val);

            Teach((int)0,
                (_a, _b) => _a + _b,
                (_a, _b) => Math.Abs(_b - _a),
                (_from, _to, _pct) => (int)(_from + System.Math.Round((_to - _from) * _pct)),
                (_from, _to, _val) => (float)(_val - _from) / (float)(_to - _from));
            TeachCoord<int>(1, _val => (int)_val[0], _val => _val);

            Teach((uint)0,
                (_a, _b) => (uint)(_a + _b),
                (_a, _b) => Math.Abs(_b - _a),
                (_from, _to, _pct) => (uint)(_from + System.Math.Round((float)(_to - _from) * _pct)),
                (_from, _to, _val) => (float)(_val - _from) / (float)(_to - _from));
            TeachCoord<uint>(1, _val => (uint)_val[0], _val => _val);

            Teach((long)0,
                (_a, _b) => (long)(_a + _b),
                (_a, _b) => Math.Abs(_b - _a),
                (_from, _to, _pct) => (long)(_from + System.Math.Round((float)(_to - _from) * _pct)),
                (_from, _to, _val) => (float)(_val - _from) / (float)(_to - _from));
            TeachCoord<long>(1, _val => (long)_val[0], _val => _val);

            Teach((ulong)0,
                (_a, _b) => (ulong)(_a + _b),
                (_a, _b) => (float)Math.Abs((double)(_b - _a)),
                (_from, _to, _pct) => (ulong)(_from + System.Math.Round((float)(_to - _from) * _pct)),
                (_from, _to, _val) => (float)(_val - _from) / (float)(_to - _from));
            TeachCoord<ulong>(1, _val => (ulong)_val[0], _val => _val);

            Teach((float)0,
                (_a, _b) => _a + _b,
                (_a, _b) => Math.Abs(_b - _a),
                (_from, _to, _pct) => (_from + (_to - _from) * _pct),
                (_from, _to, _val) => (_val - _from) / (_to - _from));
            TeachCoord<float>(1, _val => _val[0], _val => _val);

            Teach((double)0,
                (_a, _b) => (double)(_a + _b),
                (_a, _b) => Math.Abs((float)(_b - _a)),
                (_from, _to, _pct) => (_from + (_to - _from) * _pct),
                (_from, _to, _val) => (float)((_val - _from) / (_to - _from)));
            TeachCoord<double>(1, _val => (double)_val[0], _val => (float)_val);


            Teach((decimal)0,
                (_a, _b) => _a + _b,
                (_a, _b) => Math.Abs((float)(_b - _a)),
                (_from, _to, _pct) => (_from + (_to - _from) * (decimal)_pct),
                (_from, _to, _val) => (float)((_val - _from) / (_to - _from)));
            
            TeachCoord<decimal>(1, _val => (decimal)_val[0], _val => (float)_val);

            m_InitDone = true;
        }

        /// <summary>
        /// Basic LERP float function: (_from + (_to - _from) * _pct)
        /// </summary>
        /// <param name="_from"></param>
        /// <param name="_to"></param>
        /// <param name="_pct"></param>
        /// <returns></returns>
        public static float Lerpf(float _from, float _to, float _pct) { return (_from + (_to - _from) * _pct); }
        
        /// <summary>
        /// Basic ILERP float function: (_val - _from) / (_to - _from)
        /// </summary>
        /// <param name="_from"></param>
        /// <param name="_to"></param>
        /// <param name="_val"></param>
        /// <returns></returns>
        public static float ILerpf(float _from, float _to, float _val) { return (_val - _from) / (_to - _from); }
        
        /// <summary>
        /// Delegate for getting T object's zero equivalent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public delegate T FuncZero<out T>();

        /// <summary>
        /// Type construction function.
        /// </summary>
        /// <example>_vals => new Vector2(_vals[0], _vals[1])</example>
        /// <typeparam name="T">Type to construct</typeparam>
        /// <param name="_vals">float values to use for construction.</param>
        /// <returns>Constructed object</returns>
        public delegate T FuncConstruct<out T>(params float[] _vals);

        /// <summary>
        /// Dimension retrieval function.
        /// </summary>
        /// <example>
        /// //(retrieve Vector2 x value) 
        /// _vec => _vec.x
        /// </example>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="_obj">Object to retrieve from.</param>
        /// <returns>Dimension data (float)</returns>
        public delegate float FuncGetDim<in T>(T _obj);

        /// <summary>
        /// Type addition function.
        /// </summary>
        /// <example>
        /// //Vector3 example
        /// (_a, _b) => _a + _b;
        /// </example>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="_a">Operand A</param>
        /// <param name="_b">Operand B</param>
        /// <returns>Result of addition</returns>
        public delegate T FuncAdd<T>(T _a, T _b);

        /// <summary>
        /// Type length function.
        /// </summary>
        /// <example>
        /// //Vector3 example
        /// (_a, _b) => (_b - _a).magnitude;
        /// </example>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="_a">Operand A</param>
        /// <param name="_b">Operand B</param>
        /// <returns>Length between A and B</returns>
        public delegate float FuncLength<in T>(T _a, T _b);

        /// <summary>
        /// Lerp between two objects of type T.
        /// </summary>
        /// <example>
        /// // Vector3 example
        /// (_from, _to, _pct) => {
        ///    var x = Teacher.Lerpf(_from.x, _to.x, _pct);
        ///    var y = Teacher.Lerpf(_from.y, _to.y, _pct);
        ///    var z = Teacher.Lerpf(_from.z, _to.z, _pct);
        ///    return new Vector3(x, y, z);
        /// };
        /// </example>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="_from">Lerp from object</param>
        /// <param name="_to">Lerp to object</param>
        /// <param name="_pct">Percent to Lerp (1.0 = 100%)</param>
        /// <returns>Result of Lerp</returns>
        public delegate T FuncLerp<T>(T _from, T _to, float _pct);

        /// <summary>
        /// Inverse Lerp two objects of type T.  Get percentage an object is linearly relative to a start and end object. (Implementation result should be clearly understood)
        /// </summary>
        /// <example>
        /// // Vector3 example:
        /// (_from, _to, _val) => {
        ///    float from = _from.x;
        ///    float to = _to.x;
        ///    float val = _val.x;
        ///    if (_from.x == _to.x)
        ///        if (_from.y == _to.y)
        ///        {
        ///            from = _from.z;
        ///            to = _to.z;
        ///            val = _val.z;
        ///        }
        ///        else
        ///        {
        ///            from = _from.y;
        ///            to = _to.y;
        ///            val = _val.y;
        ///        }
        ///    if (from == to) return 0;
        ///    return (from - val)/(from - to);
        /// };
        /// </example>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="_from">Inverse Lerp from object</param>
        /// <param name="_to">Inverse Lerp to object</param>
        /// <param name="_val">Value to inverse lerp</param>
        /// <returns></returns>
        public delegate float FuncILerp<T>(T _from, T _to, T _val);

        /// <summary>
        /// Teach ioDriver how to use type T.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="_zero">Zero equivalent of Type T</param>
        /// <param name="_add">Addition function for type T</param>
        /// <param name="_length">Length function for type T</param>
        /// <param name="_lerp">LERP function for type T</param>
        /// <param name="_iLerp">ILERP function for type T</param>
        public static void Teach<T>(T _zero, Teacher.FuncAdd<T> _add, Teacher.FuncLength<T> _length, Teacher.FuncLerp<T> _lerp, Teacher.FuncILerp<T> _iLerp)
        {
            DTypeInfo<T>.ZeroFunc = () => _zero;
            DTypeInfo<T>.Zero = _zero;
            DTypeInfo<T>.Add = _add;
            DTypeInfo<T>.Length = _length;
            DTypeInfo<T>.Lerp = _lerp;
            DTypeInfo<T>.ILerp = _iLerp;

            if (m_TaughtTypes == null)
                ioDriver.Init();
            if (!m_TaughtTypes.ContainsKey(typeof (T)))
                m_TaughtTypes.Add(typeof (T), new HashSet<TeachType>());

            m_TaughtTypes[typeof(T)].Add(TeachType.Zero);
            m_TaughtTypes[typeof(T)].Add(TeachType.Add);
            m_TaughtTypes[typeof(T)].Add(TeachType.Length);
            m_TaughtTypes[typeof(T)].Add(TeachType.Lerp);
            m_TaughtTypes[typeof(T)].Add(TeachType.ILerp);
        }

        /// <summary>
        /// Teach ioDriver how to create equivalent zero value of type T.
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="_zero">Zero equivalent value of T</param>
        public static void TeachZero<T>(T _zero)
        {
            DTypeInfo<T>.ZeroFunc = () => _zero;
            DTypeInfo<T>.Zero = _zero;

            if (m_TaughtTypes == null)
                ioDriver.Init();
            if (!m_TaughtTypes.ContainsKey(typeof(T)))
                m_TaughtTypes.Add(typeof(T), new HashSet<TeachType>());
            m_TaughtTypes[typeof(T)].Add(TeachType.Zero);
        }

        /// <summary>
        /// Teach ioDriver perform addition operation on type T.
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="_addFunc">Function containing addition operation for type T.</param>
        public static void TeachAdd<T>(Teacher.FuncAdd<T> _addFunc)
        {
            DTypeInfo<T>.Add = _addFunc;

            if (m_TaughtTypes == null)
                ioDriver.Init();
            if (!m_TaughtTypes.ContainsKey(typeof(T)))
                m_TaughtTypes.Add(typeof(T), new HashSet<TeachType>());
            m_TaughtTypes[typeof(T)].Add(TeachType.Add);
        }

        /// <summary>
        /// Teach ioDriver how to calculate relative length between two values of type T.
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="_lengthFunc">Function containing length operation for type T.</param>
        public static void TeachLength<T>(Teacher.FuncLength<T> _lengthFunc)
        {
            DTypeInfo<T>.Length = _lengthFunc;

            if (m_TaughtTypes == null)
                ioDriver.Init();
            if (!m_TaughtTypes.ContainsKey(typeof(T)))
                m_TaughtTypes.Add(typeof(T), new HashSet<TeachType>());
            m_TaughtTypes[typeof(T)].Add(TeachType.Length);
        }

        /// <summary>
        /// Teach ioDriver how to Lerp type T.
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="_lerpFunc">Function containing LERP opertion for type T</param>
        public static void TeachLerp<T>(Teacher.FuncLerp<T> _lerpFunc)
        {
            DTypeInfo<T>.Lerp = _lerpFunc;

            if (m_TaughtTypes == null)
                ioDriver.Init();
            if (!m_TaughtTypes.ContainsKey(typeof(T)))
                m_TaughtTypes.Add(typeof(T), new HashSet<TeachType>());
            m_TaughtTypes[typeof(T)].Add(TeachType.Lerp);
        }

        /// <summary>
        /// Teach ioDriver how to inverse LERP type T.
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="_iLerpFunc">Function containing ILERP operation for type T</param>
        public static void TeachILerp<T>(Teacher.FuncILerp<T> _iLerpFunc)
        {
            DTypeInfo<T>.ILerp = _iLerpFunc;

            if (m_TaughtTypes == null)
                ioDriver.Init();
            if (!m_TaughtTypes.ContainsKey(typeof(T)))
                m_TaughtTypes.Add(typeof(T), new HashSet<TeachType>());
            m_TaughtTypes[typeof(T)].Add(TeachType.ILerp);
        }

        /// <summary>
        /// Teach ioDriver how to create instances and read dimensional data from type T.
        /// <seealso cref="TeachCoord{T}(int,FuncConstruct{T},FuncGetDim{T}[])"/>
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="_constructs">Dictionary of constructor functions, keyed by number of dimensions</param>
        /// <param name="_getFuncs">Params of get dimension functions</param>
        public static void TeachCoord<T>(Dictionary<int, FuncConstruct<T>> _constructs, params FuncGetDim<T>[] _getFuncs)
        {
            var maxDimCount = 0;
            foreach (var dimCount in _constructs.Keys)
            {
                if (dimCount > _getFuncs.Length)
                    throw new Exception("Received constructor dim count greater than number of get dims functions.");
                if (maxDimCount < dimCount) maxDimCount = dimCount;
            }

            if (maxDimCount < _getFuncs.Length)
                throw new Exception(
                    "Did not receive constructor that handles all provided dim funcs. Max Constructor Dim: " +
                    maxDimCount + " Get Dim function count: " + _getFuncs.Length);

            // Teach Constructors
            foreach (var pair in _constructs)
                DTypeInfo<T>.SetConstruct(pair.Value, pair.Key);

            // Teach dimension data retrieval 
            DTypeInfo<T>.GetDimsFunc = new FuncGetDim<T>[_getFuncs.Length + 1];
            _getFuncs.CopyTo(DTypeInfo<T>.GetDimsFunc, 1);

            if (m_TaughtTypes == null)
                ioDriver.Init();
            if (!m_TaughtTypes.ContainsKey(typeof(T)))
                m_TaughtTypes.Add(typeof(T), new HashSet<TeachType>());
            m_TaughtTypes[typeof(T)].Add(TeachType.Coord);
        }

        /// <summary>
        /// Teach ioDriver how to create instances and read dimensional data from type T.
        /// <seealso cref="TeachCoord{T}(Dictionary{int,FuncConstruct{T}},FuncGetDim{T}[])"/>
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="_constrDims">Number of dimensions in the constructor</param>
        /// <param name="_constrFunc">Constructor function</param>
        /// <param name="_getFuncs">Params of get dimension functions</param>
        public static void TeachCoord<T>(int _constrDims, FuncConstruct<T> _constrFunc, params FuncGetDim<T>[] _getFuncs)
        {
            var constrs = new Dictionary<int, FuncConstruct<T>> { { _constrDims, _constrFunc } };
            TeachCoord(constrs, _getFuncs);

        }

        /// <summary>
        /// Retrieve a list of known Teacher functions for specified type.
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        public static List<TeachType> GetKnownTaughtFrom(Type _type)
        {
            var known = new List<TeachType>();

            if (_type.ContainsGenericParameters) return known;

            var dType = typeof(DTypeInfo<>);
            var type = dType.MakeGenericType(new[] { _type });

            var add = type.GetField("Add", BindingFlags.Public | BindingFlags.Static);
            var length = type.GetField("Length", BindingFlags.Public | BindingFlags.Static);
            var lerp = type.GetField("Lerp", BindingFlags.Public | BindingFlags.Static);
            var ilerp = type.GetField("ILerp", BindingFlags.Public | BindingFlags.Static);
            var zero = type.GetField("ZeroFunc", BindingFlags.Public | BindingFlags.Static);
            var getDims = type.GetField("GetDimsFunc", BindingFlags.Public | BindingFlags.Static);
            var constructs = type.GetField("Constructs", BindingFlags.Public | BindingFlags.Static);

            //TODO change when working
            try
            {
                if (add.GetValue(type) != null) known.Add(TeachType.Add);
                if (length.GetValue(type) != null) known.Add(TeachType.Length);
                if (lerp.GetValue(type) != null) known.Add(TeachType.Lerp);
                if (ilerp.GetValue(type) != null) known.Add(TeachType.ILerp);
                if (zero.GetValue(type) != null) known.Add(TeachType.Zero);
            }
            catch (Exception _e)
            {
                Log.Err("Exception on type: " + type.ToString() + " | Exception: " + _e.ToString());
            }

            var dimsLen = ((Array)getDims.GetValue(type)).Length;
            var constLen = ((Array)constructs.GetValue(type)).Length;
            if (dimsLen > 1 && constLen > 1)
                known.Add(TeachType.Coord);

            return known;

            /*
            public static T Zero; //Needed for Step, Rate and Conditioner
            public static Teacher.FuncAdd<T> Add;//Needed for DStep
            public static Teacher.FuncLength<T> Length; //Needed for Speed
            public static Teacher.FuncLerp<T> Lerp; //Needed for path, Rate, Step, Mapped, Tween
            public static Teacher.FuncILerp<T> ILerp; //Needed for Mapped, Tween
            public static Teacher.FuncGetDim<T>[] GetDimsFunc = new Teacher.FuncGetDim<T>[1];
            public static Teacher.FuncConstruct<T>[] Constructs = new Teacher.FuncConstruct<T>[1];
             * */
        }

        /// <summary>
        /// Teachable function types
        /// </summary>
        public enum TeachType
        {
            /// See <see cref="FuncAdd{T}"/>
            Add,
            /// See <see cref="FuncConstruct{T}"/> and <see cref="FuncGetDim{T}"/>
            Coord,
            /// See <see cref="FuncILerp{T}"/>
            ILerp,
            /// See <see cref="FuncLength{T}"/>
            Length,
            /// See <see cref="FuncLerp{T}"/>
            Lerp,
            /// See <see cref="FuncZero{T}"/>
            Zero
        }

    }


    private static class DTypeInfo<T>
    {
        public static T Zero;
        public static Teacher.FuncZero<T> ZeroFunc; //Needed for Step, Rate and Conditioner
        public static Teacher.FuncAdd<T> Add;//Needed for DStep
        public static Teacher.FuncLength<T> Length; //Needed for Speed
        public static Teacher.FuncLerp<T> Lerp; //Needed for path, Rate, Step, Mapped, Tween
        public static Teacher.FuncILerp<T> ILerp; //Needed for Mapped, Tween
        public static Teacher.FuncGetDim<T>[] GetDimsFunc = new Teacher.FuncGetDim<T>[1];
        public static Teacher.FuncConstruct<T>[] Constructs = new Teacher.FuncConstruct<T>[1];

        public static bool IsValid { get { return Lerp != null; } }

        public static T LerpExt(T _from, T _to, float _pct, float _clampPctMin, float _clampPctMax)
        {
            if (_pct > _clampPctMax)
                _pct = _clampPctMax;
            if (_pct < _clampPctMin)
                _pct = _clampPctMin;
            return Lerp(_from, _to, _pct);
        }

        public static void SetConstruct(Teacher.FuncConstruct<T> _constFunc, int _paramCount)
        {
            if (Constructs.Length <= _paramCount)
            {
                var newConstructs = new Teacher.FuncConstruct<T>[_paramCount + 1];
                Constructs.CopyTo(newConstructs, 0);
                Constructs = newConstructs;
            }
            Constructs[_paramCount] = _constFunc;
        }

        public static bool IsConstructableDim(int _dim) { return Constructs.Length + 1 == _dim; }
        public static int[] ConstructableDims
        {
            get
            {
                var dimList = new List<int>();
                for (int dim = 1; dim < Constructs.Length; ++dim)
                    if (Constructs[dim] != null)
                        dimList.Add(dim);
                return dimList.ToArray();
            }
        }
        public static int DimCount { get { return GetDimsFunc.Length - 1; } }
        public static bool IsGettableDim(int _dim) { return (_dim >= 1 && _dim <= GetDimsFunc.Length); }

        public static int[] GettableDims
        {
            get
            {
                List<int> gettableDims = new List<int>();
                for (int dim = 1; dim <= DimCount; ++dim)
                    gettableDims.Add(dim);
                return gettableDims.ToArray();
            }

        }
    }


}