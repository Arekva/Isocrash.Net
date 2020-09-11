﻿using Newtonsoft.Json;

namespace Isocrash.Net
{
    public class NetPlayerHandshake : NetObject
    {
        /// <summary>
        /// The username of the player, set if offline
        /// </summary>
        public string Username
        {
            get => _username;
            set => _username = value;
        }
        private string _username;

        /// <summary>
        /// The connection token of the player, set if online
        /// </summary>
        public string Token
        {
            get => _token;
            set => _token = value;
        }
        private string _token;

        /// <summary>
        /// Creates a new connection handshake
        /// </summary>
        /// <param name="username">The username of the player (OFFLINE)</param>
        /// <param name="token">The connection token of the player (ONLINE)</param>
        [JsonConstructor]
        public NetPlayerHandshake(string username, string token)
        {
            this._username = username;
            this._token = token;
        }
    }
}