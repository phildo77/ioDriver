
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;


public static partial class ioDriver
{
    #region Nested Interfaces

    /// Untyped Interface for <see cref="Path.Base{T}"/>
    public interface IPath
    {
        #region Properties

        /// Get / Sets wether the path is closed.
        bool Closed
        {
            get;
            set;
        }

        /// Get the type of waypoints (T).
        Type WaypointType
        {
            get;
        }

        #endregion Properties

        #region Methods


        /// Try to get an array of the waypoints of the path as specified type.  Returns success.
        bool TryGetPathAs<T>(out T[] _points);

        /// <summary>
        /// Retrieve path's points as an array of floats.
        /// </summary>
        /// The float array's first dimension is ordered index of retrieved waypoints.  
        /// Second dimension is retreived data by waypoint dimension (x,y,z,w,etc.).
        /// <param name="_dimCount">Number of dimensions for a point</param>
        /// <returns>Path points</returns>
        float[,] GetPathPoints(out int _dimCount);

        /// <summary>
        /// Retrieve path's points as an array of <see cref="VecN"/>
        /// </summary>
        /// <returns>Points as VecN</returns>
        VecN[] GetPathPoints();

        #endregion Methods

    }


    /// Untyped interface for <see cref="Path.Spline{T}"/> objects. (Fluency helper)
    public interface ISpline
    {
        #region Properties

        /// See <see cref="Path.Spline{T}.DimsToSpline"/>
        int[] DimsToSpline
        {
            get;
            set;
        }

        /// See <see cref="Path.Spline{T}.PointMode"/>
        Path.SplinePointMode PointMode { get; set; }

        /// See <see cref="Path.Spline{T}.ModePCPointCount"/>
        int ModePCPointCount { get; set; }

        /// See <see cref="Path.Spline{T}.ModeSLSegmentLen"/>
        float ModeSLSegmentLen
        {
            get;
            set;
        }

        /// See <see cref="Path.Spline{T}.ModeSLSegmentAcc"/>
        float ModeSLSegmentAcc { get; set; }

        /// See <see cref="Path.Spline{T}.ModeMAMinAngle"/>
        float ModeMAMinAngle { get; set; }

        /// See <see cref="Path.Spline{T}.ModeMAMinLength"/>
        float ModeMAMinLength { get; set; }


        #endregion Properties
    }

    /// Interface for drivers that operate on a <see cref="Path.Base{T}"/>
    public interface IPathable
    {
        #region Methods

        /// Get untyped interface to <see cref="Path.Base{T}"/>
        IPath GetPath();

        #endregion Methods
    }

    #endregion Nested Interfaces

    #region Methods

    /// <summary>
    /// Create new cubic spline from this path/spline's frame.
    /// </summary>
    /// <typeparam name="T">Waypoint type</typeparam>
    /// <param name="_pathObj">this path object</param>
    /// <param name="_autoBuild">Enable Autobuild?</param>
    /// <returns>New cubic spline</returns>
    public static Path.Cubic<T> SplineCubic<T>(this Path.Base<T> _pathObj, bool _autoBuild = true)
    {
        return Path.CreateCubic(_pathObj, _autoBuild);
    }

    /// <summary>
    /// Create new Bezier spline from this path/spline's frame.
    /// </summary>
    /// <typeparam name="T">Waypoint type</typeparam>
    /// <param name="_pathObj">this path object</param>
    /// <param name="_autoBuild">Enable Autobuild?</param>
    /// <returns>New Bezier spline</returns>
    public static Path.Bezier<T> SplineBezier<T>(this Path.Base<T> _pathObj, bool _autoBuild = true)
    {
        return Path.CreateBezier(_pathObj, _autoBuild);
    }

    /// <summary>
    /// Create new linear path from this spline's frame.
    /// </summary>
    /// <typeparam name="T">Waypoint type</typeparam>
    /// <param name="_splineObj">this spline</param>
    /// <param name="_autoBuild">Enable Autobuild?</param>
    /// <returns>New linear path</returns>
    public static Path.Linear<T> CreateLinearFromFrame<T>(this Path.Spline<T> _splineObj, bool _autoBuild = true)
    {
        return Path.CreateLinear(_splineObj, _autoBuild);
    }

    /// <summary>
    /// Create new linear path from this spline's <see cref="Path.Base{T}.PathPoints">path points</see>.
    /// </summary>
    /// <typeparam name="T">Waypoint type</typeparam>
    /// <param name="_splineObj">this spline</param>
    /// <param name="_autoBuild">Enable Autobuild?</param>
    /// <returns>New linear path</returns>
    public static Path.Linear<T> CreateLinearFromSpline<T>(this Path.Spline<T> _splineObj, bool _autoBuild = true)
    {
        return Path.CreateLinear(_splineObj.Closed, ToList(_splineObj.PathPoints), _autoBuild);
    }





    private static VecN ToVecN<T>(T _obj)
    {
        var dimCnt = DTypeInfo<T>.DimCount;
        var vals = new float[dimCnt];
        for (int idx = 0; idx < dimCnt; ++idx)
            vals[idx] = DTypeInfo<T>.GetDimsFunc[idx + 1](_obj);
        return new VecN(vals);

        //TODO Cache
    }

    private static IEnumerable<VecN> ToVecNs<T>(IEnumerable<T> _pts)
    {
        return _pts.Select(ToVecN);
    }

    #endregion Methods

    #region Nested Types

    /// <summary>
    /// Drive target defined by path with drive map.
    /// </summary>
    /// Proportional drive to target path Map.  Driver Map point A is mapped to the beginning of the path and Driver Map point B is mapped to the end of the path.
    /// <para>Shorthand: <see cref="Map{TTar,TDri}(Action{TTar},Path.Base{TTar},Func{TDri},TDri,TDri,string)"/></para>
    /// <para>Shorthand: <see cref="Map{TTar,TDri}(Expression{Func{TTar}},Path.Base{TTar},Func{TDri},TDri,TDri,string)"/></para>
    /// <seealso cref="DMapped{TTar,TDri}"/><seealso cref="DMappedSimple{TTar,TDri}"/>
    /// <typeparam name="TTar">Target Type</typeparam>
    /// <typeparam name="TDri">Driver Type</typeparam>
    public class DMappedPath<TTar, TDri> : DMapped<TTar, TDri>, IPathable
    {
        #region Fields

        /// The path 
        public Path.Base<TTar> Path { get; set; }

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Mapped PathPoints Driver constructor. 
        /// <seealso cref="DMapped{TTar,TDri}"/>
        /// </summary>
        /// <param name="_path">PathPoints object to map driver to.</param>
        /// <param name="_driverMapBegin">Drive value to map to beginning of path</param>
        /// <param name="_driverMapEnd">Drive value to map to end of path</param>
        /// <param name="_tarAction"></param>
        /// <param name="_driver"></param>
        /// <param name="_name"></param>
        public DMappedPath(Action<TTar> _tarAction, Path.Base<TTar> _path, Func<TDri> _driver, TDri _driverMapBegin, TDri _driverMapEnd, string _name = null)
            : base(_tarAction, _driver, _driverMapBegin, _driverMapEnd, _name)
        {
            Path = _path;
        }

        /// <summary>
        /// Mapped PathPoints Driver constructor. 
        /// <seealso cref="DMapped{TTar,TDri}"/>
        /// </summary>
        /// <param name="_path">PathPoints object to map the driver to.</param>
        /// <param name="_driverMapBegin">Drive value to map to beginning of path</param>
        /// <param name="_driverMapEnd">Drive value to map to end of path</param>
        /// <param name="_targetExpr"></param>
        /// <param name="_driver"></param>
        /// <param name="_name"></param>
        public DMappedPath(Expression<Func<TTar>> _targetExpr, Path.Base<TTar> _path, Func<TDri> _driver, TDri _driverMapBegin, TDri _driverMapEnd, string _name = null)
            : base(_targetExpr, _driver, _driverMapBegin, _driverMapEnd, _name)
        {
            Path = _path;
        }

        #endregion Constructors

        #region Properties

        /// Get string representation of driver type
        public override string NiceType
        {
            get { return "SpeedSimple<" + typeof(TTar) + "," + typeof(TDri) + ">"; }
        }

        #endregion Properties

        #region Methods


        IPath IPathable.GetPath()
        {
            return Path;
        }

        /// Returns waypoint value at specified percent along path.
        protected override TTar PctToTarget(float _pct)
        {
            return Path.ValueAt(_pct);
        }

        #endregion Methods
    }

    /// <summary>
    /// Drive target at a specified rate over defined path.
    /// </summary>
    /// Can be looped (<see cref="LoopType"/>). 
    /// <typeparam name="TTar">Waypoint Type</typeparam>
    public class DSpeedPath<TTar> : DSpeed<TTar>, IPathable
    {
        #region Fields

        /// Path object to drive from.
        protected Path.Base<TTar> m_Path;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Speed Path driver action constructor.
        /// <seealso cref="DSpeed{TTar}"/>
        /// </summary>
        /// <param name="_path">Path object to drive from</param>
        /// <param name="_tarAction"></param>
        /// <param name="_speedDriver"></param>
        /// <param name="_name"></param>
        public DSpeedPath(Action<TTar> _tarAction, Path.Base<TTar> _path, Func<float> _speedDriver, string _name = null)
            : base(_tarAction, _speedDriver, _name)
        {
            m_Path = _path;
        }

        /// <summary>
        /// Speed Path driver expression constructor.
        /// <seealso cref="DSpeed{TTar}"/>
        /// </summary>
        /// <param name="_path">Path object to drive from</param>
        /// <param name="_targetExpr"></param>
        /// <param name="_speedDriver"></param>
        /// <param name="_name"></param>
        public DSpeedPath(Expression<Func<TTar>> _targetExpr, Path.Base<TTar> _path, Func<float> _speedDriver, string _name = null)
            : base(_targetExpr, _speedDriver, _name)
        {
            m_Path = _path;
        }

        #endregion Constructors

        #region Properties

        /// Get string representation of driver type
        public override string NiceType
        {
            get { return "SpeedPath<" + typeof(TTar) + ">"; }
        }

        #endregion Properties

        #region Methods

        /// Get this driver's path object.
        public Path.Base<TTar> GetPath()
        {
            return m_Path;
        }

        IPath IPathable.GetPath()
        {
            return m_Path;
        }

        /// Returns the length of the path.
        protected override float TargetLen()
        {
            return m_Path.PathLength;
        }

        /// Returns the value of the path at specified percent along its length.
        protected override TTar TargetLerp(float _pct)
        {
            return m_Path.ValueAt(_pct);
        }

        #endregion Methods
    }

