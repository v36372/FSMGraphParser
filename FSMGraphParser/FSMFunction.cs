///<author>Emad Barsoum</author>
///<email>ebarsoum@msn.com</email>
///<date>March 23, 2002</date>
///<copyright>
///This code is Copyright to Emad Barsoum, it can be used or changed for free without removing the header
///information which is the author name, email and date or refer to this information if any change made. 
///</copyright>
///<summary>
///This class <c>Function</c> use the transformation from infix notation to postfix notation to evalute most
///Mathematic expression, it support most operators (+,-,*,/,%,^), functions from 0 to any number of parameters
///and also a user defined function by using delegate, also it support variables in the expression, it will
///generate a symbol table that can be updated at run time.
///</summary>

using System;
using System.Collections;
using System.Collections.Generic;

namespace EB.Math
{
    /// <summary>
    /// </summary>
    /// 
    public enum Type { Variable, Value, Operator, Function, Result, Bracket, Comma, Error, Enum }
    public struct Symbol
    {
        public string m_name;
        public double m_value;
        public Type m_type;
        public override string ToString()
        {
            return m_name;
        }
    }
    public delegate Symbol EvaluateFunctionDelegate(string name, params object[] args);
    public delegate object EvaluateVariableDelegate(string name);
    public class Function
    {
        public double Result
        {
            get
            {
                return m_result;
            }
        }

        public List<Symbol> Equation
        {
            get
            {
                return m_equation;
            }
        }
        public List<Symbol> Postfix
        {
            get
            {
                return new List<Symbol>(m_postfix);
            }
        }

        public EvaluateFunctionDelegate DefaultFunctionEvaluation
        {
            set
            {
                m_defaultFunctionEvaluation = value;
            }
        }

        public EvaluateVariableDelegate DefaultVariableEvaluation
        {
            set
            {
                m_defaultVariableEvaluation = value;
            }
        }

        public bool Error
        {
            get
            {
                return m_bError;
            }
        }

        public string ErrorDescription
        {
            get
            {
                return m_sErrorDescription;
            }
        }

        public ArrayList Variables
        {
            get
            {
                ArrayList var = new ArrayList();
                foreach (Symbol sym in m_equation)
                {
                    if ((sym.m_type == Type.Variable) && (!var.Contains(sym)))
                        var.Add(sym);
                }
                return var;
            }
            set
            {
                foreach (Symbol sym in value)
                {
                    for (int i = 0; i < m_postfix.Count; i++)
                    {
                        if ((sym.m_name == ((Symbol)m_postfix[i]).m_name) && (((Symbol)m_postfix[i]).m_type == Type.Variable))
                        {
                            Symbol sym1 = (Symbol)m_postfix[i];
                            sym1.m_value = sym.m_value;
                            m_postfix[i] = sym1;
                        }
                    }
                }
            }
        }

        public Function()
        {
            m_nullSym = new Symbol();
            m_nullSym.m_name = "null";
            m_nullSym.m_type = Type.Value;
            m_nullSym.m_value = 0.0;
        }

        public void UpdatePosfix(List<Symbol> data)
        {
            m_postfix.Clear();
            for (int i = 0; i < data.Count; i++)
            {
                m_postfix.Add(data[i]);
            }
        }

