namespace net.puk06.CanvasAnimation.Models
{
    public enum TransitionType
    {
        /// <summary>
        /// トランジションなし。変化が即時に行われます。
        /// </summary>
        None,

        /// <summary>
        /// 線形トランジション。一定の速度で変化します。
        /// </summary>
        Linear,

        /// <summary>
        /// イーズイン。最初はゆっくり始まり、終わりに向かって加速します。
        /// </summary>
        EaseIn,

        /// <summary>
        /// イーズアウト。最初は速く始まり、終わりに向かって減速します。
        /// </summary>
        EaseOut,

        /// <summary>
        /// イーズインアウト。最初はゆっくり始まり、中間で加速し、終わりに向かって減速します。
        /// </summary>
        EaseInOut
    }
}
