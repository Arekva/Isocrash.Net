﻿using Newtonsoft.Json;

namespace Isocrash.Net
{
    public class NetPlayer : NetObject
    {
        public string UserName
        {
            get => _userName;
            set => _userName = value;
        }

        private string _userName; 
        
        public float Xpos
        {
            get => _xpos;
            set => _xpos = value;
        }

        private float _xpos = 0;

        public float Ypos
        {
            get => _ypos;
            set => _ypos = value;
        }

        private float _ypos = 0;

        public float Zpos
        {
            get => _zpos;
            set => _zpos = value;
        }

        private float _zpos = 0;


        public float Xrot
        {
            get => _xrot;
            set => _xrot = value;
        }

        private float _xrot = 0;

        public float Yrot
        {
            get => _yrot;
            set => _yrot = value;
        }

        private float _yrot = 0;

        public float Zrot
        {
            get => _zrot;
            set => _zrot = value;
        }

        private float _zrot = 0;

        public float Wrot
        {
            get => _wrot;
            set => _wrot = value;
        }

        private float _wrot = 0;

        [JsonConstructor]
        public NetPlayer(string UserName, float xpos, float ypos, float zpos, float xrot, float yrot, float zrot, float wrot)
        {
            this._userName = UserName;
            this._xpos = xpos;
            this._ypos = ypos;
            this._zpos = zpos;
            this._xrot = xrot;
            this._yrot = ypos;
            this._zrot = zpos;
            this._wrot = wrot;
        }
    }
}