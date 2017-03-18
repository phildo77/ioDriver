
using System;
using System.Linq.Expressions;

public static partial class ioDriver
{
	/// <summary>
	/// Drive target directly with drive value.
	/// </summary>
	/// This driver is a fancy way to lock target to driver value:  Target = Driver
	/// <typeparam name="TTar">Target/Driver Type</typeparam>
	public class DDirect<TTar> : DTransfer<TTar, TTar>
	{

		/// Get string representation of driver type
		public override string NiceType { get { return "Direct<" + typeof(TTar) + ">"; } }

		/// <summary>
		/// Direct drive constructor. <seealso cref="DTransfer{TTar,TDri}(System.Action{TTar},System.Func{TDri},string)"/>
		/// </summary>
		public DDirect(Action<TTar> _tarAction, Func<TTar> _driver, string _name = null)
			: base(_tarAction, _driver, _name) { }

		/// <summary>
		/// Direct drive constructor. <seealso cref="DTransfer{TTar,TDri}(System.Linq.Expressions.Expression{System.Func{TTar}},System.Func{TDri},string)"/>
		/// </summary>
		public DDirect(Expression<Func<TTar>> _targetExpr, Func<TTar> _driver, string _name = null)
			: base(_targetExpr, _driver, _name) { }

		///  Returns <param name="_drive"/>.  See <see cref="DTransfer{TTar,TDri}.DriveToTarget"/>
		protected override TTar DriveToTarget(TTar _drive) { return _drive; }

	}

	/// <summary>
	/// Base Mapped Driver.  Drive target over specified range with mapped drive points.
	/// </summary>
	/// Linearly map two drive points to two points on some target range.  Can be eased. See <see cref="EaseType"/>
	/// <typeparam name="TTar">Target type</typeparam>
	/// <typeparam name="TDri">Drive type</typeparam>
	public abstract class DMapped<TTar, TDri> : DTransfer<TTar, TDri>, IDMapped
	{
		private static Teacher.FuncILerp<TDri> m_ILerpCache;
		private Func<float, float> m_EaseCache;
		private static bool m_CacheDone = false;

		/// Drive value "from" / "A" map point.
		protected TDri m_DriveMapFrom;
		/// Drive value "to" / "B" map point.
		protected TDri m_DriveMapTo;
		/// Custom ease type key.  <see cref="Easing.DefineCustomEasingFunc"/>
		protected object m_EaseTypeCustomKey;

		/// Minimum percentage (1 = 100%) that drive value will be clamped to.
		public float ClampDriveMinPct { get; set; }
		/// Maximum percentage (1 = 100%) that drive value will be clamped to.
		public float ClampDriveMaxPct { get; set; }


		/// Get driver's drive percent progress (debug mode)
		public float DebugDrivePct { get; private set; }
		/// Get driver's clamped drive percent progress (debug mode)
		public float DebugClampedDrivePct { get; private set; }
		/// Get driver's eased percent progress (debug mode)
		public float DebugEasedPct { get; private set; }

		/// See <see cref="ioDriver.EaseType"/>
		public EaseType easeType { get; protected set; }
		/// See <see cref="Easing.DefineCustomEasingFunc"/>
		public object EaseTypeCustomKey
		{
			get { return m_EaseTypeCustomKey; }
			private set
			{
				easeType = EaseType.Custom;
				m_EaseTypeCustomKey = value;
			}
		}


		/// <summary>
		/// Mapped driver constructor. <seealso cref="DBase"/>
		/// </summary>
		/// <param name="_driverMapA">Drive map point A.</param>
		/// <param name="_driverMapB">Drive map point B.</param>
		protected DMapped(Action<TTar> _tarAction, Func<TDri> _driver, TDri _driverMapA, TDri _driverMapB, string _name = null)
			: base(_tarAction, _driver, _name)
		{
			if (!m_CacheDone) DoCache();
			InitMapped();
			m_DriveMapFrom = _driverMapA;
			m_DriveMapTo = _driverMapB;
		}

		/// <summary>
		/// Mapped driver constructor. <seealso cref="DBase"/>
		/// </summary>
		/// <param name="_driverMapA">Drive map point A.</param>
		/// <param name="_driverMapB">Drive map point B.</param>
		protected DMapped(Expression<Func<TTar>> _targetExpr, Func<TDri> _driver, TDri _driverMapA, TDri _driverMapB, string _name = null)
			: base(_targetExpr, _driver, _name)
		{
			if (!m_CacheDone) DoCache();
			InitMapped();
			m_DriveMapFrom = _driverMapA;
			m_DriveMapTo = _driverMapB;

			

		}


		private void DoCache()
		{
			m_ILerpCache = DTypeInfo<TDri>.ILerp;
			m_CacheDone = true;
		}

