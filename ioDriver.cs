using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;


/// <summary>
/// Drive or tween almost anything!
/// </summary>
public static partial class ioDriver
{
    #region Fields


    private static Dictionary<string, string> DriverIDsByName = new Dictionary<string, string>();
    private static Dictionary<object, HashSet<string>> DriverIDsByObject = new Dictionary<object, HashSet<string>>
    {
        {TargetInfo.TARGET_NONE, new HashSet<string>()}
    };
    private static Dictionary<object, HashSet<string>> DriverIDsByTag = new Dictionary<object, HashSet<string>>();
    private static Dictionary<string, DBase> DriversByID = new Dictionary<string, DBase>();
    private static HashSet<DBase> DriversDebugTracked = new HashSet<DBase>();
    private static Dictionary<object, Func<float, float>> m_CustomEaseFuncs = new Dictionary<object, Func<float, float>>();
    private static bool m_DebugEnableGlobal = false;

    private static Type m_UnityEngine;
    private static bool? m_UnityPresent = null;
    private static bool m_UnityMgrPresent = false;

    private static float m_MaxUpdateFrequency = Defaults.MaxUpdateFrequency;
    private static float m_TimescaleGlobal = Defaults.Timescale;

    #endregion Fields


    #region Constructors

    static ioDriver()
    {
        Init();
    }

    #endregion Constructors

    #region Enumerations

    /// <summary>
    /// Set driver's behaviour when a conflict is detected.
    /// </summary>
    /// NOTE:  Conflict detection is performed only once, when the driver is started.
    public enum ConflictBehavior
    {
        /// Ignore any conflicts and run drivers in parallel.
        Ignore,
        /// Replace will cancel the currently running driver and replace it with the new one.
        Replace,
        /// Cancel will immediately dispose the new driver leaving the current one in place.
        Cancel
    }

    /// <summary>
    /// Easing Functions based on Robert Penner's Easing Functions.<br/>
    /// http://robertpenner.com/easing/ <br/>
    /// Licence: http://robertpenner.com/easing_terms_of_use.html <br/>
    /// Cheat sheet: http://easings.net/ <br/>
    ///
    /// TERMS OF USE - EASING EQUATIONS<br/>
    ///
    /// Open source under the BSD License.<br/>
    ///
    /// Copyright © 2001 Robert Penner<br/>
    /// All rights reserved.<br/>
    ///
    /// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:<br/>
    ///
    ///   Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.<br/>
    ///   Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.<br/>
    ///   Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.<br/>
    ///
    /// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
    /// </summary>
    public enum EaseType : byte
    {
        /// Ease with Linear Interpolation
        Linear,
        /// Ease with EaseInQuad
        EaseInQuad,
        /// Ease with EaseOutQuad
        EaseOutQuad,
        /// Ease with EaseInOutQuad
        EaseInOutQuad,
        /// Ease with EaseInCubic
        EaseInCubic,
        /// Ease with EaseOutCubic
        EaseOutCubic,
        /// Ease with EaseInOutCubic
        EaseInOutCubic,
        /// Ease with EaseInQuart
        EaseInQuart,
        /// Ease with EaseOutQuart
        EaseOutQuart,
        /// Ease with EaseInOutQuart
        EaseInOutQuart,
        /// Ease with EaseInQuint
        EaseInQuint,
        /// Ease with EaseOutQuint
        EaseOutQuint,
        /// Ease with EaseInOutQuint
        EaseInOutQuint,
        /// Ease with EaseInSine
        EaseInSine,
        /// Ease with EaseOutSine
        EaseOutSine,
        /// Ease with EaseInOutSine
        EaseInOutSine,
        /// Ease with EaseInExpo
        EaseInExpo,
        /// Ease with EaseOutExpo
        EaseOutExpo,
        /// Ease with EaseInOutExpo
        EaseInOutExpo,
        /// Ease with EaseInCirc
        EaseInCirc,
        /// Ease with EaseOutCirc
        EaseOutCirc,
        /// Ease with EaseInOutCirc
        EaseInOutCirc,
        /// Ease with EaseInBounce
        EaseInBounce,
        /// Ease with EaseOutBounce
        EaseOutBounce,
        /// Ease with EaseInOutBounce
        EaseInOutBounce,
        /// Ease with EaseInBack
        EaseInBack,
        /// Ease with EaseOutBack
        EaseOutBack,
        /// Ease with EaseInOutBack
        EaseInOutBack,
        /// Ease with EaseInElastic
        EaseInElastic,
        /// Ease with EaseOutElastic
        EaseOutElastic,
        /// Ease with EaseInOutElastic
        EaseInOutElastic,
        /// Ease with <see cref="Easing.DefineCustomEasingFunc">user defined easing function</see>
        Custom
    }

    /// Immediate mode events for <see cref="DBase"/>.  
    /// For use with <see cref="ioDriverFluency.SetAddEvent{T}(T,ioDriver.Events,ioDriver.Event.Handler{T})"/>
    /// <seealso cref="DBase.OnStart"/><seealso cref="DBase.OnFinish"/><seealso cref="DBase.OnCancel"/>
    /// <seealso cref="DBase.OnPause"/><seealso cref="DBase.OnUnpause"/><seealso cref="DBase.OnPauseToggle"/>
    public enum Events
    {
        /// See <see cref="DBase.OnStart"/>
        OnStart,
        /// See <see cref="DBase.OnFinish"/>
        OnFinish,
        /// See <see cref="DBase.OnCancel"/>
        OnCancel,
        /// See <see cref="DBase.OnPause"/>
        OnPause,
        /// See <see cref="DBase.OnUnpause"/>
        OnUnpause,
        /// See <see cref="DBase.OnPauseToggle"/>
        OnPauseToggle
    }

    /// Immediate mode events for <see cref="ILoopable"/>
    /// For use with <see cref="ioDriverFluency.SetAddEvent{T}(T,ioDriver.EventsLoop,ioDriver.Event.Handler{T})"/>
    /// <seealso cref="ILoopable.OnLoopCycleComplete"/><seealso cref="ILoopable.OnLoopPingPongForeComplete"/>
    /// <seealso cref="ILoopable.OnLoopPingPongBackComplete"/>
    public enum EventsLoop
    {
        /// See <see cref="ILoopable.OnLoopCycleComplete"/>
        OnLoopCycleComplete,
        /// See <see cref="ILoopable.OnLoopPingPongForeComplete"/>
        OnLoopPingPongForeComplete,
        /// See <see cref="ILoopable.OnLoopPingPongBackComplete"/>
        OnLoopPingPongBackComplete
    }

    /// Used for <see cref="ILoopable"/> drivers.
    public enum LoopType
    {
        /// Perform driver with one cycle over set <see cref="DBase.Duration"/>.
        Once,
        /// Repeat driver action over specified cycle count or cycle time over Duration.  With repeats starting over at start point.
        Repeat,
        /// Ping Pong driver action over specified cycle count or cycle time over duration.  With repeats starting over at last reached end point of cycle.
        PingPong,
    }

    /// Ping Pong loop specific progression direction.
    public enum PingPongDir
    {
        /// Loop's cycle progress is moving from 0 to 1.
        Forward,
        /// Loop's cycle progress is moving from 1 to 0.
        Backward
    }

    #endregion Enumerations


    /// Injector delegate used for <see cref="DTransfer{TTar,TDri}.TargetInjector"/> 
    /// and <see cref="DTransfer{TTar,TDri}.DriveInjector"/>
    public delegate T Injector<T>(T _value);

    /// Delegate used for <see cref="OnPump"/> event.
    public delegate void OnPumpAction();

    /// Called every "pump" operation.  Note that this is not fired if not enough time has passed to meet
    /// the current <see cref="MaxUpdateFrequency"/> setting.
    public static event OnPumpAction OnPump;

    #region Nested Interfaces

    /// Interface for loopable drivers.
    /// <seealso cref="ioDriver.DTween{TTar}"/><seealso cref="ioDriver.DSpeed{TTar}"/>
    public interface ILoopable
    {
        #region Events

        /// Fired when each loop cycle reaches its loop duration
        event Action OnLoopCycleComplete;

        /// Fired when loop cycle reaches its loop duration
        /// if loop type is <see cref="LoopType.PingPong"/> and 
        /// ping pong mode is <see cref="PingPongDir.Backward"/>
        event Action OnLoopPingPongBackComplete;

        /// Fired when loop cycle reaches its loop duration
        /// if loop type is <see cref="LoopType.PingPong"/> and 
        /// ping pong mode is <see cref="PingPongDir.Forward"/>
        event Action OnLoopPingPongForeComplete;

        #endregion Events

        #region Properties

        /// Time in seconds passed in current loop cycle.
        float ElapsedCycleTime
        {
            get;
        }

        /// Total loop cycle count this driver is set to run.
        float LoopCycleCount
        {
            get;
        }

        /// Progress in current cycle.
        float LoopCycleProgress
        {
            get;
        }

        /// Get the current loop type. <seealso cref="LoopType"/>
        LoopType loopType
        {
            get;
        }

        /// Get the current ping pong direction.  <seealso cref="PingPongDir"/>
        PingPongDir PingPongDirection
        {
            get;
        }

