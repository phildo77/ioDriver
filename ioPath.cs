
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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
    public static Path.Cubic<T> SplineCubic<T>(this Path.Base<T> _pathObj)
    {
        return Path.CreateCubic(_pathObj);
    }

    /// <summary>
    /// Create new Bezier spline from this path/spline's 
    /// <see cref="Path.Base{T}.m_FrameWaypoints">frame</see>.
    /// </summary>
    /// <typeparam name="T">Waypoint type</typeparam>
    /// <param name="_pathObj">this path object</param>
    /// <returns>New Bezier spline</returns>
    public static Path.Bezier<T> SplineBezier<T>(this Path.Base<T> _pathObj)
    {
        return Path.CreateBezier(_pathObj);
    }

    /// <summary>
    /// Create new linear path from this spline's <see cref="Path.Base{T}.m_FrameWaypoints">frame</see>.
    /// </summary>
    /// <typeparam name="T">Waypoint type</typeparam>
    /// <param name="_splineObj">this spline</param>
    /// <returns>New linear path</returns>
    public static Path.Linear<T> CreateLinearFromFrame<T>(this Path.Spline<T> _splineObj)
    {
        return Path.CreateLinear(_splineObj);
    }

    /// <summary>
    /// Create new linear path from this spline's <see cref="Path.Base{T}.PathPoints">path points</see>.
    /// </summary>
    /// <typeparam name="T">Waypoint type</typeparam>
    /// <param name="_splineObj">this spline</param>
    /// <returns>New linear path</returns>
    public static Path.Linear<T> CreateLinearFromSpline<T>(this Path.Spline<T> _splineObj)
    {
        return Path.CreateLinear(_splineObj.Closed, ToList(_splineObj.PathPoints));
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
            return m_Path.Length;
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


        // TODO fix all of these if static factory bug gets fixed (mono / Unity)
        /// <summary>
        /// Create linear path from existing path object's frame.
        /// </summary>
        /// <typeparam name="T">Waypoint type</typeparam>
        /// <param name="_pathObj">Path to copy wayponts from</param>
        /// <returns>New linear path object</returns>
        public static Linear<T> CreateLinear<T>(Path.Base<T> _pathObj)
        {
            var path = new Linear<T>(ToList(_pathObj.GetFrameWaypoints()), _pathObj.Closed);
            path.Build();
            return path;
        }


        /// <summary>
        /// Creates new linear path from provided list of waypoints.
        /// </summary>
        /// <typeparam name="T">Waypoint type</typeparam>
        /// <param name="_waypoints">List of waypoints</param>
        /// <param name="_closed">Closed path?</param>
        /// <returns>New linear path object</returns>
        public static Linear<T> CreateLinear<T>(bool _closed, List<T> _waypoints)
        {
            var path = new Linear<T>(_waypoints, _closed);
            path.Build();
            return path;
        }

        /// <summary>
        /// Create new linear path from provided waypoint params.
        /// </summary>
        /// <typeparam name="T">Waypoint type</typeparam>
        /// <param name="_closed">Closed spline?</param>
        /// <param name="_waypoints">List of waypoints</param>
        /// <returns>New linear path object</returns>
        public static Linear<T> CreateLinear<T>(bool _closed, params T[] _waypoints)
        {
            var path = new Linear<T>(ToList(_waypoints), _closed);
            path.Build();
            return path;
        }

        /// <summary>
        /// Create new Bezier spline from provided path's frame.
        /// </summary>
        /// <typeparam name="T">Waypoint Type</typeparam>
        /// <param name="_pathObj">Path object containing waypoints to use</param>
        /// <returns>New Bezier spline object</returns>
        public static Bezier<T> CreateBezier<T>(Base<T> _pathObj)
        {
            var path = new Bezier<T>(ToList(_pathObj.GetFrameWaypoints()), _pathObj.Closed);
            path.Build();
            return path;
        }

        /// <summary>
        /// Create new Bezier spline from provided list of frame waypoints.
        /// </summary>
        /// <typeparam name="T">Waypoint Type</typeparam>
        /// <param name="_frameWaypoints">List of waypoints for frame</param>
        /// <param name="_closed">Closed spline?</param>
        /// <returns>New Bezier spline object</returns>
        public static Bezier<T> CreateBezier<T>(bool _closed, List<T> _frameWaypoints)
        {
            var path = new Bezier<T>(_frameWaypoints, _closed);
            path.Build();
            return path;
        }

        /// <summary>
        /// Create new Bezier spline from provided frame waypoint params.
        /// </summary>
        /// <typeparam name="T">Waypoint Type</typeparam>
        /// <param name="_closed">Closed spline?</param>
        /// <param name="_frameWaypoints">List of waypoints for frame</param>
        /// <returns>New Bezier spline object</returns>
        public static Bezier<T> CreateBezier<T>(bool _closed, params T[] _frameWaypoints)
        {
            var waypoints = ToList(_frameWaypoints);
            var path = new Bezier<T>(waypoints, _closed);
            path.Build();
            return path;
        }

        /// <summary>
        /// Create new cubic spline from provided path object's frame.
        /// </summary>
        /// <typeparam name="T">Waypoint type</typeparam>
        /// <param name="_pathObj">Path object</param>
        /// <returns>New cubic spline object</returns>
        public static Cubic<T> CreateCubic<T>(Base<T> _pathObj)
        {
            var path = new Cubic<T>(ToList(_pathObj.GetFrameWaypoints()), _pathObj.Closed);
            path.Build();
            return path;
        }

        /// <summary>
        /// Create new cubic spline from provided list of frame waypoints.
        /// </summary>
        /// <typeparam name="T">Waypoint Type</typeparam>
        /// <param name="_frameWaypoints">List of waypoints for frame</param>
        /// <param name="_closed">Closed spline?</param>
        /// <returns>New cubic spline object</returns>
        public static Cubic<T> CreateCubic<T>(bool _closed, List<T> _frameWaypoints)
        {
            var path = new Cubic<T>(_frameWaypoints, _closed);
            path.Build();
            return path;
        }

        /// <summary>
        /// Create new cubic spline from provided frame waypoint params.
        /// </summary>
        /// <typeparam name="T">Waypoint Type</typeparam>
        /// <param name="_closed">Closed spline?</param>
        /// <param name="_frameWaypoints">List of waypoints for frame</param>
        /// <returns>New cubic spline object</returns>
        public static Cubic<T> CreateCubic<T>(bool _closed, params T[] _frameWaypoints)
        {
            var path = new Cubic<T>(ToList(_frameWaypoints), _closed);
            path.Build();
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

            /// List of segments in path.
            protected List<Segment> m_PathSegments;

            /// List of raw waypoints used to define the path's frame (the points the path is to pass through), 
            /// will not have duplicate beginning and end points if closed
            protected List<T> m_FrameWaypoints;

            /// List of raw segments used to define the path's frame (the points the path is to pass through).
            /// Will include final segment if closed.
            protected List<Segment> m_FrameSegments;

            /// Is this a closed path?
            protected bool m_Closed;

            /// <summary>
            /// Base path object constructor.  Note that <see cref="Build"/> is not called during construction.
            /// Child classes will need to either call this in their constructor or through a static factory method.
            /// </summary>
            /// <param name="_frameWaypoints">List of waypoints used to define path's frame</param>
            /// <param name="_closed">Is this a closed path?</param>
            protected Base(List<T> _frameWaypoints, bool _closed)
            {
                m_FrameWaypoints = _frameWaypoints;
                m_Closed = _closed;
                if (!m_Closed) return;
                //If closed enforce that first and last are not equal.
                if (m_FrameWaypoints[0].Equals(m_FrameWaypoints[m_FrameWaypoints.Count - 1]))
                    m_FrameWaypoints.RemoveAt(m_FrameWaypoints.Count - 1);
            }

            /// Is this a closed path?  <see cref="Build"/> is called on change.
            public virtual bool Closed
            {
                get { return m_Closed; }
                set
                {
                    if (m_Closed == value) return;
                    m_Closed = value;
                    Build();
                }
            }

            /// Get the length of the path.
            public float Length
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
            protected float m_FrameLength
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

            /// Get frame segment that lies at specified frame pct. <seealso cref="GetPathSegmentAt"/>
            public Segment GetFrameSegmentAt(float _pct)
            {
                if (_pct < 0f || _pct > 1f)
                {
                    Log.Err("_pct must be between 0 and 1.  Returning null.");
                    return null;
                }
                if (_pct == 0) return FrameSegments[0];
                if (_pct == 1) return FrameSegments[FrameSegments.Length - 1];

                foreach (Segment seg in FrameSegments)
                    if (seg.PctEnd > _pct)
                        return seg;
                return FrameSegments[FrameSegments.Length - 1];
            }

            /// Get path segment that lies at specified path pct. <seealso cref="GetFrameSegmentAt"/>
            public Segment GetPathSegmentAt(float _pct)
            {
                if (_pct < 0f || _pct > 1f)
                {
                    Log.Err("_pct out of range.  Must be between 0 and 1f.  Returning null.");
                    return null;
                }
                if (_pct == 0) return PathSegments[0];
                if (_pct == 1) return PathSegments[PathSegments.Length - 1];

                foreach (Segment seg in PathSegments)
                    if (seg.PctEnd > _pct)
                        return seg;
                return PathSegments[PathSegments.Length - 1];
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

                for (int idx = 0; idx < segs.Length; ++idx)
                {
                    var curSeg = segs[idx];
                    var segPtA = ToVecN(PathPoints[curSeg.FromIdx]);
                    var segPtB = ToVecN(PathPoints[curSeg.FromIdx + 1]);
                    VecN curPtOnLine = null;
                    var curDist = VecN.PointToLineDistance(segPtA, segPtB, ToVecN(_point), out curPtOnLine);
                    if (curDist < closestDist)
                    {
                        closestDist = curDist;
                        pointOnLine = curPtOnLine;
                        closestSeg = curSeg;
                    }
                }

                var nearestOnPath = pointOnLine.To<T>();
                _nearestSegment = closestSeg;
                var pctInSeg = VecN.ILerp(ToVecN(PathPoints[closestSeg.FromIdx]), ToVecN(PathPoints[closestSeg.FromIdx + 1]),
                    ToVecN(nearestOnPath));

                _pctOnPath = Teacher.Lerpf(_nearestSegment.PctStart, _nearestSegment.PctEnd, pctInSeg);

                return nearestOnPath;
            }


            /// Add new frame waypoint
            public virtual void AddFrameWaypoint(T _waypoint)
            {
                m_FrameWaypoints.Add(_waypoint);
                Build();
            }

            /// Insert new frame waypoint at specified index.
            public virtual void InsertFrameWaypoint(int _index, T _waypoint)
            {
                m_FrameWaypoints.Insert(_index, _waypoint);
                Build();
            }

            /// Replace the data at index with specified data.
            public virtual void UpdateFrameWaypoint(int _index, T _waypoint)
            {
                m_FrameWaypoints[_index] = _waypoint;
                Build();
            }

            /// Remove frame waypoint at specified index.
            public virtual void RemoveFrameWaypoint(int _index)
            {
                m_FrameWaypoints.RemoveAt(_index);
                Build();
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

                Build();
            }


            private static T LerpPath(float _pct, List<T> _points, List<Segment> _segments, float _pathLen, bool _closed)
            {
                if (_pct == 0f) return _points[0];
                if (_pct == 1f) return _closed ? _points[0] : _points[_points.Count - 1];

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
                if (_closed && ((tgtSeg.FromIdx + 1) == _points.Count))
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
                return LerpPath(_pct, ToList(PathPoints), m_PathSegments, Length, false);
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
                return LerpPath(_pct, m_FrameWaypoints, m_FrameSegments, m_FrameLength, m_Closed);
            }


            /// Updates frame segment data.  Makes call to abstract <see cref="UpdatePath"/>
            public void Build()
            {

                //TODO make protected when static factory is fixed
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
                m_FrameSegments = new List<Segment>();
                for (int idx = 0; idx < lengths.Length; ++idx)
                {
                    var startLen = cumLen;
                    cumLen += lengths[idx];
                    m_FrameSegments.Add(new Segment(idx, startLen / totLen, cumLen / totLen, lengths[idx]));
                }

                UpdatePath();
            }

            /// Override to update <see cref="PathPoints"/> and <see cref="PathSegments"/> here.
            protected abstract void UpdatePath();

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

        /// <summary>
        /// Base spline object.
        /// </summary>
        /// <typeparam name="T">Waypoint Type</typeparam>
        public abstract class Spline<T> : Base<T>, ISpline
        {
            /// Backing field for <see cref="DimsToSpline"/>
            protected int[] m_DimsToSpline;

            /// Backing field for <see cref="SegmentLength"/><seealso cref="Defaults.SegmentLength"/>
            protected float m_SegmentLength = Defaults.SegmentLength;

            private float m_SegmentAccuracy = Defaults.SegmentAccuracy;


            /// <summary>
            /// Constructor.  Note <see cref="Base{T}.Build"/> is not called during construction.
            /// <seealso cref="Base{T}(System.Collections.Generic.List{T},bool)"/>
            /// </summary>
            protected Spline(List<T> _frameWaypoints, bool _closed)
                : base(_frameWaypoints, _closed)
            {
                m_DimsToSpline = CheckDims(null);
            }
            
            /// Get/Set spline segment lengths.  
            /// Calls <see cref="UpdatePath"/> on change.
            /// Setting this to a new value will rebuild the spline.
            public float SegmentLength
            {
                get { return m_SegmentLength; }
                set
                {
                    if (m_SegmentLength == value) return;
                    m_SegmentLength = value;
                    UpdatePath();
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
                    if (value == m_SegmentAccuracy) return;
                    if (m_SegmentAccuracy <= 0 || m_SegmentAccuracy >= 0.9999f)
                    {
                        Log.Err("Segment accuracy cannot be less than zero and must be less than 1.  Setting default of '" + Defaults.SegmentAccuracy + "'");
                        m_SegmentAccuracy = Defaults.SegmentAccuracy;
                        return;
                    }
                    if (value < m_SegmentAccuracy)
                        UpdatePath();
                    m_SegmentAccuracy = value;
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
                    UpdatePath();
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

            
            /// Populates <see cref="Base{T}.PathPoints"/> and <see cref="Base{T}.PathSegments"/>
            /// by sampling <see cref="SplineValueAt"/>, population method depends on <see cref="ModeEQ"/>
            protected override void UpdatePath()
            {

                if (m_SegmentLength <= 0)
                    m_SegmentLength = 0.1f;

                float allowedError = m_SegmentLength * m_SegmentAccuracy;

                float estLen = m_FrameLength;

                float toCheckPct = m_SegmentLength / estLen;
                float fromCheckPct = 0;

                bool done = false;
                var min = m_SegmentLength - allowedError;
                var max = m_SegmentLength + allowedError;
                var lenA = -1f;
                var lenB = -1f;
                var pctA = -1f;
                var pctB = -1f;
                var path = new List<T> { m_FrameWaypoints[0] };
                var totalLength = 0f;
                var lengths = new List<float>();
                while (!done)
                {
                    var pctSpan = toCheckPct - fromCheckPct;
                    var len = DTypeInfo<T>.Length(SplineValueAt(fromCheckPct), SplineValueAt(toCheckPct));

                    if (toCheckPct >= 1f && len <= max)
                    {

                        toCheckPct = 1f;
                        done = true;

                    }
                    else if (len > max)
                    {
                        lenB = len;
                        pctB = toCheckPct;
                        if (lenA != -1)
                        {
                            toCheckPct = pctA + (pctB - pctA) / 2;
                            continue;
                        }
                        toCheckPct = fromCheckPct + pctSpan * m_SegmentLength / len;
                        continue;
                    }
                    else if (len < min)
                    {
                        lenA = len;
                        pctA = toCheckPct;
                        if (lenB != -1)
                        {
                            toCheckPct = pctA + (pctB - pctA) / 2;
                            continue;
                        }
                        toCheckPct = fromCheckPct + pctSpan * m_SegmentLength / len;
                        continue;

                    }
                    lenA = lenB = pctA = pctB = -1f;
                    totalLength += len;
                    lengths.Add(len);
                    path.Add(SplineValueAt(toCheckPct));

                    if (done)
                        break;
                    var nextPctAdder = toCheckPct - fromCheckPct;
                    fromCheckPct = toCheckPct;
                    toCheckPct = fromCheckPct + nextPctAdder;
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
                m_PathSegments = new List<Segment>();
                for (int idx = 0; idx < PathPoints.Length - 1; ++idx)
                {
                    var frmPct = progressLen/totalLength;
                    progressLen += lengths[idx];
                    var toPct = progressLen/totalLength;
                    m_PathSegments.Add(new Segment(idx, frmPct, toPct, lengths[idx]));
                }

            }

            /// <summary>
            /// Get the percentage at which the specified frame waypoint lies along the spline.
            /// </summary>
            /// <param name="_index">Index of frame waypoint</param>
            /// <returns>Percentage (0 to 1)</returns>
            public float FindFrameWaypointSplinePct(int _index)
            {
                if (_index < 0 || (Closed ? _index > m_FrameWaypoints.Count : _index >= m_FrameWaypoints.Count))
                    throw new IndexOutOfRangeException("Invalid waypoint index.");

                if ((m_Closed ? _index == m_FrameWaypoints.Count : _index == m_FrameWaypoints.Count - 1))
                    return 1f;
                if (_index == 0) return 0;

                T waypoint = m_FrameWaypoints[_index];

                var minDist = float.PositiveInfinity;
                var closestPointIdx = -1;
                for (int idx = 0; idx < PathPoints.Length; ++idx)
                {
                    var dist = DTypeInfo<T>.Length(waypoint, PathPoints[idx]);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestPointIdx = idx;
                    }
                }

                if (closestPointIdx == PathPoints.Length - 1)
                    return m_PathSegments[m_PathSegments.Count - 1].PctEnd;

                var nearestSeg = m_PathSegments[closestPointIdx];
                var segStartWp = PathPoints[closestPointIdx];
                var segEndWp = PathPoints[closestPointIdx + 1];
                var distFromStart = DTypeInfo<T>.Length(segStartWp, waypoint);
                var distFromEnd = DTypeInfo<T>.Length(segEndWp, waypoint);
                var pctOfDist = distFromStart / (distFromStart / distFromEnd);
                var pctInSeg = nearestSeg.Length * pctOfDist;
                var lengthPct = nearestSeg.Length / Length;
                var pctAdder = pctInSeg * lengthPct;
                return nearestSeg.PctStart + pctAdder;
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
            public Bezier(List<T> _frameWaypoints, bool _closed)
                : base(_frameWaypoints, _closed)
            {
                m_Control = Control.CreateDefault(this);
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

            /// Control data for each frame waypoint
            public Control[] control
            {
                get { return m_Control; }
            }

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

            private static float bezier(float _pct, Control _a, Control _b, Teacher.FuncGetDim<T> _funcGetVal)
            {
                return bezier(_pct,
                    _funcGetVal(_a.Waypoint),
                    _funcGetVal(_a.OutPt),
                    _funcGetVal(_b.InPt),
                    _funcGetVal(_b.Waypoint));
            }

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
                    if (Closed)
                        return m_FrameWaypoints[0];
                    else
                        return m_FrameWaypoints[m_FrameWaypoints.Count - 1];

                int idxB = 0;

                while (_pct > GetFrameWaypointPct(idxB))
                    idxB++;

                int idxA = idxB - 1;
                float pctSpan = GetFrameWaypointPct(idxB) - GetFrameWaypointPct(idxA);
                float segPct = (_pct - GetFrameWaypointPct(idxA)) / pctSpan;
                if (idxB == m_FrameWaypoints.Count) idxB = 0;

                var cntlA = m_Control[idxA];
                var cntlB = m_Control[idxB];

                float[] vals = new float[DTypeInfo<T>.DimCount];
                for (int idx = 0; idx < DTypeInfo<T>.DimCount; ++idx)
                {
                    if (Contains(m_DimsToSpline, (idx + 1)))
                        vals[idx] = bezier(segPct, cntlA, cntlB, DTypeInfo<T>.GetDimsFunc[idx + 1]);
                    else
                        vals[idx] = DTypeInfo<T>.GetDimsFunc[idx + 1](FrameWaypointValueAt(_pct));
                }

                return DTypeInfo<T>.Constructs[DTypeInfo<T>.DimCount](vals);
            }

            /// <summary>
            /// Contains control information for the spline
            /// </summary>
            public class Control
            {
                /// Get the waypoint index for this control object.
                public int Index
                {
                    get;
                    private set;
                }

                private bool m_Colinear = true;
                private VecN m_VIn;
                private VecN m_VOut;
                private Bezier<T> m_Parent;

                /// <summary>
                /// Bezier control object constructor
                /// </summary>
                /// <param name="_parent">Reference to parent Bezier spline object</param>
                /// <param name="_index">Waypoint index this control point is tied to</param>
                /// <param name="_tanOutDir">Tangent out direction (in direction will be opposite, colinear)</param>
                /// <param name="_inMag">Distance to place IN control point from waypoint along tangent.</param>
                /// <param name="_outMag">Distance to place OUT control point from waypoint along tangent</param>
                public Control(Bezier<T> _parent, int _index, T _tanOutDir, float _inMag, float _outMag)
                {
                    m_Parent = _parent;
                    Index = _index;
                    var outDir = ToVecN(_tanOutDir).Normalized;
                    m_VIn = -outDir * _inMag;
                    m_VOut = outDir * _outMag;
                    m_Colinear = true;
                }

                /// <summary>
                /// Bezier control object constructor
                /// </summary>
                /// <param name="_parent">Reference to parent Bezier spline object</param>
                /// <param name="_index">Waypoint index this control point is tied to</param>
                /// <param name="_InPt">World space point defining end of "in" control vector, pointing away from waypoint</param>
                /// <param name="_OutPt">World space point defining end of "out" control vector, pointing away from waypoint</param>
                /// <param name="_colinear">Should the control vectors be colinear?</param>
                public Control(Bezier<T> _parent, int _index, T _InPt, T _OutPt, bool _colinear)
                {
                    m_Parent = _parent;
                    Index = _index;
                    m_VIn = ToVecN(_InPt) - m_Waypoint;
                    m_VOut = ToVecN(_OutPt) - m_Waypoint;
                    m_Colinear = _colinear;
                }

                /// <summary>
                /// Bezier control object constructor, copy data from provided control object
                /// </summary>
                /// <param name="_parent">Reference to parent Bezier spline object</param>
                /// <param name="_index">Waypoint index this control point is tied to</param>
                /// <param name="_control">Control data to copy</param>
                public Control(Bezier<T> _parent, int _index, Control _control)
                {
                    m_Parent = _parent;
                    Index = _index;
                    m_VIn = _control.m_VIn;
                    m_VOut = _control.m_VOut;
                    m_Colinear = _control.Colinear;
                }

                private Control()
                {
                }

                /// <summary>
                /// Create default control for specified bezier spline.
                /// </summary>
                /// Creates colinear control with magnitude based on neighboring waypoint data.
                /// <param name="_parent">Bezier object to create control data for</param>
                /// <returns>List of control objects</returns>
                public static Control[] CreateDefault(Bezier<T> _parent)
                {
                    var control = new List<Control>();
                    for (int idx = 0; idx < _parent.m_FrameWaypoints.Count; ++idx)
                        control.Add(CreateDefault(_parent, idx));
                    return control.ToArray();
                }

                /// <summary>
                /// Create default control for specified waypoint along spline.
                /// </summary>
                /// <param name="_parent">Spline object</param>
                /// <param name="_idx">Index of waypoint to create control for</param>
                /// <returns>Created control object</returns>
                public static Control CreateDefault(Bezier<T> _parent, int _idx)
                {
                    var c = new Control();
                    c.m_Parent = _parent;
                    c.Index = _idx;
                    var wps = _parent.m_FrameWaypoints;
                    var n = wps.Count;

                    var wp = ToVecN(wps[_idx]);
                    VecN from = null;
                    VecN to = null;
                    if (_idx == 0)
                        from = _parent.Closed ? ToVecN(wps[n - 1]) : wp;
                    else if (_idx == n - 1)
                        to = _parent.Closed ? ToVecN(wps[0]) : wp;

                    from = from ?? ToVecN(wps[_idx - 1]);
                    to = to ?? ToVecN(wps[_idx + 1]);

                    c.m_VOut = (to - from).Normalized;
                    c.m_VIn = -c.m_VOut;
                    if (wp != to)
                        c.m_VOut *= (to - from).Magnitude * Defaults.BezierMagPct;

                    if (wp != from)
                        c.m_VIn *= (to - from).Magnitude * Defaults.BezierMagPct;
                    c.Colinear = true;
                    return c;
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
                            OutVec = (-m_VIn.Normalized * m_VOut.Magnitude).To<T>();
                    }
                }

                /// Distance between waypoint and IN control point along tangent axis
                public float InMag
                {
                    get { return m_VIn.Magnitude; }
                    set
                    {
                        if (m_VIn.Magnitude == value) return;
                        var mag = Math.Abs(value);
                        m_VIn = m_VIn.Normalized * mag;
                        m_Parent.UpdatePath();
                    }
                }

                /// Distance between waypoint and OUT control point along tangent axis
                public float OutMag
                {
                    get { return m_VOut.Magnitude; }
                    set
                    {
                        if (m_VOut.Magnitude == value) return;
                        var mag = Math.Abs(value);
                        m_VOut = m_VOut.Normalized * mag;
                        m_Parent.UpdatePath();
                    }
                }

                /// Waypoint on original path.  Spline will pass through this point.
                public T Waypoint
                {
                    get { return m_Waypoint.To<T>(); }
                }

                /// Get tangent direction pointing towards the OUT control point (from waypoint)
                public T OutDir
                {
                    get { return m_VOut.Normalized.To<T>(); }
                }

                /// Get tangent direction pointing towards the IN control point (from waypoint)
                public T InDir
                {
                    get { return Colinear ? (-m_VOut).Normalized.To<T>() : m_VIn.Normalized.To<T>(); }
                }

                /// Out control point
                public T OutPt
                {
                    get { return (m_Waypoint + m_VOut).To<T>(); }
                    set
                    {
                        var outPt = ToVecN(value);
                        if (ToVecN(OutPt) == outPt) return;
                        m_VOut = outPt - m_Waypoint;
                        if (Colinear)
                            m_VIn = -(m_VOut.Normalized) * InMag;
                        m_Parent.UpdatePath();
                    }
                }

                /// In control point
                public T InPt
                {
                    get { return (m_Waypoint + m_VIn).To<T>(); }
                    set
                    {
                        var inPt = ToVecN(value);
                        if (ToVecN(InPt) == inPt) return;
                        m_VIn = inPt - m_Waypoint;
                        if (Colinear)
                            m_VOut = -(m_VIn.Normalized) * OutMag;
                        m_Parent.UpdatePath();
                    }
                }

                /// Get/Set "In" control vector (waypoint to in control point)
                public T InVec
                {
                    get { return m_VIn.To<T>(); }
                    set
                    {
                        var inVec = ToVecN(value);
                        if (m_VIn == inVec) return;
                        m_VIn = inVec;
                        if (Colinear)
                            m_VOut = -(m_VIn.Normalized) * OutMag;
                        m_Parent.UpdatePath();
                    }
                }

                /// Get/Set "Out" control vector (waypoint to out control point)
                public T OutVec
                {
                    get { return m_VOut.To<T>(); }
                    set
                    {
                        var outVec = ToVecN(value);
                        if (m_VOut == outVec) return;
                        m_VOut = outVec;
                        if (Colinear)
                            m_VIn = -(m_VOut.Normalized) * InMag;
                        m_Parent.UpdatePath();
                    }
                }

                private VecN m_Waypoint
                {
                    get { return ToVecN(m_Parent.m_FrameWaypoints[Index]); }
                }

                /// Set the waypoint index for this control object.
                public void SetIndex(int _index)
                {
                    Index = _index;
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
            public Cubic(List<T> _frameWaypoints, bool _closed)
                : base(_frameWaypoints, _closed)
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

            /// Updates cubic values then calls base spline <see cref="Spline{T}.UpdatePath"/>
            protected override void UpdatePath()
            {
                m_Cubs = new CubicValue[DTypeInfo<T>.DimCount][];
                foreach (var dim in m_DimsToSpline)
                    m_Cubs[dim - 1] = !Closed
                        ? CalcNatCubic(m_FrameWaypoints.ToArray(), DTypeInfo<T>.GetDimsFunc[dim])
                        : CalcNatCubicClosed(m_FrameWaypoints.ToArray(), DTypeInfo<T>.GetDimsFunc[dim]);

                base.UpdatePath();
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

                float[] vals = new float[DTypeInfo<T>.DimCount];
                for (int idx = 0; idx < DTypeInfo<T>.DimCount; ++idx)
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
                    return (3*d*_u + 2*c)*_u + b;
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
            public Linear(List<T> _waypoints, bool _closed)
                : base(_waypoints, _closed)
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
            protected override void UpdatePath()
            {
                var points = new List<T>(m_FrameWaypoints);
                if (Closed)
                    points.Add(m_FrameWaypoints[0]);
                PathPoints = points.ToArray();
                m_PathSegments = m_FrameSegments;
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
                float sqrDist = 0;
                for (int idx = 0; idx < DimCount; ++idx)
                    sqrDist += Vals[idx] * Vals[idx];

                return (float)Math.Sqrt(sqrDist);
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
                result = (from - val)/(from - to);
            }

            return result;
        }

        public static float PointToLineDistance(VecN _linePtA, VecN _linePtB, VecN _point, out VecN _pointOnLine)
        {
            var line = _linePtB - _linePtA;
            var lineMag = line.Magnitude;
            var lambda = VecN.Dot((_point - _linePtA), line)/lineMag;
            lambda = Math.Min(Math.Max(0f, lambda),lineMag);
            var p = line*lambda*(1/lineMag);
            _pointOnLine = _linePtA + p;
            return (_linePtA + p - _point).Magnitude;
        }

        public static VecN NearestPointOnLine(VecN _linePtA, VecN _linePtB, VecN _point)
        {
            var lineDir = (_linePtB - _linePtA).Normalized;
            var v = _point - _linePtA;
            var d = VecN.Dot(v, lineDir);
            return _linePtA + lineDir*d;
        }

        #endregion Methods
    }

    #endregion Nested Types
}