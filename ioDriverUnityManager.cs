#if ioUNITY
using System;
using System.Collections.Generic;

using UnityEngine;

namespace ioDriverUnity
{


    /// Unity Specific Extensions
    public static class ioDriverUnityExt
    {
        #region Methods

        /// <summary>
        /// Move gameobject to its CURRENT position from point in local space over time.
        /// </summary>
        /// <param name="_thisGo">Gameobject to move</param>
        /// <param name="_from">Point to start moving from in local space</param>
        /// <param name="_duration">Duration in seconds over which to move this gameobject</param>
        /// <param name="_name">Optional name for driver</param>
        /// <returns>Configured driver</returns>
        public static ioDriver.DTweenSimple<Vector3> ioMoveFromLocal(this GameObject _thisGo, Vector3 _from, float _duration, string _name = null)
        {
            return ioDriver.Tween(() => _thisGo.transform.localPosition, _from, _thisGo.transform.localPosition, _duration, _name);
        }

        /// <summary>
        /// Move gameobject to its CURRENT position from point in world space over time.
        /// </summary>
        /// <param name="_thisGo">Gameobject to move</param>
        /// <param name="_from">Point to start moving from in world space</param>
        /// <param name="_duration">Duration in seconds over which to move this gameobject</param>
        /// <param name="_name">Optional name for driver</param>
        /// <returns>Configured driver</returns>
        public static ioDriver.DTweenSimple<Vector3> ioMoveFromWorld(this GameObject _thisGo, Vector3 _from, float _duration, string _name = null)
        {
            return ioDriver.Tween(() => _thisGo.transform.position, _from, _thisGo.transform.position, _duration, _name);
        }

        /// <summary>
        /// Move gameobject between two points in local space over time.
        /// </summary>
        /// <param name="_thisGo">Gameobject to move</param>
        /// <param name="_from">Point to start moving from in local space</param>
        /// <param name="_to">Point to move to in local space</param>
        /// <param name="_duration">Duration in seconds over which to move this gameobject</param>
        /// <param name="_name">Optional name for driver</param>
        /// <returns>Configured driver</returns>
        public static ioDriver.DTweenSimple<Vector3> ioMoveLocal(this GameObject _thisGo, Vector3 _from, Vector3 _to, float _duration, string _name = null)
        {
            return ioDriver.Tween(() => _thisGo.transform.localPosition, _from, _to, _duration, _name);
        }

        /// <summary>
        /// Move gameobject to position in world space from its CURRENT position over time.
        /// </summary>
        /// <param name="_thisGo">Gameobject to move</param>
        /// <param name="_to">Point to move to in world space</param>
        /// <param name="_duration">Duration in seconds over which to move this gameobject</param>
        /// <param name="_name">Optional name for driver</param>
        /// <returns>Configured driver</returns>
        public static ioDriver.DTweenSimple<Vector3> ioMoveTo(this GameObject _thisGo, Vector3 _to, float _duration, string _name = null)
        {
            return ioDriver.Tween(() => _thisGo.transform.position, _thisGo.transform.position, _to, _duration, _name);
        }

        /// <summary>
        /// Move gameobject to position in local space from its CURRENT position over time.
        /// </summary>
        /// <param name="_thisGo">Gameobject to move</param>
        /// <param name="_to">Point to move to in local space</param>
        /// <param name="_duration">Duration in seconds over which to move this gameobject</param>
        /// <param name="_name">Optional name for driver</param>
        /// <returns>Configured driver</returns>
        public static ioDriver.DTweenSimple<Vector3> ioMoveToLocal(this GameObject _thisGo, Vector3 _to, float _duration, string _name = null)
        {
            return ioDriver.Tween(() => _thisGo.transform.localPosition, _thisGo.transform.localPosition, _to, _duration, _name);
        }

        /// <summary>
        /// Move gameobject between two points in world space over time.
        /// </summary>
        /// <param name="_thisGo">Gameobject to move</param>
        /// <param name="_from">Point to start moving from in world space</param>
        /// <param name="_to">Point to move to in world space</param>
        /// <param name="_duration">Duration in seconds over which to move this gameobject</param>
        /// <param name="_name">Optional name for driver</param>
        /// <returns>Configured driver</returns>
        public static ioDriver.DTweenSimple<Vector3> ioMoveWorld(this GameObject _thisGo, Vector3 _from, Vector3 _to, float _duration, string _name = null)
        {
            return ioDriver.Tween(() => _thisGo.transform.position, _from, _to, _duration, _name);
        }

