using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Isocrash.Net.Gamelogic
{
    [Serializable]
    public struct DictionnaryChunk
    {
        public string[] Dictionnary
        {
            get
            {
                return _dictionnary;
            }

            set
            {
                _dictionnary = value;
            }
        }
        private string[] _dictionnary;

        public int[] References
        {
            get
            {
                return _references;
            }

            set
            {
                _references = value;
            }
        }
        private int[] _references;

        [JsonConstructor]
        public DictionnaryChunk(string[] dictionnary, int[] references)
        {
            this._dictionnary = dictionnary;
            this._references = references;
        }
    }

    [Serializable]
    public struct LinearDictionnaryChunk
    {
        public string[] Dictionnary
        {
            get
            {
                return _dictionnary;
            }

            set
            {
                _dictionnary = value;
            }
        }
        private string[] _dictionnary;

        public LinearCompressedReference[] References
        {
            get
            {
                return _references;
            }

            set
            {
                _references = value;
            }
        }
        private LinearCompressedReference[] _references;

        [JsonConstructor]
        public LinearDictionnaryChunk(string[] dictionnary, LinearCompressedReference[] references)
        {
            this._dictionnary = dictionnary;
            this._references = references;
        }
    }
}
