using Isocrash.Net;

namespace ClientStandalone
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Client.SetNickname("Arekva");
            Client.Connect("127.0.0.0", 26656);
            
            while(true){}
        }
    }
}