    /// <summary>
    /// Drive target through path over specified duration.  
    /// </summary>
    /// Can be eased (<see cref="EaseType"/> and/or looped (<see cref="LoopType"/>). 
    /// <para>Shorthand: <see cref="Tween{TTar}(Expression{Func{TTar}}, Path.Base{TTar}, float, string)"/></para>
    /// <para>Shorthand: <see cref="Tween{TTar}(Action{TTar}, Path.Base{TTar}, float, string)"/></para>
    /// <seealso cref="DTween{TTar}"/><seealso cref="DTweenSimple{TTar}"/>
    /// <typeparam name="TTar">Target Type.</typeparam>
    public class DTweenPath<TTar> : DTween<TTar>, IPathable
    {
        #region Fields

        private readonly Path.Base<TTar> m_Path;

        #endregion Fields

        #region Constructors

        /// Constructor using target action. See <see cref="DTween{TTar}"/>
        public DTweenPath(Action<TTar> _tarAction, Path.Base<TTar> _path, float _cycleDuration, string _name = null)
            : base(_tarAction, _cycleDuration, _name)
        {
            m_Path = _path;
        }

        /// Constructor using expression. See <see cref="DTween{TTar}"/>
        public DTweenPath(Expression<Func<TTar>> _targetExpr, Path.Base<TTar> _path, float _cycleDuration, string _name = null)
            : base(_targetExpr, _cycleDuration, _name)
        {
            m_Path = _path;
        }

        #endregion Constructors

        #region Properties

        /// Get string representation of driver type
        public override string NiceType
        {
            get { return "TweenPath<" + typeof(TTar) + ">"; }
        }

        #endregion Properties

        #region Methods

        /// Get this driver's path object.
        public Path.Base<TTar> GetPath()
        {
            return m_Path;
        }

        IPath IPathable.GetPath()
        {
            return m_Path;
        }

        /// Returns waypoint value at specified percent along path.
        protected override TTar PctToTarget(float _pct)
        {
            return m_Path.ValueAt(_pct);
        }

        #endregion Methods
    }

    /// Path and spline objects and data.
    public static class Path
    {

        static Path()
        {
            ioDriver.Init();
        }

        #region Nested Interfaces



        #endregion Nested Interfaces


        // TODO fix all of these if (Call BuildPath in constructor instead of Build here) static factory bug gets fixed (mono / Unity)
        /// <summary>
        /// Create linear path from existing path object's frame.
        /// </summary>
        /// <typeparam name="T">Waypoint type</typeparam>
        /// <param name="_pathObj">Path to copy wayponts from</param>
        /// <param name="_autoBuild">Enable Autobuild?</param>
        /// <returns>New linear path object</returns>
        public static Linear<T> CreateLinear<T>(Path.Base<T> _pathObj, bool _autoBuild = true)
        {
            var path = new Linear<T>(ToList(_pathObj.GetFrameWaypoints()), _pathObj.Closed, _autoBuild);
            if (_autoBuild) path.Build();
            return path;
        }


        /// <summary>
        /// Creates new linear path from provided list of waypoints.
        /// </summary>
        /// <typeparam name="T">Waypoint type</typeparam>
        /// <param name="_waypoints">List of waypoints</param>
        /// <param name="_closed">Closed path?</param>
        /// <param name="_autoBuild">Enable Autobuild?</param>
        /// <returns>New linear path object</returns>
        public static Linear<T> CreateLinear<T>(bool _closed, IEnumerable<T> _waypoints, bool _autoBuild = true)
        {
            var path = new Linear<T>(_waypoints, _closed, _autoBuild);
            if (_autoBuild) path.Build();
            return path;
        }

        /// <summary>
        /// Create new Bezier spline from provided path's frame.
        /// </summary>
        /// <typeparam name="T">Waypoint Type</typeparam>
        /// <param name="_pathObj">Path object containing waypoints to use</param>
        /// <param name="_autoBuild">Enable Autobuild?</param>
        /// <returns>New Bezier spline object</returns>
        public static Bezier<T> CreateBezier<T>(Base<T> _pathObj, bool _autoBuild = true)
        {
            var path = new Bezier<T>(ToList(_pathObj.GetFrameWaypoints()), _pathObj.Closed, _autoBuild);
            if (_autoBuild) path.Build();
            return path;
        }

        /// <summary>
        /// Create new Bezier spline from provided list of frame waypoints.
        /// </summary>
        /// <typeparam name="T">Waypoint Type</typeparam>
        /// <param name="_frameWaypoints">List of waypoints for frame</param>
        /// <param name="_closed">Closed spline?</param>
        /// <param name="_autoBuild">Enable Autobuild?</param>
        /// <returns>New Bezier spline object</returns>
        public static Bezier<T> CreateBezier<T>(bool _closed, IEnumerable<T> _frameWaypoints, bool _autoBuild = true)
        {
            var path = new Bezier<T>(_frameWaypoints, _closed, _autoBuild);
            if (_autoBuild) path.Build();
            return path;
        }


        /// <summary>
        /// Create new cubic spline from provided path object's frame.
        /// </summary>
        /// <typeparam name="T">Waypoint type</typeparam>
        /// <param name="_pathObj">Path object</param>
        /// <param name="_autoBuild">Enable Autobuild?</param>
        /// <returns>New cubic spline object</returns>
        public static Cubic<T> CreateCubic<T>(Base<T> _pathObj, bool _autoBuild = true)
        {
            var path = new Cubic<T>(ToList(_pathObj.GetFrameWaypoints()), _pathObj.Closed, _autoBuild);
            if (_autoBuild) path.Build();
            return path;
        }

        /// <summary>
        /// Create new cubic spline from provided list of frame waypoints.
        /// </summary>
        /// <typeparam name="T">Waypoint Type</typeparam>
        /// <param name="_frameWaypoints">List of waypoints for frame</param>
        /// <param name="_closed">Closed spline?</param>
        /// <param name="_autoBuild">Enable Autobuild?</param>
        /// <returns>New cubic spline object</returns>
        public static Cubic<T> CreateCubic<T>(bool _closed, IEnumerable<T> _frameWaypoints, bool _autoBuild = true)
        {
            var path = new Cubic<T>(_frameWaypoints, _closed, _autoBuild);
            if (_autoBuild) path.Build();
            return path;
        }

        #region Other

        /// <summary>
        /// Base path object.
        /// </summary>
        /// <typeparam name="T">Waypoint type</typeparam>
        public abstract class Base<T> : IPath
        {

            /// Maintained array of path waypoints, last point of path will equal first
            /// point if the path is <see cref="Closed"/>.
            public T[] PathPoints
            {
                get;
                protected set;
            }

            /// Array of VecN built points for this path.
            protected VecN[] PathPointsVN;

            /// Array of segments in path.
            protected Segment[] m_PathSegments;

            /// List of frame waypoints stored as <see cref="VecN"/>
            protected List<VecN> m_FrameVN;

            /// List of raw segments used to define the path's frame (the points the path is to pass through).
            /// Will include final segment if closed.
            protected Segment[] m_FrameSegments;

            /// Is this a closed path?
            protected bool m_Closed;
            
            /// If set to true, this path object will rebuild itself on any settings change.
            public bool AutoBuild;

            /// <summary>
            /// Base path object constructor.  Note that <see cref="Build"/> is not called during construction.
            /// Child classes will need to either call this in their constructor or through a static factory method.
            /// </summary>
            /// <param name="_frameWaypoints">List of waypoints used to define path's frame</param>
            /// <param name="_closed">Is this a closed path?</param>
            /// <param name="_autoBuild">Enable Autobuild?</param>
            protected Base(IEnumerable<T> _frameWaypoints, bool _closed, bool _autoBuild)
            {
                m_FrameVN = ToVecNs(_frameWaypoints).ToList();
                m_Closed = _closed;
                //If closed enforce that first and last are not equal.
                if (m_Closed)
                    if (m_FrameVN[0].Equals(m_FrameVN[m_FrameVN.Count - 1]))
                        m_FrameVN.RemoveAt(m_FrameVN.Count - 1);

                BuildFrame();
                AutoBuild = _autoBuild;
            }

            /// Is this a closed path?  <see cref="Build"/> is called on change.
            public virtual bool Closed
            {
                get { return m_Closed; }
                set
                {
                    if (m_Closed == value) return;
                    m_Closed = value;
                    IBuild(true);
                }
            }

            /// Get the length of the path.
            public float PathLength
            {
                get
                {
                    var len = 0f;
                    foreach (var segment in m_PathSegments)
                        len += segment.Length;
                    return len;
                }
            }

            /// Get the length of the path's frame (Linear path), including
            /// final segment if closed.
            public float FrameLength
            {
                get
                {
                    var len = 0f;
                    foreach (var segment in m_FrameSegments)
                        len += segment.Length;
                    return len;
                }
            }

            /// Get actual path's segment list.
            public Segment[] PathSegments
            {
                get { return m_PathSegments.ToArray(); }
            }

            /// Get path's frame segments
            public Segment[] FrameSegments
            {
                get { return m_FrameSegments.ToArray(); }
            }

            /// Get a copy of the frame segment that lies at specified frame pct. <seealso cref="GetPathSegmentAt"/>
            public Segment GetFrameSegmentAt(float _pct)
            {
                if (_pct < 0f || _pct > 1f)
                {
                    Log.Err("_pct must be between 0 and 1.  Returning null.");
                    return null;
                }
                if (_pct == 0) return new Segment(FrameSegments[0]);
                if (_pct == 1) return new Segment(FrameSegments[FrameSegments.Length - 1]);

                foreach (Segment seg in FrameSegments)
                    if (seg.PctEnd > _pct)
                        return new Segment(seg);
                return new Segment(FrameSegments[FrameSegments.Length - 1]);
            }

            /// Get a copy of the path segment that lies at specified path pct. <seealso cref="GetFrameSegmentAt"/>
            public Segment GetPathSegmentAt(float _pct)
            {
                if (_pct < 0f || _pct > 1f)
                {
                    Log.Err("_pct out of range.  Must be between 0 and 1f.  Returning null.");
                    return null;
                }
                if (_pct == 0) return new Segment(PathSegments[0]);
                if (_pct == 1) return new Segment(PathSegments[PathSegments.Length - 1]);

                foreach (Segment seg in PathSegments)
                    if (seg.PctEnd > _pct)
                        return new Segment(seg);
                return new Segment(PathSegments[PathSegments.Length - 1]);
            }

            /// Get percent along frame for specified frame waypoint
            public float GetFrameWaypointPct(int _index)
            {
                if (_index < 0f || Closed ? _index > m_FrameVN.Count : _index > m_FrameVN.Count - 1)
                {
                    Log.Err("Waypoint index out of range.  Received '" + _index + "'");
                    return float.NaN;
                }
                if (_index == 0) return 0f;
                return _index == (Closed ? m_FrameVN.Count : m_FrameVN.Count - 1) ? 1f : m_FrameSegments[_index].PctStart;
            }

            /// Finds the nearest position on the path to the indicated point. <seealso cref="GetNearestTo(T,out float, out Segment)"/>
            public T GetNearestTo(T _point)
            {
                float pct;
                return GetNearestTo(_point, out pct);
            }

