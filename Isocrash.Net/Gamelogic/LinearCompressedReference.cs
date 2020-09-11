using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Isocrash.Net.Gamelogic
{
    public struct LinearCompressedReference
    {
        public int Length
        { 
            get { return _length; }
            set { _length = value; }
        }
        private int _length;

        public int Index
        {
            get
            {
                return _index;
            }

            set
            {
                _index = value;
            }
        }
        private int _index;

        public LinearCompressedReference(int length, int index)
        {
            this._length = length;
            this._index = index;
        }

    }
}
