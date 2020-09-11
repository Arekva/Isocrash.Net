using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Isocrash.Net.Gamelogic;
using Newtonsoft.Json;

namespace Isocrash.Net
{
    public class NetChunk : NetObject
    {
        public Vector2Int Position
        {
            get
            {
                return this._position;
            }

            set
            {
                this._position = value;
            }
        }
        private Vector2Int _position;

        public DictionnaryChunk Blocks
        {
            get
            {
                return _blocks;
            }

            set
            {
                _blocks = value;
            }
        }
        private DictionnaryChunk _blocks;

        [JsonConstructor]
        public NetChunk(DictionnaryChunk blocks, Vector2Int position)
        {
            this._blocks = blocks;
            this._position = position;
        }
    }
}