            /// Finds the nearest position on the path to the indicated point. <seealso cref="GetNearestTo(T,out float, out Segment)"/>
            public T GetNearestTo(T _point, out float _pctOnPath)
            {
                Segment seg;
                return GetNearestTo(_point, out _pctOnPath, out seg);
            }

            /// <summary>
            /// Finds the nearest position on the path to the indicated point.
            /// </summary>
            /// <param name="_point">Point to use for nearest position</param>
            /// <param name="_pctOnPath">Populated with percent along path of nearest position was found</param>
            /// <param name="_nearestSegment">Populated with segment that contains nearest position</param>
            /// <returns>Nearest Position</returns>
            public T GetNearestTo(T _point, out float _pctOnPath, out Segment _nearestSegment)
            {
                var closestDist = float.PositiveInfinity;
                Segment closestSeg = null;
                VecN pointOnLine = null;
                var segs = PathSegments;
                var ptsN = PathPointsVN;
                var searchFromN = ToVecN(_point);

                for (int idx = 0; idx < segs.Length; ++idx)
                {
                    var curSeg = segs[idx];
                    var segPtA = ptsN[curSeg.FromIdx];
                    var segPtB = ptsN[curSeg.FromIdx + 1];
                    VecN curPtOnLine = null;
                    var curDist = VecN.PointToLineDistanceSquared(segPtA, segPtB, searchFromN, out curPtOnLine);
                    if (curDist < closestDist)
                    {
                        closestDist = curDist;
                        pointOnLine = curPtOnLine;
                        closestSeg = curSeg;
                    }
                }

                var nearestOnPath = pointOnLine;
                _nearestSegment = closestSeg;
                var pctInSeg = VecN.ILerp(ptsN[closestSeg.FromIdx], ptsN[closestSeg.FromIdx + 1],
                    nearestOnPath);

                _pctOnPath = Teacher.Lerpf(_nearestSegment.PctStart, _nearestSegment.PctEnd, pctInSeg);

                return nearestOnPath.To<T>();
            }


            /// Add new frame waypoint
            public virtual void AddFrameWaypoint(T _waypoint)
            {
                m_FrameVN.Add(ToVecN(_waypoint));
                IBuild(true);
            }

            /// Insert new frame waypoint at specified index.
            public virtual void InsertFrameWaypoint(int _index, T _waypoint)
            {
                m_FrameVN.Insert(_index, ToVecN(_waypoint));
                IBuild(true);
            }

            /// Replace the data at index with specified data.
            public virtual void UpdateFrameWaypoint(int _index, T _waypoint)
            {
                m_FrameVN[_index] = ToVecN(_waypoint);
                IBuild(true);
            }

            /// Remove frame waypoint at specified index.
            public virtual void RemoveFrameWaypoint(int _index)
            {
                m_FrameVN.RemoveAt(_index);
                IBuild(true);
            }

            /// Get an array of this path's frame waypoints.
            public T[] GetFrameWaypoints()
            {
                return VecN.ToArray<T>(m_FrameVN);
            }

            /// <summary>
            /// Offset all of this path's frame points by T and rebuild the path.
            /// </summary>
            /// <param name="_offset">Offset value</param>
            public void Offset(T _offset)
            {
                var offsetN = ToVecN(_offset);
                for (int idx = 0; idx < m_FrameVN.Count; ++idx)
                    m_FrameVN[idx] = (m_FrameVN[idx] + offsetN);

                IBuild(true);
            }

            private static VecN LerpPathN(float _pct, VecN[] _points, Segment[] _segments, float _pathLen, bool _closed)
            {
                if (_pct == 0f) return _points[0];
                if (_pct == 1f) return _closed ? _points[0] : _points[_points.Length - 1];

                var tgtLen = _pathLen * _pct;

                var curLen = 0f;
                var segIdx = 0;
                while (curLen < tgtLen)
                    curLen += _segments[segIdx++].Length;

                if (curLen == 0) return _points[0];

                var tgtSeg = _segments[segIdx - 1];
                curLen -= tgtSeg.Length;
                var segTgtLen = tgtLen - curLen;

                VecN fromVal = _points[tgtSeg.FromIdx];
                VecN toVal;
                if (_closed && ((tgtSeg.FromIdx + 1) == _points.Length))
                    toVal = _points[0];
                else
                    toVal = _points[tgtSeg.FromIdx + 1];

                return VecN.Lerp(fromVal, toVal, segTgtLen / _segments[segIdx - 1].Length);
            }

            /// <summary>
            /// Get this path's value at specified percent along path via <see cref="PathPoints"/>.
            /// </summary>
            /// <param name="_pct">Percent along path (0 to 1)</param>
            /// <returns></returns>
            public T ValueAt(float _pct)
            {
                return ValueAtN(_pct).To<T>();
            }

            /// <summary>
            /// Get VecN representation of point at specified percent along this path.
            /// </summary>
            /// <param name="_pct">Percent along path to retreive</param>
            /// <returns>VecN object at percent</returns>
            protected VecN ValueAtN(float _pct)
            {
                return LerpPathN(_pct, PathPointsVN, m_PathSegments, PathLength, m_Closed);
            }

            /// <summary>
            /// Get this path's frame value at specified percent along frame.
            /// </summary>
            /// <param name="_pct"></param>
            /// <returns></returns>
            protected T FrameValueAt(float _pct)
            {
                return FrameValueAtN(_pct).To<T>();
            }

            /// <summary>
            /// Get VecN representing point at specified percent along frame.
            /// </summary>
            /// <param name="_pct">Percent along frame to retrieve</param>
            /// <returns>VecN object at _pct</returns>
            protected VecN FrameValueAtN(float _pct)
            {
                var frmPts = ToList(m_FrameVN);
                if (Closed)
                    frmPts.Add(m_FrameVN[0]);
                return LerpPathN(_pct, frmPts.ToArray(), m_FrameSegments, FrameLength, m_Closed);
            }

            /// <summary>
            /// "Internal Build" rebuilds frame if requested then if <see cref="AutoBuild"/> = true, will call <see cref="BuildPath()"/>.
            /// </summary>
            /// <param name="_rebuildFrame">Rebuild Frame?</param>
            protected void IBuild(bool _rebuildFrame)
            {
                if (_rebuildFrame)
                    BuildFrame();

                if (AutoBuild) BuildPath();
            }

            /// Calculate path data, populate <see cref="PathPoints"/> and <see cref="PathSegments"/>
            /// Use when all settings are complete and Autobuild is set to false.
            public void Build()
            {
                BuildPath();
            }

            private void BuildFrame()
            {
                var totLen = 0f;
                var lengths = new float[m_Closed ? m_FrameVN.Count : m_FrameVN.Count - 1];
                for (int idx = 0; idx < m_FrameVN.Count - 1; ++idx)
                {
                    lengths[idx] = (m_FrameVN[idx + 1] - m_FrameVN[idx]).Magnitude;
                    totLen += lengths[idx];
                }
                if (m_Closed)
                {
                    lengths[lengths.Length - 1] = (m_FrameVN[0] - m_FrameVN[m_FrameVN.Count - 1]).Magnitude;
                    totLen += lengths[lengths.Length - 1];
                }

                var cumLen = 0f;
                m_FrameSegments = new Segment[lengths.Length];
                for (int idx = 0; idx < lengths.Length; ++idx)
                {
                    var startLen = cumLen;
                    cumLen += lengths[idx];
                    m_FrameSegments[idx] = new Segment(idx, startLen / totLen, cumLen / totLen, lengths[idx]);
                }

            }

            /// Override to update <see cref="PathPoints"/> and <see cref="PathSegments"/> here.
            protected abstract void BuildPath();


            /// Object representing segment between two points on a <see cref="Base{T}"/>.
            /// Convenience data carrier.
            public class Segment
            {
                /// Index of point on path for beginning of segment
                public int FromIdx
                {
                    get;
                    private set;
                }

                /// Percent on path this segment begins
                public float PctStart
                {
                    get;
                    private set;
                }

                /// Percent on path this segment ends
                public float PctEnd
                {
                    get;
                    private set;
                }

                /// Length of segment
                public float Length
                {
                    get;
                    private set;
                }

                /// Constructor
                public Segment(int _fromIdx, float _pctStart, float _pctEnd, float _length)
                {
                    FromIdx = _fromIdx;
                    PctStart = _pctStart;
                    PctEnd = _pctEnd;
                    Length = _length;
                }

                /// Copy constructor
                public Segment(Segment _segment)
                {
                    FromIdx = _segment.FromIdx;
                    PctStart = _segment.PctStart;
                    PctEnd = _segment.PctEnd;
                    Length = _segment.Length;
                }

                /// Returns string representation of the data in this segment.
                public override string ToString()
                {
                    return "Seg - FrmIdx: " + FromIdx + " PctStart: " + PctStart + " PctEnd: " + PctEnd + " Len: " + Length;
                }
            }

            float[,] IPath.GetPathPoints(out int _dimCount)
            {

                if (m_FrameVN.Count == 0)
                {
                    _dimCount = 0;
                    return new float[0, 0];
                }

                _dimCount = m_FrameVN[0].DimCount;
                if (_dimCount == 0)
                {
                    _dimCount = 0;
                    return new float[0, 0];
                }

                var points = new float[PathPointsVN.Length, _dimCount];
                for (int idx = 0; idx < PathPointsVN.Length; ++idx)
                    for (int dc = 0; dc < _dimCount; ++dc)
                        points[idx, dc - 1] = PathPointsVN[idx].Vals[dc];
                return points;
            }

            bool IPath.TryGetPathAs<TW>(out TW[] _points)
            {
                if (!typeof(TW).IsSubclassOf(typeof(T)) && typeof(TW) != typeof(T))
                {
                    _points = null;
                    return false;
                }
                _points = new TW[PathPoints.Length];
                for (int idx = 0; idx < PathPoints.Length; ++idx)
                    _points[idx] = (TW)(object)PathPoints[idx];
                return true;
            }

            VecN[] IPath.GetPathPoints()
            {
                var pts = new VecN[PathPointsVN.Length];
                for (int idx = 0; idx < PathPointsVN.Length; ++idx)
                {
                    pts[idx] = new VecN(PathPointsVN[idx]);
                }
                return pts;
            }

            Type IPath.WaypointType
            {
                get { return typeof(T); }
            }

        }

        /// <summary>
        /// Determines which mode spline points are calculated.
        /// </summary>
        public enum SplinePointMode
        {
            /// Build spline points based on a minimum angle between segments.
            MinAngle,
            /// Build spline points based on a minimum length for segments.
            SegmentLength,
            /// Build spline points based on number of points
            PointCount
        }

