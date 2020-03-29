﻿using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Isocrash.Net
{
    /// <summary>
    /// Message structure meant to be used on TCP-IP.
    /// </summary>
    [Serializable]
    public class NetMessage : NetObject
    {
        /// <summary>
        /// The color of the message
        /// </summary>
        public ConsoleColor Color
        {
            get => _color;
            set => _color = value;
        }
        private ConsoleColor _color;
        /// <summary>
        /// The text of the message
        /// </summary>
        public string Text
        {
            get => _text;
            set => _text = value;
        }
        private string _text;
        /// <summary>
        /// Message structure meant to be used on TCP-IP.
        /// </summary>
        /// <param name="text">The text</param>
        public NetMessage(string text)
        {
            this._color = ConsoleColor.White;
            this._text = text;
        }
        /// <summary>
        /// Message structure meant to be used on TCP-IP.
        /// </summary>
        /// <param name="text">The text</param>
        /// <param name="color">The color of the text</param>
        public NetMessage(string text, ConsoleColor color)
        {
            this._text = text;
            this._color = color;
        }
        
        [JsonConstructor]
        public NetMessage(ConsoleColor color, string text)
        {
            this._text = text;
            this._color = color;
        }

        public override string ToString()
        {
            return _text;
        }

        /*public override void Send(Socket socket)
        {
            string str = JsonConvert.SerializeObject(this);
            byte[] bytes = System.Text.Encoding.Unicode.GetBytes(str);
            socket.Send(bytes);
        }*/

        /*public void Send(TcpClient client)
        {
            string str = JsonConvert.SerializeObject(this);
            byte[] bytes = System.Text.Encoding.Unicode.GetBytes(str);
            client.GetStream().Write(bytes, 0, bytes.Length);
        }*/

        /// <summary>
        /// Write the message in the <see cref="System.Console"/>.
        /// </summary>
        public void WriteConsole()
        {
            Console.ForegroundColor = this._color;
            Console.WriteLine(this._text);
            Console.ResetColor();
        }

        public static implicit operator String(NetMessage msg)
        {
            return msg._text;
        }
    }
}