		private void InitMapped()
		{
			m_EaseCache = Easing.GetEasingFunction(Defaults.easeType);
			DebugDrivePct = DebugClampedDrivePct = DebugEasedPct = float.NaN;
            ClampDriveMinPct = float.NegativeInfinity;
		    ClampDriveMaxPct = float.PositiveInfinity;
            AddCustomDebug("Drive %", () => DebugDrivePct);
			AddCustomDebug("Drive % (Clamped)", () => DebugClampedDrivePct);
			AddCustomDebug("Target % (Eased)", () => DebugEasedPct);
			AddCustomDebug("Target % (Clamped)", () => DebugEasedPct);
		}

		/// <summary>
		/// Override this to tell the driver how to map drive percent (DriveMapA at 0 and DriveMapB at 1)
		/// This method is run AFTER any easing has been done.
		/// </summary>
		/// <param name="_pct">Percent (1 = 100%) of drive value after any easing.</param>
		/// <returns>TTar value to send to target action.</returns>
		protected abstract TTar PctToTarget(float _pct);

		/// Core Mapped Drive to Target functionality.  Calculates percentage progress of drive
		/// along drive map end points, clamps it (if applicable), eases it (if applicable), 
		/// then returns the result of this value passed through <see cref="PctToTarget"/>
		protected sealed override TTar DriveToTarget(TDri _drive)
		{
			if (DebugEnable || ioDriver.DebugEnableGlobal) return DriveToTargetDebug(_drive);
			var drivePct = m_ILerpCache(m_DriveMapFrom, m_DriveMapTo, _drive);
            if (drivePct > ClampDriveMaxPct) drivePct = ClampDriveMaxPct;
            if (drivePct < ClampDriveMinPct) drivePct = ClampDriveMinPct;
			var easedPct = m_EaseCache(drivePct);
			return PctToTarget(easedPct);
		}

		/// Drive to Target functionality for debug mode.  (not ideal performance)
		protected TTar DriveToTargetDebug(TDri _drive)
		{
			DebugDrivePct = DebugClampedDrivePct = m_ILerpCache(m_DriveMapFrom, m_DriveMapTo, _drive);
            if (DebugClampedDrivePct > ClampDriveMaxPct)
                DebugClampedDrivePct = ClampDriveMaxPct;
            if (DebugClampedDrivePct < ClampDriveMinPct)
                DebugClampedDrivePct = ClampDriveMinPct;
			if (DebugClampedDrivePct == float.PositiveInfinity || DebugClampedDrivePct == float.NegativeInfinity || DebugClampedDrivePct == float.NaN)
			{
                Log.Err("Mapped Error: Drive Percent is " + DebugClampedDrivePct + " after drive clamp (Min: " + ClampDriveMinPct + " Max: " + ClampDriveMaxPct + "). Forcing Zero");
				DebugClampedDrivePct = 0;
			}

			DebugEasedPct = m_EaseCache(DebugClampedDrivePct);

            return PctToTarget(DebugEasedPct);
		}

		/// <summary>Set this driver's <see cref="EaseType"/> </summary>
		/// <param name="_easeType"><see cref="EaseType"/> to set this driver to.</param>
		public void Ease(EaseType _easeType)
		{
			if (_easeType == EaseType.Custom)
			{
				Log.Err("Set Custom Easing with EaseCustom(object _customEaseType).  Setting easing to default: " +
					Defaults.easeType);
				easeType = Defaults.easeType;
			}
			else
				easeType = _easeType;
			m_EaseCache = Easing.GetEasingFunction(easeType);

		}

		/// <summary>Set this driver's ease type to a custom user defined ease type.</summary>
		/// <param name="_customEaseTypeKey">User defined key for custom ease type.</param>
		public void EaseCustom(object _customEaseTypeKey)
		{
			if (!m_CustomEaseFuncs.ContainsKey(_customEaseTypeKey))
			{
				Log.Err("Custom key " + _customEaseTypeKey + " not defined!  Setting easeType to Default: " + Defaults.easeType);
				easeType = Defaults.easeType;
				m_EaseCache = Easing.GetEasingFunction(easeType);
				return;
			}
			EaseTypeCustomKey = _customEaseTypeKey;
			m_EaseCache = m_CustomEaseFuncs[EaseTypeCustomKey];
		}

		/// <summary>
		/// Sets driver's drive clamp range to the specified values as a percentage (1 = 100%) of the Drive span when created.
		/// </summary>
		/// <param name="_clampMinPercent">Drive value will be clamped to _clampMinPercent of the span if it tries to Drive below _clampMinPercent of the span.</param>
		/// <param name="_clampMaxPercent">Drive value will be clamped to _clampMaxPercent of the span if it tries to Drive above _clampMaxPercent of the span.</param>
		public void ClampDrive(float _clampMinPercent, float _clampMaxPercent)
		{
			ClampDriveMaxPct = _clampMaxPercent;
			ClampDriveMinPct = _clampMinPercent;
		}
	}

