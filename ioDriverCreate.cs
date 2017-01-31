using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

public static partial class ioDriver
{
    
    /// Shorthand base constructor. <seealso cref="DBase"/>
    public static DBase Base(string _name = null)
    {
        return new DBase(_name);
    }

    /// Shorthand step constructor.  <seealso cref="DStep{TTar}" />
    public static DStep<TTar> Step<TTar>(Action<TTar> _targetAction, TTar _startValue, TTar _stepRate,
        string _name = null)
    {
        return new DStep<TTar>(_targetAction, () => _startValue, () => _stepRate, _name);
    }

    /// Shorthand step constructor.  <seealso cref="DStep{TTar}" />
    public static DStep<TTar> Step<TTar>(Expression<Func<TTar>> _targetExp, TTar _startValue, TTar _stepRate,
        string _name = null)
    {
        return new DStep<TTar>(_targetExp, () => _startValue, () => _stepRate, _name);
    }

    /// Shorthand step constructor (dynamic step rate).  <seealso cref="DStep{TTar}" />
    public static DStep<TTar> Step<TTar>(Action<TTar> _targetAction, TTar _startValue, Func<TTar> _dynStepRate,
        string _name = null)
    {
        return new DStep<TTar>(_targetAction, () => _startValue, _dynStepRate, _name);
    }

    /// Shorthand step constructor (dynamic step rate).  <seealso cref="DStep{TTar}" />
    public static DStep<TTar> Step<TTar>(Expression<Func<TTar>> _targetExp, TTar _startValue, Func<TTar> _dynStepRate,
        string _name = null)
    {
        return new DStep<TTar>(_targetExp, () => _startValue, _dynStepRate, _name);
    }

    /// Shorthand step constructor (dynamic start and step rate).  <seealso cref="DStep{TTar}" />
    public static DStep<TTar> Step<TTar>(Action<TTar> _targetAction, Func<TTar> _dynStartVal, Func<TTar> _dynStepRate,
        string _name = null)
    {
        return new DStep<TTar>(_targetAction, _dynStartVal, _dynStepRate, _name);
    }

    /// Shorthand step constructor (dynamic start and step rate).  <seealso cref="DStep{TTar}" />
    public static DStep<TTar> Step<TTar>(Expression<Func<TTar>> _targetExp, Func<TTar> _dynStartVal, Func<TTar> _dynStepRate,
        string _name = null)
    {
        return new DStep<TTar>(_targetExp, _dynStartVal, _dynStepRate, _name);
    }
    

    /// Shorthand tween constructor.  <seealso cref="DTweenSimple{TTar}" />
    public static DTweenSimple<TTar> Tween<TTar>(Action<TTar> _targetAction, TTar _from, TTar _to, float _cycleDuration, string _name = null)
    {
        return new DTweenSimple<TTar>(_targetAction, _from, _to, _cycleDuration, _name);
        
    }

    /// Shorthand tween constructor.  <seealso cref="DTweenSimple{TTar}"/>
    public static DTweenSimple<TTar> Tween<TTar>(Expression<Func<TTar>> _targetExpr, TTar _from, TTar _to, float _cycleDuration, string _name = null)
    {
        return new DTweenSimple<TTar>(_targetExpr, _from, _to, _cycleDuration, _name);
    }

    /// Shorthand tween constructor (dynamic start). <seealso cref="DTweenSimple{TTar}"/>
    public static DTweenSimple<TTar> Tween<TTar>(Action<TTar> _targetAction, Func<TTar> _fromOnStart, Func<TTar> _toOnStart, float _cycleDuration, string _name = null)
    {
        return new DTweenSimple<TTar>(_targetAction, _fromOnStart, _toOnStart, _cycleDuration, _name);
    }

    /// Shorthand tween constructor (dynamic start). <seealso cref="DTweenSimple{TTar}"/>
    public static DTweenSimple<TTar> Tween<TTar>(Expression<Func<TTar>> _targetExpr, Func<TTar> _fromOnStart, Func<TTar> _toOnStart, float _cycleDuration, string _name = null)
    {
        return new DTweenSimple<TTar>(_targetExpr, _fromOnStart, _toOnStart, _cycleDuration, _name);
    }

