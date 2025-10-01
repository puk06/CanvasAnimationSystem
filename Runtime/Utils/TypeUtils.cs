using UdonSharp;

namespace net.puk06.CanvasAnimation.Utils
{
    public class TypeUtils : UdonSharpBehaviour
    {
        public static int BoolToInt(bool value)
        {
            return value ? 1 : 0;
        }

        public static bool IntToBool(int value)
        {
            return value == 1;
        }
    }
}