        // Unity GameObject Extensions
        /// <summary>
        /// Creates a step driver that rotates the specified GameObject around its center.
        /// </summary>
        /// <param name="_go">Gameobject to rotate</param>
        /// <param name="_rotFromAngle">Euler angles to start rotation from</param>
        /// <param name="_ratePerSec">Rate in degrees per second per axis</param>
        /// <param name="_name">Optional name to assign to the returned step driver</param>
        /// <returns>Configured step driver that rotates specified gameobject</returns>
        public static ioDriver.DStep<Vector3> ioRotate(this GameObject _go, Vector3 _ratePerSec, Vector3 _rotFromAngle, string _name = null)
        {
            var action = ActionRotate(_go);
            return ioDriver.Step(action, _rotFromAngle, _ratePerSec, _name)
                .SetTargetObject(_go.transform.eulerAngles);
        }

        /// <summary>
        /// Creates a step driver that rotates the specified GameObject around its center.
        /// </summary>
        /// <param name="_go">Gameobject to rotate</param>
        /// <param name="_ratePerSec">Rate in degrees per second</param>
        /// <param name="_name">Optional name to assign to the returned step driver</param>
        /// <returns>Configured step driver</returns>
        public static ioDriver.DStep<Vector3> ioRotate(this GameObject _go, Vector3 _ratePerSec, string _name = null)
        {
            return _go.ioRotate(_ratePerSec, _go.transform.eulerAngles, _name);
        }

        /// <summary>
        /// Creates a rate driver that rotates the specified GameObject around a pivot.
        /// </summary>
        /// <param name="_go">GameObject to rotate</param>
        /// <param name="_pivot">World space pivot point</param>
        /// <param name="_rotRate">Rotation rate in degrees per second</param>
        /// <param name="_name">Optional name to assign to the returned step driver</param>
        /// <returns>Configured rate driver</returns>
        public static ioDriver.DRate<Vector3> ioRotateAround(this GameObject _go, Vector3 _pivot, Vector3 _rotRate, string _name = null)
        {
            var action = ActionRotateAround(_pivot, _go);
            return new ioDriver.DRate<Vector3>(action, () => _rotRate, _name)
                .SetTargetObject(_go.transform.position);
        }

        /// <summary>
        /// Tween gameobject's local scale bewteen two scale endpoints over time.
        /// </summary>
        /// <param name="_thisGo">Gameobject to scale</param>
        /// <param name="_from">Scale to begin the scale tween from.</param>
        /// <param name="_to">Scale to end the scale tween on.</param>
        /// <param name="_duration">Time in seconds over which to perform scale tween.</param>
        /// <returns>Configured driver</returns>
        public static ioDriver.DTweenSimple<Vector3> ioScale(this GameObject _thisGo, Vector3 _from, Vector3 _to, float _duration)
        {
            return ioDriver.Tween(() => _thisGo.transform.localScale, _from, _to, _duration);
        }

        /// <summary>
        /// Tween gameobject's local scale from specified scale to its CURRENT scale over time.
        /// </summary>
        /// <param name="_thisGo">Gameobject to scale</param>
        /// <param name="_from">Scale at which to begin the tween.</param>
        /// <param name="_duration">Time in seconds over which to perform scale tween.</param>
        /// <returns>Configured driver</returns>
        public static ioDriver.DTweenSimple<Vector3> ioScaleFrom(this GameObject _thisGo, Vector3 _from, float _duration)
        {
            return ioDriver.Tween(() => _thisGo.transform.localScale, _from, _thisGo.transform.localScale, _duration);
        }