    /// Shorthand mapped driver constructor.  <seealso cref="DTweenPath{TTar}(Action{TTar},Path.Base{TTar},float,string)"/>
    public static DTweenPath<TTar> Tween<TTar>(Action<TTar> _targetAction, Path.Base<TTar> _path, float _cycleDuration, string _name = null)
    {
        return new DTweenPath<TTar>(_targetAction, _path, _cycleDuration, _name);
    }

    /// Shorthand mapped driver constructor.  <seealso cref="DTweenPath{TTar}(Expression{Func{TTar}},Path.Base{TTar},float,string)"/>
    public static DTweenPath<TTar> Tween<TTar>(Expression<Func<TTar>> _targetExpression, Path.Base<TTar> _path, float _cycleDuration, string _name = null)
    {
        return new DTweenPath<TTar>(_targetExpression, _path, _cycleDuration, _name);
    }

    /// Shorthand mapped driver constructor.  <seealso cref="DMappedSimple"/>
    public static DMappedSimple<TTar, TDri> Map<TTar, TDri>(Action<TTar> _targetAction, TTar _targetMapA, TTar _targetMapB, Func<TDri> _driverMethod, TDri _mapDriveA, TDri _mapDriveB, string _name = null)
    {
        return new DMappedSimple<TTar, TDri>(_targetAction, _targetMapA, _targetMapB, _driverMethod, _mapDriveA, _mapDriveB, _name);
    }

    /// Shorthand mapped driver constructor.  <seealso cref="DMappedSimple"/>
    public static DMappedSimple<TTar, TDri> Map<TTar, TDri>(Expression<Func<TTar>> _targetExpr, TTar _targetMapA, TTar _targetMapB, Func<TDri> _driverMethod, TDri _driveMapA, TDri _driveMapB, string _name = null)
    {
        return new DMappedSimple<TTar, TDri>(_targetExpr, _targetMapA, _targetMapB, _driverMethod, _driveMapA, _driveMapB, _name);
    }

    /// Shorthand mapped path driver constructor.  <seealso cref="DMappedPath"/>
    public static DMappedPath<TTar, TDri> Map<TTar, TDri>(Action<TTar> _targetAction, Path.Base<TTar> _path, Func<TDri> _driverMethod, TDri _mapDriveA, TDri _mapDriveB, string _name = null)
    {
        return new DMappedPath<TTar, TDri>(_targetAction, _path, _driverMethod, _mapDriveA, _mapDriveB, _name);
    }

    /// Shorthand mapped path driver constructor.  <seealso cref="DMappedPath"/>
    public static DMappedPath<TTar, TDri> Map<TTar, TDri>(Expression<Func<TTar>> _targetExpr, Path.Base<TTar> _path, Func<TDri> _driverMethod, TDri _driveMapA, TDri _driveMapB, string _name = null)
    {
        return new DMappedPath<TTar, TDri>(_targetExpr, _path, _driverMethod, _driveMapA, _driveMapB, _name);
    }

    /// Shorthand rate driver constructor.  <seealso cref="DRate{TTar}"/>
    public static DRate<TTar> Rate<TTar>(Expression<Func<TTar>> _targetExpr, Func<TTar> _rateFunc, string _name = null)
    {
        return new DRate<TTar>(_targetExpr, _rateFunc, _name);
    }

    /// Shorthand rate driver constructor.  <seealso cref="DRate{TTar}"/>
    public static DRate<TTar> Rate<TTar>(Action<TTar> _tarAction, Func<TTar> _rateFunc, string _name = null)
    {
        return new DRate<TTar>(_tarAction, _rateFunc, _name);
    }

    /// Shorthand rate driver constructor.  <seealso cref="DRate{TTar}"/>
    public static DRate<TTar> Rate<TTar>(Expression<Func<TTar>> _targetExpr, TTar _constRate, string _name = null)
    {
        return new DRate<TTar>(_targetExpr, () => _constRate, _name);
    }

