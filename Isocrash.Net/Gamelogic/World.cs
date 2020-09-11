using System;
using System.Threading;

namespace Isocrash.Net.Gamelogic
{
    public static class World
    {
        private static Random _generationRandom = new Random();
        public static Random WorldRandom = new Random();
        public static int Seed { get; private set; }
        public static uint StartRadius = 37-22;
        public const uint StartLevel = 22;

        public static string SavePath { get; private set; }

        internal static Thread _worldThread;

        internal static bool StartGenerationDone = false;
        
        public static void SetWorldSeed(int seed)
        {
            Seed = seed;
            _generationRandom = new Random(seed);
        }

        public static void StartDebug()
        {
            _worldThread = new Thread(Start);
            _worldThread.Start();
        }

        private static void Start()
        {
            SavePath = FileSystem.GetSpecialDirectory(Folder.Save).FullName;
            Ticket.CreateTicket(0, 0, StartLevel, TicketType.Start, TicketPreviousDirection.None, true);
            StartGenerationDone = true;
        }

        internal static void Tick()
        {
            Ticket[] tickets = Ticket._tickets.ToArray();

            for (int i = 0; i < tickets.Length; i++)
            {
                tickets[i].Tick();
            }
        } 
    }
}