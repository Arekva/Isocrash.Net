using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isocrash.Net.Gamelogic
{
    public class ItemDef
    {
        public string Identifier { get; private set; }

        internal short CacheIndex { get; private set; }

        internal ItemDef(string id, short cacheIndex)
        {
            this.Identifier = id;
        }

        public override string ToString()
        {
            return this.Identifier;
        }
    }
}