        /// <summary>
        /// Tween gameobject's lossy scale from specified scale to its CURRENT scale over time.
        /// </summary>
        /// <param name="_thisGo">Gameobject to scale</param>
        /// <param name="_from">Scale at which to begin the tween.</param>
        /// <param name="_duration">Time in seconds over which to perform scale tween.</param>
        /// <returns>Configured driver</returns>
        public static ioDriver.DTweenSimple<Vector3> ioScaleFromLossy(this GameObject _thisGo, Vector3 _from, float _duration)
        {
            return ioDriver.Tween(() => _thisGo.transform.lossyScale, _from, _thisGo.transform.localScale, _duration);
        }

        /// <summary>
        /// Tween gameobject's lossy scale bewteen two scale endpoints over time.
        /// </summary>
        /// <param name="_thisGo">Gameobject to scale</param>
        /// <param name="_from">Scale to begin the scale operation from.</param>
        /// <param name="_to">Scale to end the scale operation on.</param>
        /// <param name="_duration">Time in seconds over which to perform scale tween.</param>
        /// <returns>Configured driver</returns>
        public static ioDriver.DTweenSimple<Vector3> ioScaleLossy(this GameObject _thisGo, Vector3 _from, Vector3 _to, float _duration)
        {
            return ioDriver.Tween(() => _thisGo.transform.lossyScale, _from, _to, _duration);
        }

        /// <summary>
        /// Tween gameobject's local scale from its CURRENT scale to specified scale over time.
        /// </summary>
        /// <param name="_thisGo">Gameobject to scale</param>
        /// <param name="_to">Scale to end the scale operation on.</param>
        /// <param name="_duration">Time in seconds over which to perform scale tween.</param>
        /// <returns>Configured driver</returns>
        public static ioDriver.DTweenSimple<Vector3> ioScaleTo(this GameObject _thisGo, Vector3 _to, float _duration)
        {
            return ioDriver.Tween(() => _thisGo.transform.localScale, _thisGo.transform.localScale, _to, _duration);
        }

        /// <summary>
        /// Tween gameobject's lossy scale from its CURRENT scale to specified scale over time.
        /// </summary>
        /// <param name="_thisGo">Gameobject to scale</param>
        /// <param name="_to">Scale to end the scale operation on.</param>
        /// <param name="_duration">Time in seconds over which to perform scale tween.</param>
        /// <returns>Configured driver</returns>
        public static ioDriver.DTweenSimple<Vector3> ioScaleToLossy(this GameObject _thisGo, Vector3 _to, float _duration)
        {
            return ioDriver.Tween(() => _thisGo.transform.lossyScale, _thisGo.transform.localScale, _to, _duration);
        }

        public static ioDriver.DTweenPath<Vector3> ioTweenPath(this GameObject _go, ioDriver.Path.Base<Vector3> _path, float _duration, string _name = null)
        {
            return ioDriver.Tween(() => _go.transform.position, _path, _duration, _name);
        }

        public static ioDriver.DTweenPath<Vector3> Tween(this ioDriver.Path.Base<Vector3> _path, GameObject _go,
            float _duration, string _name = null)
        {
            return ioDriver.Tween(() => _go.transform.position, _path, _duration, _name);
        }

        private static Action<Vector3> ActionRotate(GameObject _go)
        {
            return angles => _go.transform.eulerAngles = NormAngles(angles);
        }

        private static Action<Vector3> ActionRotateAround(Vector3 _pivot, GameObject _go)
        {
            Action<Vector3> rotAction = angles =>
            {
                var xfrm = _go.transform;
                var pivot = _pivot;
                xfrm.position = RotateAroundPivot(xfrm.position, pivot, angles);
            };
            return rotAction;
        }

        private static Vector3 NormAngles(Vector3 _angles)
        {
            return new Vector3(_angles.x % 360, _angles.y % 360, _angles.z % 360);
        }

        private static Vector3 RotateAroundPivot(Vector3 _point, Vector3 _pivot, Vector3 _angles)
        {
            return Quaternion.Euler(_angles) * (_point - _pivot) + _pivot;
        }

        #endregion Methods
    }


    /// <summary>
    /// Automatically generated MonoBehaviour that provides the "Pump" for the driver manager to update running drivers.
    /// </summary>
    public class ioDriverUnityManager : MonoBehaviour
    {
        #region Fields

        /// If set to true AND <see cref="Enabled"/> is true, 
        /// ioDriver's timescale will dynamically match Unity's Timescale. 
        /// <seealso cref="ioDriver.TimescaleGlobal"/> 
        public static bool UseUnityTimescale = true;