    /// Shorthand rate driver constructor.  <seealso cref="DRate{TTar}"/>
    public static DRate<TTar> Rate<TTar>(Action<TTar> _tarAction, TTar _constRate, string _name = null)
    {
        return new DRate<TTar>(_tarAction, () => _constRate, _name);
    }

    /// Shorthand speed driver constructor.  <seealso cref="DSpeedSimple{TTar}"/>
    public static DSpeedSimple<TTar> Speed<TTar>(Expression<Func<TTar>> _tarExpr, TTar _from,
        TTar _to, float _constantSpeed, string _name = null)
    {
        return new DSpeedSimple<TTar>(_tarExpr, _from, _to, () => _constantSpeed, _name);
    }

    /// Shorthand speed driver constructor.  <seealso cref="DSpeedSimple{TTar}"/>
    public static DSpeedSimple<TTar> Speed<TTar>(Action<TTar> _tarAction, TTar _from,
        TTar _to, float _constantSpeed, string _name = null)
    {
        return new DSpeedSimple<TTar>(_tarAction, _from, _to, () => _constantSpeed, _name);
    }

    /// Shorthand speed driver constructor.  <seealso cref="DSpeedSimple{TTar}"/>
    public static DSpeedSimple<TTar> Speed<TTar>(Expression<Func<TTar>> _tarExpr, TTar _from, TTar _to,
        Func<float> _speedDriver, string _name = null)
    {
        return new DSpeedSimple<TTar>(_tarExpr, _from, _to, _speedDriver, _name);
    }

    /// Shorthand speed driver constructor.  <seealso cref="DSpeedSimple{TTar}"/>
    public static DSpeedSimple<TTar> Speed<TTar>(Action<TTar> _tarAction, TTar _from, TTar _to,
        Func<float> _speedDriver, string _name = null)
    {
        return new DSpeedSimple<TTar>(_tarAction, _from, _to, _speedDriver, _name);
    }

    /// Shorthand path speed driver expression constructor with constant speed.  <seealso cref="DSpeedPath{TTar}"/>
    public static DSpeedPath<TTar> Speed<TTar>(Expression<Func<TTar>> _tweenExpr, IPath _path,
        float _startSpeed, string _name = null)
    {
        if (_path.WaypointType != typeof(TTar))
        {
            Log.Err("Path and target types don't match.");
            return null;
        }
        return new DSpeedPath<TTar>(_tweenExpr, _path as Path.Base<TTar>, () => _startSpeed, _name);
    }

    /// Shorthand path speed driver action constructor with constant speed.  <seealso cref="DSpeedPath{TTar}"/>
    public static DSpeedPath<TTar> Speed<TTar>(Action<TTar> _tweenAction, Path.Base<TTar> _path,
        float _startSpeed, string _name = null)
    {
        return new DSpeedPath<TTar>(_tweenAction, _path, () => _startSpeed, _name);
    }

    /// Shorthand path speed driver expression constructor with dynamic speed (Function).  <seealso cref="DSpeedPath{TTar}"/>
    public static DSpeedPath<TTar> Speed<TTar>(Expression<Func<TTar>> _tweenExpr, Path.Base<TTar> _path,
        Func<float> _speedDriver, string _name = null)
    {
        return new DSpeedPath<TTar>(_tweenExpr, _path, _speedDriver, _name);
    }

    /// Shorthand path speed driver action constructor with dynamic speed (Function).  <seealso cref="DSpeedPath{TTar}"/>
    public static DSpeedPath<TTar> Speed<TTar>(Action<TTar> _tweener, Path.Base<TTar> _path,
        Func<float> _speedDriver, string _name = null)
    {
        return new DSpeedPath<TTar>(_tweener, _path, _speedDriver, _name);
    }

    /// Shorthand direct driver constructor.  <seealso cref="DDirect{TTar}"/>
    public static DDirect<TTar> Direct<TTar>(Action<TTar> _targetAction, Func<TTar> _driverMethod, string _name = null)
    {
        return new DDirect<TTar>(_targetAction, _driverMethod, _name);
    }

