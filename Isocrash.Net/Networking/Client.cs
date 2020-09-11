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
        public static Logger _clientLogger = new Logger(ConsoleLog, ConsoleLogWarning, ConsoleLogError, ConsoleLogException);
        public static List<NetObject> _receivedData = new List<NetObject>();

        #endregion

        #region Public Methods
        #region Server Logger Setters
        /// <summary>
        /// Set a new logger used by the server.
        /// </summary>
        /// <param name="info">The function used to log information</param>
        /// <param name="warning">The function used to log warnings</param>
        /// <param name="error">The function used to log errors</param>
        /// <param name="exception">The function used to log exceptions</param>
        public static void SetClientLogger(
            Logger.LoggerCallback info, Logger.LoggerCallback warning,
            Logger.LoggerCallback error, Logger.LoggerCallback exception)
        {
            _clientLogger = new Logger(info, warning, error, exception);
        }
        /// <summary>
        /// Set a new function for the logger
        /// </summary>
        /// <param name="type">The type of log the function will apply on</param>
        /// <param name="function">The function used to log the type</param>
        public static void SetClientLogger(LogType type, Logger.LoggerCallback function)
        {
            switch (type)
            {
                case LogType.Info:
                    _clientLogger._infoCallback = function;
                    break;
                case LogType.Warning:
                    _clientLogger._warningCallback = function;
                    break;
                case LogType.Error:
                    _clientLogger._errorCallback = function;
                    break;
                case LogType.Exception:
                    _clientLogger._exceptionCallback = function;
                    break;
                default: // .Info:
                    _clientLogger._infoCallback = function;
                    break;
            }
        }
        #endregion

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
        /*public static ReadOnlyCollection<NetObject> ReceivedData
        {
            get
            {
                ReadOnlyCollection<NetObject> objs = _receivedData.AsReadOnly();
                _receivedData.Clear();
                return objs;
            }
        }*/

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

        public static void Log(object message)
        {
            _clientLogger.Log(message);
        }
        public static void LogWarning(object message)
        {
            _clientLogger.LogWarning(message);
        }
        public static void LogError(object message)
        {
            _clientLogger.LogError(message);
        }
        public static void LogException(object message)
        {
            _clientLogger.LogException(message);
        }

        #region Console Logging Methods
        /// <summary>
        /// Console information logging
        /// </summary>
        /// <param name="message">The message to log</param>
        private static void ConsoleLog(object message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Console warning logging
        /// </summary>
        /// <param name="message">The message to log</param>
        private static void ConsoleLogWarning(object message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Console error logging
        /// </summary>
        /// <param name="message">The message to log</param>
        private static void ConsoleLogError(object message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Console exception logging
        /// </summary>
        /// <param name="message">The message to log</param>
        private static void ConsoleLogException(object message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        #endregion

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
            _clientLogger.Log("Connected to server");

            Task.Run(() => SendDataToServer());
            await GatherServerData();
            
            _clientLogger.Log("Disconnected from server");
        }
        private static async Task GatherServerData()
        {
            try
            {
                while (Connected)
                {
                    
                    _receivedData.Add(await GetDataAsync(_client.Client));
                    //_clientLogger.Log("ADDED NEW OBJECT DATA");
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
            catch(Exception e) { _clientLogger.LogError(e);}
        }
        private static async Task<NetObject> GetDataAsync(Socket client)
        {
            return await Task.Run(() =>
            {
                //_clientLogger.Log("Waiting for data");
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
                //_clientLogger.Log("Got new object");
                return NetObject.GetContent(msg);
            });
        }
        #endregion
    }
}