        public void Parse(string equation)
        {
            int state = 1;
            string temp = "";
            Symbol ctSymbol;

            m_bError = false;
            m_sErrorDescription = "None";

            m_equation.Clear();
            m_postfix.Clear();

            int nPos = 0;
            //-- Remove all white spaces from the equation string --
            equation = equation.Trim();
            while ((nPos = equation.IndexOf(' ')) != -1)
                equation = equation.Remove(nPos, 1);

            for (int i = 0; i < equation.Length; i++)
            {
                switch (state)
                {
                    case 1:
                        if (Char.IsNumber(equation[i]))
                        {
                            state = 2;
                            temp += equation[i];
                        }
                        else if (Char.IsLetter(equation[i]))
                        {
                            state = 3;
                            temp += equation[i];
                        }
                        else
                        {
                            ctSymbol.m_name = equation[i].ToString();
                            ctSymbol.m_value = 0;
                            switch (ctSymbol.m_name)
                            {
                                case ",":
                                    ctSymbol.m_type = Type.Comma;
                                    break;
                                case "(":
                                case ")":
                                case "[":
                                case "]":
                                case "{":
                                case "}":
                                    ctSymbol.m_type = Type.Bracket;
                                    break;
                                default:
                                    ctSymbol.m_type = Type.Operator;
                                    break;
                            }
                            m_equation.Add(ctSymbol);
                        }
                        break;
                    case 2:
                        if ((Char.IsNumber(equation[i])) || (equation[i] == '.'))
                            temp += equation[i];
                        else if (!Char.IsLetter(equation[i]))
                        {
                            state = 1;
                            ctSymbol.m_name = temp;
                            ctSymbol.m_value = Double.Parse(temp);
                            ctSymbol.m_type = Type.Value;
                            m_equation.Add(ctSymbol);
                            ctSymbol.m_name = equation[i].ToString();
                            ctSymbol.m_value = 0;
                            switch (ctSymbol.m_name)
                            {
                                case ",":
                                    ctSymbol.m_type = Type.Comma;
                                    break;
                                case "(":
                                case ")":
                                case "[":
                                case "]":
                                case "{":
                                case "}":
                                    ctSymbol.m_type = Type.Bracket;
                                    break;
                                default:
                                    ctSymbol.m_type = Type.Operator;
                                    break;
                            }
                            m_equation.Add(ctSymbol);
                            temp = "";
                        }
                        break;
                    case 3:
                        if (Char.IsLetterOrDigit(equation[i]) || equation[i] == '_')
                            temp += equation[i];
                        else
                        {
                            state = 1;
                            ctSymbol.m_name = temp;
                            ctSymbol.m_value = 0;
                            if (equation[i] == '(')
                                ctSymbol.m_type = Type.Function;
                            else if (equation[i] == ':')
                            {
                                ctSymbol.m_name = FSMGraphParser.RegisteredEnum.GetEnumType(temp);
                                string tmp = "";
                                i++;

                                while (Char.IsLetterOrDigit(equation[i]) || equation[i] == '_')
                                {
                                   tmp += equation[i];
                                    i++;
                                }
                                ctSymbol.m_type = Type.Enum;
                                ctSymbol.m_value = FSMGraphParser.RegisteredEnum.GetEnumKey(temp, tmp);
                            }
                            else
                            {
                                if (ctSymbol.m_name == "pi")
                                    ctSymbol.m_value = System.Math.PI;
                                else if (ctSymbol.m_name == "e")
                                    ctSymbol.m_value = System.Math.E;
                                ctSymbol.m_type = Type.Variable;
                            }
                            m_equation.Add(ctSymbol);
                            ctSymbol.m_name = equation[i].ToString();
                            ctSymbol.m_value = 0;
                            switch (ctSymbol.m_name)
                            {
                                case ",":
                                    ctSymbol.m_type = Type.Comma;
                                    break;
                                case "(":
                                case ")":
                                case "[":
                                case "]":
                                case "{":
                                case "}":
                                    ctSymbol.m_type = Type.Bracket;
                                    break;
                                default:
                                    ctSymbol.m_type = Type.Operator;
                                    break;
                            }
                            m_equation.Add(ctSymbol);
                            temp = "";
                        }
                        break;
                }
            }
            if (temp != "")
            {
                ctSymbol.m_name = temp;
                if (state == 2)
                {
                    ctSymbol.m_value = Double.Parse(temp);
                    ctSymbol.m_type = Type.Value;
                }
                else
                {
                    if (ctSymbol.m_name == "pi")
                        ctSymbol.m_value = System.Math.PI;
                    else if (ctSymbol.m_name == "e")
                        ctSymbol.m_value = System.Math.E;  
                    else
                        ctSymbol.m_value = 0;
                    ctSymbol.m_type = Type.Variable;
                }
                m_equation.Add(ctSymbol);
            }

            // Add Dummy Value for Non-Paramater Function
            for (int i = 0; i < m_equation.Count - 1; i++)
            {
                if (m_equation[i].m_name == "(" && m_equation[i + 1].m_name == ")")
                {
                    Symbol tmp = new Symbol();
                    tmp.m_type = Type.Value;
                    tmp.m_name = "null";
                    tmp.m_value = 0;
                    m_equation.Insert(i + 1, tmp);
                }
            }
        }

