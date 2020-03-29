using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Isocrash.Net
{
    public class Player
    {
        private List<NetObject> _enqueuedObjects = new List<NetObject>();

        /// <summary>
        /// The nickname of the player
        /// </summary>
        public string Nickname { get; }
        /// <summary>
        /// The GUID of the player
        /// </summary>
        public Guid Identifier { get; }
        /// <summary>
        /// The <see cref="TcpClient"/> of the player
        /// </summary>
        public TcpClient TCP { get; }

        /// <summary>
        /// Creates an online (authenticated) player
        /// </summary>
        /// <param name="infos">The authentication information of the player</param>
        /// <param name="clientConnection">The client TCP</param>
        private Player(AuthInfo infos, TcpClient clientConnection)
        {
            this.Nickname = infos.Nickname;
            this.Identifier = infos.Identifier;
            this.TCP = clientConnection;
        }
        /// <summary>
        /// Creates an offline player
        /// </summary>
        /// <param name="nickname"></param>
        /// <param name="clientConnection">The client TCP</param>
        private Player(string nickname, TcpClient clientConnection)
        {
            this.Nickname = nickname;
            // Generate GUID from nickname hash
            using (MD5 md5 = MD5.Create())
            {
                byte[] nickhash = md5.ComputeHash(Encoding.ASCII.GetBytes(nickname));
                this.Identifier = new Guid(nickhash);
            }

            this.TCP = clientConnection;
        }
        
        /// <summary>
        /// Creates an online (authenticated) player
        /// </summary>
        /// <param name="infos">The authentication information of the player</param>
        /// <param name="clientConnection">The client TCP</param>
        internal static Player CreateAuthenticated(AuthInfo infos, TcpClient clientConnection)
        {
            return new Player(infos, clientConnection);
        }
        
        /// <summary>
        /// Creates an offline player
        /// </summary>
        /// <param name="nickname"></param>
        /// <param name="clientConnection">The client TCP</param>
        internal static Player CreateOffline(string nickname, TcpClient clientConnection)
        {
            //Server.Log("Starting offline player creation...");
            return new Player(nickname, clientConnection);
            
        }

        public void Disconnect()
        {
            TCP.Close();
            Server._connectedPlayers.Remove(this);
        }
        
        internal void EnqueueData(NetObject data)
        {
            if (data == null) return;
            
            _enqueuedObjects.Add(data);
        }

        internal void SendEnqueuedData()
        {
            NetObject[] netObjects = _enqueuedObjects.ToArray();
            for (int i = 0; i < netObjects.Length; i++)
            {
                NetObject.SendContent(netObjects[i], this.TCP.Client);
            }
        }
    }
}