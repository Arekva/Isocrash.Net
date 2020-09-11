using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace Isocrash.Net.Gamelogic
{
    public class Ticket : ITickable, IEquatable<Ticket>
    {
        public const uint MaxLevel = 38;
        public const uint InaccessibleLevel = 34;
        public const uint BorderLevel = 33;
        public const uint TickingLevel = 32;

        internal static List<Ticket> _tickets { get; set; } = new List<Ticket>();
        

        public uint Level { get; internal set; }
        public TicketType InvokeType { get; internal set; }
        public int LifeTime { get; private set; }

        public Vector2Int Position { get; }
        
        public Chunk Chunk { get; set; }

        public delegate void TicketCreationDelegate(Ticket created);

        public static TicketCreationDelegate OnCreation;

        public void DebugTicket()
        {
            // DEBUG
            ConsoleColor bgcolor;
            switch (this.LoadType)
            {
                case LoadType.Inaccessible:
                    bgcolor = ConsoleColor.Gray;
                    break;
                case LoadType.Border:
                    bgcolor = ConsoleColor.Cyan;
                    break;
                case LoadType.Ticking:
                    bgcolor = ConsoleColor.Yellow;
                    break;
                case LoadType.EntityTicking:
                    bgcolor = ConsoleColor.Green;
                    break;
                default: // unknown
                    bgcolor = ConsoleColor.Black;
                    break;
            }
            
            // middle
            int midx = 120/2;
            int midy = 60/2;
            Console.BackgroundColor = bgcolor;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(midx + (this.Position.X*2), midy - this.Position.Y);
            Console.Write(this.Level);
            Console.ResetColor();
        }

        public static Ticket CreateTicket(int x, int y, uint level, TicketType invokeType, TicketPreviousDirection previousDir, bool propagates = false, int lifeTime = -1)
        {
            if (level >= MaxLevel) return null;
            
            Ticket ticket = new Ticket(level, invokeType, lifeTime, new Vector2Int(x,y));

            // If already existing => edit
            if (Get(new Vector2Int(x,y)) != null)
            {
                ticket = Get(new Vector2Int(x, y));
                
                ticket.EditLevel(level, TicketEditType.Inferior);
            }

            else
            {
                _tickets.Add(ticket);
            }
            
            //  Recursively create tickets around in a square shape
            //
            //  36 36 36 36 36 36 36   a.k.a    n+3 n+3 n+3 n+3 n+3 n+3 n+3
            //  36 35 35 35 35 35 36            n+3 n+2 n+2 n+2 n+2 n+2 n+3 
            //  36 35 34 34 34 35 36            n+3 n+2 n+1 n+1 n+1 n+2 n+3
            //  36 35 34 33 34 35 36            n+3 n+2 n+1  n  n+1 n+2 n+3
            //  36 35 34 34 34 35 36            n+3 n+2 n+1 n+1 n+1 n+2 n+3
            //  36 35 35 35 35 35 36            n+3 n+2 n+2 n+2 n+2 n+2 n+3
            //  36 36 36 36 36 36 36            n+3 n+3 n+3 n+3 n+3 n+3 n+3
            //
            //  So create tickets in x+1, y+1, x-1, y-1, x+1 y+1, x-1 y-1, x+1 y-1, x-1 y+1
            //  (do not create if already exiting, edit its level if existing
            //ticket.DebugTicket();
            
            if (propagates)
            {
                // for each level until max is reached
                for (int lvl = (int)level + 1, dist = 1; lvl < MaxLevel + 2; lvl++, dist++)
                {
                    // limits
                    int minx = x - dist;
                    int maxx = x + dist;
                    int maxy = y + dist;
                    int miny = y - dist;

                    // top line
                    for (int i = minx + 1; i < maxx + 1; i++)
                    {
                        CreateTicket(i, maxy, (uint)lvl, invokeType, TicketPreviousDirection.None, false, lifeTime);
                    }

                    // right line
                    for (int i = maxy - 1; i > miny - 1; i--)
                    {
                        CreateTicket(maxx, i, (uint)lvl, invokeType, TicketPreviousDirection.None, false, lifeTime);
                    }

                    // bottom line
                    for (int i = maxx - 1; i > minx - 1; i--)
                    {
                        CreateTicket(i, miny, (uint)lvl, invokeType, TicketPreviousDirection.None, false, lifeTime);
                    }

                    // left line
                    for (int i = miny + 1; i < maxy + 1; i++)
                    {
                        CreateTicket(minx, i, (uint)lvl, invokeType, TicketPreviousDirection.None, false, lifeTime);
                    }
                }
            }
            
            return ticket;
        }

        private Ticket(uint level, TicketType invokeType, Vector2Int position)
        {
            this.Position = position;
            this.Level = level;
            this.InvokeType = invokeType;
            this.LifeTime = -1;
            Chunk = CreateChunk();
        }

        private Ticket(uint level, TicketType invokeType, int lifeTime, Vector2Int position)
        {
            this.Position = position;
            this.Level = level;
            this.InvokeType = invokeType;
            this.LifeTime = lifeTime;
            Chunk = CreateChunk();

            OnCreation?.Invoke(this);
        }

        public static Ticket Get(Vector2Int position)
        {
            foreach (Ticket t in _tickets)
            {
                if (t.Position == position)
                {
                    return t;
                }
            }

            return null;
            //return _tickets.First(ticket => ticket.Position == position);
        }

        public bool Equals(Ticket ticket)
        {
            if (ticket is Ticket)
                return this.Position == ticket.Position;
            return false;
        }

        public void EditLevel(uint newLevel, TicketEditType type = TicketEditType.Override)
        {
            uint levelToSet = this.Level;

            switch (type)
            {
                case TicketEditType.Inferior:
                {
                    if (newLevel < this.Level)
                    {
                        levelToSet = newLevel;
                    }
                }
                    break;
                case TicketEditType.Superior:
                {
                    if (newLevel > this.Level)
                    {
                        levelToSet = newLevel;
                    }
                }
                    break;
            }
            
            this.Level = levelToSet;

            //this.DebugTicket();
        }

        public LoadType LoadType
        {
            get
            {
                uint lvl = this.Level;
                if (lvl < TickingLevel) return LoadType.EntityTicking;
                if (lvl < BorderLevel) return LoadType.Ticking;
                if (lvl < InaccessibleLevel) return LoadType.Border;
                return LoadType.Inaccessible;
            }
        }

        public void Tick()
        {
            if (LifeTime == 0) return;
            
            if (LifeTime != -1) LifeTime--;

            LoadType loadType = this.LoadType;
            if (loadType == LoadType.Ticking)
            {
                Chunk.Tick();
            }
        }

        public void Unload()
        {
            // middle
            int midx = 120 / 2;
            int midy = 60 / 2;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(midx + (this.Position.X * 2), midy - this.Position.Y);
            Console.Write(this.Level);
            Console.ResetColor();

            this.Chunk.Unload();
            this.Chunk = null;
            _tickets.Remove(this);

        }

        private Chunk CreateChunk()
        {
            Chunk chunk = new Chunk(this, Generator.GetChunk(this.Position.X, this.Position.Y, out bool gen));

            if(gen)
            {
                Save(chunk, this.Position.X, this.Position.Y);
            }
            return chunk;
        }
        
        private static void Save(Chunk chunk, int x, int y)
        {
            string worldPath = World.SavePath;
            string extention = ".json";
            string fullPath = worldPath + $"/c{x}_{y}" + extention;

            File.WriteAllText(fullPath, chunk.ToJSON());
        }
        public void Save()
        {

            Save(this.Chunk, this.Position.X, this.Position.Y);

            //sw.Start();
            /*using (FileStream fs = new FileStream(fullPath, FileMode.Create))
            {
                using (GZipStream gzip = new GZipStream(fs, CompressionLevel.Optimal))
                {
                    string json = this.Chunk.ToJSON();
                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes(json);

                    gzip.Write(bytes, 0, bytes.Length);
                }
            }*/
            
            //sw.Stop();

            //Server.LogWarning("Saved chunk in " + Math.Round(sw.Elapsed.TotalMilliseconds,2) + " ms");
            //sw.Reset();
        }
    }
}