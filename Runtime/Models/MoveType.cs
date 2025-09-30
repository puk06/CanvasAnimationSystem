namespace net.puk06.CanvasAnimation.Models
{
    /// <summary>
    /// アニメーションの変化方向を表します。
    /// </summary>
    public enum AnimationDirection
    {
        /// <summary>
        /// 指定された値（位置・スケールなど）に向かって変化します。
        /// </summary>
        To,

        /// <summary>
        /// 指定された値から現在の状態に向かって変化します。
        /// </summary>
        From
    }
}
