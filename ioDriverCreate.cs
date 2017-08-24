using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

public static partial class ioDriver
{
    /// <summary>
    /// Shorthand base constructor. <seealso cref="DBase"/>
    /// </summary>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DBase Base(string _name = null)
    {
        return new DBase(_name);
    }

    /// <summary>
    /// Shorthand step constructor.  <seealso cref="DStep{TTar}" />
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_targetAction"></param>
    /// <param name="_startValue"></param>
    /// <param name="_stepRate"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DStep<TTar> Step<TTar>(Action<TTar> _targetAction, TTar _startValue, TTar _stepRate,
        string _name = null)
    {
        return new DStep<TTar>(_targetAction, () => _startValue, () => _stepRate, _name);
    }

    /// <summary>
    /// Shorthand step constructor.  <seealso cref="DStep{TTar}" />
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_targetExp"></param>
    /// <param name="_startValue"></param>
    /// <param name="_stepRate"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DStep<TTar> Step<TTar>(Expression<Func<TTar>> _targetExp, TTar _startValue, TTar _stepRate,
        string _name = null)
    {
        return new DStep<TTar>(_targetExp, () => _startValue, () => _stepRate, _name);
    }

    /// <summary>
    /// Shorthand step constructor (dynamic step rate).  <seealso cref="DStep{TTar}" />
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_targetAction"></param>
    /// <param name="_startValue"></param>
    /// <param name="_dynStepRate"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DStep<TTar> Step<TTar>(Action<TTar> _targetAction, TTar _startValue, Func<TTar> _dynStepRate,
        string _name = null)
    {
        return new DStep<TTar>(_targetAction, () => _startValue, _dynStepRate, _name);
    }

    /// <summary>
    /// Shorthand step constructor (dynamic step rate).  <seealso cref="DStep{TTar}" />
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_targetExp"></param>
    /// <param name="_startValue"></param>
    /// <param name="_dynStepRate"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DStep<TTar> Step<TTar>(Expression<Func<TTar>> _targetExp, TTar _startValue, Func<TTar> _dynStepRate,
        string _name = null)
    {
        return new DStep<TTar>(_targetExp, () => _startValue, _dynStepRate, _name);
    }

    /// <summary>
    /// Shorthand step constructor (dynamic start and step rate).  <seealso cref="DStep{TTar}" />
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_targetAction"></param>
    /// <param name="_dynStartVal"></param>
    /// <param name="_dynStepRate"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DStep<TTar> Step<TTar>(Action<TTar> _targetAction, Func<TTar> _dynStartVal, Func<TTar> _dynStepRate,
        string _name = null)
    {
        return new DStep<TTar>(_targetAction, _dynStartVal, _dynStepRate, _name);
    }

    /// <summary>
    /// Shorthand step constructor (dynamic start and step rate).  <seealso cref="DStep{TTar}" />
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_targetExp"></param>
    /// <param name="_dynStartVal"></param>
    /// <param name="_dynStepRate"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DStep<TTar> Step<TTar>(Expression<Func<TTar>> _targetExp, Func<TTar> _dynStartVal, Func<TTar> _dynStepRate,
        string _name = null)
    {
        return new DStep<TTar>(_targetExp, _dynStartVal, _dynStepRate, _name);
    }

    /// <summary>
    /// Shorthand tween constructor.  <seealso cref="DTweenSimple{TTar}" />
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_targetAction"></param>
    /// <param name="_from"></param>
    /// <param name="_to"></param>
    /// <param name="_cycleDuration"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DTweenSimple<TTar> Tween<TTar>(Action<TTar> _targetAction, TTar _from, TTar _to, float _cycleDuration, string _name = null)
    {
        return new DTweenSimple<TTar>(_targetAction, _from, _to, _cycleDuration, _name);

    }

    /// <summary>
    /// Shorthand tween constructor.  <seealso cref="DTweenSimple{TTar}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_targetExpr"></param>
    /// <param name="_from"></param>
    /// <param name="_to"></param>
    /// <param name="_cycleDuration"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DTweenSimple<TTar> Tween<TTar>(Expression<Func<TTar>> _targetExpr, TTar _from, TTar _to, float _cycleDuration, string _name = null)
    {
        return new DTweenSimple<TTar>(_targetExpr, _from, _to, _cycleDuration, _name);
    }

    /// <summary>
    /// Shorthand tween constructor (dynamic start). <seealso cref="DTweenSimple{TTar}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_targetAction"></param>
    /// <param name="_fromOnStart"></param>
    /// <param name="_toOnStart"></param>
    /// <param name="_cycleDuration"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DTweenSimple<TTar> Tween<TTar>(Action<TTar> _targetAction, Func<TTar> _fromOnStart, Func<TTar> _toOnStart, float _cycleDuration, string _name = null)
    {
        return new DTweenSimple<TTar>(_targetAction, _fromOnStart, _toOnStart, _cycleDuration, _name);
    }

    /// <summary>
    /// Shorthand tween constructor (dynamic start). <seealso cref="DTweenSimple{TTar}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_targetExpr"></param>
    /// <param name="_fromOnStart"></param>
    /// <param name="_toOnStart"></param>
    /// <param name="_cycleDuration"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DTweenSimple<TTar> Tween<TTar>(Expression<Func<TTar>> _targetExpr, Func<TTar> _fromOnStart, Func<TTar> _toOnStart, float _cycleDuration, string _name = null)
    {
        return new DTweenSimple<TTar>(_targetExpr, _fromOnStart, _toOnStart, _cycleDuration, _name);
    }

    /// <summary>
    ///  Shorthand mapped driver constructor.  <seealso cref="DTweenPath{TTar}(Action{TTar},Path.Base{TTar},float,string)"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_targetAction"></param>
    /// <param name="_path"></param>
    /// <param name="_cycleDuration"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DTweenPath<TTar> Tween<TTar>(Action<TTar> _targetAction, Path.Base<TTar> _path, float _cycleDuration, string _name = null)
    {
        return new DTweenPath<TTar>(_targetAction, _path, _cycleDuration, _name);
    }

    /// <summary>
    /// Shorthand mapped driver constructor.  <seealso cref="DTweenPath{TTar}(Expression{Func{TTar}},Path.Base{TTar},float,string)"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_targetExpression"></param>
    /// <param name="_path"></param>
    /// <param name="_cycleDuration"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DTweenPath<TTar> Tween<TTar>(Expression<Func<TTar>> _targetExpression, Path.Base<TTar> _path, float _cycleDuration, string _name = null)
    {
        return new DTweenPath<TTar>(_targetExpression, _path, _cycleDuration, _name);
    }

    /// <summary>
    /// Shorthand mapped driver constructor.  <seealso cref="DMappedSimple{TTar,TDri}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <typeparam name="TDri"></typeparam>
    /// <param name="_targetAction"></param>
    /// <param name="_targetMapA"></param>
    /// <param name="_targetMapB"></param>
    /// <param name="_driverMethod"></param>
    /// <param name="_mapDriveA"></param>
    /// <param name="_mapDriveB"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DMappedSimple<TTar, TDri> Map<TTar, TDri>(Action<TTar> _targetAction, TTar _targetMapA, TTar _targetMapB, Func<TDri> _driverMethod, TDri _mapDriveA, TDri _mapDriveB, string _name = null)
    {
        return new DMappedSimple<TTar, TDri>(_targetAction, _targetMapA, _targetMapB, _driverMethod, _mapDriveA, _mapDriveB, _name);
    }

    /// <summary>
    /// Shorthand mapped driver constructor.  <seealso cref="DMappedSimple{TTar,TDri}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <typeparam name="TDri"></typeparam>
    /// <param name="_targetExpr"></param>
    /// <param name="_targetMapA"></param>
    /// <param name="_targetMapB"></param>
    /// <param name="_driverMethod"></param>
    /// <param name="_driveMapA"></param>
    /// <param name="_driveMapB"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DMappedSimple<TTar, TDri> Map<TTar, TDri>(Expression<Func<TTar>> _targetExpr, TTar _targetMapA, TTar _targetMapB, Func<TDri> _driverMethod, TDri _driveMapA, TDri _driveMapB, string _name = null)
    {
        return new DMappedSimple<TTar, TDri>(_targetExpr, _targetMapA, _targetMapB, _driverMethod, _driveMapA, _driveMapB, _name);
    }

    /// <summary>
    /// Shorthand mapped path driver constructor.  <seealso cref="DMappedPath{TTar,TDri}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <typeparam name="TDri"></typeparam>
    /// <param name="_targetAction"></param>
    /// <param name="_path"></param>
    /// <param name="_driverMethod"></param>
    /// <param name="_mapDriveA"></param>
    /// <param name="_mapDriveB"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DMappedPath<TTar, TDri> Map<TTar, TDri>(Action<TTar> _targetAction, Path.Base<TTar> _path, Func<TDri> _driverMethod, TDri _mapDriveA, TDri _mapDriveB, string _name = null)
    {
        return new DMappedPath<TTar, TDri>(_targetAction, _path, _driverMethod, _mapDriveA, _mapDriveB, _name);
    }

    /// <summary>
    /// Shorthand mapped path driver constructor.  <seealso cref="DMappedPath{TTar,TDri}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <typeparam name="TDri"></typeparam>
    /// <param name="_targetExpr"></param>
    /// <param name="_path"></param>
    /// <param name="_driverMethod"></param>
    /// <param name="_driveMapA"></param>
    /// <param name="_driveMapB"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DMappedPath<TTar, TDri> Map<TTar, TDri>(Expression<Func<TTar>> _targetExpr, Path.Base<TTar> _path, Func<TDri> _driverMethod, TDri _driveMapA, TDri _driveMapB, string _name = null)
    {
        return new DMappedPath<TTar, TDri>(_targetExpr, _path, _driverMethod, _driveMapA, _driveMapB, _name);
    }

    /// <summary>
    /// Shorthand rate driver constructor.  <seealso cref="DRate{TTar}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_targetExpr"></param>
    /// <param name="_rateFunc"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DRate<TTar> Rate<TTar>(Expression<Func<TTar>> _targetExpr, Func<TTar> _rateFunc, string _name = null)
    {
        return new DRate<TTar>(_targetExpr, _rateFunc, _name);
    }

    /// <summary>
    /// Shorthand rate driver constructor.  <seealso cref="DRate{TTar}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_tarAction"></param>
    /// <param name="_rateFunc"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DRate<TTar> Rate<TTar>(Action<TTar> _tarAction, Func<TTar> _rateFunc, string _name = null)
    {
        return new DRate<TTar>(_tarAction, _rateFunc, _name);
    }

    /// <summary>
    /// Shorthand rate driver constructor.  <seealso cref="DRate{TTar}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_targetExpr"></param>
    /// <param name="_constRate"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DRate<TTar> Rate<TTar>(Expression<Func<TTar>> _targetExpr, TTar _constRate, string _name = null)
    {
        return new DRate<TTar>(_targetExpr, () => _constRate, _name);
    }

    /// <summary>
    /// Shorthand rate driver constructor.  <seealso cref="DRate{TTar}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_tarAction"></param>
    /// <param name="_constRate"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DRate<TTar> Rate<TTar>(Action<TTar> _tarAction, TTar _constRate, string _name = null)
    {
        return new DRate<TTar>(_tarAction, () => _constRate, _name);
    }

    /// <summary>
    /// Shorthand speed driver constructor.  <seealso cref="DSpeedSimple{TTar}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_tarExpr"></param>
    /// <param name="_from"></param>
    /// <param name="_to"></param>
    /// <param name="_constantSpeed"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DSpeedSimple<TTar> Speed<TTar>(Expression<Func<TTar>> _tarExpr, TTar _from,
        TTar _to, float _constantSpeed, string _name = null)
    {
        return new DSpeedSimple<TTar>(_tarExpr, _from, _to, () => _constantSpeed, _name);
    }

    /// <summary>
    /// Shorthand speed driver constructor.  <seealso cref="DSpeedSimple{TTar}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_tarAction"></param>
    /// <param name="_from"></param>
    /// <param name="_to"></param>
    /// <param name="_constantSpeed"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DSpeedSimple<TTar> Speed<TTar>(Action<TTar> _tarAction, TTar _from,
        TTar _to, float _constantSpeed, string _name = null)
    {
        return new DSpeedSimple<TTar>(_tarAction, _from, _to, () => _constantSpeed, _name);
    }

    /// <summary>
    /// Shorthand speed driver constructor.  <seealso cref="DSpeedSimple{TTar}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_tarExpr"></param>
    /// <param name="_from"></param>
    /// <param name="_to"></param>
    /// <param name="_speedDriver"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DSpeedSimple<TTar> Speed<TTar>(Expression<Func<TTar>> _tarExpr, TTar _from, TTar _to,
        Func<float> _speedDriver, string _name = null)
    {
        return new DSpeedSimple<TTar>(_tarExpr, _from, _to, _speedDriver, _name);
    }

    /// <summary>
    /// Shorthand speed driver constructor.  <seealso cref="DSpeedSimple{TTar}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_tarAction"></param>
    /// <param name="_from"></param>
    /// <param name="_to"></param>
    /// <param name="_speedDriver"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DSpeedSimple<TTar> Speed<TTar>(Action<TTar> _tarAction, TTar _from, TTar _to,
        Func<float> _speedDriver, string _name = null)
    {
        return new DSpeedSimple<TTar>(_tarAction, _from, _to, _speedDriver, _name);
    }

    /// <summary>
    /// Shorthand path speed driver expression constructor with constant speed.  <seealso cref="DSpeedPath{TTar}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_tweenExpr"></param>
    /// <param name="_path"></param>
    /// <param name="_startSpeed"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Shorthand path speed driver action constructor with constant speed.  <seealso cref="DSpeedPath{TTar}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_tweenAction"></param>
    /// <param name="_path"></param>
    /// <param name="_startSpeed"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DSpeedPath<TTar> Speed<TTar>(Action<TTar> _tweenAction, Path.Base<TTar> _path,
        float _startSpeed, string _name = null)
    {
        return new DSpeedPath<TTar>(_tweenAction, _path, () => _startSpeed, _name);
    }

    /// <summary>
    /// Shorthand path speed driver expression constructor with dynamic speed (Function).  <seealso cref="DSpeedPath{TTar}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_tweenExpr"></param>
    /// <param name="_path"></param>
    /// <param name="_speedDriver"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DSpeedPath<TTar> Speed<TTar>(Expression<Func<TTar>> _tweenExpr, Path.Base<TTar> _path,
        Func<float> _speedDriver, string _name = null)
    {
        return new DSpeedPath<TTar>(_tweenExpr, _path, _speedDriver, _name);
    }

    /// <summary>
    /// Shorthand path speed driver action constructor with dynamic speed (Function).  <seealso cref="DSpeedPath{TTar}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_tweener"></param>
    /// <param name="_path"></param>
    /// <param name="_speedDriver"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DSpeedPath<TTar> Speed<TTar>(Action<TTar> _tweener, Path.Base<TTar> _path,
        Func<float> _speedDriver, string _name = null)
    {
        return new DSpeedPath<TTar>(_tweener, _path, _speedDriver, _name);
    }

    /// <summary>
    /// Shorthand direct driver constructor.  <seealso cref="DDirect{TTar}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_targetAction"></param>
    /// <param name="_driverMethod"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static DDirect<TTar> Direct<TTar>(Action<TTar> _targetAction, Func<TTar> _driverMethod, string _name = null)
    {
        return new DDirect<TTar>(_targetAction, _driverMethod, _name);
    }

    /// <summary>
    /// Shorthand direct driver constructor.  <seealso cref="DDirect{TTar}"/>
    /// </summary>
    /// <typeparam name="TTar"></typeparam>
    /// <param name="_targetExpr"></param>
    /// <param name="_driverMethod"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
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
    /// Create new driver chain from provided set of drivers.
    /// </summary>
    /// <param name="_chain">List of drivers to make chain</param>
    /// <returns>New chain object</returns>
    public static DChain Chain(IEnumerable<DBase> _chain)
    {
        return new DChain(_chain);
    }

}


