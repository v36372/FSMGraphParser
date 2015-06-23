using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMGraphParser
{
    static class RegisteredEnum
    {
        public  enum EnumList
        {
            VirtualKey,
            State
        };

        public  enum VirtualKey
        {
            P = 80,
            U = 85,
            R = 82,
            S = 83
        };

        public  enum State
        {
            Idle = 1,
            Play = 2,
            Pause = 3,
            Stop = 4,
            Resume = 5,
            AnyState = 6
        };
        public static string GetEnumType(string EnumType)
        {
            return Convert.ToString((int)Enum.Parse(typeof(EnumList), EnumType));
        }

        public static double GetEnumKey(string EnumType, string EnumKey)
        {
            if(EnumType == "VirtualKey")
            {
                return Convert.ToDouble(Enum.Parse(typeof(VirtualKey), EnumKey));
            }
            else if(EnumType == "State")
            {
                return Convert.ToDouble(Enum.Parse(typeof(State), EnumKey));
            }

            return 0.0;
        }
        
    }
}
