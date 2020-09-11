using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Isocrash.Net.Gamelogic
{
    public static class Generator
    {
        public static short[,,] GetChunk(int x, int y, out bool generated)
        {
            string fileName = FileSystem.GetSpecialDirectory(Folder.Save).FullName + $"c{x}_{y}.json";
           
            if (File.Exists(fileName))
            {
                generated = false;
                return LoadFromSave(fileName);
            }
            else
            {
                generated = true;
                return Generate(x, y);
            }
            
        }

        private static short[,,] Generate(int chunkx, int chunky)
        {
            short[,,] blocks = new short[Chunk.Width, Chunk.Height, Chunk.Depth];

            for (int z = 0; z < Chunk.Depth; z++)
            {
                for (int y = 0; y < Chunk.Height; y++)
                {
                    for (int x = 0; x < Chunk.Width; x++)
                    {
                        string id = "isocrash:air";

                        if (y < 64)
                        {
                            if (y == 63)
                            {
                                id = "isocrash:grass";
                            }
                            else if (y > 59)
                            {
                                id = "isocrash:dirt";
                            }
                            else
                            {
                                id = "isocrash:stone";
                            }

                            if (y == 2)
                            {
                                if(World.WorldRandom.NextDouble() < 0.33D)
                                {
                                    id = "isocrash:bedrock";
                                }
                            }
                            if (y == 1)
                            {
                                if (World.WorldRandom.NextDouble() < 0.66D)
                                {
                                    id = "isocrash:bedrock";
                                }
                            }
                            if (y == 0)
                            {
                                id = "isocrash:bedrock";
                            }
                        }

                        //Server.Log(id);
                        blocks[x,y,z] = ItemCache.GetCacheIndex(id);//new Block(id);
                    }
                }
            }

            return blocks;
        }

        private static short[,,] LoadFromSave(string path)
        {
            //string json = File.ReadAllText(path);

            JsonSerializer serializer = new JsonSerializer();
            using (StreamReader sr = File.OpenText(path))
            using (JsonTextReader jtr = new JsonTextReader(sr))
            {
                DictionnaryChunk dc = (DictionnaryChunk)serializer.Deserialize(jtr, typeof(DictionnaryChunk));

                short[,,] blocks = new short[Chunk.Width, Chunk.Height, Chunk.Depth];
                int chunkindex = 0;

                for (int z = 0; z < Chunk.Depth; z++)
                {
                    for (int y = 0; y < Chunk.Height; y++)
                    {
                        for (int x = 0; x < Chunk.Width; x++)
                        {
                            blocks[x,y,z] = /*new Block(*/ItemCache.GetCacheIndex(dc.Dictionnary[dc.References[chunkindex]])/*)*/;
                            chunkindex++;
                        }
                    }
                }

                return blocks;
            }
        }

        private static void IndexTo3D(int i, int sizex, int sizey, out int x, out int y, out int z)
        {
            x = i % sizex;
            y = (i / sizex) % sizey;
            z = i / (sizex * sizey);
        }
    }
}