/// Convenience methods for building drivers / driver config.
public static class ioDriverFluency //TODO cleanup/split
{


    #region DTransfer Fluency
    /// <summary>
    /// Fluency method. See <see cref="ioDriver.DBase.Name"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <param name="_name"></param>
    /// <returns></returns>
    public static T SetName<T>(this T _thisDriver, string _name) where T : ioDriver.DBase
    {
        _thisDriver.Name = _name;
        return _thisDriver;
    }

    /// <summary>
    /// Fluency method. See <see cref="ioDriver.DBase.Tag"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <param name="_tag"></param>
    /// <returns></returns>
    public static T SetTag<T>(this T _thisDriver, object _tag) where T : ioDriver.DBase
    {
        _thisDriver.Tag = _tag;
        return _thisDriver;
    }

    /// <summary>
    /// Fluency method.  See <see cref="ioDriver.DBase.Delay"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <param name="_delay"></param>
    /// <returns></returns>
    public static T SetDelay<T>(this T _thisDriver, float _delay) where T : ioDriver.DBase
    {
        _thisDriver.Delay = _delay;
        return _thisDriver;
    }

    /// <summary>
    /// Fluency method. See <see cref="ioDriver.DBase.Duration"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <param name="_duration"></param>
    /// <returns></returns>
    public static T SetDuration<T>(this T _thisDriver, float _duration) where T : ioDriver.DBase
    {
        _thisDriver.Duration = _duration;
        return _thisDriver;
    }

