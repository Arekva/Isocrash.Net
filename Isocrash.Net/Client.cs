using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;

namespace Isocrash.Net
{
    public static class Client
    {
        #region Public Attributes
        public static bool Connected { get; private set; }
        public static string AuthToken { get; private set; }
        public static string Nickname { get; private set; }
        #endregion
        
        #region Private Attributes
        private static Thread _dataThread;
        private static List<NetObject> _enqueuedData = new List<NetObject>();
        private static TcpClient _client;
        private static Logger _clientLogger = new Logger(Console.WriteLine,Console.WriteLine,Console.WriteLine,Console.WriteLine);
        private static List<NetObject> _receivedData = new List<NetObject>();
        
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Set nickname (Offline); Works only while not connected to server.
        /// </summary>
        /// <param name="nickname"></param>
        public static void SetNickname(string nickname)
        {
            if (!Connected) Nickname = nickname;
        }
        /// <summary>
        /// Set authentication token (Online); Works only while not connected to server.
        /// </summary>
        /// <param name="token"></param>
        public static void SetAuthenticationToken(string token)
        {
            if (!Connected) AuthToken = token;
        }
        /// <summary>
        /// Enqueue data to send to server (only works while connected)
        /// </summary>
        /// <param name="content"></param>
        public static void EnqueueData(NetObject content)
        {
            if (content == null) return;
            if(Connected) _enqueuedData.Add(content);
        }

        /// <summary>
        /// Get and clear the the received data.
        /// </summary>
        public static ReadOnlyCollection<NetObject> ReceivedData
        {
            get
            {
                ReadOnlyCollection<NetObject> objs = _receivedData.AsReadOnly();
                _receivedData.Clear();
                return objs;
            }
        }

        /// <summary>
        /// Connect to server, returns if success
        /// </summary>
        /// <param name="ipAddress">The server address</param>
        /// <param name="port">The server port</param>
        /// <returns>Returns connection success</returns>
        public static bool Connect(string ipAddress, int port)
        {
            bool handshakesent = false;
            try
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                _client = new TcpClient();
                _client.Connect(ip);
                NetPlayerHandshake handshake = new NetPlayerHandshake(Client.Nickname, Client.AuthToken);
                handshakesent = true;
                NetObject.SendContent(handshake, _client.Client);

                _dataThread = new Thread(DoDataForThread)
                {
                    Priority = ThreadPriority.AboveNormal
                };
                _dataThread.Start();
                
                    
                Connected = true;
                return true;
            }
            
            catch(Exception e)
            {
                if(!handshakesent)
                    _clientLogger.LogError("Couldn't connect to server: " + e);
                else
                    _clientLogger.LogError("Couldn't connect to server: token not valid.");
                return false;
            }
        }

        /// <summary>
        /// Disconnects from server
        /// </summary>
        public static void Disconnect()
        {
            if (!Connected || _client == null) return;
            _dataThread.Abort();
            Connected = false;
            _client.Close();
        }
        #endregion
        
        #region Private Methods

        private static void DoDataForThread()
        {
            DoData();
        }
        private static async Task DoData()
        {
            _clientLogger.LogWarning("Connected to server");

            Task.Run(() => SendDataToServer());
            await GatherServerData();
            
            _clientLogger.LogWarning("Disconnected from server");
        }
        private static async Task GatherServerData()
        {
            try
            {
                while (Connected)
                {
                    _receivedData.Add(await GetDataAsync(_client.Client));
                }
            }
            
            //Disconnection
            catch {}
        }
        private static async Task SendDataToServer()
        {
            try
            {
                while (Connected)
                {
                    await Task.Run(() =>
                    {
                        NetObject[] netObjects = _enqueuedData.ToArray();
                        _enqueuedData.Clear();

                        for (int i = 0; i < netObjects.Length; i++)
                        {
                            NetObject.SendContent(netObjects[i], _client.Client);
                        }
                    });
                }
            }
            
            //Disconnection
            catch {}
        }
        private static async Task<NetObject> GetDataAsync(Socket client)
        {
            return await Task.Run(() =>
            {
                byte[] sizeInfo = new byte[4];
                Int32 totalread = 0, currentread = 0;
                currentread = totalread = client.Receive(sizeInfo);


                while (totalread < sizeInfo.Length && currentread > 0)
                {
                    currentread = client.Receive(sizeInfo,
                        totalread, //offset into the buffer
                        sizeInfo.Length - totalread, //max amount to read
                        SocketFlags.None);

                    totalread += currentread;
                }

                Int32 messageSize = 0;

                //could optionally call BitConverter.ToInt32(sizeinfo, 0);
                messageSize |= sizeInfo[0];
                messageSize |= (((Int32) sizeInfo[1]) << 8);
                messageSize |= (((Int32) sizeInfo[2]) << 16);
                messageSize |= (((Int32) sizeInfo[3]) << 24);

                Byte[] data = new Byte[messageSize];

                //read the first chunk of data
                totalread = 0;
                currentread = totalread = client.Receive(data,
                    totalread, //offset into the buffer
                    data.Length - totalread, //max amount to read
                    SocketFlags.None);
                //if we didn't get the entire message, read some more until we do
                while (totalread < messageSize && currentread > 0)
                {
                    currentread = client.Receive(data,
                        totalread, //offset into the buffer
                        data.Length - totalread, //max amount to read
                        SocketFlags.None);
                    totalread += currentread;
                }
                
                string msg = System.Text.Encoding.Unicode.GetString(data);
                return NetObject.GetContent(msg);
            });
        }
        #endregion
    }
}