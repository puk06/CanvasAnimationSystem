using net.puk06.CanvasAnimation.Models;
using UdonSharp;
using UnityEngine;

namespace net.puk06.CanvasAnimation.Utils
{
    public class ArrayUtils : UdonSharpBehaviour
    {
        public static void InitializeValues(float[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = -1;
            }
        }

        public static void InitializeValues(int[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = -1;
            }
        }

        public static void InitializeValues(Vector3[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Vector3.positiveInfinity;
            }
        }

        public static void InitializeValues(Color[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ColorUtils.GetInvalidColor();
            }
        }

        public static int AssignArrayValue<T>(T[] array, T value)
        {
            if (value == null) return -1;

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    array[i] = value;
                    return i;
                }

                continue;
            }

            return -1;
        }

        public static int AssignArrayValue(float[] array, float value)
        {
            if (value == -1) return -1;

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == -1)
                {
                    array[i] = value;
                    return i;
                }

                continue;
            }

            return -1;
        }

        public static int AssignArrayValue(int[] array, int value)
        {
            if (value == -1) return -1;

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == -1)
                {
                    array[i] = value;
                    return i;
                }

                continue;
            }

            return -1;
        }

        public static int AssignArrayValue(Vector3[] array, Vector3 value)
        {
            if (MathUtils.IsPositiveInfinity(value)) return -1;

            for (int i = 0; i < array.Length; i++)
            {
                if (MathUtils.IsPositiveInfinity(array[i]))
                {
                    array[i] = value;
                    return i;
                }

                continue;
            }

            return -1;
        }

        public static int AssignArrayValue(Color[] array, Color value)
        {
            if (ColorUtils.IsInvalidColor(value)) return -1;

            for (int i = 0; i < array.Length; i++)
            {
                if (ColorUtils.IsInvalidColor(array[i]))
                {
                    array[i] = value;
                    return i;
                }

                continue;
            }

            return -1;
        }

        public static int AssignArrayValue(TransitionType[] array, TransitionType value)
        {
            if (value == TransitionType.None) return -1;

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == TransitionType.None)
                {
                    array[i] = value;
                    return i;
                }

                continue;
            }

            return -1;
        }

        public static int AssignArrayValue(AnimationMode[] array, AnimationMode value)
        {
            if (value == AnimationMode.None) return -1;

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == AnimationMode.None)
                {
                    array[i] = value;
                    return i;
                }

                continue;
            }

            return -1;
        }

        public static int AssignArrayValue(ElementType[] array, ElementType value)
        {
            if (value == ElementType.None) return -1;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == ElementType.None)
                {
                    array[i] = value;
                    return i;
                }

                continue;
            }

            return -1;
        }
    }
}
