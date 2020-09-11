using System;
using System.Threading;
using Isocrash.Net;
using Isocrash.Net.Gamelogic;

namespace Standalone
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.SetWindowPosition(0, 0);
            Console.SetWindowSize(120, 60);

            Server.UseAuth = false;
            Server.SetIntegrated(true);
            Server.Start("0.0.0.0", 26656);



            //Thread.Sleep(100);
            Client.SetNickname("Arekva");
            Client.Connect("127.0.0.1", 26656);

            //while(Server.Running) { }


            while (Client.Connected)
            {
                
                if(Client._receivedData.Count > 0)
                {
                    //Server._connectedPlayers.Add(this);Client.Log("data");
                    NetObject[] objs = Client._receivedData.ToArray();
                    Client._receivedData.Clear();

                    for (int i = 0; i < objs.Length; i++)
                    {
                        NetObject obj = objs[i];

                        if(obj is NetMessage msg)
                        {
                            msg.WriteConsole();
                        }
                    }
                }

                Thread.Sleep(16);
                //Client.EnqueueData(new NetInput(Console.ReadKey(true).Key));
            }
        }
    }
}