    /// Shorthand direct driver constructor.  <seealso cref="DDirect{TTar}"/>
    public static DDirect<TTar> Direct<TTar>(Expression<Func<TTar>> _targetExpr, Func<TTar> _driverMethod, string _name = null)
    {
        return new DDirect<TTar>(_targetExpr, _driverMethod, _name);
    }

    /// <summary>
    /// Create new driver chain from provided drivers in param order.
    /// </summary>
    /// <param name="_chain">Drivers to add to chain, in order</param>
    /// <returns>New chain object</returns>
    public static DChain Chain(params ioDriver.DBase[] _chain)
    {
        return new DChain(_chain.ToList());
    }

    /// <summary>
    /// Create new driver chain from provided list of drivers.
    /// </summary>
    /// <param name="_chain">List of drivers to make chain</param>
    /// <returns>New chain object</returns>
    public static DChain Chain(List<DBase> _chain)
    {
        return new DChain(_chain);
    }

}


/// Convenience methods for building drivers / driver config.
public static class ioDriverFluency //TODO cleanup/split
{
    

    #region DTransfer Fluency
    /// Fluency method. See <see cref="ioDriver.DBase.Name"/>
    public static T SetName<T>(this T _thisDriver, string _name) where T : ioDriver.DBase
    {
        _thisDriver.Name = _name;
        return _thisDriver;
    }

    /// Fluency method. See <see cref="ioDriver.DBase.Tag"/>
    public static T SetTag<T>(this T _thisDriver, object _tag) where T : ioDriver.DBase
    {
        _thisDriver.Tag = _tag;
        return _thisDriver;
    }

    /// Fluency method.  See <see cref="ioDriver.DBase.Delay"/>
    public static T SetDelay<T>(this T _thisDriver, float _delay) where T : ioDriver.DBase
    {
        _thisDriver.Delay = _delay;
        return _thisDriver;
    }

    /// Fluency method. See <see cref="ioDriver.DBase.Duration"/>
    public static T SetDuration<T>(this T _thisDriver, float _duration) where T : ioDriver.DBase
    {
        _thisDriver.Duration = _duration;
        return _thisDriver;
    }

    /// Fluency method. See <see cref="ioDriver.DBase.conflictBehavior"/>
    public static T SetConflictBehavior<T>(this T _thisDriver, ioDriver.ConflictBehavior _behavior) where T : ioDriver.DBase
    {
        _thisDriver.conflictBehavior = _behavior;
        return _thisDriver;
    }

    /// Fluency method. See <see cref="ioDriver.DBase.ManualTimescale(float)"/>
    public static T SetManualTimeScale<T>(this T _thisDriver, float _timescale, bool _useManualTimescale = true) where T : ioDriver.DBase
    {
        _thisDriver.ManualTimescale(_timescale);
        _thisDriver.UseManualTimescale = _useManualTimescale;
        return _thisDriver;
    }

    /// Fluency method. See <see cref="ioDriver.DBase.ManualTimescale(Func{float})"/>
    public static T SetManualTimescale<T>(this T _thisDriver, Func<float> _timescaleFunc, bool _useManualTimescale = true) where T : ioDriver.DBase
    {
        _thisDriver.ManualTimescale(_timescaleFunc);
        _thisDriver.UseManualTimescale = _useManualTimescale;
        return _thisDriver;

    }



    /// Sets <see cref="ioDriver.DBase.UseManualTimescale">UseManualTimescale</see> of this driver to false.
    public static T SetUseGlobalTimescale<T>(this T _thisDriver) where T : ioDriver.DBase
    {
        _thisDriver.UseManualTimescale = false;
        return _thisDriver;
    }

    /// Fluency method. See <see cref="ioDriver.DBase.TargetObject"/>
    public static T SetTargetInfo<T>(this T _thisDriver, ioDriver.TargetInfo _targetInfo) where T : ioDriver.DBase
    {
        _thisDriver.TargetInfo = _targetInfo;
        return _thisDriver;
    }

    /// Fluency method. Creates new TargetInfo object with the specified _targetObject for this driver.  See <see cref="ioDriver.DBase.TargetInfo"/>
    public static T SetTargetObject<T>(this T _thisDriver, object _targetObject) where T : ioDriver.DBase
    {
        _thisDriver.TargetInfo = new ioDriver.TargetInfo(_targetObject.ToString(),_targetObject);
        return _thisDriver;
    }

