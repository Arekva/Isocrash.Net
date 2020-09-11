namespace Isocrash.Net.Gamelogic
{
    public struct Vector2Int
    {
        /// <summary>
        /// +X (1,0)
        /// </summary>
        public static Vector2Int Right => new Vector2Int(1,0);
        /// <summary>
        /// -X (-1,0)
        /// </summary>
        public static Vector2Int Left => new Vector2Int(-1,0);
        /// <summary>
        /// +Y (0,1)
        /// </summary>
        public static Vector2Int Up => new Vector2Int(0,1);
        /// <summary>
        /// -Y (0,-1)
        /// </summary>
        public static Vector2Int Down => new Vector2Int(0,-1);
        /// <summary>
        /// Null vector (0,0)
        /// </summary>
        public static Vector2Int Zero => new Vector2Int(0,0);
        /// <summary>
        /// One vector (1,1)
        /// </summary>
        public static Vector2Int One => new Vector2Int(1,1);
        /// <summary>
        /// -One vector (-1,-1)
        /// </summary>
        public static Vector2Int MinusOne => new Vector2Int(-1,-1);

        public int X { get; set; }
        public int Y { get; set; }

        public Vector2Int(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        
        public static bool operator ==(Vector2Int v1, Vector2Int v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y;
        }
        
        public static bool operator !=(Vector2Int v1, Vector2Int v2)
        {
            return v1.X != v2.X || v1.Y != v2.Y;
        }
    }
}