using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JFSM
{
    public class Command
    {
        public OpCode opCode;
        public List<int> Args;
        public int OutputVariableIndex;
        public string DataName;

        public string GetString()
        {
            StringBuilder res = new StringBuilder();
            res.Append(((int)opCode).ToString());
            res.Append(",");
            res.Append(DataName);
            res.Append(",");
            res.Append(OutputVariableIndex);
            res.Append(",");
            if (Args != null)
            {
                res.Append(Args.Count);
                for (int i = 0; i < Args.Count; i++)
                {
                    res.Append(",");
                    res.Append(Args[i]);
                }
            }
            else
            {
                res.Append(0);
            }
            return res.ToString();
        }

        public void LoadString(string data)
        {
            string[] tmp = data.Split(',');
            int index = 0;
            this.opCode = (OpCode)int.Parse(tmp[index++]);
            this.DataName = tmp[index++];
            this.OutputVariableIndex = int.Parse(tmp[index++]);

            int count = int.Parse(tmp[index++]);
            if (count >= 0)
            {
                this.Args = new List<int>();
                for (int i = 0; i < count; i++)
                {
                    this.Args.Add(int.Parse(tmp[index + i]));
                }
            }
            else
            {
                this.Args = null;
            }
        }
    }
    public enum OpCode
    {
        OpVariableEval,
        OpFunctionEval,
        OpLT,
        OpGT,
        OpAdd,
        OpSub,
        OpMul,
        OpDiv,
        OpPow,
        OpEqual,
        OpAnd,
        OpOr,
        OpNot,
        OpConstant,
        OpVariable,
        opResult,
        OpEnum
    }
}