    /// Fluency method. See <see cref="ioDriver.DBase.PauseFor"/>
    public static T SetPauseFor<T>(this T _thisDriver, float _pauseDuration) where T : ioDriver.DBase
    {
        _thisDriver.PauseFor(_pauseDuration);
        return _thisDriver;
    }

    /// Fluency method. Sets <see cref="ioDriver.DBase.Paused"/> to true. 
    public static T SetPause<T>(this T _thisDriver) where T : ioDriver.DBase
    {
        _thisDriver.Paused = true;
        return _thisDriver;
    }

    /// Fluency method. Sets <see cref="ioDriver.DBase.Paused"/> to false.
    public static T SetUnpause<T>(this T _thisDriver) where T : ioDriver.DBase
    {
        _thisDriver.Paused = false;
        return _thisDriver;
    }

    /// Set driver debug mode.
    public static T SetDebugEnable<T>(this T _thisDriver, bool _debug = true) where T : ioDriver.DBase
    {
        _thisDriver.DebugEnable = _debug;
        return _thisDriver;
    }
    
    /// Start driver.
    public static T Go<T>(this T _thisDriver) where T : ioDriver.DBase
    {
        _thisDriver.Start();
        return _thisDriver;
    }

    #endregion
    #region DMapped Fluency
    /// Fluency method. See <see cref="ioDriver.DMapped{TTar,TDri}.Ease"/>
    public static T SetEasing<T>(this T _thisDriver, ioDriver.EaseType _easeType) where T : ioDriver.IDMapped
    {
        _thisDriver.Ease(_easeType);
        return _thisDriver;
    }

    /// Fluency method. See <see cref="ioDriver.DMapped{TTar,TDri}.EaseCustom"/>
    public static T SetCustomEasing<T>(T _thisDriver, object _customEaseTypeKey) where T : ioDriver.IDMapped
    {
        _thisDriver.EaseCustom(_customEaseTypeKey);
        return _thisDriver;
    }
    
    /// Fluency method. See <see cref="ioDriver.DMapped{TTar,TDri}.ClampDrive"/>
    public static T SetClampPctDrive<T>(this T _thisDriver, float _clampMinPct, float _clampMaxPct) where T : ioDriver.IDMapped
    {
        _thisDriver.ClampDrive(_clampMinPct, _clampMaxPct);
        return _thisDriver;
    }
    #endregion
    #region DTween Fluency
    /// Fluency method. See <see cref="ioDriver.DTween{TTar}.LoopCycleDuration"/>
    public static T SetLoopCycleDuration<T>(this T _thisDriver, float _cycleDuration) where T : ioDriver.IDTween
    {
        _thisDriver.LoopCycleDuration = _cycleDuration;
        return _thisDriver;
    }
    #endregion
    #region ILoopable Fluency
    /// Fluency method. See <see cref="ioDriver.ILoopable.LoopPingPong"/>
    public static T SetLoopPingPong<T>(this T _thisDriver, float _cycleCount = float.PositiveInfinity, float _startPct = 0) where T : ioDriver.ILoopable
    {
        _thisDriver.LoopPingPong(_cycleCount, _startPct);
        return _thisDriver;
    }

    /// Fluency method. See <see cref="ioDriver.ILoopable.LoopRepeat"/>
    public static T SetLoopRepeat<T>(this T _thisDriver, float _cycleCount = float.PositiveInfinity, float _startPct = 0) where T : ioDriver.ILoopable
    {
        _thisDriver.LoopRepeat(_cycleCount, _startPct);
        return _thisDriver;
    }

    /// Fluency method. See <see cref="ioDriver.ILoopable.LoopOnce"/>
    public static T SetLoopOnce<T>(this T _thisDriver) where T : ioDriver.ILoopable
    {
        _thisDriver.LoopOnce();
        return _thisDriver;
    }

    #endregion
    
    
    /// Fluency Method.  <see cref="ioDriver.Path.Base{T}.Closed"/>
    public static T SetClosed<T>(this T _path, bool _closed) where T : ioDriver.IPath
    {
        _path.Closed = _closed;
        return _path;
    }