        /// <summary>
        /// Base spline object.
        /// </summary>
        /// <typeparam name="T">Waypoint Type</typeparam>
        public abstract class Spline<T> : Base<T>, ISpline
        {
            /// Constant representing automatic minimum segement length to search to for 
            /// <see cref="SplinePointMode.MinAngle"/> mode.
            public const float MA_LENGTH_AUTO = float.NegativeInfinity;

            /// Backing field for <see cref="DimsToSpline"/>
            protected int[] m_DimsToSpline;

            /// Backing field for <see cref="ModeSLSegmentLen"/>
            protected float m_ModeSLSegmentLen;

            /// Backing field for <see cref="ModeSLSegmentAcc"/>
            private float m_ModeSLSegmentAcc;

            private SplinePointMode m_PointMode;

            /// <summary>
            /// Get / Set the build mode. <seealso cref="SplinePointMode"/>
            /// </summary>
            public SplinePointMode PointMode
            {
                get { return m_PointMode; }
                set
                {
                    if (m_PointMode == value) return;
                    m_PointMode = value;
                    IBuild(false);
                }
            }


            private float m_ModeMAMinAngle;

            /// Get / Set minimum angle search for mode <see cref="SplinePointMode.MinAngle"/>.
            /// Will not rebuild if current <see cref="PointMode"/> is other than <see cref="SplinePointMode.MinAngle"/>
            public float ModeMAMinAngle
            {
                get { return m_ModeMAMinAngle; }
                set
                {
                    if (value <= 0 || value >= 180)
                    {
                        Log.Err("Min Angle must be between 0 and 180 degrees.  Setting Min Angle to 90 Degrees.");
                        m_ModeMAMinAngle = 90f;
                    }
                    if (m_ModeMAMinAngle == value) return;
                    m_ModeMAMinAngle = value;
                    if (PointMode == SplinePointMode.MinAngle)
                        IBuild(false);
                }
            }

            private float m_ModeMAMinLength;

            /// Get / Set minimum segment length search for mode <see cref="SplinePointMode.MinAngle"/>.
            /// Will not rebuild if current <see cref="PointMode"/> is other than <see cref="SplinePointMode.MinAngle"/>
            public float ModeMAMinLength
            {
                get { return m_ModeMAMinLength; }
                set
                {
                    if (value <= 0 && value != MA_LENGTH_AUTO)
                    {
                        m_ModeMAMinLength = FrameLength / 500f;
                        Log.Err("Min Angle Min length must greater than zero.  Setting to " + m_ModeMAMinLength);

                    }
                    else
                    {
                        if (m_ModeMAMinLength == value) return;
                        m_ModeMAMinLength = value;
                    }

                    if (PointMode == SplinePointMode.MinAngle)
                        IBuild(false);
                }
            }

            private int m_ModePCPointCount;

            /// Get / Set point count for <see cref="SplinePointMode.PointCount"/>.
            /// Will not rebuild if current <see cref="PointMode"/> is other than <see cref="SplinePointMode.PointCount"/>
            public int ModePCPointCount
            {
                get { return m_ModePCPointCount; }
                set
                {
                    if (value <= 1)
                    {
                        m_ModePCPointCount = Closed ? 3 : 2;
                        Log.Err("Point Count must be greater than 1. Setting to " + m_ModePCPointCount);
                    }
                    else
                    {
                        if (m_ModePCPointCount == value) return;
                        m_ModePCPointCount = value;
                    }

                    if (PointMode == SplinePointMode.PointCount)
                        IBuild(false);
                }
            }


            /// <summary>
            /// Constructor.  Note <see cref="Base{T}.Build"/> is not called during construction.
            /// <seealso cref="Base{T}(IEnumerable{T},bool,bool)"/>
            /// </summary>
            protected Spline(IEnumerable<T> _frameWaypoints, bool _closed, bool _autoBuild)
                : base(_frameWaypoints, _closed, _autoBuild)
            {
                m_DimsToSpline = CheckDims(null);
                m_ModeSLSegmentLen = GetDefaultSegmentLength();
                m_ModeMAMinLength = MA_LENGTH_AUTO;
            }

            /// Get / Set target segment length for mode <see cref="SplinePointMode.SegmentLength"/>.
            /// Will not rebuild if current <see cref="PointMode"/> is other than <see cref="SplinePointMode.SegmentLength"/>
            public float ModeSLSegmentLen
            {
                get
                {
                    return m_ModeSLSegmentLen;
                }
                set
                {
                    if (m_ModeSLSegmentLen == value) return;
                    if (value <= 0)
                    {
                        m_ModeSLSegmentLen = GetDefaultSegmentLength();
                        Log.Err("Segment length must greater than zero.  Setting to '" + m_ModeSLSegmentLen + "'");
                    }
                    else
                        m_ModeSLSegmentLen = value;
                    if (PointMode == SplinePointMode.SegmentLength)
                        IBuild(false);
                }
            }

            /// Get / Set target segment length accuracy for mode <see cref="SplinePointMode.SegmentLength"/>.
            /// ie. Segments will be of length of 2.0f +/- 0.1 with <see cref="ModeSLSegmentLen"/> of 2.0f and Segment Accuracy of 0.05
            /// Will not rebuild if current <see cref="PointMode"/> is other than <see cref="SplinePointMode.SegmentLength"/>
            public float ModeSLSegmentAcc
            {
                get { return m_ModeSLSegmentAcc; }
                set
                {
                    if (m_ModeSLSegmentAcc == value) return;
                    var val = value;
                    if (value <= 0 || value >= 0.9999f)
                    {
                        val = 0.25f;
                        Log.Err("Segment accuracy cannot be less than zero and must be less than 1.  Setting to '" + val + "'");
                    }
                    m_ModeSLSegmentAcc = val;
                    if (PointMode == SplinePointMode.SegmentLength)
                        IBuild(false);
                }

            }

            /// List of dimensions to be splined from frame waypoints.
            public int[] DimsToSpline
            {
                get { return m_DimsToSpline.Clone() as int[]; }
                set
                {
                    var dimsToSpline = CheckDims(value);
                    if (dimsToSpline == m_DimsToSpline) return;
                    m_DimsToSpline = dimsToSpline;
                    IBuild(false);
                }
            }

            /// <summary>
            /// Returns value of spline at specified percentage along spline.
            /// </summary>
            /// <param name="_pct">Percentage along spline (0 to 1f)</param>
            /// <returns>Value at specified percentage</returns>
            public T SplineValueAt(float _pct) { return SplineValueAtN(_pct).To<T>(); }


            /// <summary>
            /// Get representation of the tangent value at specified percent along spline.
            /// </summary>
            /// <param name="_pct">Percent along spline (0 to 1f)</param>
            /// <returns>Tangent / Derivative at _pct along spline</returns>
            public T SplineTangentAt(float _pct) { return SplineTangentAtN(_pct).To<T>(); }

            /// <summary>
            /// Returns VecN value of spline at specified percentage along spline.
            /// Built in splines (see <see cref="Bezier{T}"/> and <see cref="Cubic{T}"/>) caluclate value using
            /// Spline's mathematical formula.  
            /// </summary>
            /// <param name="_pct">Percentage along spline (0 to 1f)</param>
            /// <returns>Value at specified percentage</returns>
            protected abstract VecN SplineValueAtN(float _pct);

            /// <summary>
            /// Get the VecN representation of the tangent value at specified percent along spline.
            /// </summary>
            /// <param name="_pct">Percent along spline (0 to 1f)</param>
            /// <returns>Tangent / Derivative Vector at _pct along spline</returns>
            protected abstract VecN SplineTangentAtN(float _pct);

            private float GetDefaultSegmentLength()
            {
                return FrameLength / 10f;
            }


