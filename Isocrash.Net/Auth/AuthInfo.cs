using System;

namespace Isocrash.Net
{
    internal struct AuthInfo
    {
        public string Nickname { get; }
        public Guid Identifier { get; }

        public AuthInfo(string nickname, Guid globalId)
        {
            this.Nickname = nickname;
            this.Identifier = globalId;
        }
    }
}