        private static bool applicationIsQuitting = false;


        private static ioDriverUnityManager m_Instance;

        private bool m_Enabled;

        #endregion Fields

        #region Properties

        /// Get or Set whether ioDriver.Pump() is called every frame.  
        public static bool Enabled
        {
            get
            {
                if (Instance == null) return false;
                return Instance.m_Enabled;
            }
            set
            {
                if (Instance == null) return;
                Instance.m_Enabled = value;
            }
        }

        internal static ioDriverUnityManager Instance
        {
            get
            {
                if (applicationIsQuitting)
                    return null;

                if (m_Instance == null)
                {
                    m_Instance = (ioDriverUnityManager)FindObjectOfType(typeof(ioDriverUnityManager));

                    if (FindObjectsOfType(typeof(ioDriverUnityManager)).Length > 1)
                    {
                        Debug.LogError("[ioDriverUnityManager] Something went really wrong " +
                            " - there should never be more than 1 ioDriverUnityManager!" +
                            " Reopenning the scene might fix it.");
                        return m_Instance;
                    }

                    if (m_Instance == null)
                    {
                        GameObject umGO = new GameObject("ioDriverUnityManager");
                        m_Instance = umGO.AddComponent<ioDriverUnityManager>();
                        umGO.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
                    }
                }

                return m_Instance;
            }
        }

        #endregion Properties

        #region Methods

        internal static void Init()
        {
            ioDriver.TimescaleGlobal = UnityEngine.Time.timeScale;
            TeachUnity();
            if (Application.isPlaying)
                Enabled = true;
        }

        void OnApplicationQuit()
        {
            ioDriver.Reset();
            DestroyImmediate(this.gameObject);
        }

        private static float InvLerpColor(Color _from, Color _to, Color _val)
        {
            float from = _from.r;
            float to = _to.r;
            float val = _val.r;
            if (_from.r == _to.r)
                if (_from.g == _to.g)
                {
                    if (_from.b == _to.b)
                    {
                        from = _from.a;
                        to = _to.a;
                        val = _val.a;
                    }
                    else
                    {
                        from = _from.b;
                        to = _to.b;
                        val = _val.b;
                    }
                }
                else
                {
                    from = _from.g;
                    to = _to.g;
                    val = _val.g;
                }
            if (from == to) return 0;
            return (from - val) / (from - to);
        }

        private static float InvLerpVector2(Vector2 _from, Vector2 _to, Vector2 _val)
        {
            float from = _from.x;
            float to = _to.x;
            float val = _val.x;
            if (_from.x == _to.x)
            {
                from = _from.y;
                to = _to.y;
                val = _val.y;
            }
            if (from == to) return 0;
            return (from - val) / (from - to);
        }

        private static float InvLerpVector3(Vector3 _from, Vector3 _to, Vector3 _val)
        {
            float from = _from.x;
            float to = _to.x;
            float val = _val.x;
            if (_from.x == _to.x)
                if (_from.y == _to.y)
                {
                    from = _from.z;
                    to = _to.z;
                    val = _val.z;
                }
                else
                {
                    from = _from.y;
                    to = _to.y;
                    val = _val.y;
                }
            if (from == to) return 0;
            return (from - val) / (from - to);
        }

        private static float InvLerpVector4(Vector4 _from, Vector4 _to, Vector4 _val)
        {
            float from = _from.x;
            float to = _to.x;
            float val = _val.x;
            if (_from.x == _to.x)
                if (_from.y == _to.y)
                {
                    if (_from.z == _to.z)
                    {
                        from = _from.w;
                        to = _to.w;
                        val = _val.w;
                    }
                    else
                    {
                        from = _from.z;
                        to = _to.z;
                        val = _val.z;
                    }
                }
                else
                {
                    from = _from.y;
                    to = _to.y;
                    val = _val.y;
                }

            if (from == to) return 0;
            return (from - val) / (from - to);
        }