    /// <summary>
    /// Fluency method. See <see cref="ioDriver.DBase.conflictBehavior"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <param name="_behavior"></param>
    /// <returns></returns>
    public static T SetConflictBehavior<T>(this T _thisDriver, ioDriver.ConflictBehavior _behavior) where T : ioDriver.DBase
    {
        _thisDriver.conflictBehavior = _behavior;
        return _thisDriver;
    }

    /// <summary>
    /// Fluency method. See <see cref="ioDriver.DBase.TimescaleLocal"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <param name="_timescale"></param>
    /// <param name="_useTimescaleLocal"></param>
    /// <returns></returns>
    public static T SetTimescaleLocal<T>(this T _thisDriver, float _timescale, bool _useTimescaleLocal = true) where T : ioDriver.DBase
    {
        _thisDriver.TimescaleLocal = _timescale;
        _thisDriver.UseTimescaleLocal = _useTimescaleLocal;
        return _thisDriver;
    }

    /// <summary>
    /// Sets <see cref="ioDriver.DBase.UseTimescaleLocal">UseTimescaleLocal</see> of this driver to false.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <returns></returns>
    public static T SetUseGlobalTimescale<T>(this T _thisDriver) where T : ioDriver.DBase
    {
        _thisDriver.UseTimescaleLocal = false;
        return _thisDriver;
    }

