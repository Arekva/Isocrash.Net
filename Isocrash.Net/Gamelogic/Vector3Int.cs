namespace Isocrash.Net.Gamelogic
{
    public struct Vector3Int
    {
        /// <summary>
        /// +X (1,0,0)
        /// </summary>
        public static Vector3Int Right => new Vector3Int(1,0,0);
        /// <summary>
        /// -X (-1,0,0)
        /// </summary>
        public static Vector3Int Left => new Vector3Int(-1,0,0);
        /// <summary>
        /// +Y (0,1,0)
        /// </summary>
        public static Vector3Int Up => new Vector3Int(0,1,0);
        /// <summary>
        /// -Y (0,-1,0)
        /// </summary>
        public static Vector3Int Down => new Vector3Int(0,-1,0);
        /// <summary>
        /// +Z (0,0,1)
        /// </summary>
        public static Vector3Int Forward => new Vector3Int(0,0,1);
        /// <summary>
        /// -Z (0,0,-1)
        /// </summary>
        public static Vector3Int Backward => new Vector3Int(0,0,-1);
        /// <summary>
        /// Null vector (0,0,0)
        /// </summary>
        public static Vector3Int Zero => new Vector3Int(0,0,0);
        /// <summary>
        /// One vector (1,1,1)
        /// </summary>
        public static Vector3Int One => new Vector3Int(1,1,1);
        /// <summary>
        /// -One vector (-1,-1,-1)
        /// </summary>
        public static Vector3Int MinusOne => new Vector3Int(-1,-1,-1);

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public Vector3Int(int x, int y)
        {
            this.X = x;
            this.Y = y;
            this.Z = 0;
        }

        public Vector3Int(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public static bool operator ==(Vector3Int v1, Vector3Int v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
        }

        public static bool operator !=(Vector3Int v1, Vector3Int v2)
        {
            return v1.X != v2.X || v1.Y != v2.Y || v1.Z != v2.Z;
        }

    }
}