            /// Populates <see cref="Base{T}.PathPoints"/> and <see cref="Base{T}.PathSegments"/>
            protected override void BuildPath()
            {
                Func<float, float, float> glptFuncSq =
                    (_fromPct, _toPct) =>
                        (SplineValueAtN(_fromPct) - SplineValueAtN(_toPct)).MagnitudeSquared;


                var timeStamp = System.DateTime.UtcNow.Ticks;

                if (PointMode == SplinePointMode.PointCount)
                {
                    if (m_ModePCPointCount <= 1)
                        m_ModePCPointCount = 100; //TODO make default const?
                    
                    var frmPct = 0f;
                    var ptsN = new List<VecN> { SplineValueAtN(frmPct) };
                    var segs = new List<Segment>();
                    for (int idx = 1; idx < m_ModePCPointCount; ++idx)
                    {
                        var toPct = (float) (idx)/ (float)(m_ModePCPointCount - 1);
                        ptsN.Add(SplineValueAtN(toPct));
                        segs.Add(new Segment(idx, frmPct, toPct, (ptsN[idx - 1] - ptsN[idx]).Magnitude));
                        frmPct = toPct;
                    }

                    PathPointsVN = ptsN.ToArray();
                    m_PathSegments = segs.ToArray();
                }
                else if (PointMode == SplinePointMode.SegmentLength)
                {
                    if (m_ModeSLSegmentLen <= 0)
                        m_ModeSLSegmentLen = GetDefaultSegmentLength();
                    if (m_ModeSLSegmentAcc <= 0 || m_ModeSLSegmentAcc >= 1f)
                        m_ModeSLSegmentAcc = 0.3f;

                    var tgtLenMinSq = (m_ModeSLSegmentLen - m_ModeSLSegmentLen * m_ModeSLSegmentAcc)
                        * (m_ModeSLSegmentLen - m_ModeSLSegmentLen * m_ModeSLSegmentAcc);
                    var tgtLenMaxSq = (m_ModeSLSegmentLen + m_ModeSLSegmentLen * m_ModeSLSegmentAcc)
                        * (m_ModeSLSegmentLen + m_ModeSLSegmentLen * m_ModeSLSegmentAcc);
                    var guessPct = m_ModeSLSegmentLen / SplineLengthEstimated();

                    var fromPtN = m_FrameVN[0];
                    var endPtN = SplineValueAtN(1f);
                    var pathN = new List<VecN> { fromPtN };
                    var totalLength = 0f;
                    var lengths = new List<float>();



                    var fromPct = 0f;
                    var done = false;
                    while (!done)
                    {

                        var wrapped = false;
                        var pctA = fromPct;
                        var pctB = float.MaxValue;
                        var curPct = fromPct + guessPct;

                        float lenSq;
                        if (curPct >= 1f)
                        {
                            curPct = 1f;
                            var checkLenSq = (endPtN - fromPtN).MagnitudeSquared;
                            if (checkLenSq <= tgtLenMinSq)
                            {
                                done = true;
                                lenSq = checkLenSq;
                            }
                            else
                                lenSq = glptFuncSq(pctA, curPct);
                        }
                        else
                            lenSq = glptFuncSq(pctA, curPct);



                        while (!done)
                        {

                            if (lenSq >= tgtLenMinSq && lenSq <= tgtLenMaxSq)
                                break;

                            if (lenSq < tgtLenMinSq && curPct > pctA)
                            {
                                pctA = curPct;
                                if (!wrapped)
                                    curPct += guessPct;
                                else
                                    curPct = pctA + (pctB - pctA) / 2f;
                            }
                            else if (lenSq > tgtLenMaxSq && curPct <= pctB)
                            {
                                wrapped = true;
                                pctB = curPct;
                                curPct = pctA + (pctB - pctA) / 2f;
                            }

                            if (pctA > 1f)
                            {
                                done = true;
                                curPct = 1f;
                            }
                            lenSq = glptFuncSq(fromPct, curPct);

                            /*
                            var curTicks = DateTime.UtcNow.Ticks - timeStamp;
                            if (curTicks / System.TimeSpan.TicksPerMillisecond > TimeOut)
                            {
                                Log.Err("Timeout building path.  Timeout = " + TimeOut + " ms.  Setting mode Point Count");
                                m_PointMode = SplinePointMode.PointCount;
                                m_ModePCPointCount = 100;
                                BuildPath();
                                return;
                            }
                             * */
                        }

                        var pt = SplineValueAtN(curPct);

                        pathN.Add(pt);
                        var len = (float)Math.Sqrt(lenSq);
                        lengths.Add(len);
                        totalLength += len;
                        fromPct = curPct;
                        fromPtN = pt;
                    }

                    if (pathN.Count < 2)
                    {
                        Log.Warn("Segment Length too long.  (ModeSLSegmentLen = '" + ModeSLSegmentLen + "')");
                        PathPointsVN = new VecN[] { m_FrameVN[0], m_FrameVN[m_FrameVN.Count - 1] };
                    }
                    else
                        PathPointsVN = pathN.ToArray();

                    //Update Segment pct
                    var progressLen = 0f;
                    m_PathSegments = new Segment[PathPointsVN.Length - 1];
                    for (int idx = 0; idx < m_PathSegments.Length; ++idx)
                    {
                        var frmPct = progressLen / totalLength;
                        progressLen += lengths[idx];
                        var toPct = progressLen / totalLength;
                        m_PathSegments[idx] = new Segment(idx, frmPct, toPct, lengths[idx]);
                    }
                }
                else if(m_PointMode == SplinePointMode.MinAngle)
                {

                    var minLen = (m_ModeMAMinLength == MA_LENGTH_AUTO) ? (SplineLengthEstimated() / 500f) : m_ModeMAMinLength;
                    var ptsN = new List<VecN> { m_FrameVN[0] };
                    var pctList = new List<float> { 0f };
                    for (int idx = 1; idx < m_FrameVN.Count; ++idx)
                        pctList.Add(GetFrameWaypointSplinePct(idx));
                    if (Closed)
                        pctList.Add(1f);

                    for (int idx = 1; idx < pctList.Count; ++idx)
                    {
                        ptsN.AddRange(BuildMinAngle2(pctList[idx - 1], pctList[idx], m_ModeMAMinAngle, minLen));
                        ptsN.Add(ToVecN(SplineValueAt(pctList[idx])));
                    }

                    /* -- Entire path no split (unless closed, split in half)
                    var minLen = (m_ModeMAMinLength == MA_LENGTH_AUTO) ? (SplineLengthEstimated() / 500f) : m_ModeMAMinLength;
                    var ptsN = new List<VecN> { m_FrameVN[0] };
                    if (!Closed)
                    {
                        ptsN.AddRange(BuildMinAngle2(0f, 1f, m_ModeMAMinAngle, minLen));
                    }
                    else
                    {
                        ptsN.AddRange(BuildMinAngle2(0,0.5f, m_ModeMAMinAngle, minLen));
                        ptsN.AddRange(BuildMinAngle2(0.5f,1f,m_ModeMAMinAngle,minLen));
                    }
                    ptsN.Add(SplineValueAtN(1f));
                     */


                    var totLen = 0f;
                    var lengths = new float[ptsN.Count - 1];
                    //Get total length
                    for (int idx = 1; idx < ptsN.Count; ++idx)
                    {
                        var curLen = (ptsN[idx] - ptsN[idx - 1]).Magnitude;
                        totLen += curLen;
                        lengths[idx - 1] = curLen;
                    }

                    //Build segments
                    var progressLen = 0f;

                    m_PathSegments = new Segment[ptsN.Count - 1];
                    for (int idx = 0; idx < m_PathSegments.Length; ++idx)
                    {
                        var frmPct = progressLen / totLen;
                        progressLen += lengths[idx];
                        var toPct = progressLen / totLen;
                        m_PathSegments[idx] = new Segment(idx, frmPct, toPct, lengths[idx]);
                    }

                    PathPointsVN = ptsN.ToArray();
                }
                PathPoints = VecN.ToArray<T>(PathPointsVN);
            }


            private List<VecN> BuildMinAngle2(float _fromPct, float _toPct, float _minAngle, float _minLen)
            {
                var newPts = new List<VecN>();


                var prevPtN = SplineValueAtN(_fromPct);
                var nextPtN = SplineValueAtN(_toPct);
                var lenSq = (prevPtN - nextPtN).MagnitudeSquared;
                if (lenSq < (_minLen * _minLen)) return newPts;

                var halfPct = _fromPct + (_toPct - _fromPct) / 2f;
                var curPtN = SplineValueAtN(halfPct);

                var prevVecN = curPtN - prevPtN;
                var nextVecN = nextPtN - curPtN;

                var curAngle = ioMath.ToDegrees(VecN.Angle(prevVecN, nextVecN));


                newPts.AddRange(BuildMinAngle2(_fromPct, halfPct, _minAngle, _minLen));
                if (curAngle > _minAngle) newPts.Add(curPtN);
                newPts.AddRange(BuildMinAngle2(halfPct, _toPct, _minAngle, _minLen));

                return newPts;
            }



            /// <summary>
            /// Get the percentage at which the specified frame waypoint lies along the spline.
            /// </summary>
            /// <param name="_index">Index of frame waypoint</param>
            /// <returns>Percentage (0 to 1)</returns>
            public float GetFrameWaypointSplinePct(int _index)
            {
                if (_index < 0 || (Closed ? _index > m_FrameVN.Count : _index >= m_FrameVN.Count))
                    throw new IndexOutOfRangeException("Invalid waypoint index.");

                if ((m_Closed ? _index == m_FrameVN.Count : _index == m_FrameVN.Count - 1))
                    return 1f;
                if (_index == 0) return 0;

                var sectionCnt = m_FrameVN.Count - 1;
                if (Closed) sectionCnt++;
                return (float)_index / sectionCnt;

            }

            private static int[] CheckDims(int[] _dimsToSpline)
            {
                if (_dimsToSpline == null) return DTypeInfo<T>.GettableDims;
                var checkedDims = new List<int>(_dimsToSpline);
                foreach (var dim in _dimsToSpline)
                {
                    if (!DTypeInfo<T>.IsGettableDim(dim))
                    {
                        Log.Err("Dimension " + dim +
                                " specified but I don't know how to that dimension (Use TeachCoord).  Not splining " +
                                dim + ".");
                        checkedDims.Remove(dim);
                    }
                }
                return checkedDims.ToArray();
            }

            /// <summary>
            /// Get estimated length of spline.
            /// </summary>
            /// <param name="_segmentCnt">Number of segments to break spline into</param>
            /// <returns>Estimated length</returns>
            public float SplineLengthEstimated(int _segmentCnt = 99)
            {
                var estLen = 0f;
                for (int idx = 0; idx <= _segmentCnt; ++idx)
                    estLen += DTypeInfo<T>.Length(SplineValueAt((float)idx / _segmentCnt),
                        SplineValueAt((float)(idx + 1) / _segmentCnt));
                return estLen;
            }
        }

        /// <summary>
        /// Bezier Spline Path Object.
        /// </summary>
        /// <typeparam name="T">Waypoint Type</typeparam>
        public class Bezier<T> : Spline<T>
        {
            /// List of bezier waypoint control data
            protected Control[] m_Control;

            /// <summary>
            /// Constructor.  Note that <see cref="Base{T}.Build()"/> is not called during construction. 
            /// Use static factory methods.
            /// <seealso cref="CreateBezier{T}(bool,IEnumerable{T},bool)"/>
            /// <seealso cref="CreateBezier{T}(Base{T},bool)"/>
            /// </summary>
            public Bezier(IEnumerable<T> _frameWaypoints, bool _closed, bool _autoBuild)
                : base(_frameWaypoints, _closed, _autoBuild)
            {
                m_Control = CreateDefaultCtl();

            }

            //TODO fix when static factory bug is fixed (mono/unity)
            // <summary>
            // Factory construction method to create fully built Bezier spline.
            // </summary>
            // <param name="_frameWaypoints">List of control waypoints</param>
            // <param name="_closed">Closed spline?</param>
            // <param name="_dimsToSpline">Dimensions of type T to spline, null for all known dimensions.  Dimensions not in the list will be linear.</param>
            // <returns>Built Bezier spline</returns>
            /*public static Bezier<T> Create(List<T> _frameWaypoints, bool _closed, List<int> _dimsToSpline = null)
            {
             
                var bez = new Bezier<T>(_frameWaypoints, _closed, _dimsToSpline);
                bez.Build();
                return bez;
            }*/


            /// <summary>
            /// Bezier base calculation.
            /// </summary>
            /// <param name="_pct">Location along curve in percent to calculate (0 to 1f)</param>
            /// <param name="_p0">Start point</param>
            /// <param name="_p1">Start out control point</param>
            /// <param name="_p2">End in control point</param>
            /// <param name="_p3">End point</param>
            /// <returns>Calculated point</returns>
            public static float bezier(float _pct, float _p0, float _p1, float _p2, float _p3)
            {
                float p = 0;
                p += _p0 * ((1 - _pct) * (1 - _pct) * (1 - _pct));
                p += _p1 * (3 * _pct * (1 - _pct) * (1 - _pct));
                p += _p2 * (3 * _pct * _pct * (1 - _pct));
                p += _p3 * (_pct * _pct * _pct);
                return p;
            }

            /// <summary>
            /// Bezier derivative calculation.
            /// </summary>
            /// <param name="_pct">Location along curve in percent to calculate (0 to 1f)</param>
            /// <param name="_p0">Start point</param>
            /// <param name="_p1">Start out control point</param>
            /// <param name="_p2">End in control point</param>
            /// <param name="_p3">End point</param>
            /// <returns>Calculated derivative</returns>
            public static float bezierDeriv(float _pct, float _p0, float _p1, float _p2, float _p3)
            {
                float p = 0;
                p += _p0 * -3 * ((1 - _pct) * (1 - _pct));
                p += _p1 * 3 * ((1 - _pct) * (1 - _pct));
                p -= _p1 * 6 * _pct * (1 - _pct);
                p -= _p2 * 3 * _pct * _pct;
                p += _p2 * 6 * _pct * (1 - _pct);
                p += _p3 * 3 * _pct * +_pct;
                return p;
            }