    /// <summary>
    /// Fluency method. See <see cref="ioDriver.DBase.TargetObject"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <param name="_targetInfo"></param>
    /// <returns></returns>
    public static T SetTargetInfo<T>(this T _thisDriver, ioDriver.TargetInfo _targetInfo) where T : ioDriver.DBase
    {
        _thisDriver.TargetInfo = _targetInfo;
        return _thisDriver;
    }

    /// <summary>
    /// Fluency method. Creates new TargetInfo object with the specified _targetObject for this driver.  See <see cref="ioDriver.DBase.TargetInfo"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <param name="_targetObject"></param>
    /// <returns></returns>
    public static T SetTargetObject<T>(this T _thisDriver, object _targetObject) where T : ioDriver.DBase
    {
        if (_targetObject == null)
            _thisDriver.TargetInfo = null;
        else
            _thisDriver.TargetInfo = new ioDriver.TargetInfo(_targetObject.ToString(), _targetObject);
        return _thisDriver;
    }

    /// <summary>
    /// Fluency method. See <see cref="ioDriver.DBase.PauseFor"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <param name="_pauseDuration"></param>
    /// <returns></returns>
    public static T SetPauseFor<T>(this T _thisDriver, float _pauseDuration) where T : ioDriver.DBase
    {
        _thisDriver.PauseFor(_pauseDuration);
        return _thisDriver;
    }