	/// <summary>
	/// Drive target defined by end points with drive map.
	/// </summary>
	/// Linearly maps Driver Map Point A to Target "From" and Driver Map Point B to Target "To".
	/// <typeparam name="TTar">Target type</typeparam>
	/// <typeparam name="TDri">Drive type</typeparam>
	public class DMappedSimple<TTar, TDri> : DMapped<TTar, TDri>
	{
		private static Teacher.FuncLerp<TTar> m_LerpCache;
		private static bool m_CacheDone = false;
		private readonly TTar m_From;
		private readonly TTar m_To;

		/// Get string representation of driver type
		public override string NiceType { get { return "MappedSimple<" + typeof(TTar) + "," + typeof(TDri) + ">"; } }


		/// <summary>
		/// Simple mapped action constructor.
		/// <seealso cref="DMapped{TTar,TDri}"/>
		/// </summary>
		/// <param name="_from">Target value to map to Driver Map Point A</param>
		/// <param name="_to">Target value to map to Driver Map Point B</param>
		public DMappedSimple(Action<TTar> _tarAction, TTar _from, TTar _to, Func<TDri> _driver, TDri _driverMapA, TDri _driverMapB, string _name = null)
			: base(_tarAction, _driver, _driverMapA, _driverMapB, _name)
		{
			if (!m_CacheDone) DoCache();
			m_From = _from;
			m_To = _to;
		}

		/// <summary>
		/// Simple mapped expression constructor.
		/// <seealso cref="DMapped{TTar,TDri}"/>
		/// </summary>
		/// <param name="_from">Target value to map to Driver Map Point A</param>
		/// <param name="_to">Target value to map to Driver Map Point B</param>
		public DMappedSimple(Expression<Func<TTar>> _targetExpr, TTar _from, TTar _to, Func<TDri> _driver, TDri _driverMapA, TDri _driverMapB, string _name = null)
			: base(_targetExpr, _driver, _driverMapA, _driverMapB, _name)
		{
			if (!m_CacheDone) DoCache();
			m_From = _from;
			m_To = _to;
		}

		private static void DoCache()
		{
			m_LerpCache = DTypeInfo<TTar>.Lerp;
			m_CacheDone = true;
		}


		/// <summary>
		/// Tell the mapped driver how to lerp the mapped target. Required override.
		/// </summary>
		/// <param name="_pct">Percent (1f = 100%) to lerp the target.  (After any easing)</param>
		/// <returns>Target value to set the target to.</returns>
		protected override TTar PctToTarget(float _pct)
		{
			return m_LerpCache(m_From, m_To, _pct);
		}
	}


	/// Fluency interface.  See <see cref="DMapped{TTar,TDri}"/>
	public interface IDMapped
	{
        /// See <see cref="DMapped{TTar,TDri}.Ease"/>
	    void Ease(EaseType _easeType);
	    /// See <see cref="DMapped{TTar,TDri}.EaseCustom"/>
	    void EaseCustom(object _customEaseTypeKey);
		/// See <see cref="DMapped{TTar,TDri}.ClampDrive"/>
		void ClampDrive(float _clampMinPercent, float _clampMaxPercent);
		/// See <see cref="DMapped{TTar,TDri}.easeType"/>
		EaseType easeType { get; }
		/// See <see cref="DMapped{TTar,TDri}.EaseTypeCustomKey"/>
		object EaseTypeCustomKey { get; }
        /// See <see cref="DMapped{TTar,TDri}.DebugEasedPct"/>
        float DebugEasedPct { get; }

		/// See <see cref="DMapped{TTar,TDri}.ClampDriveMaxPct"/>
		float ClampDriveMaxPct { get; set; }
		/// See <see cref="DMapped{TTar,TDri}.ClampDriveMinPct"/>
		float ClampDriveMinPct { get; set; }
		/// See <see cref="DMapped{TTar,TDri}.DebugDrivePct"/>
		float DebugDrivePct { get; }
		/// See <see cref="DMapped{TTar,TDri}.DebugClampedDrivePct"/>
		float DebugClampedDrivePct { get; }

	}

    /* TODO Implement?
    public interface IEaseable
    {
        
    }
     * */

	/// <summary>
	/// Drive target at a specified rate. 
	/// </summary>
	/// <typeparam name="TTar">Target Type</typeparam>
	public class DRate<TTar> : DTransfer<TTar, TTar>
	{
		private static TTar m_ZeroCache;
		private static Teacher.FuncLerp<TTar> m_LerpCache;

		/// Get string representation of driver type
		public override string NiceType { get { return "Rate<" + typeof(TTar) + ">"; } }

		/// <summary>
		/// Rate driver action constructor.
		/// <seealso cref="DBase"/>
		/// </summary>
		/// <param name="_ratePerSec">Rate per second</param>
		public DRate(Action<TTar> _tarAction, Func<TTar> _ratePerSec, string _name = null)
			: base(_tarAction, _ratePerSec, _name)
		{
			DoCache();
		}

