
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
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

        /// Get an array of the waypoints of the path, untyped (objects).
        object[] GetPathPoints();

        /// Try to get an array of the waypoints of the path as specified type.  Returns success.
        bool TryGetPathAs<T>(out T[] _points);

        /// <summary>
        /// Attempt to get PathPoints's waypoints as a list of float values.
        /// </summary>
        /// The float array's first dimension is ordered index of retrieved waypoints.  
        /// Second dimension is retreived data by waypoint dimension (x,y,z,w,etc.).
        /// If the waypoints could not be converted to float, the float array will be set to null.
        /// <param name="_points">Array to populate with waypoint data.</param>
        /// <returns>Whether the retrieval was successful or not.</returns>
        bool TryPathFloat(out float[,] _points);

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

        /// See <see cref="Path.Spline{T}.SegmentLength"/>
        float SegmentLength
        {
            get;
            set;
        }

        /// See <see cref="Path.Spline{T}.SegmentAccuracy"/>
        float SegmentAccuracy { get; set; }

        Path.SplinePointMode PointMode { get; set; }

        float MinAngle { get; set; }
        float MinAngleMinLength { get; set; }


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
    /// Create new cubic spline from this path/spline's 
    /// <see cref="Path.Base{T}.m_FrameWaypoints">frame</see>.
    /// </summary>
    /// <typeparam name="T">Waypoint type</typeparam>
    /// <param name="_pathObj">this path object</param>
    /// <returns>New cubic spline</returns>
    public static Path.Cubic<T> SplineCubic<T>(this Path.Base<T> _pathObj, bool _autoBuild = true)
    {
        return Path.CreateCubic(_pathObj, _autoBuild);
    }

    /// <summary>
    /// Create new Bezier spline from this path/spline's 
    /// <see cref="Path.Base{T}.m_FrameWaypoints">frame</see>.
    /// </summary>
    /// <typeparam name="T">Waypoint type</typeparam>
    /// <param name="_pathObj">this path object</param>
    /// <returns>New Bezier spline</returns>
    public static Path.Bezier<T> SplineBezier<T>(this Path.Base<T> _pathObj, bool _autoBuild = true)
    {
        return Path.CreateBezier(_pathObj, _autoBuild);
    }

    /// <summary>
    /// Create new linear path from this spline's <see cref="Path.Base{T}.m_FrameWaypoints">frame</see>.
    /// </summary>
    /// <typeparam name="T">Waypoint type</typeparam>
    /// <param name="_splineObj">this spline</param>
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
    /// <returns>New linear path</returns>
    public static Path.Linear<T> CreateLinearFromSpline<T>(this Path.Spline<T> _splineObj, bool _autoBuild = true)
    {
        return Path.CreateLinear(_splineObj.Closed, ToList(_splineObj.PathPoints), _autoBuild);
    }





    private static VecN ToVecN<T>(T _obj)
    {
        Init();
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

        /// PathPoints object to drive from.
        protected Path.Base<TTar> m_Path;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Speed PathPoints driver action constructor.
        /// <seealso cref="DSpeed{TTar}"/>
        /// </summary>
        /// <param name="_path">PathPoints of waypoints</param>
        public DSpeedPath(Action<TTar> _tarAction, Path.Base<TTar> _path, Func<float> _speedDriver, string _name = null)
            : base(_tarAction, _speedDriver, _name)
        {
            m_Path = _path;
        }

        /// <summary>
        /// Speed PathPoints driver action constructor.
        /// <seealso cref="DSpeed{TTar}"/>
        /// </summary>
        /// <param name="_path">PathPoints of waypoints</param>
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

        /// <summary>
        /// Constructor using target action. See <see cref="DTween{TTar}"/>
        /// </summary>
        /// <param name="_path">Path to tween through</param>
        public DTweenPath(Action<TTar> _tarAction, Path.Base<TTar> _path, float _cycleDuration, string _name = null)
            : base(_tarAction, _cycleDuration, _name)
        {
            m_Path = _path;
        }

        /// <summary>
        /// Constructor using expression. See <see cref="DTween{TTar}"/>
        /// </summary>
        /// <param name="_path">Path to tween through</param>
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
        /// <returns>New linear path object</returns>
        public static Linear<T> CreateLinear<T>(bool _closed, List<T> _waypoints, bool _autoBuild = true)
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
        /// <returns>New Bezier spline object</returns>
        public static Bezier<T> CreateBezier<T>(bool _closed, List<T> _frameWaypoints, bool _autoBuild = true)
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
        /// <returns>New cubic spline object</returns>
        public static Cubic<T> CreateCubic<T>(bool _closed, List<T> _frameWaypoints, bool _autoBuild = true)
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

            protected VecN[] PathPointsVN;

            /// Array of segments in path.
            protected Segment[] m_PathSegments;

            /// List of raw waypoints used to define the path's frame (the points the path is to pass through), 
            /// will not have duplicate beginning and end points if closed
            protected List<T> m_FrameWaypoints;

            protected List<VecN> m_FrameVN;

            /// List of raw segments used to define the path's frame (the points the path is to pass through).
            /// Will include final segment if closed.
            protected Segment[] m_FrameSegments;

            /// Is this a closed path?
            protected bool m_Closed;

            /// How long before timeout in <see cref="BuildPath"/>
            public float TimeOut = 1000;

            public bool IsValid { get; private set; }

            public bool AutoBuild;

            /// <summary>
            /// Base path object constructor.  Note that <see cref="Build"/> is not called during construction.
            /// Child classes will need to either call this in their constructor or through a static factory method.
            /// </summary>
            /// <param name="_frameWaypoints">List of waypoints used to define path's frame</param>
            /// <param name="_closed">Is this a closed path?</param>
            protected Base(IEnumerable<T> _frameWaypoints, bool _closed, bool _autoBuild)
            {
                m_FrameWaypoints = _frameWaypoints.ToList();
                m_Closed = _closed;
                //If closed enforce that first and last are not equal.
                if (m_Closed)
                    if (m_FrameWaypoints[0].Equals(m_FrameWaypoints[m_FrameWaypoints.Count - 1]))
                        m_FrameWaypoints.RemoveAt(m_FrameWaypoints.Count - 1);

                BuildFrame();
                IsValid = false;
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
                if (_index < 0f || Closed ? _index > m_FrameWaypoints.Count : _index > m_FrameWaypoints.Count - 1)
                {
                    Log.Err("Waypoint index out of range.  Received '" + _index + "'");
                    return float.NaN;
                }
                if (_index == 0) return 0f;
                return _index == (Closed ? m_FrameWaypoints.Count : m_FrameWaypoints.Count - 1) ? 1f : m_FrameSegments[_index].PctStart;
            }

            public T GetNearestTo(T _point)
            {
                float pct;
                return GetNearestTo(_point, out pct);
            }

            public T GetNearestTo(T _point, out float _pctOnPath)
            {
                Segment seg;
                return GetNearestTo(_point, out _pctOnPath, out seg);
            }

            public T GetNearestTo(T _point, out float _pctOnPath, out Segment _nearestSegment)
            {
                var closestDist = float.PositiveInfinity;
                Segment closestSeg = null;
                VecN pointOnLine = null;
                var segs = PathSegments;
                var pts = PathPoints;

                for (int idx = 0; idx < segs.Length; ++idx)
                {
                    var curSeg = segs[idx];
                    var segPtA = ToVecN(pts[curSeg.FromIdx]);
                    var segPtB = ToVecN(pts[curSeg.FromIdx + 1]);
                    VecN curPtOnLine = null;
                    var curDist = VecN.PointToLineDistanceSquared(segPtA, segPtB, ToVecN(_point), out curPtOnLine);
                    if (curDist < closestDist)
                    {
                        closestDist = curDist;
                        pointOnLine = curPtOnLine;
                        closestSeg = curSeg;
                    }
                }

                var nearestOnPath = pointOnLine.To<T>();
                _nearestSegment = closestSeg;
                var pctInSeg = VecN.ILerp(ToVecN(pts[closestSeg.FromIdx]), ToVecN(pts[closestSeg.FromIdx + 1]),
                    ToVecN(nearestOnPath));

                _pctOnPath = Teacher.Lerpf(_nearestSegment.PctStart, _nearestSegment.PctEnd, pctInSeg);

                return nearestOnPath;
            }


            /// Add new frame waypoint
            public virtual void AddFrameWaypoint(T _waypoint)
            {
                m_FrameWaypoints.Add(_waypoint);
                IBuild(true);
            }

            /// Insert new frame waypoint at specified index.
            public virtual void InsertFrameWaypoint(int _index, T _waypoint)
            {
                m_FrameWaypoints.Insert(_index, _waypoint);
                IBuild(true);
            }

            /// Replace the data at index with specified data.
            public virtual void UpdateFrameWaypoint(int _index, T _waypoint)
            {
                m_FrameWaypoints[_index] = _waypoint;
                IBuild(true);
            }

            /// Remove frame waypoint at specified index.
            public virtual void RemoveFrameWaypoint(int _index)
            {
                m_FrameWaypoints.RemoveAt(_index);
                IBuild(true);
            }

            /// Get an array of this path's frame waypoints.
            public T[] GetFrameWaypoints()
            {
                return m_FrameWaypoints.ToArray();
            }

            /// <summary>
            /// Offset all of this path's frame points by T and rebuild the path.
            /// </summary>
            /// <param name="_offset">Offset value</param>
            public void Offset(T _offset)
            {
                for (int idx = 0; idx < m_FrameWaypoints.Count; ++idx)
                    m_FrameWaypoints[idx] = (ToVecN(m_FrameWaypoints[idx]) + ToVecN(_offset)).To<T>();

                IBuild(true);
            }


            private static T LerpPath(float _pct, T[] _points, Segment[] _segments, float _pathLen, bool _closed)
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

                T fromVal = _points[tgtSeg.FromIdx];
                T toVal;
                if (_closed && ((tgtSeg.FromIdx + 1) == _points.Length))
                    toVal = _points[0];
                else
                    toVal = _points[tgtSeg.FromIdx + 1];

                return DTypeInfo<T>.Lerp(fromVal, toVal, segTgtLen / _segments[segIdx - 1].Length);
            }

            /// <summary>
            /// Get this path's value at specified percent along path via <see cref="PathPoints"/>.
            /// </summary>
            /// <param name="_pct">Percent along path (0 to 1)</param>
            /// <returns></returns>
            public T ValueAt(float _pct)
            {
                return LerpPath(_pct, PathPoints, m_PathSegments, PathLength, false);
            }

            /// <summary>
            /// Get this path's frame value at specified percent along frame.
            /// </summary>
            /// <param name="_pct"></param>
            /// <returns></returns>
            protected T FrameWaypointValueAt(float _pct)
            {
                var frmPts = ToList(m_FrameWaypoints);
                if (Closed)
                    frmPts.Add(m_FrameWaypoints[0]);
                return LerpPath(_pct, m_FrameWaypoints.ToArray(), m_FrameSegments, FrameLength, m_Closed);
            }

            protected void IBuild(bool _rebuildFrame)
            {
                if (_rebuildFrame)
                    BuildFrame();

                if (AutoBuild)
                    IsValid = BuildPath();
                else
                    IsValid = false;
            }

            /// Calculate path data, populate <see cref="PathPoints"/> and <see cref="PathSegments"/>
            public void Build()
            {
                IsValid = BuildPath();
            }

            private void BuildFrame()
            {
                m_FrameVN = ToVecNs(m_FrameWaypoints).ToList();

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

                /* - T way
                var totLen = 0f;
                var lengths = new float[m_Closed ? m_FrameWaypoints.Count : m_FrameWaypoints.Count - 1];
                for (int idx = 0; idx < m_FrameWaypoints.Count - 1; ++idx)
                {
                    lengths[idx] = DTypeInfo<T>.Length(m_FrameWaypoints[idx], m_FrameWaypoints[idx + 1]);
                    totLen += lengths[idx];
                }
                if (m_Closed)
                {
                    lengths[lengths.Length - 1] =
                        DTypeInfo<T>.Length(m_FrameWaypoints[m_FrameWaypoints.Count - 1], m_FrameWaypoints[0]);
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
                 * */

            }

            /// Override to update <see cref="PathPoints"/> and <see cref="PathSegments"/> here.
            /// This function is iterated until done or until time out. TODO add doc for TimeOut
            protected abstract bool BuildPath();


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

                public override string ToString()
                {
                    return "Seg - FrmIdx: " + FromIdx + " PctStart: " + PctStart + " PctEnd: " + PctEnd + " Len: " + Length;
                }
            }

            bool IPath.TryPathFloat(out float[,] _points)
            {
                if (m_FrameWaypoints.Count == 0)
                {
                    _points = null;
                    return false;
                }

                int dCount = DTypeInfo<T>.DimCount;
                if (dCount == 0)
                {
                    _points = null;
                    return false;
                }

                _points = new float[PathPoints.Length, dCount];
                for (int idx = 0; idx < PathPoints.Length; ++idx)
                    for (int dc = 1; dc <= dCount; ++dc)
                        _points[idx, dc - 1] = DTypeInfo<T>.GetDimsFunc[dc](PathPoints[idx]);
                return true;
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

            object[] IPath.GetPathPoints()
            {
                var asObj = new object[PathPoints.Length];
                for (int idx = 0; idx < PathPoints.Length; ++idx)
                    asObj[idx] = (object)PathPoints[idx];
                return asObj;
            }

            Type IPath.WaypointType
            {
                get { return typeof(T); }
            }

        }

        public enum SplinePointMode
        {
            MinAngle,
            SegmentLength
        }

        /// <summary>
        /// Base spline object.
        /// </summary>
        /// <typeparam name="T">Waypoint Type</typeparam>
        public abstract class Spline<T> : Base<T>, ISpline
        {
            public const float MA_LENGTH_AUTO = float.NegativeInfinity;

            /// Backing field for <see cref="DimsToSpline"/>
            protected int[] m_DimsToSpline;

            /// Backing field for <see cref="SegmentLength"/>
            protected float m_SegmentLength;

            /// Backing field for <see cref="SegmentAccuracy"/>
            private float m_SegmentAccuracy;

            private SplinePointMode m_PointMode;

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


            private float m_MinAngle;

            public float MinAngle
            {
                get { return m_MinAngle; }
                set
                {
                    if (value <= 0 || value >= 180)
                    {
                        Log.Err("Min Angle must be between 0 and 180 degrees.  Setting Min Angle to 90 Degrees.");
                        m_MinAngle = 90f;
                    }
                    if (m_MinAngle == value) return;
                    m_MinAngle = value;
                    if (PointMode == SplinePointMode.MinAngle)
                        IBuild(false);
                }
            }

            private float m_MinAngleMinLength;

            public float MinAngleMinLength
            {
                get { return m_MinAngleMinLength; }
                set
                {
                    if (value <= 0 && value != MA_LENGTH_AUTO)
                    {
                        m_MinAngleMinLength = GetDefaultMinAngleMinLength();
                        Log.Err("Min Angle Min length must greater than zero.  Setting to " + m_MinAngleMinLength);

                    }
                    else
                    {
                        if (m_MinAngleMinLength == value) return;
                        m_MinAngleMinLength = value;
                    }

                    if (PointMode == SplinePointMode.MinAngle)
                        IBuild(false);
                }
            }


            /// <summary>
            /// Constructor.  Note <see cref="Base{T}.Build"/> is not called during construction.
            /// <seealso cref="Base{T}(System.Collections.Generic.List{T},bool)"/>
            /// </summary>
            protected Spline(IEnumerable<T> _frameWaypoints, bool _closed, bool _autoBuild)
                : base(_frameWaypoints, _closed, _autoBuild)
            {
                m_DimsToSpline = CheckDims(null);
                m_SegmentLength = GetDefaultSegmentLength();
                m_MinAngleMinLength = MA_LENGTH_AUTO;
            }

            /// Get/Set spline segment lengths.  
            /// Calls <see cref="UpdatePath"/> on change.
            /// Setting this to a new value will rebuild the spline.
            public float SegmentLength
            {
                get
                {
                    return m_SegmentLength;
                }
                set
                {
                    if (m_SegmentLength == value) return;
                    if (value <= 0)
                    {
                        m_SegmentLength = GetDefaultSegmentLength();
                        Log.Err("Segment length must greater than zero.  Setting to '" + m_SegmentLength + "'");
                    }
                    else
                        m_SegmentLength = value;
                    if (PointMode == SplinePointMode.SegmentLength)
                        IBuild(false);
                }
            }

            /// Get/Set the percentage of accuracy when calculating segments lengths.  Between 0 and 1f.  Closer to zero will be more accurate but will be less efficient.
            /// ie. Segments will be of length of 2.0f +/- 0.1 with <see cref="SegmentLength"/> of 2.0f and Segment Accuracy of 0.05
            /// On set, will rebuild the spline if new value is less than current value.  Will not rebuild otherwise.
            public float SegmentAccuracy
            {
                get { return m_SegmentAccuracy; }
                set
                {
                    if (m_SegmentAccuracy == value) return;
                    var val = value;
                    if (value <= 0 || value >= 0.9999f)
                    {
                        val = 0.25f;
                        Log.Err("Segment accuracy cannot be less than zero and must be less than 1.  Setting to '" + val + "'");
                    }
                    m_SegmentAccuracy = val;
                    if (PointMode == SplinePointMode.SegmentLength)
                        IBuild(false);
                }

            }

            /// List of dimensions to be splined from frame waypoints.
            /// <see cref="UpdatePath"/> is called on change
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
            /// Abstract - Override to implement.  Built in splines (see <see cref="Bezier{T}"/> and <see cref="Cubic{T}"/>) caluclate value using
            /// Spline's mathematical formula.  This will not produce the same result as <see cref="Base{T}.ValueAt"/>, that calculation is done using 
            /// linear, eqidistant segments.
            /// </summary>
            /// <param name="_pct">Percentage along spline (0 to 1f)</param>
            /// <returns>Value at specified percentage</returns>
            public abstract T SplineValueAt(float _pct);

            public float GetDefaultSegmentLength()
            {
                return FrameLength / 10f;
            }

            public float GetDefaultMinAngleMinLength()
            {
                return SplineLengthEstimated() / (float)1000;
            }

            /// Populates <see cref="Base{T}.PathPoints"/> and <see cref="Base{T}.PathSegments"/>
            /// by sampling <see cref="SplineValueAt"/>, population method depends on <see cref="ModeEQ"/>
            protected override bool BuildPath()
            {
                Func<float, float, float> glptFunc =
                    (_fromPct, _toPct) =>
                        DTypeInfo<T>.Length(SplineValueAt(_fromPct), SplineValueAt(_toPct));

                var timeStamp = System.DateTime.UtcNow.Ticks;
                if (PointMode == SplinePointMode.SegmentLength)
                {
                    if (m_SegmentLength <= 0)
                        m_SegmentLength = GetDefaultSegmentLength();
                    if (m_SegmentAccuracy <= 0 || m_SegmentAccuracy >= 1f)
                        m_SegmentAccuracy = 0.3f;

                    var tgtLenMin = m_SegmentLength - m_SegmentLength * m_SegmentAccuracy;
                    var tgtLenMax = m_SegmentLength + m_SegmentLength * m_SegmentAccuracy;
                    var guessPct = m_SegmentLength / FrameLength;

                    var fromPt = m_FrameWaypoints[0];
                    var endPt = SplineValueAt(1f);
                    var path = new List<T> { fromPt };
                    var pathN = new List<VecN> { ToVecN(fromPt) };
                    var totalLength = 0f;
                    var lengths = new List<float>();



                    var fromPct = 0f;

                    var iters = 0;

                    var done = false;
                    float len;
                    while (!done)
                    {

                        var wrapped = false;
                        var pctA = fromPct;
                        var pctB = float.MaxValue;
                        var curPct = fromPct + guessPct;

                        if (curPct >= 1f)
                        {
                            curPct = 1f;
                            var checkLen = DTypeInfo<T>.Length(fromPt, endPt);
                            if (checkLen <= tgtLenMin)
                            {
                                done = true;
                                len = checkLen;
                            }
                            else
                                len = glptFunc(pctA, curPct);
                        }
                        else
                            len = glptFunc(pctA, curPct);



                        while (!done)
                        {

                            if (len > tgtLenMin && len <= tgtLenMax)
                                break;

                            if (len < tgtLenMin && curPct > pctA)
                            {
                                pctA = curPct;
                                if (!wrapped)
                                    curPct += guessPct;
                                else
                                    curPct = pctA + (pctB - pctA) / 2f;
                            }
                            else if (len > tgtLenMax && curPct <= pctB)
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
                            len = glptFunc(fromPct, curPct);
                            var curTicks = DateTime.UtcNow.Ticks - timeStamp;
                            if (curTicks / System.TimeSpan.TicksPerMillisecond > TimeOut)
                            {
                                Log.Err("Timeout building path.  Timeout = " + TimeOut + " ms");
                                return false;
                            }
                        }

                        var pt = SplineValueAt(curPct);

                        path.Add(pt);
                        pathN.Add(ToVecN(pt));
                        lengths.Add(len);
                        totalLength += len;
                        fromPct = curPct;
                        fromPt = pt;
                    }

                    if (path.Count < 2)
                    {
                        Log.Warn("Segment Length too long.  (SegmentLength = '" + SegmentLength + "')");
                        PathPoints = new T[] { m_FrameWaypoints[0], m_FrameWaypoints[m_FrameWaypoints.Count - 1] };
                    }
                    else
                        PathPoints = path.ToArray();

                    //Update Segment pct
                    var progressLen = 0f;
                    m_PathSegments = new Segment[PathPoints.Length - 1];
                    for (int idx = 0; idx < m_PathSegments.Length; ++idx)
                    {
                        var frmPct = progressLen / totalLength;
                        progressLen += lengths[idx];
                        var toPct = progressLen / totalLength;
                        m_PathSegments[idx] = new Segment(idx, frmPct, toPct, lengths[idx]);
                    }
                    return true;
                }
                else
                {

                    /*
                    if (m_MinAngle <= 0 || m_MinAngle >= 180)
                        m_MinAngle = 5f;
                    if (m_MinAngleSamples <= 1)
                        m_MinAngleSamples = 100;

                    var pctAdder = 1f / (float) m_MinAngleSamples;


                    var allPts = new List<T>();
                    for (float pct = 0f; pct < 1f; pct += pctAdder)
                        allPts.Add(SplineValueAt(pct));
                    allPts.Add(SplineValueAt(1f));

                    var pts = new List<T> {allPts[0]};
                    var ptsN = new List<VecN> {ToVecN(allPts[0])};
                    var ptCnt = allPts.Count;
                    var lengths = new List<float>();
                    var totLen = 0f;



                    var lastPinN = ptsN[0];
                    var prevPtN = lastPinN;
                    var nextPtN = ToVecN(allPts[1]);

                    var curAngleTot = 0f;
                    var lastPinIdx = 0;

                    for (int idx = 1; idx < ptCnt - 2; ++idx)
                    {
                        var curPtN = nextPtN;
                        nextPtN = ToVecN(allPts[idx + 1]);
                        var prevVecN = curPtN - prevPtN;
                        var nextVecN = nextPtN - curPtN;

                        var angleD = ioMath.ToDegrees(VecN.Angle(prevVecN, nextVecN));
                        curAngleTot += angleD;
                        if (curAngleTot >= 2 * MinAngle && lastPinIdx != idx - 1 && idx != 1)
                        {
                            var backVecN = prevPtN - lastPinN;
                            var backMag = backVecN.Magnitude;
                            pts.Add(prevPtN.To<T>());
                            ptsN.Add(prevPtN);
                            lengths.Add(backMag);
                            totLen += lengths.Last();

                            lastPinN = prevPtN;
                        }

                        if (curAngleTot >= MinAngle)
                        {
                            pts.Add(curPtN.To<T>());
                            ptsN.Add(curPtN);
                            var lastPinVecN = (curPtN - lastPinN);
                            lengths.Add(lastPinVecN.Magnitude);
                            totLen += lengths.Last();

                            lastPinIdx = idx;
                            lastPinN = curPtN;
                            curAngleTot = 0f;
                        }

                        prevPtN = curPtN;
                    }

                    pts.Add(allPts[ptCnt - 1]);
                    ptsN.Add(ToVecN(pts[pts.Count - 1]));
                    lengths.Add((ptsN[ptsN.Count - 1] - lastPinN).Magnitude);

                    


                    //Setup Segments
                    var progressLen = 0f;

                    m_PathSegments = new Segment[pts.Count - 1];
                    for (int idx = 0; idx < m_PathSegments.Length; ++idx)
                    {
                        var frmPct = progressLen / totLen;
                        progressLen += lengths[idx];
                        var toPct = progressLen / totLen;
                        m_PathSegments[idx] = new Segment(idx, frmPct, toPct, lengths[idx]);
                    }

                    PathPoints = pts.ToArray();
                    PathPointsVN = ptsN.ToArray();
                    return true;
                     * */

                    Profile.Begin("Build Min Angle");
                    var minLen = (m_MinAngleMinLength == MA_LENGTH_AUTO) ? GetDefaultMinAngleMinLength() : m_MinAngleMinLength;
                    var ptsN = new List<VecN> { m_FrameVN[0] };
                    var pctList = new List<float> { 0f };
                    for (int idx = 1; idx < m_FrameWaypoints.Count; ++idx)
                        pctList.Add(GetFrameWaypointSplinePct(idx));
                    if (Closed)
                        pctList.Add(1f);

                    for (int idx = 1; idx < pctList.Count; ++idx)
                    {
                        ptsN.AddRange(BuildMinAngle2(pctList[idx - 1], pctList[idx], 0, m_MinAngle, minLen));
                        ptsN.Add(ToVecN(SplineValueAt(pctList[idx])));
                    }
                    Profile.End();




                    var pts = new T[ptsN.Count];
                    pts[0] = ptsN[0].To<T>();


                    var totLen = 0f;
                    var lengths = new float[ptsN.Count - 1];
                    //Get total length
                    for (int idx = 1; idx < ptsN.Count; ++idx)
                    {
                        var curLen = (ptsN[idx] - ptsN[idx - 1]).Magnitude;
                        totLen += curLen;
                        lengths[idx - 1] = curLen;
                        pts[idx] = ptsN[idx].To<T>();
                    }

                    //Build segments
                    var progressLen = 0f;

                    m_PathSegments = new Segment[pts.Length - 1];
                    for (int idx = 0; idx < m_PathSegments.Length; ++idx)
                    {
                        var frmPct = progressLen / totLen;
                        progressLen += lengths[idx];
                        var toPct = progressLen / totLen;
                        m_PathSegments[idx] = new Segment(idx, frmPct, toPct, lengths[idx]);
                    }

                    PathPoints = pts;
                    PathPointsVN = ptsN.ToArray();
                    return true;
                }


            }


            private List<VecN> BuildMinAngle2(float _fromPct, float _toPct, int _curDepth, float _minAngle, float _minLen)
            {
                var newPts = new List<VecN>();


                var prevPtN = ToVecN(SplineValueAt(_fromPct));
                var nextPtN = ToVecN(SplineValueAt(_toPct));
                var lenSq = (prevPtN - nextPtN).MagnitudeSquared;
                if (lenSq < (_minLen * _minLen)) return newPts;

                var halfPct = _fromPct + (_toPct - _fromPct) / 2f;
                var curPtN = ToVecN(SplineValueAt(halfPct));

                var prevVecN = curPtN - prevPtN;
                var nextVecN = nextPtN - curPtN;

                var curAngle = ioMath.ToDegrees(VecN.Angle(prevVecN, nextVecN));

                var nextDepth = (curAngle > _minAngle) ? 0 : _curDepth + 1;

                newPts.AddRange(BuildMinAngle2(_fromPct, halfPct, nextDepth, _minAngle, _minLen));
                if (curAngle > _minAngle) newPts.Add(curPtN);
                newPts.AddRange(BuildMinAngle2(halfPct, _toPct, nextDepth, _minAngle, _minLen));

                return newPts;
            }



            /// <summary>
            /// Get the percentage at which the specified frame waypoint lies along the spline.
            /// </summary>
            /// <param name="_index">Index of frame waypoint</param>
            /// <returns>Percentage (0 to 1)</returns>
            public float GetFrameWaypointSplinePct(int _index)
            {
                if (_index < 0 || (Closed ? _index > m_FrameWaypoints.Count : _index >= m_FrameWaypoints.Count))
                    throw new IndexOutOfRangeException("Invalid waypoint index.");

                if ((m_Closed ? _index == m_FrameWaypoints.Count : _index == m_FrameWaypoints.Count - 1))
                    return 1f;
                if (_index == 0) return 0;

                var sectionCnt = m_FrameWaypoints.Count - 1;
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

            private float SplineLengthEstimated()
            {
                var estLen = 0f;
                var sampleCount = 100;
                for (int idx = 0; idx < sampleCount; ++idx)
                    estLen += DTypeInfo<T>.Length(SplineValueAt((float)idx / sampleCount),
                        SplineValueAt((float)(idx + 1) / sampleCount));
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
            /// <seealso cref="CreateBezier{T}(bool,List{T})"/>
            /// <seealso cref="CreateBezier{T}(bool,T[])"/>
            /// <seealso cref="CreateBezier{T}(Base{T})"/>
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

            /// <summary>
            /// Get value along bezier spline at specified percent.  Calculated
            /// using bezier mathematics (not a linear progression).
            /// </summary>
            /// <param name="_pct">Percent along spline (0 to 1)</param>
            /// <returns>Value at specified percent</returns>
            public override T SplineValueAt(float _pct)
            {
                if (_pct <= 0f)
                    return m_FrameWaypoints[0];
                if (_pct >= 1f)
                    return Closed ? m_FrameWaypoints[0] : m_FrameWaypoints[m_FrameWaypoints.Count - 1];

                var bezCnt = m_FrameWaypoints.Count;
                if (!Closed) bezCnt--;

                var bezPos = _pct * bezCnt;
                var bezIdx = (int)bezPos;
                float bezPct = (bezPos - bezIdx);

                var bezIdxB = bezIdx + 1;
                if (bezIdxB == m_FrameWaypoints.Count)
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
                        vals[idx] = DTypeInfo<T>.GetDimsFunc[idx + 1](FrameWaypointValueAt(_pct));
                }

                return DTypeInfo<T>.Constructs[DTypeInfo<T>.DimCount](vals);
            }

            /// <summary>
            /// Create default control for specified waypoint along spline.
            /// </summary>
            /// <param name="_idx">Index of waypoint to create control for</param>
            /// <returns>Created control object</returns>
            public Control CreateDefaultCtl(int _idx)
            {
                var wps = m_FrameWaypoints;
                var n = wps.Count;

                var wp = ToVecN(wps[_idx]);
                VecN from = null;
                VecN to = null;
                if (_idx == 0)
                    from = Closed ? ToVecN(wps[n - 1]) : wp;
                else if (_idx == n - 1)
                    to = Closed ? ToVecN(wps[0]) : wp;

                from = from ?? ToVecN(wps[_idx - 1]);
                to = to ?? ToVecN(wps[_idx + 1]);

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
            /// Create default control for specified bezier spline.
            /// </summary>
            /// Creates colinear control with magnitude based on neighboring waypoint data.
            /// <param name="_parent">Bezier object to create control data for</param>
            /// <returns>List of control objects</returns>
            private Control[] CreateDefaultCtl()
            {
                var control = new List<Control>();
                for (int idx = 0; idx < m_FrameWaypoints.Count; ++idx)
                    control.Add(CreateDefaultCtl(idx));
                return control.ToArray();
            }

            public Control[] GetControl()
            {
                var cntl = new Control[m_Control.Length];
                for (int idx = 0; idx < m_Control.Length; ++idx)
                {
                    cntl[idx] = new Control(m_Control[idx]);
                }
                return cntl;
            }

            public void SetControl(Control[] _newControl)
            {
                if (_newControl.Length != m_Control.Length)
                {
                    Log.Err("SetControl : New control array length does not match.  Got " + _newControl.Length +
                            " and expected " + m_Control.Length);
                    return;
                }

                for (int idx = 0; idx < _newControl.Length; ++idx)
                    m_Control[idx] = _newControl[idx];

                IBuild(false);
            }

            public void SetControl(int _wayptIdx, Control _control)
            {
                if (_wayptIdx < 0 || _wayptIdx >= m_FrameWaypoints.Count)
                {
                    Log.Err("SetControl : Waypoint index out of range.  Got " + _wayptIdx);
                    return;
                }

                m_Control[_wayptIdx] = _control;

                IBuild(false);
            }

            public void SetControlOutPt(int _wayptIdx, T _outPt)
            {
                if (_wayptIdx < 0 || _wayptIdx >= m_FrameWaypoints.Count)
                {
                    Log.Err("SetControlOutPt : Waypoint index out of range.  Got " + _wayptIdx);
                    return;
                }
                var cntl = m_Control[_wayptIdx];
                var wayPt = ToVecN(m_FrameWaypoints[_wayptIdx]);
                var outPt = ToVecN(_outPt);
                var newVec = (outPt - wayPt);
                if (ToVecN(cntl.OutVec) == newVec) return;
                cntl.OutVec = newVec.To<T>();

                IBuild(false);
            }

            public void SetControlInPt(int _wayptIdx, T _inPt)
            {
                if (_wayptIdx < 0 || _wayptIdx >= m_FrameWaypoints.Count)
                {
                    Log.Err("SetControlInPt : Waypoint index out of range.  Got " + _wayptIdx);
                    return;
                }
                var cntl = m_Control[_wayptIdx];
                var wayPt = ToVecN(m_FrameWaypoints[_wayptIdx]);
                var inPt = ToVecN(_inPt);
                var newVec = (inPt - wayPt);
                if (newVec == ToVecN(cntl.InVec)) return;
                cntl.InVec = newVec.To<T>();

                IBuild(false);
            }


            /// <summary>
            /// Contains control information for the spline
            /// </summary>
            public class Control
            {

                private bool m_Colinear = true;
                public VecN VInN;
                public VecN VOutN;

                /// <summary>
                /// Bezier control object constructor
                /// </summary>
                /// <param name="_parent">Reference to parent Bezier spline object</param>
                /// <param name="_index">Waypoint index this control point is tied to</param>
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

                public Control(T _inVec, T _outVec, bool _colinear = true)
                {
                    VInN = ToVecN(_inVec);
                    VOutN = ToVecN(_outVec);
                    m_Colinear = _colinear;
                }


                /// <summary>
                /// Bezier control object constructor, copy data from provided control object
                /// </summary>
                /// <param name="_parent">Reference to parent Bezier spline object</param>
                /// <param name="_index">Waypoint index this control point is tied to</param>
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
            /// <seealso cref="CreateCubic{T}(bool, List{T})"/>
            /// <seealso cref="CreateCubic{T}(bool, T[])"/>
            /// <seealso cref="CreateCubic{T}(Base{T})"/>
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
            protected override bool BuildPath()
            {
                m_Cubs = new CubicValue[m_FrameVN[0].DimCount][];
                foreach (var dim in m_DimsToSpline)

                    m_Cubs[dim - 1] = !Closed ?
                        CalcNatCubic(m_FrameVN, dim - 1) :
                        CalcNatCubicClosed(m_FrameVN, dim - 1);

                return base.BuildPath();
            }

            /// <summary>
            /// Get value along cubic spline at specified percent.  Calculated
            /// using cubic (not a linear progression).
            /// </summary>
            /// <param name="_pct">Percent along spline (0 to 1)</param>
            /// <returns>Value at specified percent</returns>
            public override T SplineValueAt(float _pct)
            {
                if (_pct <= 0f) return m_FrameWaypoints[0];
                if (_pct >= 1f)
                    return Closed ? m_FrameWaypoints[0] : m_FrameWaypoints[m_FrameWaypoints.Count - 1];
                var cubPosition = _pct * m_Cubs[m_DimsToSpline[0] - 1].Length;
                int cubicNum = (int)cubPosition;
                float cubicPos = (cubPosition - cubicNum);

                float[] vals = new float[m_FrameVN[0].DimCount];
                for (int idx = 0; idx < m_FrameVN[0].DimCount; ++idx)
                {
                    if (Contains(m_DimsToSpline, (idx + 1)))
                        vals[idx] = m_Cubs[idx][cubicNum].Evaluate(cubicPos);
                    else
                        vals[idx] = DTypeInfo<T>.GetDimsFunc[idx + 1](FrameWaypointValueAt(_pct));
                }
                return DTypeInfo<T>.Constructs[DTypeInfo<T>.DimCount](vals);
            }


            /* TODO
            public T SplineTangentAt(float _pct)
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
                        vals[idx] = 0;
                }
                return DTypeInfo<T>.Constructs[DTypeInfo<T>.DimCount](vals);
            }
            */

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

            /*
            private static CubicValue[] CalcNatCubic(T[] _vals, Teacher.FuncGetDim<T> _funcGetVal)
            {
                int n = _vals.Length - 1;

                float[] gamma = new float[n + 1];
                float[] delta = new float[n + 1];
                float[] D = new float[n + 1];

                int i;
                gamma[0] = 1.0f / 2.0f;
                for (i = 1; i < n; i++)
                    gamma[i] = 1.0f / (4.0f - gamma[i - 1]);

                gamma[n] = 1.0f / (2.0f - gamma[n - 1]);

                float p0 = _funcGetVal(_vals[0]);
                float p1 = _funcGetVal(_vals[1]);

                delta[0] = 3.0f * (p1 - p0) * gamma[0];
                for (i = 1; i < n; i++)
                {
                    p0 = _funcGetVal(_vals[i - 1]);
                    p1 = _funcGetVal(_vals[i + 1]);
                    delta[i] = (3.0f * (p1 - p0) - delta[i - 1]) * gamma[i];
                }
                p0 = _funcGetVal(_vals[n - 1]);
                p1 = _funcGetVal(_vals[n]);

                delta[n] = (3.0f * (p1 - p0) - delta[n - 1]) * gamma[n];

                D[n] = delta[n];
                for (i = n - 1; i >= 0; i--)
                    D[i] = delta[i] - gamma[i] * D[i + 1];

                CubicValue[] cubics = new CubicValue[n];

                for (i = 0; i < n; i++)
                {
                    p0 = _funcGetVal(_vals[i]);
                    p1 = _funcGetVal(_vals[i + 1]);

                    cubics[i] = new CubicValue(
                        p0,
                        D[i],
                        3 * (p1 - p0) - 2 * D[i] - D[i + 1],
                        2 * (p0 - p1) + D[i] + D[i + 1]
                        );
                }
                return cubics;
            }

            private static CubicValue[] CalcNatCubicClosed(T[] _values, Teacher.FuncGetDim<T> _funcGetVal)
            {
                var values = ToList(_values);
                values.Add(values[0]);
                _values = values.ToArray();
                var n = _values.Length - 2;

                float[] w = new float[n + 1];
                float[] v = new float[n + 1];
                float[] y = new float[n + 1];
                float[] D = new float[n + 1];
                float z, F, G, H;
                int k;

                w[1] = v[1] = z = 1.0f / 4.0f;
                y[0] = z * 3 * (_funcGetVal(_values[1]) - _funcGetVal(_values[n]));
                H = 4;
                F = 3 * (_funcGetVal(_values[0]) - _funcGetVal(_values[n - 1]));
                G = 1;
                for (k = 1; k < n; k++)
                {
                    v[k + 1] = z = 1 / (4 - v[k]);
                    w[k + 1] = -z * w[k];
                    y[k] = z * (3 * (_funcGetVal(_values[k + 1]) - _funcGetVal(_values[k - 1])) - y[k - 1]);
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
                    C[k] = new CubicValue((float)_funcGetVal(_values[k]), D[k],
                        3 * (_funcGetVal(_values[k + 1]) - _funcGetVal(_values[k])) - 2 * D[k] - D[k + 1],
                        2 * (_funcGetVal(_values[k]) - _funcGetVal(_values[k + 1])) + D[k] + D[k + 1]);
                }
                C[n] = new CubicValue((float)_funcGetVal(_values[n]), D[n],
                    3 * (_funcGetVal(_values[0]) - _funcGetVal(_values[n])) - 2 * D[n] - D[0],
                    2 * (_funcGetVal(_values[n]) - _funcGetVal(_values[0])) + D[n] + D[0]);
                return C;
            }
            */
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
            /// <seealso cref="CreateLinear{T}(bool,List{T})"/>
            /// <seealso cref="CreateLinear{T}(bool,T[])"/>
            /// <seealso cref="CreateLinear{T}(Base{T})"/>
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
            protected override bool BuildPath()
            {
                var points = new List<T>(m_FrameWaypoints);
                if (Closed)
                    points.Add(m_FrameWaypoints[0]);
                PathPoints = points.ToArray();
                PathPointsVN = ToVecNs(PathPoints).ToArray();
                m_PathSegments = m_FrameSegments;
                return true;
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

        /// <summary>
        /// VecN constructor.
        /// </summary>
        /// <param name="_vals">Dimension data</param>
        public VecN(params float[] _vals)
        {
            Vals = _vals;
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

        /// Convert to type T (T must have constructor defined for this VecN's dim count) 
        /// <seealso cref="Teacher.TeachCoord{T}(int, Teacher.FuncConstruct{T}, Teacher.FuncGetDim{T}[])"/>
        /// <seealso cref="Teacher.TeachCoord{T}(Dictionary{int,Teacher.FuncConstruct{T}}, Teacher.FuncGetDim{T}[])"/>
        public T To<T>()
        {
            return DTypeInfo<T>.Constructs[DimCount](Vals);
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

        public static float PointToLineDistance(VecN _linePtA, VecN _linePtB, VecN _point, out VecN _pointOnLine)
        {
            return (float)Math.Sqrt(PointToLineDistanceSquared(_linePtA, _linePtB, _point, out _pointOnLine));
        }

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