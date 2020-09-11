using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Isocrash.Net.Gamelogic;
using ThreadPriority = System.Threading.ThreadPriority;

//using Mono.Data.Sqlite;

namespace Isocrash.Net
{
    public static class Server
    {
        #region Public Attributes
        /// <summary>
        /// If the server is using online authentication.
        /// If not, offline player are able to come.
        /// </summary>
        public static bool UseAuth { get; set; } = false;
        /// <summary>
        /// The server tick rate (game state snapshot rate)
        /// </summary>
        public static int TickRate { get; set; } = 20;
        /// <summary>
        /// The IP Address the server is listening towards
        /// </summary>
        public static IPAddress IP { get; private set; } = IPAddress.Any;
        public static bool Integrated { get; private set; } = false;
        /// <summary>
        /// The port the server is listening towards
        /// </summary>
        public static int Port { get; private set; } = 26656;
        /// <summary>
        /// If the server is running / online
        /// </summary>
        public static bool Running { get; private set; }
        /// <summary>
        /// All the connected players
        /// </summary>
        public static ReadOnlyCollection<Player> ConnectedPlayers
        {
            get => _connectedPlayers.AsReadOnly();
        }
        #endregion
        
        #region Internal Attributes
        internal static List<Player> _connectedPlayers { get; set; } = new List<Player>(); //players
        #endregion
        
        #region Private Attributes

        private static bool _dataGatherPaused = false;

        private static Thread playerDataThread;
        private static Thread snapshotThread;
        
        private static TcpListener _listener; //server
        private static Logger ServerLogger = new Logger(ConsoleLog, ConsoleLogWarning,ConsoleLogError,ConsoleLogException);

        private static bool dataCallbacksDone = false;
        
        private static readonly Dictionary<NetObject, Player> _receivedData = new Dictionary<NetObject, Player>();
        #endregion

        #region Public Methods

        public static Ticket[] GetIntegratedTickets()
        {
            return Integrated ? Ticket._tickets.ToArray() : null;
        }



        #region Server Logger Setters
        /// <summary>
        /// Set a new logger used by the server.
        /// </summary>
        /// <param name="info">The function used to log information</param>
        /// <param name="warning">The function used to log warnings</param>
        /// <param name="error">The function used to log errors</param>
        /// <param name="exception">The function used to log exceptions</param>
        public static void SetServerLogger(
            Logger.LoggerCallback info, Logger.LoggerCallback warning,
            Logger.LoggerCallback error, Logger.LoggerCallback exception)
        {
            ServerLogger._infoCallback = info;
            ServerLogger._warningCallback = warning;
            ServerLogger._errorCallback = error;
            ServerLogger._exceptionCallback = exception;
        }
        /// <summary>
        /// Set a new function for the logger
        /// </summary>
        /// <param name="type">The type of log the function will apply on</param>
        /// <param name="function">The function used to log the type</param>
        public static void SetServerLogger(LogType type, Logger.LoggerCallback function)
        {
            switch (type)
            {
                case LogType.Info:
                    ServerLogger._infoCallback = function;
                    break;
                case LogType.Warning:
                    ServerLogger._warningCallback = function;
                    break;
                case LogType.Error:
                    ServerLogger._errorCallback = function;
                    break;
                case LogType.Exception:
                    ServerLogger._exceptionCallback = function;
                    break;
                default: // .Info:
                    ServerLogger._infoCallback = function;
                    break;
            }
        }
        #endregion

        public static void SetIntegrated(bool value)
        {
            Integrated = value;
        }
        
