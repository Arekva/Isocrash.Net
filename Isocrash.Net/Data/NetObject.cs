﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Isocrash.Net
{
    public abstract class NetObject
    {
        #region Public Attributes

        /// <summary>
        /// The delegate describing the method called when data is received
        /// </summary>
        public delegate void ReceiveCallback(NetObject data);
        #endregion
        
        #region Private Attributes
        private static Dictionary<Type, ReceiveCallback> _callbacks = new Dictionary<Type, ReceiveCallback>();
        #endregion
        

        #region Public Methods
        /// <summary>
        /// Gets the NetObject data from a JSON-Serialized NetData
        /// </summary>
        /// <param name="json"></param>
        /// <returns>Returns the obtained object</returns>
        public static NetObject GetContent(string json)
        {
            NetData<NetObject> obj = // Converts to NetData
                JsonConvert.DeserializeObject<NetData<NetObject>>(json);

            return // Converts to NetData
                JsonConvert.DeserializeObject(obj.Data, obj.DataType) as NetObject;
            
        }
        
        /// <summary>
        /// Send data to a distant connection
        /// </summary>
        /// <param name="data">The wanted data as NetObject</param>
        /// <param name="socket">The distant socket</param>
        public static void SendContent(NetObject data, Socket socket)
        {
            if (data == null)
                return;
            
            Type type = data.GetType();
            Type constructed = typeof(NetData<>).MakeGenericType(type);
            ConstructorInfo ctor = constructed.GetConstructor(new Type[]{type});
            dynamic d = ctor.Invoke(new object[] { data });
            d.Send(socket);
        }

        /// <summary>
        /// Invokes the registered callback for the data type
        /// </summary>
        public void Receive()
        {
            Type type = this.GetType();

            if (_callbacks.TryGetValue(this.GetType(), out ReceiveCallback callback))
            {
                callback.Invoke(this);
            }
            
            // otherwise, no callback
        }

        public static void SetReceiveCallback(Type type, ReceiveCallback method)
        { 
            // if exists, replace
            if (_callbacks.ContainsKey(type))
                _callbacks[type] = method;
            
            else // otherwise create
                _callbacks.Add(type, method);
            
        }
        #endregion
    }
}