    /// <summary>
    /// Fluency method. Sets <see cref="ioDriver.DBase.Paused"/> to true. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <returns></returns>
    public static T SetPause<T>(this T _thisDriver) where T : ioDriver.DBase
    {
        _thisDriver.Paused = true;
        return _thisDriver;
    }

    /// <summary>
    /// Fluency method. Sets <see cref="ioDriver.DBase.Paused"/> to false.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <returns></returns>
    public static T SetUnpause<T>(this T _thisDriver) where T : ioDriver.DBase
    {
        _thisDriver.Paused = false;
        return _thisDriver;
    }

    /// <summary>
    /// Set driver debug mode.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <param name="_debug"></param>
    /// <returns></returns>
    public static T SetDebugEnable<T>(this T _thisDriver, bool _debug = true) where T : ioDriver.DBase
    {
        _thisDriver.DebugEnable = _debug;
        return _thisDriver;
    }

    /// <summary>
    /// Start driver.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <returns></returns>
    public static T Go<T>(this T _thisDriver) where T : ioDriver.DBase
    {
        _thisDriver.Start();
        return _thisDriver;
    }

    #endregion
    #region DMapped Fluency
    /// <summary>
    /// Fluency method. See <see cref="ioDriver.DMapped{TTar,TDri}.Ease"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <param name="_easeType"></param>
    /// <returns></returns>
    public static T SetEasing<T>(this T _thisDriver, ioDriver.EaseType _easeType) where T : ioDriver.IDMapped
    {
        _thisDriver.Ease(_easeType);
        return _thisDriver;
    }