            /*
            private static float bezier(float _pct, Control _a, Control _b, Teacher.FuncGetDim<T> _funcGetVal)
            {
                return bezier(_pct,
                    _funcGetVal(_a.Waypoint),
                    _funcGetVal(_a.OutPt),
                    _funcGetVal(_b.InPt),
                    _funcGetVal(_b.Waypoint));
            }
             * */

            /// See <see cref="Spline{T}.SplineValueAtN"/>
            protected override VecN SplineValueAtN(float _pct)
            {
                if (_pct <= 0f)
                    return m_FrameVN[0];
                if (_pct >= 1f)
                    return Closed ? m_FrameVN[0] : m_FrameVN[m_FrameVN.Count - 1];

                var bezCnt = m_FrameVN.Count;
                if (!Closed) bezCnt--;

                var bezPos = _pct * bezCnt;
                var bezIdx = (int)bezPos;
                float bezPct = (bezPos - bezIdx);

                var bezIdxB = bezIdx + 1;
                if (bezIdxB == m_FrameVN.Count)
                    bezIdxB = 0;


                float[] vals = new float[DTypeInfo<T>.DimCount];
                for (int idx = 0; idx < DTypeInfo<T>.DimCount; ++idx)
                {
                    var p0 = m_FrameVN[bezIdx].Vals[idx];
                    var p1 = p0 + m_Control[bezIdx].VOutN.Vals[idx];
                    var p3 = m_FrameVN[bezIdxB].Vals[idx];
                    var p2 = p3 + m_Control[bezIdxB].VInN.Vals[idx];
                    if (Contains(m_DimsToSpline, (idx + 1)))
                        vals[idx] = bezier(bezPct, p0, p1, p2, p3);
                    else
                        vals[idx] = DTypeInfo<T>.GetDimsFunc[idx + 1](FrameValueAt(_pct));
                }
                return new VecN(vals);
            }

            /// See <see cref="Spline{T}.SplineTangentAtN"/>
            protected override VecN SplineTangentAtN(float _pct)
            {
                var bezCnt = m_FrameVN.Count;
                if (!Closed) bezCnt--;

                var bezPos = _pct * bezCnt;
                var bezIdx = (int)bezPos;
                float bezPct = (bezPos - bezIdx);

                var bezIdxB = bezIdx + 1;
                if (bezIdxB == m_FrameVN.Count)
                    bezIdxB = 0;


                float[] vals = new float[DTypeInfo<T>.DimCount];
                for (int idx = 0; idx < DTypeInfo<T>.DimCount; ++idx)
                {
                    var p0 = m_FrameVN[bezIdx].Vals[idx];
                    var p1 = p0 + m_Control[bezIdx].VOutN.Vals[idx];
                    var p3 = m_FrameVN[bezIdxB].Vals[idx];
                    var p2 = p3 + m_Control[bezIdxB].VInN.Vals[idx];
                    if (Contains(m_DimsToSpline, (idx + 1)))
                        vals[idx] = bezierDeriv(bezPct, p0, p1, p2, p3);
                    else
                        vals[idx] = 0; //TODO incorrect
                }
                return new VecN(vals);

            }

            /// <summary>
            /// Create default control for specified waypoint along spline.
            /// </summary>
            /// <param name="_idx">Index of waypoint to create control for</param>
            /// <returns>Created control object</returns>
            public Control CreateDefaultCtl(int _idx)
            {
                var wps = m_FrameVN;
                var n = wps.Count;

                var wp = wps[_idx];
                VecN from = null;
                VecN to = null;
                if (_idx == 0)
                    from = Closed ? wps[n - 1] : wp;
                else if (_idx == n - 1)
                    to = Closed ? wps[0] : wp;

                from = from ?? wps[_idx - 1];
                to = to ?? wps[_idx + 1];

                var vOut = (to - from).Normalized;
                var vIn = vOut;
                var defaultMag = (to - from).Magnitude * Defaults.BezierMagPct;
                if (wp != to)
                    vOut *= defaultMag;

                if (wp != from)
                    vIn *= defaultMag;

                var cntl = new Control(vOut.To<T>(), vIn.Magnitude, vOut.Magnitude);
                return cntl;
            }

            /// <summary>
            /// Create default control for this bezier spline.
            /// </summary>
            /// Creates colinear control with magnitude based on neighboring waypoint data.
            /// <returns>Array of control objects</returns>
            private Control[] CreateDefaultCtl()
            {
                var control = new List<Control>();
                for (int idx = 0; idx < m_FrameVN.Count; ++idx)
                    control.Add(CreateDefaultCtl(idx));
                return control.ToArray();
            }

            /// Get a copy of array of control objects for this bezier spline, indexed by frame point index.
            public Control[] GetControl()
            {
                var cntl = new Control[m_Control.Length];
                for (int idx = 0; idx < m_Control.Length; ++idx)
                {
                    cntl[idx] = new Control(m_Control[idx]);
                }
                return cntl;
            }

            /// Get a copy of the control object at specified index of frame point.
            public Control GetControlAt(int _frameIdx)
            {
                if (_frameIdx < 0 || _frameIdx >= m_FrameVN.Count)
                {
                    Log.Err("SetControl : Frame waypoint index out of range.  Got " + _frameIdx);
                    return null;
                }

                return new Control(m_Control[_frameIdx]);
            }

            /// Set this Bezier spline's control with specified array of control.
            public Bezier<T> SetControl(Control[] _newControl)
            {
                if (_newControl.Length != m_Control.Length)
                {
                    Log.Err("SetControl : New control array length does not match.  Got " + _newControl.Length +
                            " and expected " + m_Control.Length);
                    return this;
                }

                for (int idx = 0; idx < _newControl.Length; ++idx)
                    m_Control[idx] = _newControl[idx];

                IBuild(false);
                return this;
            }

            /// Set this Bezier spline's control at specified frame point index with specified control object.
            public Bezier<T> SetControlAt(int _wayptIdx, Control _control)
            {
                if (_wayptIdx < 0 || _wayptIdx >= m_FrameVN.Count)
                {
                    Log.Err("SetControlAt : Waypoint index out of range.  Got " + _wayptIdx);
                    return this;
                }

                m_Control[_wayptIdx] = _control;

                IBuild(false);
                return this;
            }

            /// <summary>
            /// Set the position of the specified control's out point.
            /// </summary>
            /// <param name="_wayptIdx">Frame Point index for target control object</param>
            /// <param name="_outPt">Posotion to set control's out point</param>
            /// <returns>Updated Bezier spline object</returns>
            public Bezier<T> SetControlPosOut(int _wayptIdx, T _outPt)
            {
                if (_wayptIdx < 0 || _wayptIdx >= m_FrameVN.Count)
                {
                    Log.Err("SetControlOutPt : Waypoint index out of range.  Got " + _wayptIdx);
                    return this;
                }
                var cntl = m_Control[_wayptIdx];
                var wayPt = m_FrameVN[_wayptIdx];
                var outPt = ToVecN(_outPt);
                var newVec = (outPt - wayPt);
                if (ToVecN(cntl.OutVec) == newVec) return this;
                cntl.OutVec = newVec.To<T>();

                IBuild(false);

                return this;
            }

            /// <summary>
            /// Set the position of the specified control's in point.
            /// </summary>
            /// <param name="_wayptIdx">Frame Point index for target control object</param>
            /// <param name="_inPt">Posotion to set control's in point</param>
            /// <returns>Updated Bezier spline object</returns>
            public Bezier<T> SetControlPosIn(int _wayptIdx, T _inPt)
            {
                if (_wayptIdx < 0 || _wayptIdx >= m_FrameVN.Count)
                {
                    Log.Err("SetControlInPt : Waypoint index out of range.  Got " + _wayptIdx);
                    return this;
                }
                var cntl = m_Control[_wayptIdx];
                var wayPt = m_FrameVN[_wayptIdx];
                var inPt = ToVecN(_inPt);
                var newVec = (inPt - wayPt);
                if (newVec == ToVecN(cntl.InVec)) return this;
                cntl.InVec = newVec.To<T>();

                IBuild(false);
                return this;
            }


            /// <summary>
            /// Contains control information for the spline
            /// </summary>
            public class Control
            {

                private bool m_Colinear = true;

                /// VecN representation of in vector.  Pointing away from frame point.
                public VecN VInN;
                /// VecN representation of out vector.  Pointing away from frame point.
                public VecN VOutN;

                /// <summary>
                /// Bezier control object constructor
                /// </summary>
                /// <param name="_tanOutDir">Tangent out direction (in direction will be opposite, colinear)</param>
                /// <param name="_inMag">Distance to place IN control point from waypoint along tangent.</param>
                /// <param name="_outMag">Distance to place OUT control point from waypoint along tangent</param>
                public Control(T _tanOutDir, float _inMag, float _outMag)
                {
                    var outDir = ToVecN(_tanOutDir).Normalized;
                    VInN = -outDir * _inMag;
                    VOutN = outDir * _outMag;
                    m_Colinear = true;
                }

                /// <summary>
                /// Bezier control object constructor
                /// </summary>
                /// <param name="_inVec">Control in vector.  Pointing away from frame point.</param>
                /// <param name="_outVec">Control out vector. Pointing away from frame point.</param>
                /// <param name="_colinear">Maintain colinearity?</param>
                public Control(T _inVec, T _outVec, bool _colinear = true)
                {
                    VInN = ToVecN(_inVec);
                    VOutN = ToVecN(_outVec);
                    m_Colinear = _colinear;
                }


                /// <summary>
                /// Bezier control copy constructor.
                /// </summary>
                /// <param name="_control">Control data to copy</param>
                public Control(Control _control)
                {
                    VInN = _control.VInN;
                    VOutN = _control.VOutN;
                    m_Colinear = _control.Colinear;
                }

                private Control()
                {
                }

                /// Get/Set whether this control object is colinear.  (In/Out tangents are forced opposite directions)
                public bool Colinear
                {
                    get { return m_Colinear; }
                    set
                    {
                        if (m_Colinear == value) return;
                        m_Colinear = value;
                        if (m_Colinear)
                            OutVec = (-VInN.Normalized * VOutN.Magnitude).To<T>();
                    }
                }

                /// Distance between waypoint and IN control point along tangent axis
                public float InMag
                {
                    get { return VInN.Magnitude; }
                    set
                    {
                        if (VInN.Magnitude == value) return;
                        var mag = Math.Abs(value);
                        VInN = VInN.Normalized * mag;
                    }
                }

                /// Distance between waypoint and OUT control point along tangent axis
                public float OutMag
                {
                    get { return VOutN.Magnitude; }
                    set
                    {
                        if (VOutN.Magnitude == value) return;
                        var mag = Math.Abs(value);
                        VOutN = VOutN.Normalized * mag;
                    }
                }