        public void Infix2Postfix()
        {
            Symbol tpSym;
            tpStack.Clear();
            foreach (Symbol sym in m_equation)
            {
                if ((sym.m_type == Type.Value) || (sym.m_type == Type.Variable) || (sym.m_type == Type.Enum))
                    m_postfix.Add(sym);
                else if ((sym.m_name == "(") || (sym.m_name == "[") || (sym.m_name == "{"))
                    tpStack.Push(sym);
                else if ((sym.m_name == ")") || (sym.m_name == "]") || (sym.m_name == "}"))
                {
                    if (tpStack.Count > 0)
                    {
                        tpSym = tpStack.Pop();
                        while ((tpSym.m_name != "(") && (tpSym.m_name != "[") && (tpSym.m_name != "{"))
                        {
                            m_postfix.Add(tpSym);
                            tpSym = tpStack.Pop();
                        }
                    }
                }
                else
                {
                    if (tpStack.Count > 0)
                    {
                        tpSym = tpStack.Pop();
                        while ((tpStack.Count != 0) && ((tpSym.m_type == Type.Operator) || (tpSym.m_type == Type.Function) || (tpSym.m_type == Type.Comma)) && (Precedence(tpSym) >= Precedence(sym)))
                        {
                            m_postfix.Add(tpSym);
                            tpSym = tpStack.Pop();
                        }
                        if (((tpSym.m_type == Type.Operator) || (tpSym.m_type == Type.Function) || (tpSym.m_type == Type.Comma)) && (Precedence(tpSym) >= Precedence(sym)))
                            m_postfix.Add(tpSym);
                        else
                            tpStack.Push(tpSym);
                    }
                    tpStack.Push(sym);
                }
            }
            while (tpStack.Count > 0)
            {
                tpSym = tpStack.Pop();
                m_postfix.Add(tpSym);
            }
        }

