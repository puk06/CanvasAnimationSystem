using UdonSharp;
using UnityEngine;

namespace net.puk06.CanvasAnimation.Utils
{
    public class RectTransformUtils : UdonSharpBehaviour
    {
        public static RectTransform GetRectTransform(Component targetObject)
        {
            return targetObject.GetComponent<RectTransform>();
        }

        public static Vector3 GetPosition(Component targetObject)
        {
            RectTransform rectTransform = GetRectTransform(targetObject);
            if (rectTransform == null) return Vector3.positiveInfinity;

            return rectTransform.localPosition;
        }

        public static void SetPosition(Component targetObject, Vector3 position)
        {
            RectTransform rectTransform = GetRectTransform(targetObject);
            if (rectTransform == null) return;

            rectTransform.localPosition = position;
        }

        public static Vector3 GetRotation(Component targetObject)
        {
            RectTransform rectTransform = GetRectTransform(targetObject);
            if (rectTransform == null) return Vector3.positiveInfinity;

            return rectTransform.localEulerAngles;
        }

        public static void SetRotation(Component targetObject, Vector3 rotation)
        {
            RectTransform rectTransform = GetRectTransform(targetObject);
            if (rectTransform == null) return;

            rectTransform.localEulerAngles = rotation;
        }

        public static Vector3 GetScale(Component targetObject)
        {
            RectTransform rectTransform = GetRectTransform(targetObject);
            if (rectTransform == null) return Vector3.positiveInfinity;

            return rectTransform.localScale;
        }

        public static void SetScale(Component targetObject, Vector3 scale)
        {
            RectTransform rectTransform = GetRectTransform(targetObject);
            if (rectTransform == null) return;

            rectTransform.localScale = scale;
        }
    }
}