                /// Get tangent direction pointing towards the OUT control point (from waypoint)
                public T OutDir
                {
                    get { return VOutN.Normalized.To<T>(); }
                }

                /// Get tangent direction pointing towards the IN control point (from waypoint)
                public T InDir
                {
                    get { return Colinear ? (-VOutN).Normalized.To<T>() : VInN.Normalized.To<T>(); }
                }


                /// Get/Set "In" control vector (waypoint to in control point)
                public T InVec
                {
                    get { return VInN.To<T>(); }
                    set
                    {
                        var inVec = ToVecN(value);
                        if (VInN == inVec) return;
                        VInN = inVec;
                        if (Colinear)
                            VOutN = -(VInN.Normalized) * OutMag;
                    }
                }

                /// Get/Set "Out" control vector (waypoint to out control point)
                public T OutVec
                {
                    get { return VOutN.To<T>(); }
                    set
                    {
                        var outVec = ToVecN(value);
                        if (VOutN == outVec) return;
                        VOutN = outVec;
                        if (Colinear)
                            VInN = -(VOutN.Normalized) * InMag;
                    }
                }

            }
        }

        /// <summary>
        /// Cubic spline object.
        /// </summary>
        /// <typeparam name="T">Waypoint Type</typeparam>
        public class Cubic<T> : Spline<T>
        {
            private CubicValue[][] m_Cubs;

            /// <summary>
            /// Constructor.  Note <see cref="Base{T}.Build()"/> is not called during construction.
            /// Use static factory methods.
            /// <seealso cref="CreateCubic{T}(bool, IEnumerable{T},bool)"/>
            /// <seealso cref="CreateCubic{T}(Base{T},bool)"/>
            /// </summary>
            public Cubic(IEnumerable<T> _frameWaypoints, bool _closed, bool _autoBuild)
                : base(_frameWaypoints, _closed, _autoBuild)
            {
            }

            // TODO Fix when static factory bug is fixed (mono / Unity)
            // <summary>
            // Factory construction method to create fully built cubic spline.
            // </summary>
            // <param name="_frameWaypoints">List of control waypoints</param>
            // <param name="_closed">Closed spline?</param>
            // <param name="_dimsToSpline">Dimensions of type T to spline, null for all known dimensions.  Dimensions not in the list will be linear.</param>
            // <returns>Built cubic spline</returns>
            /*public static Cubic<T> Create(List<T> _frameWaypoints, bool _closed, List<int> _dimsToSpline = null)
            {
                var cubic = new Cubic<T>(_frameWaypoints, _closed, _dimsToSpline);
                cubic.Build();
                return cubic;
            }*/

            /// Updates cubic values then calls base spline <see cref="Spline{T}.BuildPath"/>
            protected override void BuildPath()
            {
                m_Cubs = new CubicValue[m_FrameVN[0].DimCount][];
                foreach (var dim in m_DimsToSpline)

                    m_Cubs[dim - 1] = !Closed ?
                        CalcNatCubic(m_FrameVN, dim - 1) :
                        CalcNatCubicClosed(m_FrameVN, dim - 1);

                base.BuildPath();
            }

            /// See <see cref="Spline{T}.SplineValueAtN"/>
            protected override VecN SplineValueAtN(float _pct)
            {
                if (_pct <= 0f) return m_FrameVN[0];
                if (_pct >= 1f)
                    return Closed ? m_FrameVN[0] : m_FrameVN[m_FrameVN.Count - 1];
                var cubPosition = _pct * m_Cubs[m_DimsToSpline[0] - 1].Length;
                int cubicNum = (int)cubPosition;
                float cubicPos = (cubPosition - cubicNum);

                float[] vals = new float[m_FrameVN[0].DimCount];
                for (int idx = 0; idx < m_FrameVN[0].DimCount; ++idx)
                {
                    if (Contains(m_DimsToSpline, (idx + 1)))
                        vals[idx] = m_Cubs[idx][cubicNum].Evaluate(cubicPos);
                    else
                        vals[idx] = FrameValueAtN(_pct).Vals[idx];
                }
                return new VecN(vals);
            }

            /// See <see cref="Spline{T}.SplineTangentAtN"/>
            protected override VecN SplineTangentAtN(float _pct)
            {
                if (_pct <= 0f) _pct = 0;
                if (_pct >= 1f)
                    _pct = Closed ? 0f : 1f;
                var cubPosition = _pct * m_Cubs[m_DimsToSpline[0] - 1].Length;
                int cubicNum = (int)cubPosition;
                float cubicPos = (cubPosition - cubicNum);

                float[] vals = new float[DTypeInfo<T>.DimCount];
                for (int idx = 0; idx < DTypeInfo<T>.DimCount; ++idx)
                {
                    if (Contains(m_DimsToSpline, (idx + 1)))
                        vals[idx] = m_Cubs[idx][cubicNum].EvaluateDeriv(cubicPos);
                    else
                        vals[idx] = 0; //TODO incorrect
                }
                return new VecN(vals);
            }

            private static CubicValue[] CalcNatCubic(List<VecN> _vals, int _dim)
            {
                Func<int, float> getVal = (_idx) => _vals[_idx].Vals[_dim];

                int n = _vals.Count - 1;

                float[] gamma = new float[n + 1];
                float[] delta = new float[n + 1];
                float[] D = new float[n + 1];

                int i;
                gamma[0] = 1.0f / 2.0f;
                for (i = 1; i < n; i++)
                    gamma[i] = 1.0f / (4.0f - gamma[i - 1]);

                gamma[n] = 1.0f / (2.0f - gamma[n - 1]);

                float p0 = getVal(0);
                float p1 = getVal(1);

                delta[0] = 3.0f * (p1 - p0) * gamma[0];
                for (i = 1; i < n; i++)
                {
                    p0 = getVal(i - 1);
                    p1 = getVal(i + 1);
                    delta[i] = (3.0f * (p1 - p0) - delta[i - 1]) * gamma[i];
                }
                p0 = getVal(n - 1);
                p1 = getVal(n);

                delta[n] = (3.0f * (p1 - p0) - delta[n - 1]) * gamma[n];

                D[n] = delta[n];
                for (i = n - 1; i >= 0; i--)
                    D[i] = delta[i] - gamma[i] * D[i + 1];

                CubicValue[] cubics = new CubicValue[n];

                for (i = 0; i < n; i++)
                {
                    p0 = getVal(i);
                    p1 = getVal(i + 1);

                    cubics[i] = new CubicValue(
                        p0,
                        D[i],
                        3 * (p1 - p0) - 2 * D[i] - D[i + 1],
                        2 * (p0 - p1) + D[i] + D[i + 1]
                        );
                }
                return cubics;
            }

            private static CubicValue[] CalcNatCubicClosed(List<VecN> _values, int _dim)
            {

                var values = new List<VecN>(_values);
                values.Add(values[0]);
                var n = values.Count - 2;
                Func<int, float> getVal = (_idx) => values[_idx].Vals[_dim];

                float[] w = new float[n + 1];
                float[] v = new float[n + 1];
                float[] y = new float[n + 1];
                float[] D = new float[n + 1];
                float z, F, G, H;
                int k;

                w[1] = v[1] = z = 1.0f / 4.0f;
                y[0] = z * 3 * getVal(1) - getVal(n);
                H = 4;
                F = 3 * (getVal(0) - getVal(n - 1));
                G = 1;
                for (k = 1; k < n; k++)
                {
                    v[k + 1] = z = 1 / (4 - v[k]);
                    w[k + 1] = -z * w[k];
                    y[k] = z * (3 * (getVal(k + 1) - getVal(k - 1)) - y[k - 1]);
                    H = H - G * w[k];
                    F = F - G * y[k - 1];
                    G = -v[k] * G;
                }
                H = H - (G + 1) * (v[n] + w[n]);
                y[n] = F - (G + 1) * y[n - 1];

                D[n] = y[n] / H;
                D[n - 1] = y[n - 1] - (v[n] + w[n]) * D[n];
                for (k = n - 2; k >= 0; k--)
                {
                    D[k] = y[k] - v[k + 1] * D[k + 1] - w[k + 1] * D[n];
                }

                CubicValue[] C = new CubicValue[n + 1];
                for (k = 0; k < n; k++)
                {
                    C[k] = new CubicValue((float)getVal(k), D[k],
                        3 * (getVal(k + 1) - getVal(k)) - 2 * D[k] - D[k + 1],
                        2 * (getVal(k) - getVal(k + 1)) + D[k] + D[k + 1]);
                }
                C[n] = new CubicValue((float)getVal(n), D[n],
                    3 * (getVal(0) - getVal(n)) - 2 * D[n] - D[0],
                    2 * (getVal(n) - getVal(0)) + D[n] + D[0]);
                return C;
            }

            private class CubicValue
            {
                private float a, b, c, d;

                public CubicValue(float _a, float _b, float _c, float _d)
                {
                    a = _a;
                    b = _b;
                    c = _c;
                    d = _d;
                }

                public float Evaluate(float _u)
                {
                    return (((d * _u) + c) * _u + b) * _u + a;
                }

                public float EvaluateDeriv(float _u)
                {
                    return (3 * d * _u + 2 * c) * _u + b;
                }
            }
        }

        /// <summary>
        /// Linear path.
        /// </summary>
        /// <typeparam name="T">Waypoint Type</typeparam>
        public class Linear<T> : Base<T>
        {


            /// <summary>
            /// Constructor.  Note <see cref="Base{T}.Build"/> is not called during construction.
            /// Use static factory methods.
            /// <seealso cref="CreateLinear{T}(bool,IEnumerable{T},bool)"/>
            /// <seealso cref="CreateLinear{T}(Base{T},bool)"/>
            /// </summary>
            public Linear(IEnumerable<T> _waypoints, bool _closed, bool _autoBuild)
                : base(_waypoints, _closed, _autoBuild)
            {
            }


            // <summary>
            // Factory construction method to create fully built linear path.
            // </summary>
            // <param name="_waypoints">List of control waypoints</param>
            // <param name="_closed">Closed spline?</param>
            // <returns>Built linear path</returns>
            /*
            public static Linear<U> Create<U>(bool _closed, List<U> _waypoints)
            {
                var path = new Linear<U>(_waypoints, _closed);
                path.Build();
                return path;
            }*/

            /// Builds PathPoints and PathSegments
            protected override void BuildPath()
            {
                var points = new List<VecN>(m_FrameVN);
                if (Closed)
                    points.Add(m_FrameVN[0]);
                PathPointsVN = points.ToArray();
                PathPoints = VecN.ToArray<T>(points);
                m_PathSegments = m_FrameSegments;
                return;
            }
        }


        #endregion Other

    }

    /// <summary>
    /// Vector class with variable dimension count.  Used internally to assist with splining and paths.
    /// </summary>
    public class VecN
    {
        #region Fields

        /// Dimension data
        public float[] Vals;

