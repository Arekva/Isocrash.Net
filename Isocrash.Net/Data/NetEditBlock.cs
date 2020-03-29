﻿using Newtonsoft.Json;

namespace Isocrash.Net
{
    public class NetEditBlock : NetObject
    {
        public uint Id
        {
            get => _id;
            set => _id = value;
        }
        private uint _id;

        public int X
        {
            get => _x;
            set => _x = value;
        }

        private int _x;

        public int Y
        {
            get => _y;
            set => _y = value;
        }

        private int _y;

        public int Z
        {
            get => _z;
            set => _z = value;
        }

        private int _z;

        //[JsonConstructor]
        public NetEditBlock(uint id, int x, int y, int z)
        {
            this._id = id;
            this._x = x;
            this._y = y;
            this._z = z;
        }
    }
}