		/// <summary>
		/// Rate driver expression constructor.
		/// <seealso cref="DBase"/>
		/// </summary>
		/// <param name="_ratePerSec">Rate per second</param>
		public DRate(Expression<Func<TTar>> _targetExpr, Func<TTar> _ratePerSec, string _name = null)
			: base(_targetExpr, _ratePerSec, _name)
		{
			DoCache();
		}

		private void DoCache()
		{
			if (m_ZeroCache == null) m_ZeroCache = DTypeInfo<TTar>.Zero;
			if (m_LerpCache == null) m_LerpCache = DTypeInfo<TTar>.Lerp;
		}

		/// <summary>
		/// Tell the driver how to drive the target.  Required override.
		/// </summary>
		/// <param name="_drive">Drive value.</param>
		/// <returns>Value to set the target to.</returns>
		protected override TTar DriveToTarget(TTar _drive)
		{
			return m_LerpCache(m_ZeroCache, _drive, SecsSinceLastUpdate);
		}
	}

	/// <summary>
	/// Base Speed Driver. Drive target at a specified rate over defined target range.  
	/// </summary>
	/// Can be looped (<see cref="LoopType"/>). 
	/// Slightly different than <see cref="DRate{TTar}"/> in that it can be looped and thus requires more information (<see cref="DSpeed{TTar}.TargetLen()"/> and <see cref="DSpeed{TTar}.TargetLerp(float)"/>)
	/// <typeparam name="TTar">Range waypoint type</typeparam>
	public abstract class DSpeed<TTar> : DTransfer<TTar, float>, ILoopable
	{

		/// Number of cycles to run this driver.
		public float LoopCycleCount { get; set; }
		/// Progress in current cycle (0 to 1f)
		public float LoopCycleProgress { get; private set; }
		/// Progress in total cycle count (0 to LoopCycleCount)
		public float TotalLoopingProgress { get; private set; }
		/// The last update's "in-cycle lerp" percent
		public PingPongDir PingPongDirection { get; private set; }

		/// <summary>
		/// Time in seconds that have elapsed in current loop cycle.
		/// </summary>
		public float ElapsedCycleTime { get; protected set; }

		/// <summary>
		/// Get or set this driver's <see cref="loopType"/>
		/// </summary>
		public LoopType loopType { get; private set; }

		/// <summary>
		/// Base speed by action constructor.
		/// <seealso cref="DBase"/>
		/// </summary>
		/// <param name="_speedDriver">Function that provides this driver's speed value.  Read from on each update call.</param>
		protected DSpeed(Action<TTar> _tarAction, Func<float> _speedDriver, string _name = null)
			: base(_tarAction, _speedDriver, _name)
		{
			PingPongDirection = PingPongDir.Forward;
			LoopCycleProgress = 0;
			loopType = LoopType.Once;
			TotalLoopingProgress = LoopCycleProgress = ElapsedCycleTime = 0f;
			LoopCycleCount = 1f;
		}

		/// <summary>
		/// Base speed by expression constructor.
		/// <seealso cref="DBase"/>
		/// </summary>
		/// <param name="_speedDriver">Function that provides this driver's speed value.  Read from on each update call.</param>
		protected DSpeed(Expression<Func<TTar>> _targetExpr, Func<float> _speedDriver, string _name = null)
			: base(_targetExpr, _speedDriver, _name)
		{
			PingPongDirection = PingPongDir.Forward;
			LoopCycleProgress = 0;
			loopType = LoopType.Once;
			TotalLoopingProgress = LoopCycleProgress = ElapsedCycleTime = 0f;
			LoopCycleCount = 1f;
		}


		/// Function defining the relative length of the target.  Needed to calculate current progress along target for looping.
		protected abstract float TargetLen();
		/// Function that returns the target value at specified position in percent (0 to 1f).
		protected abstract TTar TargetLerp(float _pct);

