using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace Isocrash.Net
{
    public static class Formatter
    {
        private static BinaryFormatter _formatter = new BinaryFormatter();

        public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
            {TypeNameHandling = TypeNameHandling.All};
        
        public static void BinarySerialize(Stream stream, object graph)
        {
            _formatter.Serialize(stream, graph);
        }

        public static NetObject Deserialize(byte[] data)
        {
            NetObject obj = null;
            using (MemoryStream ms = new MemoryStream())
            {
                obj = (NetObject)_formatter.Deserialize(ms);
            }

            return obj;
        }
    }
}