        /// Total number of loops this driver has already run.
        float TotalLoopingProgress
        {
            get;
        }

        #endregion Properties

        #region Methods

        /// Set this driver to <see cref="LoopType.Once"/>
        void LoopOnce();

        /// <summary>
        /// Set this driver to <see cref="LoopType.PingPong"/>
        /// </summary>
        /// <param name="_loopCycleCount">Number of cycles to run.</param>
        /// <param name="_startPct">Percent value to start at.</param>
        void LoopPingPong(float _loopCycleCount, float _startPct = 0f);

        /// <summary>
        /// Set this driver to <see cref="LoopType.Repeat"/>
        /// </summary>
        /// <param name="_loopCycleCount">Number of cycles to run.</param>
        /// <param name="_startPct">Percent value to start at.</param>
        void LoopRepeat(float _loopCycleCount, float _startPct = 0f);

        #endregion Methods
    }

    /// Untyped interface for <see cref="DTransfer{TTar, TDri}"/>
    public interface ITransfer
    {
        #region Properties

        /// Get type of drive
        Type DriveType
        {
            get;
        }

        /// Get current drive value.
        object DriveValCurrent
        {
            get;
        }

        /// See <see cref="DTransfer{TTar,TDri}.TargetExpr"/>
        string TargetExpr
        {
            get;
        }

        /// Get the target type
        Type TargetType
        {
            get;
        }

        /// Current target vlue
        object TargetValCurrent
        {
            get;
        }

        #endregion Properties
    }

    #endregion Nested Interfaces

    #region Properties

    /// When set to true ALL drivers are debug tracked and will run in Debug mode.  <see cref="DBase.UpdateDebug()"/>
    public static bool DebugEnableGlobal
    {
        get { return m_DebugEnableGlobal; }
        set
        {
            if (m_DebugEnableGlobal == value) return;
            m_DebugEnableGlobal = value;
            if (m_DebugEnableGlobal)
            {
                foreach (var driver in DriversByID.Values)
                    DriversDebugTracked.Add(driver);
            }
            else
            {
                var toRemove = new HashSet<DBase>();
                foreach (var driver in DriversDebugTracked)
                    if (!(driver as DBase).DebugEnable)
                        toRemove.Add(driver);
                foreach (var driver in toRemove)
                    DriversDebugTracked.Remove(driver);
            }

        }
    }

    /// Has the ioDriver initialization code been executed?
    public static bool InitDone
    {
        get;
        private set;
    }

    /// <summary>
    /// Seconds since ioDriver was first initialized.
    /// </summary>
    public static float SecondsSinceStart
    {
        get { return Hooks.SecsSinceStart(); }
    }

    public static float TimescaleGlobal
    {
        get { return m_TimescaleGlobal; }
        set
        {
            if (m_TimescaleGlobal == value) return;
            if (value < 0)
            {
                Log.Err("Global Timescale cannot be less than 0.  Setting default of '" + Defaults.Timescale + "'");
                m_TimescaleGlobal = Defaults.Timescale;
                return;
            }
            m_TimescaleGlobal = value;

        }
    }

    /// Get or set the maximum frequency for ioDriver to pump all running drivers and fire events.
    /// IE. if the maximum frequency is set to 60 (Hz) then drivers and events will only be updated at a maximum
    /// of 60 times per second, or maximum once every 16.67 milliseconds.  Cannot be less than or equal to zero.
    public static float MaxUpdateFrequency
    {
        get { return m_MaxUpdateFrequency; }
        set
        {
            var val = value;
            if (val <= 0)
            {
                Log.Err("Maximum Update Frequency cannot be equal to or less than 0.  Setting default of '" +
                        Defaults.MaxUpdateFrequency + "'");
                m_MaxUpdateFrequency = Defaults.MaxUpdateFrequency;
            }
            else if (val == Double.PositiveInfinity)
            {
                val = float.MaxValue;
            }
            m_MaxUpdateFrequency = val;
        }
    }

    #endregion Properties

    #region Methods

    /// Gets a count of all running drivers.
    public static int DebugGetActiveDriverCount()
    {
        return DriversByID.Count;
    }

    /// Gets a list of all drivers in debug track mode.
    public static List<DBase> DebugGetAllTracked()
    {
        return new List<DBase>(DriversDebugTracked);
    }

    /// Destroy all currently running drivers
    public static void DestroyAll()
    {
        foreach (var driver in DriversByID.Values)
            driver.Destroy();
    }

    /// Gets a dictionary of all running drivers keyed by their ID.
    public static Dictionary<string, DBase> GetAllDriversByID()
    {
        return DriversByID.ToDictionary(_pair => _pair.Key, _pair => _pair.Value);
    }

    /// Gets a dictionary of all running drivers
    public static Dictionary<string, DBase> GetAllDriversByName()
    {
        return DriversByID.ToDictionary(_pair => _pair.Value.Name, _pair => _pair.Value);
    }

    /// <summary>
    /// Get driver with specified ID.
    /// </summary>
    /// <param name="_nameOrID">The Name of ID of the driver to retrieve.</param>
    /// <returns>The running driver if found, null otherwise.</returns>
    public static DBase GetDriver(string _nameOrID)
    {
        if (DriverIDsByName.ContainsKey(_nameOrID))
            return DriversByID[DriverIDsByName[_nameOrID]];
        return DriversByID.ContainsKey(_nameOrID) ? DriversByID[_nameOrID] : null;
    }

    /// <summary>
    /// Get a set containing any/all drivers targeted at the specified object.
    /// Drivers without a target object will be found in target object <see cref="TargetInfo.TARGET_NONE"/>
    /// </summary>
    /// <param name="_target">The object target to retreive from.</param>
    /// <returns>HashSet of driver IDs currently associated with the target object.</returns>
    public static HashSet<string> GetDriverIDsOnTarget(object _target)
    {
        return DriverIDsByObject[_target];
    }

    /// <summary>
    /// Get a set containing any/all drivers with the specified tag.
    /// </summary>
    /// <param name="_tag">The tag to search for.</param>
    /// <returns>HashSet of driver IDs currently associated with the specified tag.</returns>
    public static HashSet<string> GetDriverIDsWithTag(object _tag)
    {
        return DriverIDsByTag.ContainsKey(_tag) ? DriverIDsByTag[_tag] : new HashSet<string>();
    }

    /// Initialize ioDriver.  Called in static constructor.
    public static void Init()
    {
        if (m_UnityPresent == null)
        {
            m_UnityEngine = Type.GetType("UnityEngine.Transform, UnityEngine");
            m_UnityPresent = m_UnityEngine != null;
            if (m_UnityPresent.Value)
            {
                var um = GetTypeEx("ioDriverUnity.ioDriverUnityManager");
                Log.Err("Um is " + um);
                var umInstFld = um.GetField("m_Instance", BindingFlags.NonPublic | BindingFlags.Static);
                Log.Err("Um Inst fld is " + umInstFld);
                m_UnityMgrPresent = (umInstFld.GetValue(um) != null);

                InitDone = false;
            }
        }

        //Do Unity Init if present
        if (m_UnityPresent.Value && !m_UnityMgrPresent)
        {
            var um = GetTypeEx("ioDriverUnity.ioDriverUnityManager");
            if (um != null)
                um.GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic).Invoke(um, null);


        }


        if (InitDone) return;
        InitDone = true;