		/// <summary>
		/// Core speed driver functionality.
		/// </summary>
		/// <param name="_drive">Speed value (retrieved from <see cref="DTransfer{TTar,TDri}.Driver"/>)</param>
		/// <returns>Value to apply to <see cref="DTransfer{TTar,TDri}.TarAction">Target</see></returns>
		protected sealed override TTar DriveToTarget(float _drive)
		{
			ElapsedCycleTime += SecsSinceLastUpdate;
			var dist = _drive * SecsSinceLastUpdate;
			var pctDist = dist / TargetLen();
			TotalLoopingProgress += pctDist;
			var curPct = LoopCycleProgress + pctDist;
			if (TotalLoopingProgress >= LoopCycleCount)
			{
				Stop();
				curPct = LoopCycleProgress + LoopCycleCount - (TotalLoopingProgress - pctDist);
				TotalLoopingProgress = LoopCycleCount;
			}
			else if (loopType == LoopType.PingPong)
			{
				if (PingPongDirection == PingPongDir.Backward)
				{
					curPct = LoopCycleProgress - pctDist;
					if (curPct <= 0f)
					{
					    if (OnLoopCycleComplete != null)
					        OnLoopCycleComplete();
					    if (OnLoopPingPongBackComplete != null)
					        OnLoopPingPongBackComplete();
						SwitchPingPongDir();
						curPct = -curPct;
						ElapsedCycleTime = 0;
					}
				}
				else if (curPct >= 1f)
				{
				    if (OnLoopCycleComplete != null)
				        OnLoopCycleComplete();
				    if (OnLoopPingPongForeComplete != null)
				        OnLoopPingPongForeComplete();
					SwitchPingPongDir();
					curPct = 1 - (curPct - 1);
					ElapsedCycleTime = 0;
				}
			}
			else if (curPct >= 1f)
			{
				if (loopType == LoopType.Once)
				{
					Stop();
					curPct = 1f;
				}
				else if (loopType == LoopType.Repeat)
				{
					curPct = curPct % 1f;
				    if (OnLoopCycleComplete != null)
				        OnLoopCycleComplete();
					ElapsedCycleTime = 0;
				}
			}
			LoopCycleProgress = curPct;
			return TargetLerp(LoopCycleProgress);
		}

		private void SwitchPingPongDir()
		{
			PingPongDirection = PingPongDirection == PingPongDir.Backward ? PingPongDir.Forward : PingPongDir.Backward;
		}

		/// <summary>
		/// Sets this speed driver to loop once mode.
		/// </summary>
		public void LoopOnce()
		{
			loopType = LoopType.Once;
			LoopCycleCount = 1;
		}

		/// <summary>
		/// Sets this speed driver to loop repeat mode.
		/// </summary>
		/// <param name="_loopCycleCount">Number of cycles to run.  Can be partial (ex. 0.2f, 1.7f, etc.)</param>
		/// <param name="_startPct">Percent along target to start (0 to 1f)</param>
		public void LoopRepeat(float _loopCycleCount = float.PositiveInfinity, float _startPct = 0)
		{
			loopType = LoopType.Repeat;
			LoopCycleCount = _loopCycleCount;
			LoopCycleProgress = _startPct;
		}

        /// See <see cref="ILoopable.OnLoopCycleComplete"/>
	    public event Action OnLoopCycleComplete;
        /// See <see cref="ILoopable.OnLoopPingPongForeComplete"/>
	    public event Action OnLoopPingPongForeComplete;
        /// See <see cref="ILoopable.OnLoopPingPongBackComplete"/>
	    public event Action OnLoopPingPongBackComplete;

	    /// <summary>
		/// Sets this speed driver to loop ping pong mode.
		/// </summary>
		/// <param name="_loopCycleCount">Number of cycles to run.  Can be partial (ex. 0.2f, 1.7f, etc.)</param>
		/// <param name="_startPct">Percent along target to start (0 to 1f)</param>
		public void LoopPingPong(float _loopCycleCount = float.PositiveInfinity, float _startPct = 0)
		{
			loopType = LoopType.PingPong;
			LoopCycleCount = _loopCycleCount;
			LoopCycleProgress = _startPct;
		}
	}

	/// <summary>
	/// Drive target at a specified rate through two defined endpoints. 
	/// </summary>
	/// Can be looped (<see cref="LoopType"/>). 
	/// <typeparam name="TTar">Waypoint type</typeparam>
	public class DSpeedSimple<TTar> : DSpeed<TTar>
	{
		private static bool m_CacheDone = false;
		private static Teacher.FuncLerp<TTar> m_LerpCache;
		private static Teacher.FuncLength<TTar> m_LengthCache;

		/// Get string representation of driver type
		public override string NiceType { get { return "SpeedSimple<" + typeof(TTar) + ">"; } }

		/// Start point 
		public TTar From { get; private set; }
		/// End point
		public TTar To { get; private set; }


		/// <summary>
		/// Simple speed by action constructor.
		/// <seealso cref="DSpeed{TTar}"/>
		/// </summary>
		/// <param name="_from">Start point</param>
		/// <param name="_to">End point</param>
		/// <param name="_speedDriver">Function that provides instantaneous speed value.</param>
		public DSpeedSimple(Action<TTar> _tarAction, TTar _from, TTar _to, Func<float> _speedDriver, string _name = null)
			: base(_tarAction, _speedDriver, _name)
		{
			if (!m_CacheDone) DoCache();
			From = _from;
			To = _to;
		}

		/// <summary>
		/// Simple speed by expression constructor.
		/// <seealso cref="DSpeed{TTar}"/>
		/// </summary>
		/// <param name="_from">Start point</param>
		/// <param name="_to">End point</param>
		/// <param name="_speedDriver">Function that provides instantaneous speed value.</param>
		public DSpeedSimple(Expression<Func<TTar>> _targetExpr, TTar _from, TTar _to, Func<float> _speedDriver, string _name = null)
			: base(_targetExpr, _speedDriver, _name)
		{
			if (!m_CacheDone) DoCache();
			From = _from;
			To = _to;
		}