    /// <summary>
    /// Fluency method. See <see cref="ioDriver.DMapped{TTar,TDri}.EaseCustom"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <param name="_customEaseTypeKey"></param>
    /// <returns></returns>
    public static T SetCustomEasing<T>(T _thisDriver, object _customEaseTypeKey) where T : ioDriver.IDMapped
    {
        _thisDriver.EaseCustom(_customEaseTypeKey);
        return _thisDriver;
    }

    /// <summary>
    /// Fluency method. See <see cref="ioDriver.DMapped{TTar,TDri}.ClampDrive"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <param name="_clampMinPct"></param>
    /// <param name="_clampMaxPct"></param>
    /// <returns></returns>
    public static T SetClampPctDrive<T>(this T _thisDriver, float _clampMinPct, float _clampMaxPct) where T : ioDriver.IDMapped
    {
        _thisDriver.ClampDrive(_clampMinPct, _clampMaxPct);
        return _thisDriver;
    }
    #endregion
    #region DTween Fluency
    /// <summary>
    /// Fluency method. See <see cref="ioDriver.DTween{TTar}.LoopCycleDuration"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <param name="_cycleDuration"></param>
    /// <returns></returns>
    public static T SetLoopCycleDuration<T>(this T _thisDriver, float _cycleDuration) where T : ioDriver.IDTween
    {
        _thisDriver.LoopCycleDuration = _cycleDuration;
        return _thisDriver;
    }
    #endregion
    #region ILoopable Fluency
    /// <summary>
    /// Fluency method. See <see cref="ioDriver.ILoopable.LoopPingPong"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <param name="_cycleCount"></param>
    /// <param name="_startPct"></param>
    /// <returns></returns>
    public static T SetLoopPingPong<T>(this T _thisDriver, float _cycleCount = float.PositiveInfinity, float _startPct = 0) where T : ioDriver.ILoopable
    {
        _thisDriver.LoopPingPong(_cycleCount, _startPct);
        return _thisDriver;
    }

    /// <summary>
    /// Fluency method. See <see cref="ioDriver.ILoopable.LoopRepeat"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <param name="_cycleCount"></param>
    /// <param name="_startPct"></param>
    /// <returns></returns>
    public static T SetLoopRepeat<T>(this T _thisDriver, float _cycleCount = float.PositiveInfinity, float _startPct = 0) where T : ioDriver.ILoopable
    {
        _thisDriver.LoopRepeat(_cycleCount, _startPct);
        return _thisDriver;
    }

    /// <summary>
    /// Fluency method. See <see cref="ioDriver.ILoopable.LoopOnce"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_thisDriver"></param>
    /// <returns>This driver</returns>
    public static T SetLoopOnce<T>(this T _thisDriver) where T : ioDriver.ILoopable
    {
        _thisDriver.LoopOnce();
        return _thisDriver;
    }