        #region Start Methods
        /// <summary>
        /// Starts the server
        /// </summary>
        /// <param name="listenAddress">IP:PORT Address the server is listening on ("0.0.0.0:26656" by default)</param>
        /// <returns>If the server has started with no problems</returns>
        public static bool Start(string listenAddress)
        {
            try
            {
                string[] args = listenAddress.Split(':');
                string ip = args[0];
                string sport = args[1];

                int port = Int32.Parse(sport);

                return Start(ip, port);
            }
            catch (Exception e)
            {
                LogError("Unable to start server: \"" + listenAddress + "\" is not a correct IP Address");
                return false;
            }
        }
        /// <summary>
        /// Starts the server
        /// </summary>
        /// <param name="listenIP">IP Address the server is listening on ("0.0.0.0" by default)</param>
        /// <param name="port">Port the server is listening on (26656 by default)</param>
        /// <returns>If the server has started with no problems</returns>
        public static bool Start(string listenIP, int port)
        {
            if(IPAddress.TryParse(listenIP, out IPAddress address))
            {
                return Start(address, port);
            }
            else
            {
                LogError($"Unable to start server: \"{listenIP}\" is not a valid IP address");
                return false;
            }
        }
        /// <summary>
        /// Starts the server
        /// </summary>
        /// <param name="listenIP">IP Address the server is listening on (IPAddress.Any by default)</param>
        /// <param name="port">Port the server is listening on (26656 by default)</param>
        /// <returns>If the server has started with no problems</returns>
        public static bool Start(IPAddress listenIP, int port)
        {
            return Start(new IPEndPoint(listenIP, port));
        }
        /// <summary>
        /// Starts the server
        /// </summary>
        /// <param name="listenAddress">IP:PORT Address the server is listening on (IPAddress.Any:26656 by default)</param>
        /// <returns>If the server has started with no problems</returns>
        public static bool Start(IPEndPoint listenAddress)
        {
            try
            {
                // Checks if already running
                if (Running)
                {
                    LogError("Unable to start server: already running !");
                    return false;
                }

                if (!dataCallbacksDone) RegisterDataCallbacks();
                
                //Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
                // Loads plugins
                Plugin.LoadPluginsInFolder(FileSystem.GetSpecialDirectory(Folder.Plugins).FullName);

                // Registers all commands, internals and plugins as well.
                CommandFire.FireRegistery();

                ItemDef[] items = ItemCache.LoadItems();
                for (short i = 0; i < items.Length; i++)
                {
                    //LogWarning("Loaded item " + items[i]);
                }

                //Console.CursorVisible = false;
                Stopwatch worldSW = new Stopwatch();
                worldSW.Start();
                World.StartDebug();

                uint startnum = (uint)(Math.Pow(World.StartRadius * 2 + 1,2));
                string loadmode = "Generating ";
                if (File.Exists(FileSystem.GetSpecialDirectory(Folder.Save).FullName + "c0_0.json")) loadmode = "Loading ";

                while (!World.StartGenerationDone)
                {
                   //Console.SetCursorPosition(0, 0);
                   Log(loadmode + $"world : {Math.Round((Ticket._tickets.Count / (float)startnum) * 100,0)}% ({Process.GetCurrentProcess().PrivateMemorySize64 / (int)1e6} MB) | {Ticket._tickets.Count} chunks loaded");
                   Thread.Sleep(100);
                }
                worldSW.Stop();
                 /*Task.Run(() => {

                    Ticket[] ticket = Ticket._tickets.ToArray();
                    for (int i = 0; i < ticket.Length; i++)
                    {
                        ticket[i].Unload();
                    }
                });*/
                //Console.SetCursorPosition(0, 0);
                Log($"Loading world : 100% ({Process.GetCurrentProcess().PrivateMemorySize64 / (int)1e6} MB)");
                //Console.CursorVisible = true;
                //Console.SetCursorPosition(0,1);
                //Console.WriteLine("Load done in " + Math.Round(worldSW.Elapsed.TotalMilliseconds,2) + "ms");

                
                
                _listener = new TcpListener(listenAddress);
                _listener.Start();
                Running = true;
                Log("Isocrash server started on port " + Port);
                playerDataThread = new Thread(StartPlayerLoops)
                {
                    Priority = ThreadPriority.AboveNormal, IsBackground = true
                };
                playerDataThread.Start();

                snapshotThread = new Thread(Snapshot)
                {
                    Priority = ThreadPriority.Highest, IsBackground = true
                };
                snapshotThread.Start();

                
                return true;
            }
            catch (Exception e)
            {
                LogException("[Exception] Unable to start server " + e);
                
                return false;
            }
        }
        #endregion

        #region Logging Methods
        /// <summary>
        /// Logs an information
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Log(object message)
        {
            ServerLogger.Log(message);
        }
        /// <summary>
        /// Logs a warning
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void LogWarning(object message)
        {
            ServerLogger.LogWarning(message);
        }
        /// <summary>
        /// Logs an error
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void LogError(object message)
        {
            ServerLogger.LogError(message);
        }
        /// <summary>
        /// Logs an exception
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void LogException(object message)
        {
            ServerLogger.LogException(message);
        }
        #endregion Logging Methods

        /// <summary>
        /// Shutdowns the server
        /// </summary>
        public static void Shutdown()
        {
            World._worldThread?.Abort();
            playerDataThread?.Abort();
            snapshotThread?.Abort();
            _listener?.Stop();
            _listener = null;
            Running = false;
        }
        #endregion
        
        #region Private Methods

        private static void RegisterDataCallbacks()
        {
            NetObject.SetReceiveCallback(typeof(NetMessage), TreatNetMessage);
        }