		private void DoCache()
		{
			m_LerpCache = DTypeInfo<TTar>.Lerp;
			m_LengthCache = DTypeInfo<TTar>.Length;
			m_CacheDone = true;
		}

		/// Returns the relative length between <see cref="From"/> and <see cref="To"/>
		protected override float TargetLen() { return m_LengthCache(From, To); }
		/// Returns "LERP'ed" value between <see cref="From"/> and <see cref="To"/> at specified percent (0 to 1f)
		protected override TTar TargetLerp(float _pct) { return m_LerpCache(From, To, _pct); }

	}

	/// <summary>
	/// Drive target at specified additive step rate.
	/// </summary>
	/// <typeparam name="TTar">Target Type</typeparam>
	public class DStep<TTar> : DTransfer<TTar, TTar>
	{
        /// Current additive value applied to target during update pump.
		public TTar Current;

		private static bool m_CacheDone = false;
		private static Teacher.FuncLerp<TTar> m_LerpCache;
		private static TTar m_ZeroCache;
		private static Teacher.FuncAdd<TTar> m_AddCache;

		/// Get string representation of driver type
		public override string NiceType { get { return "Step<" + typeof(TTar) + ">"; } }


		/// <summary>
		/// Step driver by action constructor.
		/// <seealso cref="DBase"/>
		/// </summary>
		/// <param name="_startFrom">Value to begin stepping from.</param>
		/// <param name="_stepRate">Step rate in seconds.</param>
		public DStep(Action<TTar> _tarAction, Func<TTar> _startFrom, Func<TTar> _stepRate, string _name = null)
			: base(_tarAction, null, _name)
		{
            if (!m_CacheDone) DoCache();
            OnStart += () => { Current = _startFrom(); };
			Driver = _stepRate;
		}

		/// <summary>
		/// Step driver by expression constructor.
		/// <seealso cref="DBase"/>
		/// </summary>
		/// <param name="_startFrom">Value to begin stepping from.</param>
		/// <param name="_stepRate">Step rate in seconds.</param>
		public DStep(Expression<Func<TTar>> _targetExpr, Func<TTar> _startFrom, Func<TTar> _stepRate, string _name = null)
			: base(_targetExpr, null, _name)
		{
            if (!m_CacheDone) DoCache();
            OnStart += () => { Current = _startFrom(); };
            Driver = _stepRate;
		}


		private void DoCache()
		{
			m_LerpCache = DTypeInfo<TTar>.Lerp;
			m_ZeroCache = DTypeInfo<TTar>.Zero;
			m_AddCache = DTypeInfo<TTar>.Add;
			m_CacheDone = true;
		}

		/// <summary>
		/// Core step driver functionality.
		/// </summary>
		/// <param name="_drive">Step rate (retrieved from <see cref="DTransfer{TTar,TDri}.Driver"/>)</param>
		/// <returns>Value to apply to target</returns>
		protected override TTar DriveToTarget(TTar _drive)
		{
			var adder = m_LerpCache(m_ZeroCache, _drive, SecsSinceLastUpdate);
			var result = m_AddCache(Current, adder);
			Current = result;
			return result;
		}

	}



	/// Fluency interface.  See <see cref="DTween{TTar}"/>

	public interface IDTween
	{
		/// See <see cref="DTween{TTar}.LoopCycleDuration"/>
		float LoopCycleDuration { get; set; }
	}

	/// <summary>
	/// Base Tween Driver. Drive target through defined target range over specified duration.
	/// </summary>
	/// Can be eased (<see cref="EaseType"/> and/or looped (<see cref="LoopType"/>). 
	/// <typeparam name="TTar">Target type</typeparam>


	public abstract class DTween<TTar> : DMapped<TTar, float>, IDTween, ILoopable
	{
		/// Get this driver's current ping-pong direction. <seealso cref="PingPongDir"/>
		public PingPongDir PingPongDirection { get; private set; }
		/// Duration in seconds for one loop cycle.
		public float LoopCycleDuration { get; set; }
		/// Total number of cycles to run
		public float LoopCycleCount { get; protected set; }
		/// Progress in current cycle in percent (0 to 1f)
		public float LoopCycleProgress { get; protected set; }
		/// Current progress along <see cref="LoopCycleCount"/> (0 to <see cref="LoopCycleCount"/>)
		public float TotalLoopingProgress { get; protected set; }

		/// <summary>
		/// Time in seconds elapsed in current loop cycle.
		/// </summary>
		public float ElapsedCycleTime { get; private set; }

		/// <summary>
		/// Get or set this driver's <see cref="loopType"/>
		/// </summary>
		public LoopType loopType { get; private set; }


