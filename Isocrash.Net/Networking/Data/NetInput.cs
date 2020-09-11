using System;

namespace Isocrash.Net
{
    public class NetInput : NetObject
    {
        public ConsoleKey Key
        {
            get { return _key; }
            set { _key = value; }
        }
        private ConsoleKey _key;

        public NetInput(ConsoleKey key)
        {
            this._key = key;
        }
    }
}