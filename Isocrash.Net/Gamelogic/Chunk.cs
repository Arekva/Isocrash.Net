using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Isocrash.Net.Gamelogic
{
    public class Chunk
    {
        public short this[int x, int y, int z]
        {
            get { return _blocks[x, y, z]; }

            set { _blocks[x, y, z] = value; }
        }
        public const int ChunkTotalBlocks = 65536;

        public const int RandomTickSpeed = 3;

        public const int Height = 256;
        public const int Width = 16;
        public const int Depth = 16;

        private short[,,] _blocks;

        public Ticket Ticket { get; private set; }

        public Chunk(Ticket ticket, short[,,] blocksCacheIndexes)
        {
            this.Ticket = ticket;
            this._blocks = blocksCacheIndexes;
        }

        public int Get1DIndex(int x, int y, int z)
        {
            return x + Width * y + Width * Height * z;
        }

        public void Tick()
        {
            for (int i = 0; i < ChunkTotalBlocks; i+= ChunkTotalBlocks / 16)
            {
                for (int j = 0; j < RandomTickSpeed; j++)
                {
                    //_blocks[World.WorldRandom.Next(i, ChunkTotalBlocks)].Tick();
                }
            }
        }

        public void Unload()
        {
            this._blocks = null;
            this.Ticket = null;
        }

        /*private string LinearCompress()
        {
            // Don't directly store the strings of the blocks 

            List<string> distinctIDs = new List<string>();
            List<LinearCompressedReference> blocksRef = new List<LinearCompressedReference>();
            //LinearCompressedReference[] blocksRef = new LinearCompressedReference[ChunkTotalBlocks];

            // for each sections
            int chunkIndex = 0;

            int blockStreak = 1;
            string streakID = "null";
            for (int i = 0; i < _sections.Length; i++)
            {
                Block[] blocks = _sections[i]._blocks;
                for (int j = 0; j < blocks.Length; j++)
                {
                    string id = blocks[j].Identifier;

                    if (id == streakID)
                    {
                        blockStreak++;
                    }

                    else
                    {
                        if (!distinctIDs.Contains(id))
                        {
                            distinctIDs.Add(id);
                        }

                        if (streakID != "null")
                        {
                            blocksRef.Add(new LinearCompressedReference(blockStreak, distinctIDs.IndexOf(id)));
                        }

                        streakID = id;
                        blockStreak = 1;
                    }

                    //if last
                    if (chunkIndex == ChunkTotalBlocks - 1)
                    {
                        blocksRef.Add(new LinearCompressedReference(blockStreak, distinctIDs.IndexOf(id)));
                    }

                    chunkIndex++;
                }
            }

            LinearDictionnaryChunk saveableChunk = new LinearDictionnaryChunk(distinctIDs.ToArray(), blocksRef.ToArray());

            /*byte[] data;
            using(MemoryStream ms = new MemoryStream())
            {
                using(BsonDataWriter writer = new BsonDataWriter(ms))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, saveableChunk);
                }

                data = ms.ToArray();
            }
            return Convert.ToBase64String(data);*/

            
            
            
            /*return JsonConvert.SerializeObject(saveableChunk, Formatting.None);
        }*/

        private string NoCompress()
        {
           return JsonConvert.SerializeObject(this.ToDictionnary(), Formatting.None);
        }

        public string ToJSON()
        {
            return this.NoCompress();
        }

        internal DictionnaryChunk ToDictionnary()
        {
            List<string> distinctIDs = new List<string>(ChunkTotalBlocks);
            int[] blocksRef = new int[ChunkTotalBlocks];

            int chunkIndex = 0;

            for (int z = 0; z < Chunk.Depth; z++)
            {
                for (int y = 0; y < Chunk.Height; y++)
                {
                    for (int x = 0; x < Chunk.Width; x++)
                    {
                        string id = ItemCache.Get(_blocks[x,y,z]).Identifier;//_blocks[i].Identifier;

                        if (!distinctIDs.Contains(id))
                        {
                            distinctIDs.Add(id);
                        }

                        blocksRef[chunkIndex] = distinctIDs.IndexOf(id);
                        chunkIndex++;
                    }
                }
            }


            return new DictionnaryChunk(distinctIDs.ToArray(), blocksRef);
        }
    }
}