        /// <summary>
		/// Base tween by action constructor.
		/// <seealso cref="DMapped{TTar,TDri}"/>
		/// </summary>
		/// <param name="_cycleDuration">Duration to tween over in seconds.</param>
		protected DTween(Action<TTar> _tarAction, float _cycleDuration, string _name = null)
			: base(_tarAction, null, 0, 1f, _name)
		{
			PingPongDirection = PingPongDir.Forward;
			LoopCycleDuration = _cycleDuration;
			LoopCycleCount = 1f;
			loopType = LoopType.Once;
			Driver = this.DoDrive;
			TotalLoopingProgress = LoopCycleProgress = ElapsedCycleTime = 0;
		}

		/// <summary>
		/// Base tween by expression constructor.
		/// <seealso cref="DMapped{TTar,TDri}"/>
		/// </summary>
		/// <param name="_cycleDuration">Duration to tween over in seconds.</param>
		protected DTween(Expression<Func<TTar>> _targetExpr, float _cycleDuration, string _name = null)
			: base(_targetExpr, null, 0, 1f, _name)
		{
			PingPongDirection = PingPongDir.Forward;
			LoopCycleDuration = _cycleDuration;
			LoopCycleCount = 1f;
			loopType = LoopType.Once;
			Driver = this.DoDrive;
			TotalLoopingProgress = LoopCycleProgress = ElapsedCycleTime = 0;
		}

        //! @cond PRIVATE

		/// <summary>
		/// Core tween functionality.
		/// </summary>
		/// <returns>Target progress percent (0 to 1f)</returns>
		protected virtual float DoDrive()
		{
			ElapsedCycleTime += SecsSinceLastUpdate;
			var progress = SecsSinceLastUpdate / LoopCycleDuration;
			LoopCycleProgress += progress;
			TotalLoopingProgress += progress;
			if (TotalLoopingProgress > LoopCycleCount)
			{
				Stop();
				LoopCycleProgress -= TotalLoopingProgress - LoopCycleCount;
				TotalLoopingProgress = LoopCycleCount;
			}
			if (LoopCycleProgress >= 1f && loopType != LoopType.Once)
			{
				if (loopType == LoopType.PingPong)
				{
					PingPongDirection = PingPongDirection == PingPongDir.Forward ? PingPongDir.Backward : PingPongDir.Forward;
					if (PingPongDirection == PingPongDir.Forward && OnLoopPingPongBackComplete != null)
						OnLoopPingPongBackComplete();
					else if (OnLoopPingPongForeComplete != null)
						OnLoopPingPongForeComplete();

				}

				ElapsedCycleTime = ElapsedCycleTime - LoopCycleDuration;
				LoopCycleProgress = LoopCycleProgress % 1f;
				if (OnLoopCycleComplete != null) OnLoopCycleComplete();
			}
			var val = LoopCycleProgress;

			if (loopType == LoopType.PingPong && PingPongDirection == PingPongDir.Backward)
				val = 1f - LoopCycleProgress;
			return val;
		}

		//! @endcond

        /// See <see cref="ILoopable.OnLoopCycleComplete"/>
		public event Action OnLoopCycleComplete;
        /// See <see cref="ILoopable.OnLoopPingPongForeComplete"/>
        public event Action OnLoopPingPongForeComplete;
        /// See <see cref="ILoopable.OnLoopPingPongBackComplete"/>
        public event Action OnLoopPingPongBackComplete;

		
		

	    /// <summary>
	    /// Sets this tween driver to loop once mode.
	    /// </summary>
	    public void LoopOnce()
	    {
	        LoopCycleCount = 1f;
	        loopType = LoopType.Once;
	    }

	    /// <summary>
	    /// Sets this tween driver to loop repeat mode.
	    /// </summary>
	    /// <param name="_loopCycleCount">Number of cycles to run.  Can be partial (ex. 0.2f, 1.7f, etc.)</param>
	    /// <param name="_startPct">Percent along target to start (0 to 1f)</param>
	    public void LoopRepeat(float _loopCycleCount = float.PositiveInfinity, float _startPct = 0)
	    {
	        LoopCycleCount = _loopCycleCount;
	        loopType = LoopType.Repeat;
	        LoopCycleProgress = TotalLoopingProgress = _startPct;
	        ElapsedCycleTime = LoopCycleDuration*_startPct;
	    }

	    /// <summary>
	    /// Sets this tween driver to loop ping pong mode.
	    /// </summary>
	    /// <param name="_loopCycleCount">Number of cycles to run.  Can be partial (ex. 0.2f, 1.7f, etc.)</param>
	    /// <param name="_startPct">Percent along target to start (0 to 1f)</param>
	    public void LoopPingPong(float _loopCycleCount = float.PositiveInfinity, float _startPct = 0)
	    {
	        LoopCycleCount = _loopCycleCount;
	        loopType = LoopType.PingPong;
	        LoopCycleProgress = TotalLoopingProgress = _startPct;
	        ElapsedCycleTime = LoopCycleDuration*_startPct;
	    }
	}