        private static Color LerpColor(Color _from, Color _to, float _pct)
        {
            var r = ioDriver.Teacher.Lerpf(_from.r, _to.r, _pct);
            var b = ioDriver.Teacher.Lerpf(_from.b, _to.b, _pct);
            var g = ioDriver.Teacher.Lerpf(_from.g, _to.g, _pct);
            var a = ioDriver.Teacher.Lerpf(_from.a, _to.a, _pct);
            return new Color(r, g, b, a);
        }

        private static Vector2 LerpVector2(Vector2 _from, Vector2 _to, float _pct)
        {
            var x = ioDriver.Teacher.Lerpf(_from.x, _to.x, _pct);
            var y = ioDriver.Teacher.Lerpf(_from.y, _to.y, _pct);
            return new Vector2(x, y);
        }

        private static Vector3 LerpVector3(Vector3 _from, Vector3 _to, float _pct)
        {
            var x = ioDriver.Teacher.Lerpf(_from.x, _to.x, _pct);
            var y = ioDriver.Teacher.Lerpf(_from.y, _to.y, _pct);
            var z = ioDriver.Teacher.Lerpf(_from.z, _to.z, _pct);
            return new Vector3(x, y, z);
        }

        private static Vector4 LerpVector4(Vector4 _from, Vector4 _to, float _pct)
        {
            var x = ioDriver.Teacher.Lerpf(_from.x, _to.x, _pct);
            var y = ioDriver.Teacher.Lerpf(_from.y, _to.y, _pct);
            var z = ioDriver.Teacher.Lerpf(_from.z, _to.z, _pct);
            var w = ioDriver.Teacher.Lerpf(_from.w, _to.w, _pct);
            return new Vector4(x, y, z, w);
        }

        // Teach Unity types
        private static void TeachUnity()
        {
            ioDriver.SetLogMethods(
                    UnityEngine.Debug.Log,
                    UnityEngine.Debug.LogWarning,
                    UnityEngine.Debug.LogError);
            ioDriver.Teacher.Teach(
                Vector2.zero,
                (_a, _b) => _a + _b,
                (_a, _b) => (_b - _a).magnitude,
                LerpVector2,
                InvLerpVector2);

            ioDriver.Teacher.Teach(
                Vector3.zero,
                (_a, _b) => _a + _b,
                (_a, _b) => (_b - _a).magnitude,
                LerpVector3,
                InvLerpVector3);

            ioDriver.Teacher.Teach(
                Vector4.zero,
                (_a, _b) => _a + _b,
                (_a, _b) => (_b - _a).magnitude,
                LerpVector4,
                InvLerpVector4);

            ioDriver.Teacher.Teach(
                new Color(0, 0, 0, 0),
                (_a, _b) => _a + _b,
                (_a, _b) =>
                {
                    var r = _b - _a;
                    return new Vector4(r.r, r.b, r.g, r.a).magnitude;
                },
                LerpColor,
                InvLerpColor);

            //TODO alpha?

            ioDriver.Teacher.TeachCoord<Vector2>(2, _vals => new Vector2(_vals[0], _vals[1]),
                _vec => _vec.x,
                _vec => _vec.y);


            var constrsV3 = new Dictionary<int, ioDriver.Teacher.FuncConstruct<Vector3>>
            {
                {3, _vals => new Vector3(_vals[0], _vals[1], _vals[2])},
                {2, _vals => new Vector3(_vals[0], _vals[1])}
            };
            ioDriver.Teacher.TeachCoord(constrsV3, _vec => _vec.x, _vec => _vec.y, _vec => _vec.z);

            var constrsV4 = new Dictionary<int, ioDriver.Teacher.FuncConstruct<Vector4>>
            {
                {4, _vals => new Vector4(_vals[0], _vals[1], _vals[2], _vals[3])},
                {3, _vals => new Vector4(_vals[0], _vals[1], _vals[2])},
                {2, _vals => new Vector4(_vals[0], _vals[1])}
            };
            ioDriver.Teacher.TeachCoord(constrsV4, _vec => _vec.x, _vec => _vec.y, _vec => _vec.z, _vec => _vec.w);

            var constrsClr = new Dictionary<int, ioDriver.Teacher.FuncConstruct<Color>>
            {
                {4, _vals => new Color(_vals[0], _vals[1], _vals[2], _vals[3])},
                {3, _vals => new Color(_vals[0], _vals[1], _vals[2])}
            };
            ioDriver.Teacher.TeachCoord(constrsClr, _vec => _vec.r, _vec => _vec.g, _vec => _vec.b, _vec => _vec.a);

        }