        private static void TreatNetMessage(NetObject obj)
        {
            NetMessage msg = obj as NetMessage;

            Console.ForegroundColor = msg.Color;
            Console.WriteLine(msg.Text);
            Console.ResetColor();
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

        private static Stopwatch TickTime = new Stopwatch();
        private static void Snapshot()
        {
            int frame = 0;
            while (Running)
            {
                try
                {
                    KeyValuePair<NetObject,Player>[] data = _receivedData.ToArray();
                    _receivedData.Clear();

                    for (int i = 0; i < data.Length; i++)
                    {
                        Player player = data[i].Value;
                        NetObject obj = data[i].Key;

                        Vector3Int p = player.Position;
                        
                        if (obj is NetInput input)
                        {
                            //Console.WriteLine(input.Key);
                            

                            switch (input.Key)
                            {
                                case ConsoleKey.Z:
                                    p = new Vector3Int(p.X, p.Y, p.Z + 1);
                                    break;
                                case ConsoleKey.S:
                                    p = new Vector3Int(p.X, p.Y, p.Z - 1);
                                    break;
                                case ConsoleKey.D:
                                    p = new Vector3Int(p.X + 1, p.Y, p.Z);
                                    break;
                                case ConsoleKey.Q:
                                    p = new Vector3Int(p.X - 1, p.Y, p.Z);
                                    break;
                            }

                            if(p != player.Position)
                            {
                                player.SetPosition(p);
                            }
                        }

                        else if(obj is NetMessage message)
                        {
                            message.WriteConsole();
                        }
                    }
                    TickTime.Start();
                    World.Tick();
                    TickTime.Stop();
                    //Log("Tick time : " + Math.Round(TickTime.Elapsed.TotalMilliseconds, 2) + " ms");
                    TickTime.Reset();

                    if(frame % TickRate == 0)
                        GC.Collect();

                    // Send data to player
                    SendAllEnqueuedData();
                    frame++;
                    int timeToSleep = (int) ((1.0F / TickRate * 1000.0F));

                    Thread.Sleep(timeToSleep);

                    
                }
                catch (Exception e)
                {
                    LogException(e);
                }
            } 
        }

        private static void StartPlayerLoops()
        {
            ReceiveEnteringConnexions();
        }
        private static async Task ReceiveEnteringConnexions()
        {
            while (Running)
            {
                //Log("Waiting for new client...");
                TcpClient newClient = await _listener.AcceptTcpClientAsync();
                Task.Run(() => ConnectPlayer(newClient));
            }
        }
        private static async Task ConnectPlayer(TcpClient client)
        {
            // Receive handshake
            NetObject firstObject = GetData(client.Client);
            firstObject.Receive();

            if (firstObject is NetPlayerHandshake handshake)
            {
                Player newPlayer = null;
                
                //TODO: Use web API to access the token
                
                
                /*if (UseAuth)
                {

                    if (handshake.Token == "")
                    {
                        LogWarning("Kicked new player: player has no token");
                        client.Close();
                    }
                    string connectionString = "URI=file:User.db";
                    string sql = 
                        $"select * from Player inner join Token on Player.NumTokenPlayer = Token.NumToken where Token.AuthToken = '" + handshake.Token + "'";
                    
                    //Request from authentication servers
                    SqliteConnection dbcon = new SqliteConnection(connectionString);
                    dbcon.Open();
                    SqliteCommand dbcmd = new SqliteCommand(sql, dbcon);
                    SqliteDataReader reader = dbcmd.ExecuteReader();

                    // if results
                    if (reader.HasRows)
                    {
                        reader.Read();
                        
                        string guid = reader.GetString(2);
                        string nick = reader.GetString(3);
                        
                        Guid playerguid = Guid.Parse(guid);

                        AuthInfo infos = new AuthInfo(nick, playerguid);
                        
                        newPlayer = Player.CreateAuthenticated(infos, client);
                    }

                    else
                    {
                        LogWarning("Kicked new player: bad token");
                        client.Close();
                    }
                }

                else
                {*/
                    newPlayer = Player.CreateOffline(handshake.Username, client);
                //}
                 
                LogWarning(newPlayer.Nickname + " (" + newPlayer.Identifier + ") connected");
                newPlayer.SetPosition(0,64,0);
                /*for (int i = 0; i < 10; i++)
                {
                    NetObject.SendContent(new NetMessage("test"), client.Client);
                }*/


                foreach (Ticket ticket in Ticket._tickets)
                {
                    //Log("Sending chunk " + ticket.Position.X + "," + ticket.Position.Y);
                    DictionnaryChunk dc = ticket.Chunk.ToDictionnary();
                    NetChunk nc = new NetChunk(dc, ticket.Position);
                    newPlayer.EnqueueData(nc);
                    //NetObject.SendContent(nc, client.Client);
                }

                //Ticket.CreateTicket(newPlayer.Position.X, newPlayer.Position.Y, 31, TicketType.Player,
                //TicketPreviousDirection.None, true);

                newPlayer.EnqueueData(new NetMessage("msg", ConsoleColor.DarkCyan));
                await GatherPlayerData(newPlayer);

                LogWarning(newPlayer.Nickname + " disconnected");
            }

            else
            {
                LogError("Kicked player : first data not handshake");
                client.Close();
            }
        }
        private static async Task GatherPlayerData(Player player)
        {
            try
            {
                while (Running)
                {
                    NetObject obj = await GetDataAsync(player.TCP.Client);
                    _receivedData.Add(obj, player);
                }
            }

            catch (Exception e)
            {
                LogError(e);
                player.Disconnect();
            }
        }
        private static NetObject GetData(Socket client)
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

        private static void SendAllEnqueuedData()
        {
            Player[] connectedPlayers = _connectedPlayers.ToArray();
            
            for (int i = 0; i < connectedPlayers.Length; i++)
            {
                Task.Run(connectedPlayers[i].SendEnqueuedData);
            }
        }

        #endregion
    }
}