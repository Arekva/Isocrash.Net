﻿using System;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Isocrash.Net
{
    /// <summary>
    /// Structure enabling Isocrash's net data exchange
    /// </summary>
    /// <typeparam name="T">NetObject to exchange</typeparam>
    [Serializable]
    public struct NetData<T> where T : NetObject
    {
        /// <summary>
        /// The serialized data type
        /// </summary>
        public Type DataType
        {
            get => _dataType;
            set => _dataType = value;
        }
        private Type _dataType;
        
        /// <summary>
        /// The serialized data
        /// </summary>
        public string Data
        {
            get => _data;
            set => _data = value;
        } 
        private string _data;

        /// <summary>
        /// Structure enabling Isocrash's net data exchange
        /// </summary>
        /// <typeparam name="T">NetObject to exchange</typeparam>
        public NetData(T dataToSend) : this()
        {
            _dataType = typeof(T);
            this._data = JsonConvert.SerializeObject(dataToSend);
        }

        public void Send(Socket socket)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(this));
            int packageSize = bytes.Length;
            socket.Send(BitConverter.GetBytes(packageSize));
            socket.Send(bytes);
        }
    }
}