    /// <summary>
    /// Add a new timed event for this driver.  Default priority.
    /// </summary>
    /// <typeparam name="T">Driver type</typeparam>
    /// <param name="_thisDriver">This driver</param>
    /// <param name="_seconds">Time in seconds of this driver's elapsed time to fire event</param>
    /// <param name="_action">Event action</param>
    /// <param name="_id">Event ID</param>
    /// <returns>This driver</returns>
    public static T SetAddEvent<T>(this T _thisDriver, float _seconds, ioDriver.Event.Handler<T> _action, string _id = null)
        where T : ioDriver.DBase
    {
        _thisDriver.AddEventTimed(_seconds, _action, _id);
        return _thisDriver;
    }

    /// <summary>
    /// Add a new timed event for this driver.  All params.
    /// </summary>
    /// <typeparam name="T">Driver type</typeparam>
    /// <param name="_thisDriver">This driver</param>
    /// <param name="_seconds">Time in seconds of this driver's elapsed time to fire event</param>
    /// <param name="_action">Event action</param>
    /// <param name="_priority">Event priority</param>
    /// <param name="_fireCount">Event fire count</param>
    /// <param name="_id">Event ID</param>
    /// <param name="_desc">Event Description</param>
    /// <returns>This driver</returns>
    public static T SetAddEvent<T>(this T _thisDriver, float _seconds, ioDriver.Event.Handler<T> _action,
        uint _priority, int _fireCount = 1, string _id = null, string _desc = null)
        where T : ioDriver.DBase
    {
        var evt = _thisDriver.AddEventTimed(_seconds, _action, _id)
            .SetPriority(_priority)
            .SetFireCount(_fireCount);
        if (_desc != null) evt.Description = _desc;
        return _thisDriver;
    }

    /// <summary>
    /// Add a new custom managed event for this driver.  Defaults.
    /// </summary>
    /// <typeparam name="T">Driver type</typeparam>
    /// <param name="_thisDriver">This driver</param>
    /// <param name="_condition">Event condition</param>
    /// <param name="_action">Event action</param>
    /// <param name="_id">Event ID</param>
    /// <returns>This driver</returns>
    public static T SetAddEvent<T>(this T _thisDriver, ioDriver.Event.Condition<T> _condition, ioDriver.Event.Handler<T> _action, string _id = null)
        where T : ioDriver.DBase
    {
        _thisDriver.AddEventUser(() => _condition(_thisDriver), _action, _id);
        return _thisDriver;
    }

    /// <summary>
    /// Add a new custom managed event for this driver.  All params.
    /// </summary>
    /// <typeparam name="T">Driver type</typeparam>
    /// <param name="_thisDriver">This driver</param>
    /// <param name="_condition">Event condition</param>
    /// <param name="_action">Event action</param>
    /// <param name="_priority">Event priority</param>
    /// <param name="_fireCount">Event fire count</param>
    /// <param name="_id">Event ID</param>
    /// <param name="_desc">Event Description</param>
    /// <returns>This driver</returns>
    public static T SetAddEvent<T>(this T _thisDriver, ioDriver.Event.Condition<T> _condition,
        ioDriver.Event.Handler<T> _action, uint _priority, int _fireCount = 1, string _id = null, string _desc = null)
        where T : ioDriver.DBase
    {
        var evt = _thisDriver.AddEventUser(() => _condition(_thisDriver), _action, _id)
            .SetFireCount(_fireCount)
            .SetPriority(_priority);
        if (_desc != null) evt.Description = _desc;
        return _thisDriver;
    }


