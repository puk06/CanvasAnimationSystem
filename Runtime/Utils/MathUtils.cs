using net.puk06.CanvasAnimation.Models;
using UdonSharp;
using UnityEngine;

namespace net.puk06.CanvasAnimation.Utils
{
    public class MathUtils : UdonSharpBehaviour
    {
        public static float ApplyEasing(float t, TransitionType type)
        {
            switch (type)
            {
                case TransitionType.Linear:
                    return t;
                case TransitionType.EaseIn:
                    return t * t;
                case TransitionType.EaseOut:
                    return 1f - ((1f - t) * (1f - t));
                case TransitionType.EaseInOut:
                    return Mathf.SmoothStep(0f, 1f, t);
                default:
                    return t;
            }
        }

        public static bool IsPositiveInfinity(Vector3 vector3)
        {
            return float.IsPositiveInfinity(vector3.x)
                && float.IsPositiveInfinity(vector3.y)
                && float.IsPositiveInfinity(vector3.z);
        }
    }
}