        public void EvaluatePostfix()
        {
            Symbol tpSym1, tpSym2, tpResult;
            Symbol sym;
            tpStack.Clear();

            fnParam.Clear();
            m_bError = false;
            for (int i = 0; i < m_postfix.Count; i++)
            {
                sym = m_postfix[i];
                if ((sym.m_type == Type.Value)
                    || (sym.m_type == Type.Variable)
                    || (sym.m_type == Type.Result)
                    || (sym.m_type == Type.Comma)
                    || (sym.m_type == Type.Enum))
                    tpStack.Push(sym);
                else if (sym.m_type == Type.Operator)
                {
                    if (tpStack.Count > 0)
                        tpSym1 = tpStack.Pop();
                    else
                        tpSym1 = m_nullSym;

                    if (tpStack.Count > 0 && sym.m_name != "!")
                        tpSym2 = tpStack.Pop();
                    else
                        tpSym2 = m_nullSym;
                    tpResult = Evaluate(tpSym2, sym, tpSym1);
                    if (tpResult.m_type == Type.Error)
                    {
                        m_bError = true;
                        m_sErrorDescription = tpResult.m_name;
                        return;
                    }
                    tpStack.Push(tpResult);
                }
                else if (sym.m_type == Type.Function)
                {
                    fnParam.Clear();
                    if (tpStack.Count > 0)
                    {
                        tpSym1 = tpStack.Pop();
                        if ((tpSym1.m_type == Type.Value) || (tpSym1.m_type == Type.Variable) || (tpSym1.m_type == Type.Result) || (tpSym1.m_type == Type.Enum))
                        {
                            tpResult = EvaluateFunction(sym.m_name, tpSym1);
                            if (tpResult.m_type == Type.Error)
                            {
                                m_bError = true;
                                m_sErrorDescription = tpResult.m_name;
                                return;
                            }
                            tpStack.Push(tpResult);
                        }
                        else if (tpSym1.m_type == Type.Comma)
                        {
                            while (tpSym1.m_type == Type.Comma)
                            {
                                tpSym1 = tpStack.Pop();
                                fnParam.Add(tpSym1);
                                tpSym1 = tpStack.Pop();
                            }
                            fnParam.Add(tpSym1);
                            object[] param = new object[fnParam.Count];
                            for (int j = 0; j < fnParam.Count; j++)
                            {
                                param[j] = fnParam[j];
                            }

                            tpResult = EvaluateFunction(sym.m_name, param);
                            if (tpResult.m_type == Type.Error)
                            {
                                m_bError = true;
                                m_sErrorDescription = tpResult.m_name;
                                return;
                            }
                            tpStack.Push(tpResult);
                        }
                        else
                        {
                            tpStack.Push(tpSym1);
                            tpResult = EvaluateFunction(sym.m_name);
                            if (tpResult.m_type == Type.Error)
                            {
                                m_bError = true;
                                m_sErrorDescription = tpResult.m_name;
                                return;
                            }
                            tpStack.Push(tpResult);
                        }
                    }
                    else
                    {
                        tpResult = EvaluateFunction(sym.m_name);
                        if (tpResult.m_type == Type.Error)
                        {
                            m_bError = true;
                            m_sErrorDescription = tpResult.m_name;
                            return;
                        }
                        tpStack.Push(tpResult);
                    }
                }
            }
            if (tpStack.Count == 1)
            {
                tpResult = tpStack.Pop();
                if (tpResult.m_type == Type.Variable)
                {
                    tpResult.m_value = EvaluateVariable(tpResult.m_name);
                }
                if (OnResult != null) OnResult(tpResult.m_name);
                m_result = tpResult.m_value;
            }
        }

        protected int Precedence(Symbol sym)
        {
            switch (sym.m_type)
            {
                case Type.Bracket:
                    return 5;
                case Type.Function:
                    return 4;
                case Type.Comma:
                    return 0;
            }
            switch (sym.m_name)
            {
                case "^":
                    return 3;
                case "/":
                case "*":
                case "%":
                    return 2;
                case "+":
                case "-":
                case "!":
                    return 1;
                case "&":
                case "|":
                case "=":
                    return 0;
            }
            return -1;
        }

        protected double EvaluateVariable(string name)
        {
            object res = m_defaultVariableEvaluation(name);
            Console.WriteLine("[VariableEvaluation] {0} -> {1}", name, res);
            if (res == null || res.GetType() != typeof(double))
                throw new Exception("[FSMFunction][Evaluate] m_defaultVariableEvaluation Should be return double type for " + name);

            if (OnVariableEval != null)
            {
                OnVariableEval(name);
            }
            return (double)res;
        }