        #endregion Fields

        #region Constructors

        static VecN()
        {
            ioDriver.Init();
        }

        /// <summary>
        /// VecN constructor
        /// </summary>
        /// <param name="_vals">Dimension data</param>
        public VecN(params float[] _vals)
        {
            Vals = _vals;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="_source">VecN data to copy for new VecN</param>
        public VecN(VecN _source)
        {
            Vals = new float[_source.Vals.Length];
            _source.Vals.CopyTo(Vals, 0);
        }


        #endregion Constructors

        #region Properties

        /// Returns the number of dimensions
        public int DimCount
        {
            get { return Vals.Length; }
        }

        /// Returns the magnitude of this vector.
        public float Magnitude
        {
            get
            {
                return (float)Math.Sqrt(MagnitudeSquared);
            }
        }

        /// Returns the magnitude of this vector squared.
        public float MagnitudeSquared
        {
            get
            {
                float sqrDist = 0;
                for (int idx = 0; idx < DimCount; ++idx)
                    sqrDist += Vals[idx] * Vals[idx];

                return (float)sqrDist;
            }
        }

        /// Get a vector that represents this vector normalized.
        public VecN Normalized
        {
            get
            {

                var mag = Magnitude;
                var norm = new float[DimCount];
                for (int idx = 0; idx < DimCount; ++idx)
                {
                    norm[idx] = Vals[idx] / mag;
                }
                return new VecN(norm);
            }
        }

        #endregion Properties

        #region Methods

        /// Vector inequality
        public static bool operator !=(VecN _a, VecN _b)
        {
            return !(_a == _b);
        }

        /// Scalar multiplication
        public static VecN operator *(VecN _a, float _scalar)
        {
            var scaled = new float[_a.DimCount];
            for (int idx = 0; idx < _a.DimCount; ++idx)
                scaled[idx] = _a.Vals[idx] * _scalar;
            return new VecN(scaled);
        }

        /// Vector addition
        public static VecN operator +(VecN _a, VecN _b)
        {
            if (_a.DimCount != _b.DimCount)
                throw new Exception("Dimension count must match.");
            var added = new float[_a.DimCount];
            for (int idx = 0; idx < _a.DimCount; ++idx)
                added[idx] = _a.Vals[idx] + _b.Vals[idx];
            return new VecN(added);
        }

        /// Vector subtraction
        public static VecN operator -(VecN _a, VecN _b)
        {
            if (_a.DimCount != _b.DimCount)
                throw new Exception("Dimension count must match.");
            var subt = new float[_a.DimCount];
            for (int idx = 0; idx < _a.DimCount; ++idx)
                subt[idx] = _a.Vals[idx] - _b.Vals[idx];
            return new VecN(subt);
        }

        /// Vector negative
        public static VecN operator -(VecN _vec)
        {
            var neg = new float[_vec.DimCount];
            for (int idx = 0; idx < _vec.DimCount; ++idx)
                neg[idx] = -_vec.Vals[idx];
            return new VecN(neg);
        }

        /// Vector equality
        public static bool operator ==(VecN _a, VecN _b)
        {
            if (System.Object.ReferenceEquals(_a, _b))
                return true;

            if (((object)_a == null) || ((object)_b == null))
                return false;

            if (_a.DimCount != _b.DimCount)
                return false;

            for (int idx = 0; idx < _a.DimCount; ++idx)
                if (_a.Vals[idx] != _b.Vals[idx]) return false;
            return true;
        }

        /// <summary>
        /// Get normalized direction of type T given a from and to point (of type T)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="_from">From point</param>
        /// <param name="_to">To Point</param>
        /// <returns>Direction result</returns>
        public static T Direction<T>(T _from, T _to)
        {
            return (ToVecN(_to) - ToVecN(_from)).Normalized.To<T>();
        }

        /// Vector Dot Product
        public static float Dot(VecN _a, VecN _b)
        {
            if (_a.DimCount != _b.DimCount)
                throw new Exception("Dimension count must match.");
            var dimCount = _a.DimCount;
            float tVal = 0;
            for (int idx = 0; idx < dimCount; ++idx)
                tVal += _a.Vals[idx] * _b.Vals[idx];
            return tVal;
        }

        /// <summary>
        /// Get angle in radians between specified VecNs.
        /// </summary>
        /// <param name="_a">First Vector</param>
        /// <param name="_b">Second Vector</param>
        /// <returns>Angle in radians</returns>
        public static float Angle(VecN _a, VecN _b)
        {
            var dot = Dot(_a, _b);
            var magA = _a.Magnitude;
            var magB = _b.Magnitude;
            var dotMag = dot / (magA * magB);
            if ((double)dotMag >= 1d) return 0f;
            if ((double)dotMag <= -1d) return (float)Math.PI;
            var rsltD = Math.Acos(dotMag);

            return (float)Math.Acos(dotMag);
        }

        /// Vector equality with precision
        public static bool EqualsApprox(VecN _a, VecN _b, float _precision = 0.0001f)
        {
            if (System.Object.ReferenceEquals(_a, _b))
                return true;

            if (((object)_a == null) || ((object)_b == null))
                return false;

            if (_a.DimCount != _b.DimCount)
                return false;

            for (int idx = 0; idx < _a.DimCount; ++idx)
                if (!ioMath.EqualsApprox(_a.Vals[idx], _b.Vals[idx])) return false;
            return true;
        }

        /// Object equality
        public override bool Equals(System.Object obj)
        {
            if (obj == null) return false;

            VecN p = obj as VecN;
            if ((System.Object)p == null) return false;

            for (int idx = 0; idx < DimCount; ++idx)
                if (Vals[idx] != p.Vals[idx]) return false;
            return true;
        }

        /// Vector equality
        public bool Equals(VecN _p)
        {
            if ((object)_p == null) return false;

            for (int idx = 0; idx < DimCount; ++idx)
                if (Vals[idx] != _p.Vals[idx]) return false;
            return true;
        }

        /// Hash code calculation
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (var val in Vals)
                    hash = hash*23 + val.GetHashCode();
                return hash;
            }
            
        }

        /// Convert to type T (T must have constructor defined for this VecN's dim count) 
        /// <seealso cref="Teacher.TeachCoord{T}(int, Teacher.FuncConstruct{T}, Teacher.FuncGetDim{T}[])"/>
        /// <seealso cref="Teacher.TeachCoord{T}(Dictionary{int,Teacher.FuncConstruct{T}}, Teacher.FuncGetDim{T}[])"/>
        public T To<T>()
        {
            return DTypeInfo<T>.Constructs[DimCount](Vals);
        }

        /// Convert to array of type T.
        /// <seealso cref="Teacher.TeachCoord{T}(int, Teacher.FuncConstruct{T}, Teacher.FuncGetDim{T}[])"/>
        /// <seealso cref="Teacher.TeachCoord{T}(Dictionary{int,Teacher.FuncConstruct{T}}, Teacher.FuncGetDim{T}[])"/>
        public static T[] ToArray<T>(IEnumerable<VecN> _points)
        {
            return _points.Select(_pt => _pt.To<T>()).ToArray();
        }

        /// String representation of this vector
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("( ");
            for (int idx = 0; idx < DimCount; ++idx)
                sb.Append(Vals[idx] + ", ");
            sb.Append(" )");
            return sb.ToString();
        }

        /// <summary>
        /// Linearly interpolate line defined by two points.
        /// </summary>
        /// <param name="_ptA">Line definition point A</param>
        /// <param name="_ptB">Line definition point B</param>
        /// <param name="_pct">Percent along line to calculate (0 to 1f)</param>
        /// <returns>Calculated LERP point</returns>
        public static VecN Lerp(VecN _ptA, VecN _ptB, float _pct)
        {
            var rslt = new float[_ptA.DimCount];
            for (int dim = 0; dim < _ptA.DimCount; ++dim)
                rslt[dim] = ioMath.Lerp(_ptA.Vals[dim], _ptB.Vals[dim], _pct);
            return new VecN(rslt);
        }

        /// <summary>
        /// Inverse lerp calculation.  Does not verify if specified value is part of line.
        /// Checks each dimension for ILerp value linearly until valid (_a dim != _b dim) is found.
        /// </summary>
        /// <param name="_ptA">Line start point</param>
        /// <param name="_ptB">Line end point</param>
        /// <param name="_value">Value to ILerp on line</param>
        /// <returns>Percent of value along line, NaN on fail</returns>
        public static float ILerp(VecN _ptA, VecN _ptB, VecN _value)
        {
            var dimCount = _ptA.DimCount;
            float result = float.NaN;

            if (_ptA == _value) return 0f;
            if (_ptB == _value) return 1f;

            for (int dim = 0; dim < dimCount; ++dim)
            {
                float from = _ptA.Vals[dim];
                float to = _ptB.Vals[dim];
                float val = _value.Vals[dim];
                if (from == to)
                    continue;
                result = (from - val) / (from - to);
            }

            return result;
        }

        /// <summary>
        /// Find distance of point from closest point on specified line.
        /// </summary>
        /// <param name="_linePtA">Line point A</param>
        /// <param name="_linePtB">Line point B</param>
        /// <param name="_point">Point to measure distance from</param>
        /// <param name="_pointOnLine">Closest point on line</param>
        /// <returns>Distance between point and closest point on line</returns>
        public static float PointToLineDistance(VecN _linePtA, VecN _linePtB, VecN _point, out VecN _pointOnLine)
        {
            return (float)Math.Sqrt(PointToLineDistanceSquared(_linePtA, _linePtB, _point, out _pointOnLine));
        }

        /// See <see cref="PointToLineDistance"/> (which is the square root of the result of this function)
        public static float PointToLineDistanceSquared(VecN _linePtA, VecN _linePtB, VecN _point, out VecN _pointOnLine)
        {
            var line = _linePtB - _linePtA;
            var lineMag = line.Magnitude;
            var lambda = VecN.Dot((_point - _linePtA), line) / lineMag;
            lambda = Math.Min(Math.Max(0f, lambda), lineMag);
            var p = line * lambda * (1 / lineMag);
            _pointOnLine = _linePtA + p;
            return (_linePtA + p - _point).MagnitudeSquared;
        }

        /// <summary>
        /// Return the nearest point found on line (defined by two points) to specified point.
        /// </summary>
        /// <param name="_linePtA">Line definition point A</param>
        /// <param name="_linePtB">Line definition point B</param>
        /// <param name="_point">Point to find nearest to on line</param>
        /// <returns>Closest point on line to _point</returns>
        public static VecN NearestPointOnLine(VecN _linePtA, VecN _linePtB, VecN _point)
        {
            var lineDir = (_linePtB - _linePtA).Normalized;
            var v = _point - _linePtA;
            var d = VecN.Dot(v, lineDir);
            return _linePtA + lineDir * d;
        }

        #endregion Methods
    }

    #endregion Nested Types
}