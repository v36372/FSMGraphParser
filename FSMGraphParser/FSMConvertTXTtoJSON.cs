using System.Collections;
using System.Collections.Generic;
using EB.Math;
using System;
using System.Linq;
using System.Text;
using System.IO;

public class FSMConvertTXTtoJSON
{

    public static void PreprocessMacro(JState state)
    {
        for (int i = 0; i < state.Fields.Count; i++)
        {
            state.Fields[i].Macro = new List<string>();
            string test = state.Fields[i].Value;
            bool isCollect = false;
            string res = "";
            for (int j = 0; j < test.Length; j++)
            {
                if (test[j] == '{')
                {
                    isCollect = true;
                    res += "{";
                    continue;
                }
                if (test[j] == '}')
                {
                    res += "}";
                    state.Fields[i].Macro.Add(res);
                    res = "";
                    isCollect = false;
                    continue;
                }
                if (isCollect)
                {
                    res += test[j];
                }
            }
        }
    }

    public static void Convert(string inputPath, string outputPath)
    {
        List<JState> m_ListState = new List<JState>();
        List<JCondition> m_ListCondition = new List<JCondition>();

        StreamReader reader = new StreamReader(inputPath);
        string str = reader.ReadToEnd();
        reader.Close();

        string[] strAll = str.Split('#');
        string strState = strAll[0];
        string strCondition = strAll[1];

        // Read State
        strState = strState.Trim();
        StringReader strReader = new StringReader(strState);
        string line = string.Empty;
        JState tempState;
        JField tempField;
        while ((line = strReader.ReadLine()) != null)
        {
            string[] words = line.Split(' ');
            tempState = new JState();
            tempField = new JField();

            tempState.Fields = new List<JField>();

            tempField.FieldName = "ID";
            tempField.Value = words[0];
            tempState.Fields.Add(tempField);
            bool isStateDefault = (words[3][0] == '*');
            bool isStateTrigger = (words[3][0] == '+');
            bool isStateFunction = (words[3].StartsWith("Func", StringComparison.OrdinalIgnoreCase));
            if (isStateDefault || isStateTrigger)
            {
                words[3] = words[3].Substring(1);
            }

            tempState.StateDefault = isStateDefault.ToString();
            tempState.StateTrigger = isStateTrigger.ToString();
            tempState.StateFunction = isStateFunction.ToString();
            tempState.X = words[1];
            tempState.Y = words[2];
            tempState.StateName = words[3];

            if (words.Length > 3)
            {
                for (int i = 4; i < words.Length; i++)
                {

                    string[] field = words[i].Split('=');

                    if (field.Length <= 0) continue;

                    tempField = new JField();

                    tempField.FieldName = field[0];
                    tempField.Value = field[1];

                    tempState.Fields.Add(tempField);
                }
            }
            PreprocessMacro(tempState);
            m_ListState.Add(tempState);
        }

        // Read Condition
        strCondition = strCondition.Trim();
        strReader = new StringReader(strCondition);
        line = string.Empty;
        Function function = new Function();
        JCondition tempCondition;
        while ((line = strReader.ReadLine()) != null)
        {

            string[] words = line.Split(' ');
            tempCondition = new JCondition();

            tempCondition.StateIDFrom = words[0];
            tempCondition.StateIDTo = words[1];

            if (words.Length > 2)
            {
                string postFix = "";
                for (int i = 2; i < words.Length; i++)
                {
                    postFix += words[i];
                    postFix += " ";
                }
                postFix = postFix.Trim();

                function.Parse(postFix);
                function.Infix2Postfix();

                if (function.Error) 
                { 
                }
                //Debug.Log("[Error] " + function.ErrorDescription);
                else
                {
                    //StringBuilder res = new StringBuilder();
                    //for (int i = 0; i < function.Postfix.Count; i++)
                    //{
                    //    res.Append(function.Postfix[i].ToString());
                    //    if (i != function.Postfix.Count - 1)
                    //    {
                    //        res.Append("|");
                    //    }
                    //}

                    tempCondition.ConditionValue = postFix.ToString();
                    JFSM.ExpressionCompiler compiller = new JFSM.ExpressionCompiler();
                    compiller.Compile(postFix);
                    tempCondition.ByteCode = compiller.GetString();
                }
            }
            m_ListCondition.Add(tempCondition);
        }

        JFiniteStateMachine FSM = new JFiniteStateMachine();
        FSM.State = m_ListState;
        FSM.Condition = m_ListCondition;
        Root root = new Root();
        root.FiniteStateMachine = FSM;

        Dictionary<string, object> rootJson = new Dictionary<string, object>();
        root.Serialize(rootJson);
        string json = JCore.MiniJSON.Json.Serialize(rootJson);
        System.IO.File.WriteAllText(outputPath, json);
    }


    public class JState
    {
        public string StateDefault { get; set; }
        public string StateTrigger { get; set; }
        public string StateFunction { get; set; }
        public string StateName { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public List<JField> Fields { get; set; }
        public void Serialize(Dictionary<string, object> root)
        {
            root["StateDefault"] = StateDefault;
            root["StateTrigger"] = StateTrigger;
            root["StateFunction"] = StateFunction;
            root["StateName"] = StateName;
            root["X"] = X;
            root["Y"] = Y;

            List<object> fieldData = new List<object>();
            root["Fields"] = fieldData;
            foreach (var item in Fields)
            {
                Dictionary<string, object> content = new Dictionary<string, object>();
                item.Serialize(content);
                fieldData.Add(content);
            }
        }

    }

    public class JField
    {
        public string FieldName { get; set; }
        public string Value { get; set; }
        public List<string> Macro { get; set; }
        public void Serialize(Dictionary<string, object> root)
        {
            root["FieldName"] = FieldName;
            root["Value"] = Value;
            List<object> macroData = new List<object>();
            root["Macro"] = macroData;
            foreach (var item in Macro)
            {
                macroData.Add(item);
            }
        }
    }

    public class JCondition
    {
        public string StateIDFrom { get; set; }
        public string StateIDTo { get; set; }
        public string ConditionValue { get; set; }
        public string ByteCode { get; set; }
        public void Serialize(Dictionary<string, object> root)
        {
            root["StateIDFrom"] = StateIDFrom;
            root["StateIDTo"] = StateIDTo;
            root["ConditionValue"] = ConditionValue;
            root["ByteCode"] = ByteCode;
        }
    }

    public class JFiniteStateMachine
    {
        public List<JState> State { get; set; }
        public List<JCondition> Condition { get; set; }
        public void Serialize(Dictionary<string, object> root)
        {
            List<object> stateData = new List<object>();
            root["State"] = stateData;
            foreach (var item in State)
            {
                Dictionary<string, object> content = new Dictionary<string, object>();
                item.Serialize(content);
                stateData.Add(content);
            }

            List<object> conditionData = new List<object>();
            root["Condition"] = conditionData;
            foreach (var item in Condition)
            {
                Dictionary<string, object> content = new Dictionary<string, object>();
                item.Serialize(content);
                conditionData.Add(content);
            }
        }
    }

    public class Root
    {
        public JFiniteStateMachine FiniteStateMachine { get; set; }
        public void Serialize(Dictionary<string, object> root)
        {
            Dictionary<string, object> content = new Dictionary<string, object>();
            root["FiniteStateMachine"] = content;
            FiniteStateMachine.Serialize(content);
        }
    }
}
