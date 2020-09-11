using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isocrash.Net.Gamelogic
{
    interface ISaveable
    {
        string ToJSON();
    }
}