    /// <summary>
    /// Add immediate mode base event. <seealso cref="ioDriver.Events"/>
    /// </summary>
    /// <typeparam name="T">Event target type</typeparam>
    /// <param name="_thisDriver">Driver to add event to</param>
    /// <param name="_event">Event type</param>
    /// <param name="_action">Event action</param>
    /// <returns>This driver</returns>
    public static T SetAddEvent<T>(this T _thisDriver, ioDriver.Events _event, ioDriver.Event.Handler<T> _action)
            where T : ioDriver.DBase
    {
        switch (_event)
        {
            case ioDriver.Events.OnStart:
                _thisDriver.OnStart += () => _action(_thisDriver);
                break;
            case ioDriver.Events.OnFinish:
                _thisDriver.OnFinish += () => _action(_thisDriver);
                break;
            case ioDriver.Events.OnCancel:
                _thisDriver.OnCancel += () => _action(_thisDriver);
                break;
            case ioDriver.Events.OnPause:
                _thisDriver.OnPause += () => _action(_thisDriver);
                break;
            case ioDriver.Events.OnUnpause:
                _thisDriver.OnUnpause += () => _action(_thisDriver);
                break;
            case ioDriver.Events.OnPauseToggle:
                _thisDriver.OnPauseToggle += () => _action(_thisDriver);
                break;
            default:
                throw new ArgumentOutOfRangeException("_event", _event, null);
        }
        return _thisDriver;
    }

    /// <summary>
    /// Extension method.  Add immediate mode loop event. <seealso cref="ioDriver.EventsLoop"/>
    /// </summary>
    /// <typeparam name="T">Event target type</typeparam>
    /// <param name="_thisDriver">Driver to add event to</param>
    /// <param name="_event">Event type</param>
    /// <param name="_action">Event action</param>
    /// <returns>This driver</returns>
    public static T SetAddEvent<T>(this T _thisDriver, ioDriver.EventsLoop _event, ioDriver.Event.Handler<T> _action)
            where T : ioDriver.ILoopable
    {
        switch (_event)
        {
            case ioDriver.EventsLoop.OnLoopCycleComplete:
                _thisDriver.OnLoopCycleComplete += () => _action(_thisDriver);
                break;
            case ioDriver.EventsLoop.OnLoopPingPongForeComplete:
                _thisDriver.OnLoopPingPongForeComplete += () => _action(_thisDriver);
                break;
            case ioDriver.EventsLoop.OnLoopPingPongBackComplete:
                _thisDriver.OnLoopPingPongBackComplete += () => _action(_thisDriver);
                break;
            default:
                throw new ArgumentOutOfRangeException("_event", _event, null);
        }
        return _thisDriver;
    }


    public static T SetSplineMode<T>(this T _spline, ioDriver.Path.SplineMode _mode)
        where T : ioDriver.ISpline
    {
        _spline.SplineMode = _mode;
        return _spline;
    }

    public static T SetModeEquidistant<T>(this T _spline, float _segmentLength)
        where T : ioDriver.ISpline
    {
        _spline.EQSegmentLength = _segmentLength;
        return _spline;
    }
    
    public static T SetModeEquidistant<T>(this T _spline)
        where T : ioDriver.ISpline
    {
        _spline.SplineMode = ioDriver.Path.SplineMode.Equidistant;
        return _spline;
    }

    public static T SetModeMathematical<T>(this T _spline, int _resolution)
        where T : ioDriver.ISpline
    {
        _spline.SplineResolution = _resolution;
        return _spline;
    }

    public static T SetModeMathematical<T>(this T _spline)
        where T : ioDriver.ISpline
    {
        _spline.SplineMode = ioDriver.Path.SplineMode.Mathematical;
        return _spline;
    }

    public static T SetEQSegmentLength<T>(this T _spline, float _length)
        where T : ioDriver.ISpline
    {
        _spline.EQSegmentLength = _length;
        return _spline;
    }

    /// Fluency Method.  See <see cref="ioDriver.Path.Spline{T}.SplineResolution"/>
    public static T SetSplineResolution<T>(this T _spline, int _resolution)
        where T : ioDriver.ISpline
    {
        _spline.SplineResolution = _resolution;
        return _spline;
    }

    /// Fluency Method. See <see cref="ioDriver.Path.Spline{T}.DimsToSpline"/>
    public static T SetDimsToSpline<T>(this T _spline, params int[] _dimsToSpline)
        where T : ioDriver.ISpline
    {
        _spline.DimsToSpline = _dimsToSpline;
        return _spline;
    }
}