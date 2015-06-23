using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMGraphParser
{
    class Program
    {
        static void Main(string[] args)
        {
            FSMConvertGraphmltoTXT.ConvertGraphmltoTXT("SoundManagerFSM.graphml");
        }
    }
}
