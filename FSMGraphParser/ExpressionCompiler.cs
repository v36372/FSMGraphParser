using EB.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JFSM
{
    public class ExpressionCompiler
    {
        List<Command> m_pCompileData;
        List<string> m_pVariableIndex;

        public List<Command> CompileResult
        {
            get
            {
                return m_pCompileData;
            }
        }

        public ExpressionCompiler()
        {
            m_pCompileData = new List<Command>();
            m_pVariableIndex = new List<string>();
        }

        public void Reset()
        {
            m_pVariableIndex.Clear();
            m_pCompileData.Clear();
        }

        public void Compile(string expression)
        {
            //Debug.Log(expression);
            Function func = new Function();
            func.DefaultFunctionEvaluation = DefaultFunctionEval;
            func.DefaultVariableEvaluation = DefalutVariableEval;
            func.OnFunctionEval += OnFunctionCall;
            func.OnVariableEval += OnVariableEval;
            func.OnOperator += OnOperator;
            func.OnResult += OnResult;

            func.Parse(expression);
            func.Infix2Postfix();
            func.EvaluatePostfix();
        }

        StringBuilder m_stringBuilderTemp;

        public string GetString()
        {
            // HHoang (optimize): cache StringBuilder
            if (m_stringBuilderTemp == null)
            {
                m_stringBuilderTemp = new StringBuilder();
            }

            m_stringBuilderTemp.Length = 0;
            m_stringBuilderTemp.Capacity = 0;

            for (int i = 0; i < m_pCompileData.Count; i++)
            {
                if (i != 0)
                    m_stringBuilderTemp.Append(":");
                m_stringBuilderTemp.Append(m_pCompileData[i].GetString());
            }
            return m_stringBuilderTemp.ToString();
        }

        void OnResult(string outputName)
        {
            Command res = new Command();
            res.opCode = OpCode.opResult;
            res.Args = null;
            res.OutputVariableIndex = _GetVariableIndex(outputName);

            m_pCompileData.Add(res);
        }

        void OnOperator(string op, Symbol name1, Symbol name2)
        {
            Command res = new Command();
            res.opCode = _GetOpCode(op);
            res.Args = new List<int>();
            if (name1.m_type == EB.Math.Type.Value)
            {
                res.Args.Add((int)OpCode.OpConstant);
                res.Args.Add((int)name1.m_value);
            }
            else
            {
                res.Args.Add((int)OpCode.OpVariable);
                res.Args.Add(_GetVariableIndex(name1.m_name));
            }

            if (name2.m_type == EB.Math.Type.Value)
            {
                res.Args.Add((int)OpCode.OpConstant);
                res.Args.Add((int)name2.m_value);
            }
            else
            {
                res.Args.Add((int)OpCode.OpVariable);
                res.Args.Add(_GetVariableIndex(name2.m_name));
            }

            res.OutputVariableIndex = _GetVariableIndex(name1 + op.ToString() + name2);

            m_pCompileData.Add(res);
        }

        private void OnVariableEval(string name)
        {
            Command res = new Command();
            res.opCode = OpCode.OpVariableEval;
            res.DataName = name;
            res.Args = null;
            res.OutputVariableIndex = _GetVariableIndex(name);

            m_pCompileData.Add(res);
        }

        void OnFunctionCall(string FunctionName, params object[] args)
        {
            Command res = new Command();
            res.opCode = OpCode.OpFunctionEval;
            res.DataName = FunctionName;
            res.Args = null;
            if (args.Length > 0)
            {
                res.Args = new List<int>();
                for (int i = 0; i < args.Length; i++)
                {
                    Symbol sym = (Symbol)args[i];
                    if (sym.m_type == EB.Math.Type.Variable)
                    {
                        res.Args.Add((int)OpCode.OpVariable);
                        res.Args.Add(_GetVariableIndex(sym.m_name));
                    }
                    else if (sym.m_type == EB.Math.Type.Value)
                    {
                        res.Args.Add((int)OpCode.OpConstant);
                        res.Args.Add((int)sym.m_value);
                    }
                    else if (sym.m_type == EB.Math.Type.Enum)
                    {
                        res.Args.Add((int)OpCode.OpEnum);
                        res.Args.Add((int)sym.m_value);
                    }
                }
            }

            res.OutputVariableIndex = _GetVariableIndex(FunctionName + "()");

            m_pCompileData.Add(res);
        }


        private int _GetVariableIndex(string name)
        {
            if (!m_pVariableIndex.Contains(name))
            {
                m_pVariableIndex.Add(name);
            }

            return m_pVariableIndex.IndexOf(name);
        }

        private OpCode _GetOpCode(string op)
        {
            switch (op)
            {
                case "<":
                    return OpCode.OpLT;
                case ">":
                    return OpCode.OpGT;
                case "+":
                    return OpCode.OpAdd;
                case "-":
                    return OpCode.OpSub;
                case "*":
                    return OpCode.OpMul;
                case "/":
                    return OpCode.OpDiv;
                case "^":
                    return OpCode.OpPow;
                case "=":
                    return OpCode.OpEqual;
                case "&":
                    return OpCode.OpAnd;
                case "|":
                    return OpCode.OpOr;
                case "!":
                    return OpCode.OpNot;
                default:
                    throw new Exception("_GetOpCode Not Found " + op);
            }
        }

        private static object DefalutVariableEval(string name)
        {
            return 1.0;
        }

        private static Symbol DefaultFunctionEval(string name, params object[] args)
        {
            Symbol res = new Symbol();
            res.m_type = EB.Math.Type.Result;
            res.m_name = name + "()";
            res.m_value = 1.0;
            return res;
        }

    }
}