        protected Symbol Evaluate(Symbol sym1, Symbol opr, Symbol sym2)
        {
            //UnityEngine.Debug.LogWarning("Evaluate");
            Symbol result;
            result.m_name = sym1.m_name + opr.m_name + sym2.m_name;
            result.m_type = Type.Result;
            result.m_value = 0;

            if (m_defaultVariableEvaluation != null)
            {
                if (sym1.m_type == Type.Variable)
                {
                    sym1.m_value = EvaluateVariable(sym1.m_name);
                }

                if (sym2.m_type == Type.Variable)
                {
                    sym2.m_value = EvaluateVariable(sym2.m_name);
                }

                if (sym1.m_type == Type.Enum)
                {
                    sym1.m_value = EvaluateVariable(sym1.m_name);
                }

                if (sym2.m_type == Type.Enum)
                {
                    sym2.m_value = EvaluateVariable(sym2.m_name);
                }
            }

            if (OnOperator != null && opr.m_name != ":")
            {
                OnOperator(opr.m_name, sym1, sym2);
            }

            switch (opr.m_name)
            {
                case "^":
                    result.m_value = System.Math.Pow(sym1.m_value, sym2.m_value);
                    break;
                case "/":
                    {
                        if (sym2.m_value > 0)
                            result.m_value = sym1.m_value / sym2.m_value;
                        else
                        {
                            result.m_name = "Divide by Zero.";
                            result.m_type = Type.Error;
                        }
                        break;
                    }
                case "*":
                    result.m_value = sym1.m_value * sym2.m_value;
                    break;
                case "%":
                    result.m_value = sym1.m_value % sym2.m_value;
                    break;
                case "+":
                    result.m_value = sym1.m_value + sym2.m_value;
                    break;
                case "-":
                    result.m_value = sym1.m_value - sym2.m_value;
                    break;
                case ">":
                    if (sym1.m_value > sym2.m_value)
                    {
                        result.m_value = 1;
                    }
                    else
                    {
                        result.m_value = 0;
                    }
                    break;
                case "<":
                    if (sym1.m_value < sym2.m_value)
                    {
                        result.m_value = 1;
                    }
                    else
                    {
                        result.m_value = 0;
                    }
                    break;
                case "=":
                    if (sym1.m_value == sym2.m_value)
                    {
                        result.m_value = 1;
                    }
                    else
                    {
                        result.m_value = 0;
                    }
                    break;
                case "|":
                    if ((int)sym1.m_value == 1 || (int)sym2.m_value == 1)
                    {
                        result.m_value = 1;
                    }
                    else
                    {
                        result.m_value = 0;
                    }
                    break;
                case "&":
                    if ((int)sym1.m_value == 1 && (int)sym2.m_value == 1)
                    {
                        result.m_value = 1;
                    }
                    else
                    {
                        result.m_value = 0;
                    }
                    break;
                case "!":
                    if ((int)sym2.m_value == 1)
                    {
                        result.m_value = 0;
                    }
                    else
                    {
                        result.m_value = 1;
                    }
                    break;
                default:
                    result.m_type = Type.Error;
                    result.m_name = "Undefine operator: " + opr.m_name + ".";
                    break;
            }
            return result;
        }

        protected Symbol EvaluateFunction(string name, params Object[] args)
        {
            Symbol result;
            result.m_name = "";
            result.m_type = Type.Result;
            result.m_value = 0;
            for (int i = 0; i < args.Length; i++)
            {
                Symbol sym = (Symbol)args[i];
                if (sym.m_type == Type.Variable)
                {
                    sym.m_value = EvaluateVariable(sym.m_name);
                    args[i] = sym;
                }
            }
            if (m_defaultFunctionEvaluation != null)
                result = m_defaultFunctionEvaluation(name, args);
            else
            {
                result.m_name = "Function: " + name + ", not found.";
                result.m_type = Type.Error;
            }
            if (OnFunctionEval != null)
            {
                OnFunctionEval(name, args);
            }
            return result;
        }

        protected bool m_bError = false;
        protected string m_sErrorDescription = "None";
        protected double m_result = 0;
        protected List<Symbol> m_equation = new List<Symbol>();
        protected List<Symbol> m_postfix = new List<Symbol>();
        List<Symbol> fnParam = new List<Symbol>();
        protected EvaluateFunctionDelegate m_defaultFunctionEvaluation;
        protected EvaluateVariableDelegate m_defaultVariableEvaluation;

        private Stack<Symbol> tpStack = new Stack<Symbol>();
        private Symbol m_nullSym;

        public event Action<string, object[]> OnFunctionEval;
        public event Action<string> OnVariableEval;
        public event Action<string, Symbol, Symbol> OnOperator;
        public event Action<string> OnResult;
    }
}