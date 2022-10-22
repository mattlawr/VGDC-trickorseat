using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Resrc
{

    [System.Serializable]
    public class UnityEventToggle
    {
        public bool alwaysTrigger = false;
        public UnityEvent on;
        public UnityEvent off;

        protected bool state = false;

        public void Reset()
        {
            state = false;
        }

        public void On()
        {
            if (!alwaysTrigger && state) return;

            state = true;
            on.Invoke();
        }
        public void Off()
        {
            if (!alwaysTrigger && !state) return;

            state = false;
            off.Invoke();
        }
    }

    namespace Tools
    {

        public static class SceneTools
        {
            public static List<T> Find<T>()
            {
                List<T> interfaces = new List<T>();
                GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
                foreach (var rootGameObject in rootGameObjects)
                {
                    T[] childrenInterfaces = rootGameObject.GetComponentsInChildren<T>();
                    foreach (var childInterface in childrenInterfaces)
                    {
                        interfaces.Add(childInterface);
                    }
                }
                return interfaces;
            }
        }

        public static class CameraTools
        {
            /// <summary>
            /// Finds how far off something is from the top of the screen and calculates an adjusted y position.
            /// Positive distance returned means above screen bound.
            /// </summary>
            public static float DistanceFromCameraViewTop(Camera camera, Vector3 target, out float adjustedY, float top = 1f)
            {
                // TODO: fix error "Screen position out of view frustum"

                Vector3 p = GetCrosshairSpacePosition(camera, target);
                float y = p.y;

                float d = y - top;

                Vector3 viewEdge = camera.WorldToViewportPoint(target);
                viewEdge.y = top;
                Vector3 newPosition = camera.ViewportToWorldPoint(viewEdge);

                adjustedY = newPosition.y;

                return d;
            }

            /// <summary>
            /// Used to calculate the view space position in relation to the crosshair.
            /// (0,0) is center, top-right is (1,1), bottom-left is (-1,-1)
            /// </summary>
            /// <returns>Vector3 (percentage within -1 to 1 x axis, y axis)</returns>
            public static Vector3 GetCrosshairSpacePosition(Camera camera, Vector3 world)
            {
                Vector3 view = camera.WorldToViewportPoint(world);  // (0,0) to (1,1)

                float pX = (view.x * 2f) - 1f;
                float pY = (view.y * 2f) - 1f;

                return new Vector3(pX, pY, view.z);
            }
        }

        public static class StringTools
        {
            public static string StringList<T>(List<T> myList)
            {
                // Concatenate all items into a single string
                // NOTE:  If the List is long, this would be more efficient with a
                // StringBuilder
                string result = "List: ";
                foreach (var item in myList)
                {
                    result += item.ToString() + ", ";
                }
                return result;
            }
        }

        public static class MathTools
        {
            public static float SmoothCubeInv(float x)
            {
                if (x <= 0) return 0f;
                if (x >= 1) return 1f;
                return 0.5f - Mathf.Sin(Mathf.Asin(1f - 2f * x) / 3f);
            }

            public static float SmoothCubeInvLerp(float a, float b, float x)
            {
                return Mathf.Lerp(a, b, SmoothCubeInv(x));
            }
        }

        public static class VectorTools
        {
            public static Vector3 CrossSafe(Vector3 a, Vector3 b)
            {
                Vector3 v = Vector3.Cross(a, b).normalized;

                if (v == Vector3.zero) v = Vector3.forward;

                return v;
            }

            public static Vector3 CancelDirection(Vector3 v, Vector3 dir)
            {
                float dot = Vector3.Dot(dir.normalized, v); // Amount of v in direction of dir
                Vector3 sub = dir.normalized * dot;

                return v - sub;
            }

            /// <summary>
            /// Projection, but original magnitude is preserved.
            /// </summary>
            public static Vector3 ConserveOnPlane(Vector3 v, Vector3 planeNormal)
            {
                Vector3 sub = Vector3.ProjectOnPlane(v, planeNormal);
                sub = sub.normalized * v.magnitude;

                return sub;
            }

            /// <summary>
            /// Multiplies a vector componenet-wise by (1, 0, 1).
            /// </summary>
            public static Vector3 FlattenOnY(Vector3 v)
            {
                Vector3 sub = Vector3.Scale(v, new Vector3(1f,0,1f));

                return sub;
            }

            /// <summary>
            /// Sets y component to MAX(0, v.y).
            /// </summary>
            public static Vector3 CancelFall(Vector3 v)
            {
                Vector3 sub = v;

                sub.y = Mathf.Max(0f, v.y);

                return sub;
            }

            /// <summary>
            /// Sets y component to MAX(0, v.y) + add.
            /// </summary>
            public static Vector3 IncreaseUpward(Vector3 v, float add)
            {
                Vector3 sub = v;

                sub.y = Mathf.Max(0f, v.y) + add;

                return sub;
            }

            /// <summary>
            /// Sets y component to MAX(0, v.y) + add.
            /// </summary>
            public static Vector3 IncreaseDownward(Vector3 v, float add)
            {
                Vector3 sub = v;

                sub.y = Mathf.Min(0f, v.y) + add;

                return sub;
            }

            public static Vector3 FlattenOnY(Vector3 v, out bool undefined)
            {
                undefined = false;
                if (Mathf.Abs(v.x) <= 0.001f && Mathf.Abs(v.z) <= 0.001f) undefined = true;

                return FlattenOnY(v);
            }
        }

    }
}
