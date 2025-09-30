namespace net.puk06.CanvasAnimation.Models
{
    /// <summary>
    /// UI 要素がアニメーションで移動する方向を表します。
    /// </summary>
    public enum MoveDirection
    {
        /// <summary>
        /// 上方向に移動して元の位置に表示されます。（下から出てくるイメージ）
        /// </summary>
        Up,

        /// <summary>
        /// 下方向に移動して元の位置に表示されます。（上から出てくるイメージ）
        /// </summary>
        Down,

        /// <summary>
        /// 左方向に移動して元の位置に表示されます。（右から出てくるイメージ）
        /// </summary>
        Left,

        /// <summary>
        /// 右方向に移動して元の位置に表示されます。（左から出てくるイメージ）
        /// </summary>
        Right
    }
}