        //Initialize Teacher
        typeof(Teacher).GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic)
            .Invoke(typeof(Teacher), null);

        //Initialize Manager
        var mgr =
            typeof(DBase).GetNestedType("Manager", BindingFlags.NonPublic);
        mgr.GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Static).Invoke(mgr, null);

        //Initialize Events
        typeof(Event).GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(typeof(Event), null);


        Log.Info("ioDriver Initialized");
    }

    /// <summary>
    /// Injector method that simply returns the input.
    /// <seealso cref="DTransfer{TTar, TDri}.DriveInjector"/>
    /// <seealso cref="DTransfer{TTar, TDri}.TargetInjector"/>
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="_value">Input value</param>
    /// <returns>Output value</returns>
    public static T InjectorPassthrough<T>(T _value) { return _value; }

    /// <summary>
    /// Pump all running drivers.
    /// Call this function once every frame.
    /// 
    /// In Unity, this is automatically hooked up to a monobehaviour (<see cref="ioDriverUnityManager">ioDriverUnityManager</see>) 
    /// that "pumps" this function every frame. 
    /// All drivers follow Unity's timescale unless otherwise specified by setting their timescale to local. See <see cref="DBase.UseTimescaleLocal"/> and <see cref="DBase.TimescaleLocal"/>
    /// </summary>
    public static void Pump()
    {
        Hooks.UpdateManager();
    }

    /// Disregard any accrued time since the last update.
    /// Tells the next update that no time has passed since the last call to update.
    public static void ResetTimestamp()
    {
        Hooks.ResetTimestamp();
    }

    /// <summary>
    /// Stops and disposes the specified driver on its next update call.
    /// </summary>
    /// <param name="_nameOrID">Name or ID of the driver to stop.</param>
    public static void StopDriver(string _nameOrID)
    {
        var driver = GetDriver(_nameOrID);
        if (driver != null)
        {
            driver.Stop();
            return;
        }
        Log.Err("Stop driver '" + _nameOrID + "' failed.  Name/ID not found.");
    }

    /// <summary>
    /// Set the methods to be called for ioDriver's log functions.
    /// </summary>
    /// <param name="_logInfoMethod">Method for "info" level reporting.</param>
    /// <param name="_logWarnMethod">Method for "warning" level reporting.</param>
    /// <param name="_logErrMethod">Method for "error" level reporting.</param>
    public static void SetLogMethods(Action<string> _logInfoMethod, Action<string> _logWarnMethod, Action<string> _logErrMethod)
    {
        Log.LogInfoMethod = _logInfoMethod;
        Log.LogWarnMethod = _logWarnMethod;
        Log.LogErrMethod = _logErrMethod;
    }

    private static object EvalExpr(Expression e)
    {
        switch (e.NodeType)
        {
            case ExpressionType.Constant:
                return (e as ConstantExpression).Value;
            case ExpressionType.MemberAccess:
                {
                    var propertyExpression = e as MemberExpression;
                    var field = propertyExpression.Member as FieldInfo;
                    var property = propertyExpression.Member as PropertyInfo;
                    var container = propertyExpression.Expression == null
                        ? null
                        : EvalExpr(propertyExpression.Expression);
                    if (field != null)
                        return field.GetValue(container);
                    else if (property != null)
                        return property.GetValue(container, null);
                    else
                        return null;
                }
            case ExpressionType.ArrayIndex:
                {
                    var arrayIndex = e as BinaryExpression;
                    var idx = (int)EvalExpr(arrayIndex.Right);
                    var array = (Array)EvalExpr(arrayIndex.Left);
                    return array.GetValue(idx);
                }

            case ExpressionType.Call: // TODO likely doesn't work with iOS / AOT
                {

                    var call = e as MethodCallExpression;

                    var methInfo = call.Method;
                    var callingObj = EvalExpr(call.Object);
                    object[] args = new object[call.Arguments.Count];
                    Type[] argsType = new Type[call.Arguments.Count];
                    for (var idx = 0; idx < call.Arguments.Count; ++idx)
                    {
                        args[idx] = EvalExpr(call.Arguments[idx]);
                        argsType[idx] = args[idx].GetType();
                    }

                    var result = InvokeDelegate(methInfo, callingObj, args);
                    return result;
                }

            default:
                return null;
        }
    }

    private static bool ExtractExpressionSet<T>(Expression<Func<T>> _expr, out Action<T> _setter, out TargetInfo _targetInfo)
    {
        _setter = null;
        _targetInfo = null;
        object targetObj = null;
        //Determine if expression is property or field
        PropertyInfo propInfo = null;
        FieldInfo fieldInfo = null;

        //Array?
        if (_expr.Body.NodeType == ExpressionType.ArrayIndex)
        {
            var bExpr = _expr.Body as BinaryExpression;
            var idx = (int)EvalExpr(bExpr.Right);
            var array = (Array)EvalExpr(bExpr.Left);
            targetObj = bExpr.Left;
            _setter = _val => array.SetValue(_val, idx);
            _targetInfo = new TargetInfo(targetObj.ToString() + ":" + idx, targetObj);
            return true;
        }

        //Property or Field?
        propInfo = ((MemberExpression)_expr.Body).Member as PropertyInfo;
        if (propInfo == null)
        {
            fieldInfo = ((MemberExpression)_expr.Body).Member as FieldInfo;
            if (fieldInfo == null)
            {
                Log.Err("Expression must be a property or a field.  Received: " + _expr.ToString());
                return false;
            }
        }

        //Get Target Object
        var memberExp = _expr.Body as MemberExpression;
        var bodyExp = memberExp.Expression;
        if (bodyExp != null)
        {
            targetObj = GetContainer(_expr);
            //Log.Info("Delete me - targetObject = '" + targetObj + "' - MemberExp: = '" + memberExp + "' - bodyExp = '" + bodyExp + "'");

            if (targetObj == null)
            {
                var sb = new StringBuilder();
                sb.AppendLine(
                    "Unable to determine calling object.  Either use Action<T> overload or simplify the expression for the property.");
                sb.AppendLine("Example:");
                sb.AppendLine(
                    "  ioDriver.Tween(() => gameObjDict[myKey].transform.position, Vector3.zero, Vector3.one, 1f, \"move \" + idx).Go();");
                sb.AppendLine("Can be simplified to:");
                sb.AppendLine("  var xfrm = gameObjDict[myKey].transform;");
                sb.AppendLine(
                    "  ioDriver.Tween(() => xfrm.position, Vector3.zero, Vector3.one, 1f, \"move \" + idx).Go();");
                sb.AppendLine("NOTE:  This may also improve performance.");
                Log.Err(sb.ToString());
                return false;
            }

            if (targetObj.GetType().IsValueType) //TODO determine if this is fixable (Compile on ios)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Calling Object of type " + targetObj.GetType() + " is a value/struct type.");
                sb.AppendLine(
                    "Sorry! Can't drive value type calling objects' properties, fields or methods with an expression.");
                sb.AppendLine("Wrap the call in an Action<T> and use the Action<T> overload.");
                Log.Err(sb.ToString());
                return false;

            }
        }
        else
        {
            targetObj = memberExp.Member.DeclaringType;
        }

        if (propInfo != null)
        {
            var declType = propInfo.DeclaringType;
            var setMethodInfo = propInfo.GetSetMethod(true) ?? declType.GetProperty(propInfo.Name).GetSetMethod(true);
            var isStatic = false;
            if (setMethodInfo != null)
                isStatic = setMethodInfo.IsStatic;
            else
            {
                Log.Err("Something went wrong!  Unable to extract property.  Expression: " + _expr);
                return false;
            }

            if (isStatic)
                _setter = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), setMethodInfo);
            else
                _setter = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), targetObj, setMethodInfo);

            _targetInfo = new TargetInfo(setMethodInfo, targetObj);
            return true;
        }
        else
        {
            _setter = _val => fieldInfo.SetValue(targetObj, _val);
            _targetInfo = new TargetInfo(fieldInfo.Name, targetObj);
            return true;
        }
    }

    private static object GetContainer<T>(Expression<Func<T>> _lambdaExpr)
    {
        return EvalExpr((_lambdaExpr.Body as MemberExpression).Expression);
    }

    /// <summary>
    /// Builds a Delegate instance from the supplied MethodInfo object and a target to invoke against.
    /// </summary>
    private static object InvokeDelegate(MethodInfo _mi, object _target, object[] _args)
    {
        if (_mi == null) throw new ArgumentNullException("_mi");

        Type delegateType;

        var typeArgs = _mi.GetParameters()
            .Select(p => p.ParameterType)
            .ToList();

        // builds a delegate type
        if (_mi.ReturnType == typeof(void))
        {
            delegateType = Expression.GetActionType(typeArgs.ToArray());

        }
        else
        {
            typeArgs.Add(_mi.ReturnType);
            delegateType = Expression.GetFuncType(typeArgs.ToArray());
        }

        // creates a binded delegate if target is supplied
        if (_target == null)
            return Delegate.CreateDelegate(delegateType, _mi).DynamicInvoke(_args);
        else
            return Delegate.CreateDelegate(delegateType, _target, _mi).DynamicInvoke(_args);
    }

    #endregion Methods

    #region Nested Types

    /// Core untyped driver functionality
    public class DBase
    {
        #region Fields

        private readonly Dictionary<string, Func<object>> m_CustomDebugInfo;

        private static ulong m_CurAutoNameNum = 0;

        private bool m_Dispose;

        private float m_TimescaleLocal;

        /// If set to true the driver will not drive nor will it account for any time passage.
        private bool m_Paused;
        private object m_Tag;

        /// Backing field for <see cref="TargetInfo"/>
        private TargetInfo m_TargetInfo;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Base driver by action constructor.
        /// </summary>
        /// Note:  Target Object will be set to <see cref="ioDriver.TargetInfo.TARGET_NONE"/> and no conflict check will be made.  Set <see cref="TargetObject"/> to enable conflict detection.
        /// <param name="_name">Name to assign to this driver.</param>
        public DBase(string _name = null)
        {
            ID = GenID();
            Name = _name ?? AutoName();
            Tag = Defaults.Tag;
            Duration = Defaults.Duration;
            Delay = Defaults.Delay;
            m_TimescaleLocal = Defaults.Timescale;
            UseTimescaleLocal = Defaults.UseTimescaleLocal;
            ElapsedTime = 0;
            m_TargetInfo = new TargetInfo();
            m_CustomDebugInfo = new Dictionary<string, Func<object>>();
            Cancelled = false;
            Finished = false;
            Started = false;
            Paused = false;
            conflictBehavior = Defaults.conflictBehavior;
        }

        #endregion Constructors

        #region Events

        /// Immediate mode event fired when <see cref="Cancel()"/> is called.
        public event Action OnCancel;

        /// Immediate mode event fired when <see cref="Stop()"/> is called.
        public event Action OnFinish;

        /// Immediate mode event fired when <see cref="Paused"/> is changed to true.
        public event Action OnPause;

        /// Immediate mode event fired when <see cref="Paused"/> is changed.
        public event Action OnPauseToggle;

        /// Immediate mode event fired when driver is started.  <seealso cref="DBase.Start"/><seealso cref="ioDriverFluency.Go{T}(T)"/>
        public event Action OnStart;

        /// Immediate mode event fired when <see cref="Paused"/> is changed to false.
        public event Action OnUnpause;

        #endregion Events

        #region Properties

        /// True if the driver was cancelled by a call to <see cref="Cancel()"/>
        public bool Cancelled
        {
            get;
            private set;
        }

        /// Gets/sets this driver's conflict behavoir for when another driver with 
        /// the same <see cref="DBase.TargetInfo"/> and/or with the same Name is detected.  Conflict check is 
        /// done when the driver is <see cref="ioDriverFluency.Go{T}">started</see> 
        /// and <see cref="TargetObject"/> is not set to <see cref="ioDriver.TargetInfo.TARGET_NONE"/>.
        /// <seealso cref="ioDriver.ConflictBehavior"/>  
        public ConflictBehavior conflictBehavior
        {
            get;
            set;
        }

        /// Get/Set this driver into debug mode.
        public bool DebugEnable
        {
            get;
            set;
        }

        /// Time in seconds this driver will wait after Go() is called before starting.
        public float Delay
        {
            get;
            set;
        }

        /// This driver's total duration in seconds.
        public float Duration
        {
            get;
            set;
        }

        /// Time in seconds passed since started, not including time passed while paused.
        public float ElapsedTime
        {
            get;
            private set;
        }

        /// True if the driver has completed running its duration.  False otherwise.
        public bool Finished
        {
            get;
            private set;
        }

        /// Generated Unique ID
        public string ID
        {
            get;
            protected set;
        }

        /// Get/Set driver's local timescale.  Not used if <see cref="UseTimescaleLocal"/> is set to false (default).  Setting this also sets <see cref="UseTimescaleLocal"/> to true;
        public float TimescaleLocal
        {
            get { return m_TimescaleLocal; }
            set
            {
                if (m_TimescaleLocal == value) return;
                if (value < 0)
                {
                    Log.Err("Driver local timescale cannot be less than zero.  Setting to default of '" + Defaults.Timescale + "'");
                    m_TimescaleLocal = Defaults.Timescale;
                }
                m_TimescaleLocal = value;
            }
        }

        /// User defined name.  Set to ID if not user set.  Must be unique among all running drivers.
        public string Name
        {
            get;
            set;
        }

        /// Override for a string representation of driver type.
        public virtual string NiceType
        {
            get { return "Base"; }
        }

        /// Set the pause state for this driver.  Pause events 
        /// will not fire if the driver has not been started, 
        /// has been cancelled or has finished.
        public bool Paused
        {
            get { return m_Paused; }
            set
            {
                if (m_Paused == value) return;
                m_Paused = value;
                if (!Started || Finished || Cancelled || m_Dispose) return;
                if (m_Paused)
                {
                    if (OnPause != null)
                    {
                        Log.Info("Firing OnPause for driver '" + Name + "'", Debug.ReportEvents);
                        OnPause();
                    }
                }
                else
                {
                    if (OnUnpause != null)
                    {
                        Log.Info("Firing OnUnpause for driver '" + Name + "'", Debug.ReportEvents);
                        OnUnpause();
                    }
                }
                if (OnPauseToggle != null)
                {
                    Log.Info("Firing OnPauseToggle for driver '" + Name + "'", Debug.ReportEvents);
                    OnPauseToggle();
                }

            }
        }

        /// Remaining time this driver will be in a <see cref="Paused"/> state.  
        /// <seealso cref="PauseFor(float)"/><seealso cref="Paused"/>
        public float PauseDuration
        {
            get;
            set;
        }

        /// Time in seconds remaining until duration is reached.
        public float RemainingTime
        {
            get { return Duration - ElapsedTime; }
        }

        /// Time in seconds since last update was called for this driver.
        public float SecsSinceLastUpdate
        {
            get;
            private set;
        }

        /// Indicates whether this driver has been started..
        public bool Started
        {
            get;
            private set;
        }

        /// User defined tag.  Does not need to be unique. Cannot be null.
        public object Tag
        {
            get { return m_Tag; }
            set
            {
                if (value == null)
                {
                    Log.Err("Tag cannot be null. Setting to default " + Defaults.Tag);
                    m_Tag = Defaults.Tag;
                    return;
                }
                m_Tag = value;

            }
        }

        /// General target information for this driver.  Used for collisions, tracking with objects and debug.
        public TargetInfo TargetInfo
        {
            get { return m_TargetInfo; }
            set
            {
                if (Started)
                {
                    Log.Warn("ioDriver.DBase.TargetInfo - Can't change target information after the driver has already been started!");
                    return;
                }
                m_TargetInfo = value;
            }
        }

        /// Shorthand access to <see cref="ioDriver.TargetInfo.TargetObject"/>.
        public object TargetObject
        {
            get { return m_TargetInfo.TargetObject; }
        }

        /// Set to true to use local timescale mode, false to use global timescale. <seealso cref="DBase.TimescaleLocal"/> <seealso cref="TimescaleGlobal"/>
        public bool UseTimescaleLocal
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Add a new debug reporting function to the driver.  
        /// </summary>
        /// <param name="_name">Key and String identifier for debug.</param>
        /// <param name="_result">User defined Debug Function.</param>
        public void AddCustomDebug(string _name, Func<object> _result)
        {
            m_CustomDebugInfo.Add(_name, _result);
        }

        /// <summary>
        /// Add an event that will be fired after a specified amount of time has passed.
        /// </summary>
        /// <typeparam name="T">Event Target Type</typeparam>
        /// <param name="_seconds">Event will fire when <see cref="ElapsedTime"/> reaches _seconds.</param>
        /// <param name="_action">Event action</param>
        /// <param name="_id">Event ID</param>
        public IEvent AddEventTimed<T>(float _seconds, Event.Handler<T> _action, string _id = null)
            where T : DBase
        {
            if (_seconds < 0)
            {
                Log.Err("ioDriver:DBase:AddEventTimed - _seconds cannot be less than zero.  No event added.");
                return null;
            }

            var evt = Event.Add(this as T, () => ElapsedTime >= _seconds, _action, _id);
            evt.Description = "Timed at " + _seconds + " seconds on " + Name;
            return evt;
        }

        /// <summary>
        /// Add new user defined managed event.
        /// </summary>
        /// <typeparam name="T">Event Target Type</typeparam>
        /// <param name="_condition">Condition function</param>
        /// <param name="_action">Event action</param>
        /// <param name="_id">Event ID</param>
        /// <returns>Created event</returns>
        public IEvent AddEventUser<T>(Event.Condition _condition, Event.Handler<T> _action, string _id = null)
            where T : DBase
        {
            var evt = Event.Add(this as T, _condition, _action, _id);
            return evt;
        }

        /// <summary>
        /// Cancel and Stop this driver.  The <see cref="OnCancel"/> event will be fired and this driver will stop and be disposed.
        /// </summary>
        public void Cancel()
        {
            if (Cancelled || Finished)
            {
                Log.Info("Cancel : Driver already cancelled or finished - Ignoring");
                return;
            }
            Finished = false;
            Cancelled = true;
        }

        /// Flag this driver to be destroyed on next update.  Events will not be fired (OnCancel, OnFinished, etc.)
        public void Destroy()
        {
            Finished = false;
            Cancelled = false;
            m_Dispose = true;
        }

        /// <summary>
        /// Retrieve debug value from user defined debug function identified by _name.
        /// </summary>
        /// <param name="_name">The name/ID of the debug value to retrieve.</param>
        /// <returns>Debug function result</returns>
        public object GetCustomDebug(string _name)
        {
            return m_CustomDebugInfo[_name]().ToString();
        }

        /// <summary>
        /// Retrieve all user defined debug functions for this driver.
        /// </summary>
        /// <returns>Dictionary of debug functions keyed by name.</returns>
        public Dictionary<string, Func<object>> GetCustomDebugAll()
        {
            return new Dictionary<string, Func<object>>(m_CustomDebugInfo);
        }
        
        /// <summary>
        /// Pause this driver for specified amount of time in seconds.
        /// </summary>
        /// <param name="_seconds">Amount of time to pause, in seconds.</param>
        public void PauseFor(float _seconds)
        {
            Paused = true;
            PauseDuration = _seconds;
        }

        /// <summary>
        /// Start this driver.  Will return false if already started, otherwise returns true.
        /// </summary>
        /// <returns>If start was successful or not</returns>
        public bool Start()
        {
            if (Started)
            {
                Log.Warn("Start : Driver '" + Name + "' already started - Ignoring.");
                return false;
            }

            Started = true;
            if (OnStart != null)
            {
                Log.Info("Firing OnStart for driver '" + Name + "'", Debug.ReportEvents);
                OnStart();
            }
            Manager.Start(this);
            return true;
        }

        /// <summary>
        /// Stop this driver.  The <see cref="OnFinish"/> event will be fired and this driver will be disposed.
        /// </summary>
        public void Stop()
        {
            if (Cancelled || Finished)
            {
                Log.Warn("Stop : Driver already cancelled or finished - Ignoring");
                return;
            }
            Finished = true;
            Cancelled = false;
        }

        /// String representation of Driver. (NiceType:Name:ID)
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Driver '" + Name + "'");
            sb.AppendLine("ID: '" + ID + "'");
            sb.AppendLine("Type: " + NiceType);
            sb.AppendLine("Target Info: " + TargetInfo);
            return sb.ToString();
        }

        /// Define for driver core functionality
        protected virtual void Update()
        {
        }

        /// Optional debug mode override.  Will be called if <see cref="DebugEnable"/> or <see cref="ioDriver.DebugEnableGlobal"/> is true.
        /// If not overridden, <see cref="Update()"/> is called.
        protected virtual void UpdateDebug()
        {
            Update();
        }

        private string AutoName()
        {
            return NiceType + "-" + m_CurAutoNameNum++;
        }

        private void UpdateDriver(float _secsSinceLastUpdate)
        {
            if (UseTimescaleLocal)
                SecsSinceLastUpdate = _secsSinceLastUpdate / TimescaleGlobal * TimescaleLocal;
            else
                SecsSinceLastUpdate = _secsSinceLastUpdate;

            if (Paused)
            {
                PauseDuration -= SecsSinceLastUpdate;
                if (PauseDuration > 0) return;
                PauseDuration = float.PositiveInfinity;
                Paused = false;
            }

            if (Delay > 0)
            {
                Delay -= SecsSinceLastUpdate;
                if (Delay > 0)
                    return;
            }

            ElapsedTime += SecsSinceLastUpdate;

            if (ElapsedTime >= Duration)
            {
                Stop();
                return;
            }

            if (DebugEnable || m_DebugEnableGlobal)
                UpdateDebug();
            else
                Update();
        }

        #endregion Methods

        #region Other

        private static class Manager
        {
            private static readonly long m_TicksAtStart = System.DateTime.Now.Ticks;
            private static float m_LastUpdateTimestamp;
            private static Queue<DBase> m_StartQueue;

            private static void Init()
            {
                m_StartQueue = new Queue<DBase>();

                MaxUpdateFrequency = Defaults.MaxUpdateFrequency;

                m_LastUpdateTimestamp = 0;
                Hooks.UpdateManager = UpdateManager;
                Hooks.ResetTimestamp = ResetTimeStamp;

                Log.Info("Manager Initialized");

                Hooks.SecsSinceStart = GetTimestampInSecs;
            }

            //Hooks
            private static void ResetTimeStamp()
            {
                m_LastUpdateTimestamp = 0;
            }

            private static void UpdateManager()
            {
                if (m_LastUpdateTimestamp == 0)
                {
                    m_LastUpdateTimestamp = GetTimestampInSecs();
                    return;
                }



                var timeStamp = GetTimestampInSecs();
                var secsSinceLastUpdate = (timeStamp - m_LastUpdateTimestamp) * TimescaleGlobal;

                if (secsSinceLastUpdate < 0)
                {
                    Log.Err("Calculated seconds since last update less than zero.  Forcing zero." +
                            " Current timescale is " + TimescaleGlobal);
                    secsSinceLastUpdate = 0;
                }

                var foo = 1f / MaxUpdateFrequency;
                if (secsSinceLastUpdate < foo) return;


                if (OnPump != null) OnPump();
                Hooks.ProcessEvents();


                var toDispose = new List<string>();

                foreach (var id in DriversByID.Keys)
                {
                    var driver = DriversByID[id];
                    if (driver.m_Dispose || driver.Finished || driver.Cancelled)
                    {
                        toDispose.Add(id);
                        continue;
                    }
                    driver.UpdateDriver(secsSinceLastUpdate);
                }
                m_LastUpdateTimestamp = timeStamp;
                foreach (string id in toDispose)
                    Dispose(id);
                while (m_StartQueue.Count > 0)
                    Manage(m_StartQueue.Dequeue());

            }

            public static void Start(DBase _driver)
            {
                ioDriver.Init();
                if (!_driver.m_Dispose) m_StartQueue.Enqueue(_driver);
            }

            /// <summary>
            /// Begin managing this driver.  Determines whether it needs to be checked for conflicting driver or can be immediately tracked.
            /// </summary>
            /// <param name="_driver">Driver to be managed.</param>
            /// <returns>m_Running and tracked driver or null if cancelled due to conflict.</returns>
            private static DBase Manage(DBase _driver)
            {
                if (_driver.TargetInfo.TargetObject != TargetInfo.TARGET_NONE)
                    return ConflictCheck(_driver);
                return Track(_driver);
            }

            /// <summary>
            /// Begins tracking this driver.  This driver will now have its UpdateDriver() called every frame.
            /// </summary>
            /// <param name="_driver"></param>
            /// <returns></returns>
            private static DBase Track(DBase _driver)
            {
                var id = _driver.ID;
                DriversByID.Add(id, _driver);
                DriverIDsByName.Add(_driver.Name, id);
                var tObj = _driver.TargetInfo.TargetObject;
                if (!DriverIDsByObject.ContainsKey(tObj))
                    DriverIDsByObject.Add(tObj, new HashSet<string>());
                DriverIDsByObject[tObj].Add(id);
                if (!DriverIDsByTag.ContainsKey(_driver.Tag))
                    DriverIDsByTag.Add(_driver.Tag, new HashSet<string>());
                DriverIDsByTag[_driver.Tag].Add(id);
                if (_driver.DebugEnable || m_DebugEnableGlobal)
                {
                    DriversDebugTracked.Add(_driver);
                }
                return DriversByID[id];
            }

            /// <summary>
            /// Determines if there is a conflict and executes behavior determined by the driver's ConflictBehavior.
            /// If no conflict detected will begin tracking this driver.
            /// </summary>
            /// <param name="_newDriver">Driver to be checked.</param>
            /// <returns>Returns driver if no conflict or if conflict is set to replace.  
            /// Null if there is a conflict and behavior is set to Cancel.</returns>
            private static DBase ConflictCheck(DBase _newDriver)
            {
                //Check Name conflict
                if (DriverIDsByName.ContainsKey(_newDriver.Name))
                {
                    var curDriver = DriversByID[DriverIDsByName[_newDriver.Name]];
                    var sb = new StringBuilder("Name Conflict - New Driver: " + _newDriver +
                        "  - Cur Driver: " + curDriver);
                    sb.AppendLine(" - New Driver Behaviour is: " + _newDriver.conflictBehavior.ToString());
                    switch (_newDriver.conflictBehavior)
                    {
                        case ConflictBehavior.Ignore:
                            _newDriver.Name = _newDriver.Name + "*" + m_CurAutoNameNum++;
                            sb.AppendLine(" New driver name: " + _newDriver.Name);
                            Log.Info(sb.ToString(), Debug.ReportConflict);
                            return ConflictCheck(_newDriver);
                            break;
                        case ConflictBehavior.Replace:
                            sb.AppendLine("Cancelling current driver and replacing.");
                            Log.Info(sb.ToString(), Debug.ReportConflict);
                            curDriver.Cancel();
                            Dispose(curDriver.ID);
                            break;
                        case ConflictBehavior.Cancel:
                            sb.AppendLine(" Cancelling new driver.");
                            Log.Info(sb.ToString(), Debug.ReportConflict);
                            return null;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                //Check target conflict
                var tObj = _newDriver.TargetInfo.TargetObject;
                if (!DriverIDsByObject.ContainsKey(tObj))
                    return Track(_newDriver);
                foreach (var id in DriverIDsByObject[tObj])
                {
                    var curDriver = DriversByID[id];
                    if (curDriver.m_TargetInfo.MetaName == _newDriver.TargetInfo.MetaName)
                    {
                        var sb = new StringBuilder("Target Conflict - New Driver: " + _newDriver +
                           "  - Cur Driver: " + curDriver);
                        sb.AppendLine(" - New Driver Behaviour is: " + _newDriver.conflictBehavior.ToString());
                        switch (_newDriver.conflictBehavior)
                        {
                            case ConflictBehavior.Ignore:
                                sb.AppendLine("Ignoring conflict.");
                                Log.Info(sb.ToString(), Debug.ReportConflict);
                                return Track(_newDriver);
                            case ConflictBehavior.Replace:
                                sb.AppendLine("Cancelling current driver and replacing.");
                                Log.Info(sb.ToString(), Debug.ReportConflict);
                                curDriver.Cancel();
                                Dispose(id);
                                return Manage(_newDriver);
                            case ConflictBehavior.Cancel:
                                sb.AppendLine("Cancelling new driver.");
                                Log.Info(sb.ToString(), Debug.ReportConflict);
                                return null;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
                return Track(_newDriver);
            }

            public static float GetTimestampInSecs()
            {
                var ticks = DateTime.Now.Ticks - m_TicksAtStart;
                var ticksPerMs = TimeSpan.TicksPerMillisecond;
                var result = (ticks / ticksPerMs) / 1000f;
                return (float)result;
            }

            /// <summary>
            /// Removes all internal references to the driver and fires <see cref="DBase.OnFinish"/> or <see cref="DBase.OnCancel"/>
            /// if applicable.
            /// </summary>
            /// <param name="_id">The id of the driver to dispose.</param>
            private static void Dispose(string _id)
            {
                var driver = DriversByID[_id];
                DriverIDsByName.Remove(driver.Name);
                DriversByID.Remove(_id);
                DriverIDsByObject[driver.TargetInfo.TargetObject].Remove(_id);
                if (DriverIDsByObject[driver.TargetInfo.TargetObject].Count == 0)
                    DriverIDsByObject.Remove(driver.TargetInfo.TargetObject);
                DriverIDsByTag[driver.Tag].Remove(_id);
                if (DriverIDsByTag[driver.Tag].Count == 0)
                    DriverIDsByTag.Remove(driver.Tag);
                Hooks.DisposeEvents(driver);

                if (driver.DebugEnable || m_DebugEnableGlobal)
                {
                    DriversDebugTracked.Remove(driver);
                }
                if (driver.Finished && driver.OnFinish != null)
                {
                    Log.Info("Firing OnFinish for driver: " + driver.Name, Debug.ReportEvents);
                    driver.OnFinish();

                }
                else if (driver.Cancelled && driver.OnCancel != null)
                {
                    Log.Info("Firing OnCancel for driver: " + driver.Name, Debug.ReportEvents);
                    driver.OnCancel();
                }
            }
        }

        #endregion Other
    }

    /// Controls Debug messaging
    public static class Debug
    {
        #region Fields

        /// Report error messages
        public static bool ReportErrorMessages = true;

        /// Report informational messages
        public static bool ReportInfoMessages = false;

        /// Report driver conflicts (override)
        public static bool ReportConflict = false;

        /// Report event messages (override)
        public static bool ReportEvents = false;

        /// Report warning messages
        public static bool ReportWarnMessages = true;

        #endregion Fields

        #region Methods

        /// Report all ioDriver messages and debug info
        public static void SetFullReporting()
        {
            ReportErrorMessages = true;
            ReportWarnMessages = true;
            ReportInfoMessages = true;
            ReportConflict = true;
            ReportEvents = true;
        }

        #endregion Methods
    }

    /// <summary>
    /// Default driver values.
    /// </summary>
    public static class Defaults
    {
        #region Fields

        /// <summary>Default behavior when a conflict is detected. <seealso cref="ConflictBehavior"/><seealso cref="DBase.conflictBehavior"/></summary>
        public static ConflictBehavior conflictBehavior = ConflictBehavior.Replace;

        /// <summary>Default delay in seconds for all driver types. <seealso cref="DBase.Delay"/></summary>
        public static float Delay = 0f;

        /// <summary>Default duration for all drivers.<seealso cref="DBase.Duration"/></summary>
        public static float Duration = float.PositiveInfinity;

        /// <summary>Default ease type for easable driver types. <seealso cref="EaseType"/></summary>
        public static EaseType easeType = EaseType.Linear;

        /// <summary>Default spline segment length. <seealso cref="Path.Spline{T}.SegmentLength"/></summary>
        public static float SegmentLength = 0.1f;

        /// <summary>Default spline segment length accuracy. <seealso cref="Path.Spline{T}.SegmentAccuracy"/></summary>
        public static float SegmentAccuracy = 0.05f;

        /// Default managed event priority. <seealso cref="IEvent.Priority"/><seealso cref="IEvent.SetPriority"/>
        public static uint EventPriority = 10;

        private static float m_Timescale = 1;

        /// <summary>Default timescale.  Cannot be less than 0.</summary>
        public static float Timescale
        {
            get { return m_Timescale; }
            set
            {
                if (m_Timescale == value) return;
                if (value < 0)
                {
                    Log.Err("Global Timescale cannot be less than 0.  Setting to 1.");
                    m_Timescale = 1f;
                    return;
                }
                m_Timescale = value;
            }
        
        }

        /// <summary>Default tag.  <seealso cref="DBase.Tag"/></summary>
        public static object Tag = "NO TAG";

        /// <summary>Default timescale behavior. <seealso cref="DBase.UseTimescaleLocal"/></summary>
        public static bool UseTimescaleLocal = false;

        /// Default control magnitude, as percent of incoming segment length.  <seealso cref="Path.Bezier{T}.Control"/>
        public static float BezierMagPct = 0.33f;

        /// Default maximum update frequency.  <seealso cref="ioDriver.MaxUpdateFrequency"/>
        public static float MaxUpdateFrequency = float.MaxValue;

        #endregion Fields

    }

    /// <summary>
    /// Type transfer driver base
    /// </summary>
    public abstract class DTransfer<TTar, TDri> : DBase, ITransfer
    {
        #region Fields

        private const string TARGET_EXPRESSION_NONE = "ACTION METHOD";

        private Injector<TDri> m_DriveInjector = InjectorPassthrough;
        private Injector<TTar> m_TargetInjector = InjectorPassthrough;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Base driver by action constructor.
        /// </summary>
        /// Note:  Target Object will be set to <see cref="TargetInfo.TARGET_NONE"/> and no conflict check will be made.  Set <see cref="DBase.TargetObject"/> to enable conflict detection.
        /// <param name="_tarAction">Target action</param>
        /// <param name="_driver">Drive Function</param>
        /// <param name="_name">Name to assign to this driver.</param>
        protected DTransfer(Action<TTar> _tarAction, Func<TDri> _driver, string _name)
            : base(_name)
        {
            TargetExpr = TARGET_EXPRESSION_NONE;
            TarAction = _tarAction;
            TargetInfo = new TargetInfo(TarAction.Method, null);
            Driver = _driver;
            if (!DTypeInfo<TTar>.IsValid)
            {
                Log.Err("ioDriver: I haven't been taught about type " + typeof(TTar));
                Destroy();
            }

            DriveInjector = _drive => _drive;
            TargetInjector = _target => _target;
        }

        /// <summary>
        /// Base driver by expression constructor.
        /// </summary>
        /// Note: Using an expression to build a driver uses reflection.  This is useful for conflict detection as the expression target object is extracted and stored (if possible via reflection).
        /// <param name="_targetExpr">Target action expression.  Expression target is extracted to <see cref="TarAction"/> and if successfully reflected, the target object is stored for conflict detection.</param>
        /// <param name="_driver">Drive Function</param>
        /// <param name="_name">Name to assign to this driver.</param>
        protected DTransfer(Expression<Func<TTar>> _targetExpr, Func<TDri> _driver, string _name)
            : base(_name)
        {
            TargetExpr = ((LambdaExpression)_targetExpr).ToString();
            Action<TTar> targetAction;
            TargetInfo info;
            if (!DTypeInfo<TTar>.IsValid)
            {
                Log.Err("ioDriver: I haven't been taught about type " + typeof(TTar));
                Destroy();
            }
            else if (!ExtractExpressionSet(_targetExpr, out targetAction, out info))
            {
                Log.Err("Failed extraction of " + _targetExpr.ToString());
                Destroy();
            }
            else
            {
                TargetInfo = info;
                TarAction = targetAction;
                Driver = _driver;
            }

            DriveInjector = _drive => _drive;
            TargetInjector = _target => _target;
        }

        #endregion Constructors

        #region Properties

        /// Sets the drive injector for this driver.
        /// Allows custom code to be run between <see cref="Driver"/> and
        /// <see cref="DriveToTarget"/>.  Defaults to <see cref="InjectorPassthrough{T}"/>
        /// <seealso cref="Update"/>
        public Injector<TDri> DriveInjector
        {
            private get { return m_DriveInjector; }
            set
            {
                if (value == null)
                {
                    Log.Err("DriveInjector cannot be null.  Injector set to passthrough.");
                    m_DriveInjector = InjectorPassthrough;
                    return;
                }
                m_DriveInjector = value;
            }
        }

        /// Function that returns driver value to be interpreted on each <see cref="Update"/> cycle.
        public Func<TDri> Driver
        {
            get;
            protected set;
        }

        /// Action to be run on each <see cref="Update"/> cycle.
        public Action<TTar> TarAction
        {
            get;
            protected set;
        }

        /// String representation of target expression, will return "ACTION METHOD" if
        /// an action method was used to create this target.
        public string TargetExpr
        {
            get;
            protected set;
        }

        /// Sets the target injector for this driver.
        /// Allows custom code to be run between <see cref="DriveToTarget"/> and
        /// <see cref="TarAction"/>.  Defaults to <see cref="InjectorPassthrough{T}"/><seealso cref="Update"/>
        public Injector<TTar> TargetInjector
        {
            private get { return m_TargetInjector; }
            set
            {
                if (value == null)
                {
                    Log.Err("DriveInjector cannot be null.  Injector set to passthrough.");
                    m_TargetInjector = InjectorPassthrough;
                    return;
                }
                m_TargetInjector = value;
            }
        }

        /// Gets current value from the <see cref="Driver"/> function,
        /// pulled during last update <see cref="Pump"/>.  Will reflect any changes
        /// made in the <see cref="DriveInjector"/>.
        public TDri CurrentDriveVal { get; private set; }

        /// Gets current value from <see cref="DriveToTarget"/> pulled
        /// during last update <see cref="Pump"/>.  Will reflect any changes
        /// made in the <see cref="TargetInjector"/>.
        public TTar CurrentTargetVal { get; private set; }

        Type ITransfer.DriveType
        {
            get { return typeof(TDri); }
        }

        object ITransfer.DriveValCurrent
        {
            get { return CurrentDriveVal; }
        }

        Type ITransfer.TargetType
        {
            get { return typeof(TTar); }
        }

        object ITransfer.TargetValCurrent
        {
            get { return CurrentTargetVal; }
        }

        #endregion Properties

        #region Methods

        /// Returns a string with information about this driver.
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.AppendLine("Transfer: " + typeof(TDri) + " -> " + typeof(TTar));
            sb.AppendLine("Target Expression: " + TargetExpr);
            return sb.ToString();
        }

        /// <summary>
        /// This is the "transmission" function.
        /// Override this method to tell the driver how to interpret the drive value of 
        /// type TDri and calculate a target value of type TTar that is passed to the 
        /// <see cref="TarAction">Target Action</see>.
        /// </summary>
        /// <param name="_drive">Value pulled from drive function</param>
        /// <returns>Value to be passed to the target action.</returns>
        protected abstract TTar DriveToTarget(TDri _drive);

        /// <summary>Core driver functionality.  Update is called for every running driver when the global <see cref="ioDriver.Pump"/> is called.</summary>
        protected override void Update()
        {
            CurrentDriveVal = DriveInjector(Driver());
            CurrentTargetVal = TargetInjector(DriveToTarget(CurrentDriveVal));
            TarAction(CurrentTargetVal);
        }

        /// Override this method to add any additional debug code.  Optional - base calls Update()
        protected override void UpdateDebug()
        {
            Update();
        }

        #endregion Methods
    }

    /// Easing functionality
    public static class Easing
    {
        #region Methods

        /// <summary>
        /// Used for adding custom, user-defined easing functions.
        /// </summary>
        /// <param name="_key">User defined ident key to identify custom easing function</param>
        /// <param name="_func">Custom easing function</param>
        public static void DefineCustomEasingFunc(object _key, Func<float, float> _func)
        {
            m_CustomEaseFuncs.Add(_key, _func);
        }

        /// <summary>
        /// Gets easing function.
        /// </summary>
        /// <param name="_key">User defined ident key for custom ease function to retrieve</param>
        /// <returns></returns>
        public static Func<float, float> GetEasingFunction(object _key)
        {
            if (m_CustomEaseFuncs.ContainsKey(_key))
                return m_CustomEaseFuncs[_key];
            Log.Err("Key '" + _key + "' for custom easing function does not exist!");
            return null;
        }

        /// <summary>
        /// Returns the easing function of provided type.
        /// </summary>
        /// <param name="_easeType">Type of easing function to get</param>
        /// <returns>Specified easing function</returns>
        public static Func<float, float> GetEasingFunction(EaseType _easeType)
        {
            switch (_easeType)
            {
                case EaseType.Linear:
                    return Linear;
                case EaseType.EaseInQuad:
                    return (EaseInQuad);
                case EaseType.EaseOutQuad:
                    return (EaseOutQuad);
                case EaseType.EaseInOutQuad:
                    return (EaseInOutQuad);
                case EaseType.EaseInCubic:
                    return (EaseInCubic);
                case EaseType.EaseOutCubic:
                    return (EaseOutCubic);
                case EaseType.EaseInOutCubic:
                    return (EaseInOutCubic);
                case EaseType.EaseInQuart:
                    return (EaseInQuart);
                case EaseType.EaseOutQuart:
                    return (EaseOutQuart);
                case EaseType.EaseInOutQuart:
                    return (EaseInOutQuart);
                case EaseType.EaseInQuint:
                    return (EaseInQuint);
                case EaseType.EaseOutQuint:
                    return (EaseOutQuint);
                case EaseType.EaseInOutQuint:
                    return (EaseInOutQuint);
                case EaseType.EaseInSine:
                    return (EaseInSine);
                case EaseType.EaseOutSine:
                    return (EaseOutSine);
                case EaseType.EaseInOutSine:
                    return (EaseInOutSine);
                case EaseType.EaseInExpo:
                    return (EaseInExpo);
                case EaseType.EaseOutExpo:
                    return (EaseOutExpo);
                case EaseType.EaseInOutExpo:
                    return (EaseInOutExpo);
                case EaseType.EaseInCirc:
                    return (EaseInCirc);
                case EaseType.EaseOutCirc:
                    return (EaseOutCirc);
                case EaseType.EaseInOutCirc:
                    return (EaseInOutCirc);
                case EaseType.EaseInBounce:
                    return (EaseInBounce);
                case EaseType.EaseOutBounce:
                    return (EaseOutBounce);
                case EaseType.EaseInOutBounce:
                    return (EaseInOutBounce);
                case EaseType.EaseInBack:
                    return (EaseInBack);
                case EaseType.EaseOutBack:
                    return (EaseOutBack);
                case EaseType.EaseInOutBack:
                    return (EaseInOutBack);
                case EaseType.EaseInElastic:
                    return (EaseInElastic);
                case EaseType.EaseOutElastic:
                    return (EaseOutElastic);
                case EaseType.EaseInOutElastic:
                    return (EaseInOutElastic);
                default:
                    throw new NotImplementedException();
            }
        }

        //Tested
        private static float EaseInBack(float _pct)
        {
            return _pct * _pct * ((1.70158f + 1) * _pct - 1.70158f);
        }

        //Tested
        private static float EaseInBounce(float _pct)
        {
            return 1f - EaseOutBounce(1f - _pct);
        }

        //Tested
        private static float EaseInCirc(float _pct)
        {
            return -((float)Math.Sqrt(1 - _pct * _pct) - 1);
        }

        //Tested
        private static float EaseInCubic(float _pct)
        {
            return _pct * _pct * _pct;
        }

        private static float EaseInElastic(float _pct)
        {
            if (_pct <= 0f) return _pct;
            if (_pct >= 1f) return _pct;
            return -((float)Math.Pow(2, 10 * (--_pct)) * (float)Math.Sin((_pct - (0.3f / 4f)) * (2 * (float)Math.PI) / 0.3f));
        }

        //Tested
        private static float EaseInExpo(float _pct)
        {
            return (float)Math.Pow(2, 10 * (_pct - 1));
        }

        private static float EaseInOutBack(float _pct)
        {
            _pct *= 2f;
            var s = 1.70158d * 1.525d;
            if ((_pct) < 1)
            {
                return (float)(0.5f * (_pct * _pct * ((s + 1) * _pct - s)));
            }
            _pct -= 2;
            return (float)(0.5f * (_pct * _pct * ((s + 1) * _pct + s) + 2));
        }

        private static float EaseInOutBounce(float _pct)
        {
            if (_pct < 0.5f)
                return EaseInBounce(_pct * 2) * 0.5f;
            return EaseOutBounce(_pct * 2 - 1) * 0.5f + 0.5f;
        }

        private static float EaseInOutCirc(float _pct)
        {
            _pct *= 2f;
            if (_pct < 1) return -0.5f * ((float)Math.Sqrt(1 - _pct * _pct) - 1);
            _pct -= 2;
            return 0.5f * ((float)Math.Sqrt(1 - _pct * _pct) + 1);
        }

        private static float EaseInOutCubic(float _pct)
        {
            _pct *= 2f;
            if (_pct < 1)
                return 0.5f * _pct * _pct * _pct;
            _pct -= 2;
            return 0.5f * (_pct * _pct * _pct + 2);
        }

        private static float EaseInOutElastic(float _pct)
        {
            if (_pct <= 0f) return _pct;
            if (_pct >= 1f) return _pct;
            _pct *= 2f;

            float p = (.3f * 1.5f);
            float s = p / 4f;

            if (_pct < 1)
                return -.5f * ((float)Math.Pow(2f, 10 * (--_pct)) * (float)Math.Sin((_pct - s) * (2 * (float)Math.PI) / p));
            return (float)Math.Pow(2, -10 * (--_pct)) * (float)Math.Sin((_pct - s) * (2 * (float)Math.PI) / p) * .5f + 1f;
        }

        private static float EaseInOutExpo(float _pct)
        {
            _pct *= 2f;
            if (_pct < 1)
                return 0.5f * (float)Math.Pow(2, 10 * (_pct - 1));
            return 0.5f * (-(float)Math.Pow(2, -10 * --_pct) + 2);
        }

        private static float EaseInOutQuad(float _pct)
        {
            _pct *= 2f;
            if (_pct < 1)
                return 0.5f * _pct * _pct;
            return -0.5f * (--_pct * (_pct - 2) - 1);
        }

        private static float EaseInOutQuart(float _pct)
        {
            _pct *= 2;
            if (_pct < 1)
                return 0.5f * _pct * _pct * _pct * _pct;
            _pct -= 2;
            return -0.5f * (_pct * _pct * _pct * _pct - 2);
        }

        private static float EaseInOutQuint(float _pct)
        {
            _pct *= 2f;
            if (_pct < 1)
                return 0.5f * _pct * _pct * _pct * _pct * _pct;
            _pct -= 2;
            return 0.5f * (_pct * _pct * _pct * _pct * _pct + 2);
        }

        private static float EaseInOutSine(float _pct)
        {
            return -0.5f * ((float)Math.Cos((float)Math.PI * _pct) - 1);
        }

        //Tested
        private static float EaseInQuad(float _pct)
        {
            return _pct * _pct;
        }

        //Tested
        private static float EaseInQuart(float _pct)
        {
            return _pct * _pct * _pct * _pct;
        }

        //Tested
        private static float EaseInQuint(float _pct)
        {
            return _pct * _pct * _pct * _pct * _pct;
        }

        //Tested
        private static float EaseInSine(float _pct)
        {
            return -1 * (float)Math.Cos(_pct * ((float)Math.PI * 0.5f)) + 1;
        }

        private static float EaseOutBack(float _pct)
        {
            return --_pct * _pct * ((1.70158f + 1) * _pct + 1.70158f) + 1;
        }

        private static float EaseOutBounce(float _pct)
        {
            if (_pct < (1f / 2.75f))
                return (7.5625f * _pct * _pct);
            if (_pct < (2f / 2.75f))
            {
                _pct -= (1.5f / 2.75f);
                return (7.5625f * (_pct) * _pct + .75f);
            }
            if (_pct < (2.5f / 2.75f))
            {
                _pct -= (2.25f / 2.75f);
                return (7.5625f * (_pct) * _pct + .9375f);
            }
            _pct -= (2.625f / 2.75f);
            return (7.5625f * (_pct) * _pct + .984375f);
        }

        private static float EaseOutCirc(float _pct)
        {
            return (float)Math.Sqrt(1 - --_pct * _pct);
        }

        private static float EaseOutCubic(float _pct)
        {
            return (--_pct * _pct * _pct + 1);
        }

        private static float EaseOutElastic(float _pct)
        {
            if (_pct <= 0f) return _pct;
            if (_pct >= 1f) return _pct;
            return ((float)Math.Pow(2, -10 * _pct) * (float)Math.Sin((_pct - (0.3f / 4f)) * (2 * (float)Math.PI) / 0.3f) + 1);
        }

        private static float EaseOutExpo(float _pct)
        {
            return -(float)Math.Pow(2, -10 * _pct) + 1;
        }

        private static float EaseOutQuad(float _pct)
        {
            return -1 * _pct * (_pct - 2);
        }

        private static float EaseOutQuart(float _pct)
        {
            return -(--_pct * _pct * _pct * _pct - 1);
        }

        private static float EaseOutQuint(float _pct)
        {
            return (--_pct * _pct * _pct * _pct * _pct + 1);
        }

        private static float EaseOutSine(float _pct)
        {
            return (float)Math.Sin(_pct * ((float)Math.PI * 0.5f));
        }

        //Tested
        private static float Linear(float _pct)
        {
            return Teacher.Lerpf(0f, 1f, _pct);
        }

        #endregion Methods
    }

    /// <summary>
    /// Provides basic driver's target info.  
    /// Used to identify conflicts and provide debug info.  
    /// </summary>
    /// NOTE: Drivers created with <see cref="DBase">Action{T}</see> type methods will have their target object set as <see cref="TARGET_NONE"/>.
    /// To enable conflict checks for drives created in this manner, set <see cref="TargetObject"/>
    //TODO Overhaul this - All not needed for DTransfer
    public class TargetInfo : IEquatable<TargetInfo>
    {
        #region Fields

        /// <summary>Drivers created without a specified target object are keyed by this object.</summary><seealso cref="DBase.TargetObject"/>
        public static readonly object TARGET_NONE = "No Target Object";

        /// <summary>Description of target object meta data.  Set to MethodInfo.ToString() if expression is used.</summary>
        /// Implemented as: @code MethodInfo.ToString() @endcode
        public readonly string MetaDesc;

        /// <summary>Name of target objects meta data.  Set to MethodInfo.Name if expression is used.</summary>
        /// Implemented as: @code MethodInfo.Name @endcode
        public readonly string MetaName;

        private object m_TargetObject;

        #endregion Fields

        #region Constructors

        /// Sets <see cref="TargetObject"/>, <see cref="MetaDesc"/> and <see cref="MetaName"/>
        /// to <see cref="TARGET_NONE"/>
        public TargetInfo()
        {
            MetaDesc = TARGET_NONE.ToString();
            MetaName = TARGET_NONE.ToString();
            m_TargetObject = TARGET_NONE;
        }

        /// Create target info using method info.
        public TargetInfo(MethodInfo _mi, object _targetObject)
        {
            MetaName = _mi.Name;
            MetaDesc = _mi.ToString();
            m_TargetObject = null;
            TargetObject = _targetObject;
        }

        /// Create target info using strings
        public TargetInfo(string _metaName, object _targetObject)
        {
            MetaName = _metaName;
            MetaDesc = _targetObject + " : " + MetaName;
            m_TargetObject = null;
            TargetObject = _targetObject;
        }

        #endregion Constructors

        #region Properties

        /// <summary> Instance of target object. </summary>
        public object TargetObject
        {
            get { return m_TargetObject; }
            set { m_TargetObject = CheckTargetObj(value); }
        }

        #endregion Properties

        #region Methods

        public static bool operator !=(TargetInfo _a, TargetInfo _b)
        {
            return !(_a == _b);
        }

        public static bool operator ==(TargetInfo _a, TargetInfo _b)
        {
            if (ReferenceEquals(null, _a) && ReferenceEquals(null, _b)) return true;
            if (ReferenceEquals(null, _a) && !ReferenceEquals(null, _b)) return false;
            if (!ReferenceEquals(null, _a) && ReferenceEquals(null, _b)) return false;
            if (!(_a is TargetInfo) || !(_b is TargetInfo)) return false;
            return _a.Equals(_b);
        }

        public bool Equals(TargetInfo _p)
        {
            if (ReferenceEquals(null, _p)) return false;
            if (ReferenceEquals(this, _p)) return true;

            if (TargetObject.GetType() != _p.TargetObject.GetType()) return false;
            if (TargetObject.GetType().IsValueType)
            {
                if (TargetObject.ToString() != _p.TargetObject.ToString()) return false;
            }
            else if (!TargetObject.Equals(_p.TargetObject)) return false;

            if (!MetaName.Equals(_p.MetaName)) return false;
            if (!MetaDesc.Equals(_p.MetaDesc)) return false;
            return true;
        }

        public override bool Equals(object _obj)
        {
            if (ReferenceEquals(null, _obj)) return false;
            if (_obj.GetType() != GetType()) return false;
            return Equals(_obj as TargetInfo);
        }

        public override int GetHashCode()
        {
            int hash = 27;
            hash = hash * 13 + TargetObject.GetHashCode();
            hash = hash * 13 + MetaName.GetHashCode();
            hash = hash * 13 + MetaDesc.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return TargetObject.ToString() + " : " + MetaName.ToString() + " : " + MetaDesc.ToString();
        }

        private object CheckTargetObj(object _targetObject)
        {
            if (_targetObject == null)
                return TARGET_NONE;
            if (_targetObject.GetType().IsValueType)
                return "Value Type " + _targetObject.GetType();
            return _targetObject;
        }

        #endregion Methods
    }

    /// <summary>
    /// Manages updating, tracking, conflicts, and disposing of drivers.  Requires an update "pump" (game loop caller).
    /// </summary>
    private static class Hooks
    {
        #region Fields

        public static Action<object> DisposeEvents;
        public static Action ProcessEvents;
        public static Action ResetTimestamp;
        public static Func<float> SecsSinceStart;
        public static Action UpdateManager;

        #endregion Fields
    }

    private static class Log
    {
        #region Fields

        private const string LOG_ID = "ioDriver";
        public static Action<string> LogInfoMethod;
        public static Action<string> LogWarnMethod;
        public static Action<string> LogErrMethod;
        #endregion Fields

        #region Methods

        public static void Err(string _msg, bool? _override = null)
        {
            var log = Debug.ReportErrorMessages;
            if (_override != null) log = _override.Value;
            if (!log) return;
            var msg = LOG_ID + " : " + _msg;

            if (LogErrMethod != null)
                LogErrMethod(msg);
            else
                System.Diagnostics.Debug.WriteLine(msg);
        }

        public static void Info(string _msg, bool? _override = null)
        {
            var log = Debug.ReportInfoMessages;
            if (_override != null) log = _override.Value;
            if (!log) return;
            var msg = LOG_ID + " : " + _msg;

            if (LogInfoMethod != null)
                LogInfoMethod(msg);
            else
                System.Diagnostics.Debug.WriteLine(msg);
        }

        public static void Warn(string _msg, bool? _override = null)
        {
            var log = Debug.ReportWarnMessages;
            if (_override != null) log = _override.Value;
            if (!log) return;
            var msg = LOG_ID + " : " + _msg;

            if (LogWarnMethod != null)
                LogWarnMethod(msg);
            else
                System.Diagnostics.Debug.WriteLine(msg);
        }

        #endregion Methods
    }

    #endregion Nested Types
}