        void Awake()
        {
            Enabled = true;
        }

        private void OnDestroy()
        {
            applicationIsQuitting = true;
            ioDriver.DestroyAll();
            m_Instance = null;
        }

        void Update()
        {
            if (Enabled)
            {
                if (UseUnityTimescale)
                    ioDriver.TimescaleGlobal = UnityEngine.Time.timeScale;
                ioDriver.Pump();
            }
        }

        #endregion Methods
    }

    /// Unity specific math support functions
    public static partial class ioMath
    {
        #region Methods

        /// <summary>
        /// Calculate the angle in degrees between two vectors.  Provided normal determines cw/ccw direction.
        /// </summary>
        /// <param name="_a">Vector to measure angle from</param>
        /// <param name="_b">Vector to measure angle to</param>
        /// <param name="_n">Normal vector to determine measurement direction (cw/ccw)</param>
        /// <returns>Angle in degrees (0 to 360)</returns>
        public static float Angle360(Vector3 _a, Vector3 _b, Vector3 _n)
        {
            return (SignedAngle(_a, _b, _n) + 180) % 360;
        }

        /// <summary>
        /// Calculate the angle in degrees between two vectors in the ccw direction.
        /// </summary>
        /// <param name="_a">Vector to measure angle from</param>
        /// <param name="_b">Vector to measure angle to</param>
        /// <returns>Angle in degrees (0 to 360)</returns>
        public static float Angle360(Vector2 _a, Vector2 _b)
        {
            return (SignedAngle(_a, _b) + 180) % 360;
        }

        /// <summary>
        /// Calculate the signed angle in degrees between two vectors.  Provided normal determines cw/ccw direction.
        /// </summary>
        /// <param name="_a">Vector to measure angle from</param>
        /// <param name="_b">Vector to measure angle to</param>
        /// <param name="_n">Normal vector to determine measurement direction (cw/ccw)</param>
        /// <returns>Signed angle in degrees (-180 to 180)</returns>
        public static float SignedAngle(Vector3 _a, Vector3 _b, Vector3 _n)
        {
            float angle = Vector3.Angle(_a, _b);
            float sign = Math.Sign(Vector3.Dot(_n, Vector3.Cross(_a, _b)));

            float signed_angle = angle * sign;

            return signed_angle;
        }

        /// <summary>
        /// Calculate the angle in degrees between two vectors in the CCW direction.
        /// </summary>
        /// <param name="_a">Vector to measure angle from</param>
        /// <param name="_b">Vector to measure angle to</param>
        /// <returns>Signed angle in degrees (-180 to 180)</returns>
        public static float SignedAngle(Vector2 _a, Vector2 _b)
        {
            return SignedAngle(_a, _b, Vector3.back);
        }

        #endregion Methods
    }


}

public static partial class ioDriver
{
    /// <summary>
    /// "Tween" specified gameobject over path.
    /// </summary>
    /// <param name="_go">Gameobject to tween</param>
    /// <param name="_path">Path to tween over</param>
    /// <param name="_duration">Duration of the tween</param>
    /// <param name="_name">User specified name</param>
    /// <returns>Tween path driver</returns>
    public static DTweenPath<Vector3> ioTween(GameObject _go, Path.Base<Vector3> _path, float _duration, string _name = null)
    {
        return Tween(() => _go.transform.position, _path, _duration, _name);
    }

    /// <summary>
    /// "Tween" specified transform over path.
    /// </summary>
    /// <param name="_xfrm">Transform to tween</param>
    /// <param name="_path">Path to tween over</param>
    /// <param name="_duration">Duration of the tween</param>
    /// <param name="_name">User specified name</param>
    /// <returns>Tween path driver</returns>
    public static DTweenPath<Vector3> ioTween(Transform _xfrm, Path.Base<Vector3> _path, float _duration,
        string _name = null)
    {
        return Tween(() => _xfrm.position, _path, _duration, _name);
    }

    internal static void Reset()
    {
        InitDone = false;
    }

}
#endif