    #endregion

    /// <summary>
    /// Fluency Method.  <see cref="ioDriver.Path.Base{T}.Closed"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_path"></param>
    /// <param name="_closed"></param>
    /// <returns>This path</returns>
    public static T SetClosed<T>(this T _path, bool _closed) where T : ioDriver.IPath
    {
        _path.Closed = _closed;
        return _path;
    }

    /// <summary>
    /// Fluency Method.  <see cref="ioDriver.Base{T}.BeforeUpdate"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_driver"></param>
    /// <param name="_beforeUpdate"></param>
    /// <returns>This driver</returns>
    public static T SetBeforeUpdate<T>(this T _driver, Action<T> _beforeUpdate) where T : ioDriver.DBase
    {
        _driver.BeforePump = () => _beforeUpdate(_driver);
        return _driver;
    }

    public static T SetAfterUpdate<T>(this T _driver, Action<T> _afterUpdate) where T : ioDriver.DBase
    {
        _driver.AfterPump = () => _afterUpdate(_driver);
        return _driver;
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

    /// <summary>
    /// Fluency Method. <seealso cref="ioDriver.Path.Spline{T}.ModeSLSegmentLen"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_spline"></param>
    /// <param name="_length"></param>
    /// <returns></returns>
    public static T SetSegmentLength<T>(this T _spline, float _length)
        where T : ioDriver.ISpline
    {
        _spline.ModeSLSegmentLen = _length;
        return _spline;
    }

    /// <summary>
    /// Fluency Method. <seealso cref="ioDriver.Path.Spline{T}.ModeSLSegmentAcc"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_spline"></param>
    /// <param name="_accuracy"></param>
    /// <returns></returns>
    public static T SetSegmentAccuracy<T>(this T _spline, float _accuracy)
        where T : ioDriver.ISpline
    {
        _spline.ModeSLSegmentAcc = _accuracy;
        return _spline;
    }

    /// <summary>
    /// Fluency Method.  See <see cref="ioDriver.Path.Spline{T}.PointMode"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_spline"></param>
    /// <param name="_pointMode"></param>
    /// <returns></returns>
    public static T SetPointMode<T>(this T _spline, ioDriver.Path.SplinePointMode _pointMode)
        where T : ioDriver.ISpline
    {
        _spline.PointMode = _pointMode;
        return _spline;
    }

    /// <summary>
    /// Fluency method. See <see cref="ioDriver.Path.Spline{T}.ModePCPointCount"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_spline"></param>
    /// <param name="_pointCount"></param>
    /// <returns></returns>
    public static T SetModePCPointCount<T>(this T _spline, int _pointCount) where T : ioDriver.ISpline
    {
        _spline.ModePCPointCount = _pointCount;
        return _spline;
    }

    /// <summary>
    /// Fluency method. See <see cref="ioDriver.Path.Spline{T}.ModeMAMinAngle"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_spline"></param>
    /// <param name="_minAngle"></param>
    /// <returns></returns>
    public static T SetModeMAMinAngle<T>(this T _spline, float _minAngle) where T : ioDriver.ISpline
    {
        _spline.ModeMAMinAngle = _minAngle;
        return _spline;
    }

    /// <summary>
    /// Fluency method. See <see cref="ioDriver.Path.Spline{T}.ModeMAMinLength"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_spline"></param>
    /// <param name="_length"></param>
    /// <returns></returns>
    public static T SetModeMAMinLength<T>(this T _spline, float _length) where T : ioDriver.ISpline
    {
        _spline.ModeMAMinLength = _length;
        return _spline;
    }

    /// <summary>
    /// Fluency Method. See <see cref="ioDriver.Path.Spline{T}.DimsToSpline"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_spline"></param>
    /// <param name="_dimsToSpline"></param>
    /// <returns></returns>
    public static T SetDimsToSpline<T>(this T _spline, params int[] _dimsToSpline)
        where T : ioDriver.ISpline
    {
        _spline.DimsToSpline = _dimsToSpline;
        return _spline;
    }
}