    /// <summary>
    /// Drive target linearly through two endpoints over specified duration.
    /// </summary>
    /// Can be eased (<see cref="EaseType"/> and/or looped (<see cref="LoopType"/>). 
    /// <typeparam name="TTar">Target Type</typeparam>
    public class DTweenSimple<TTar> : DTween<TTar>
    {
        private static Teacher.FuncLerp<TTar> m_LerpCache;
        private static bool m_CacheDone = false;

        /// Get string representation of driver type
        public override string NiceType
        {
            get { return "TweenSimple<" + typeof (TTar) + ">"; }
        }

        /// Target From value.  Target will be set to this value at duration of 0 seconds (at start).
        public TTar From { get; private set; }

        /// Target To value.  Target will be set to this value at end of duration.
        public TTar To { get; private set; }

        private readonly Func<TTar> m_FromOnStart;
        private readonly Func<TTar> m_ToOnStart;


        /// <summary>
        /// Simple Tween driver constructor. Target action with static start from/to <seealso cref="DTween{TTar}"/>
        /// </summary>
        /// <param name="_from">Tween target from.  Tween value at start time (zero seconds elapsed).</param>
        /// <param name="_to">Tween target to.  Tween value at end of duration (duration seconds elapsed).</param>
        /// <param name="_cycleDuration">Time in seconds for one cycle.  (Default is <see cref="LoopType.Once"/>)</param>
        public DTweenSimple(Action<TTar> _tarAction, TTar _from, TTar _to, float _cycleDuration, string _name = null) : base(_tarAction, _cycleDuration, _name)
        {
            if (!m_CacheDone) DoCache();
            From = _from;
            To = _to;
        }

        /// <summary>
        /// Simple Tween driver constructor. Target expression with static start from/to <seealso cref="DTween{TTar}"/>
        /// </summary>
        /// <param name="_from">Tween target from.  Tween value at start time (zero seconds elapsed).</param>
        /// <param name="_to">Tween target to.  Tween value at end of duration (duration seconds elapsed).</param>
        /// <param name="_cycleDuration">Time in seconds for one cycle.  (Default is <see cref="LoopType.Once"/>)</param>
        public DTweenSimple(Expression<Func<TTar>> _targetExpr, TTar _from, TTar _to, float _cycleDuration, string _name = null) : base(_targetExpr, _cycleDuration, _name)
        {
            if (!m_CacheDone) DoCache();
            From = _from;
            To = _to;
        }

        /// <summary>
        /// Simple Tween driver constructor.  Target Action with dynamic from/to (called at start)
        /// </summary>
        /// <param name="_tarAction">Target action</param>
        /// <param name="_fromOnStart">Function to set From value on driver start</param>
        /// <param name="_toOnStart">Function to set To value on driver start</param>
        /// <param name="_cycleDuration">Cycle duration</param>
        /// <param name="_name">Driver name</param>
        public DTweenSimple(Action<TTar> _tarAction, Func<TTar> _fromOnStart, Func<TTar> _toOnStart, float _cycleDuration, string _name = null) : base(_tarAction, _cycleDuration, _name)
        {
            if (!m_CacheDone) DoCache();
            m_FromOnStart = _fromOnStart;
            m_ToOnStart = _toOnStart;
            OnStart += () => From = m_FromOnStart();
            OnStart += () => To = m_ToOnStart();
        }

        /// <summary>
        /// Simple Tween driver constructor. Target expression with dynamic from/to (called at start)
        /// </summary>
        /// <param name="_targetExpr">Target expression</param>
        /// <param name="_fromOnStart">Function to set From value on driver start</param>
        /// <param name="_toOnStart">Function to set To value on driver start</param>
        /// <param name="_cycleDuration">Cycle duration</param>
        /// <param name="_name">Driver name</param>
        public DTweenSimple(Expression<Func<TTar>> _targetExpr, Func<TTar> _fromOnStart, Func<TTar> _toOnStart, float _cycleDuration, string _name = null) : base(_targetExpr, _cycleDuration, _name)
        {
            if (!m_CacheDone) DoCache();
            m_FromOnStart = _fromOnStart;
            m_ToOnStart = _toOnStart;
            OnStart += () => From = m_FromOnStart();
            OnStart += () => To = m_ToOnStart();
        }

        private static void DoCache()
        {
            m_LerpCache = DTypeInfo<TTar>.Lerp;
            m_CacheDone = true;
        }


        /// <summary>
        /// Returns the lerp of target between From -> To at _pct.
        /// Required override.
        /// </summary>
        /// <param name="_pct">Percent (1f = 100%) to lerp target value.</param>
        /// <returns>Value to set the target to.</returns>
        protected override TTar PctToTarget(float _pct)
        {
            return m_LerpCache(From, To, _pct);
        }
    }

}

