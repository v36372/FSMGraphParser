using System.Collections;
using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class FSMConvertGraphmltoTXT 
{
    static string OutputDir = "";

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string _importedAssets in importedAssets)
        {
            if (_importedAssets.Contains(".graphml") && !_importedAssets.Contains("[B]"))
            {
                ConvertGraphmltoTXT(_importedAssets);
            }
        }
    }
    static XmlNode FindFistNode(XmlNode root, string nodeName)
    {
        Stack<XmlNode> Stack = new Stack<XmlNode>();
        Stack.Push(root);

        while (Stack.Count > 0)
        {
            XmlNode tmp = Stack.Pop();
            for (int i = 0; i < tmp.ChildNodes.Count; i++)
            {
                if (tmp.ChildNodes[i].Name == nodeName)
                    return tmp.ChildNodes[i];
                else
                    Stack.Push(tmp.ChildNodes[i]);
            }
        }
        return null;
    }

    public static void ConvertGraphmltoTXT(string path)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(path);

        List<int> listID = new List<int>();
        List<string> listX = new List<string>();
        List<string> listY = new List<string>();
        List<string> listContentID = new List<string>();
        List<int> listFromID = new List<int>();
        List<int> listToID = new List<int>();
        List<string> listContentEquation = new List<string>();

        string[] temp;
        int id;
        string tempStr;

        XmlDocument tmpDoc = new XmlDocument();
        XmlNode tmpelemList;

        // Get State ID
        XmlNodeList elemList = doc.GetElementsByTagName("node");
        for (int i = 0; i < elemList.Count; i++)
        {
            temp = elemList[i].Attributes["id"].Value.Split('n');
            id = Convert.ToInt32(temp[1]);
            id++;
            listID.Add(id);
            // Get State Name
            tmpelemList = FindFistNode(elemList[i], "y:NodeLabel");
            listContentID.Add(tmpelemList.InnerText.Replace("\\n", " "));

            //Get XY
            tmpelemList = FindFistNode(elemList[i], "y:Geometry");
            listX.Add(tmpelemList.Attributes["x"].Value);
            listY.Add(tmpelemList.Attributes["y"].Value);

        }

        // Get Condition
        elemList = doc.GetElementsByTagName("edge");
        for (int i = 0; i < elemList.Count; i++)
        {
            // Get State ID From
            temp = elemList[i].Attributes["source"].Value.Split('n');
            id = Convert.ToInt32(temp[1]);
            id++;
            listFromID.Add(id);

            // Get State ID To
            temp = elemList[i].Attributes["target"].Value.Split('n');
            id = Convert.ToInt32(temp[1]);
            id++;
            listToID.Add(id);

            // Get Condition Name
            tmpelemList = FindFistNode(elemList[i], "y:EdgeLabel");
            if (tmpelemList == null || tmpelemList.InnerText == "")
                listContentEquation.Add("True");
            else
                listContentEquation.Add(tmpelemList.InnerText);
        }

        string[] tempPath = path.Split('/');
        string outputFileName = tempPath[tempPath.Length - 1].Replace(".graphml", "") + ".txt";
        string outputFile = OutputDir + outputFileName;
        StreamWriter sw = new StreamWriter(outputFile);

        //Debug.Log(outputFile);

        for (int i = 0; i < listContentID.Count; i++)
        {
            listContentID[i] = listContentID[i].Replace(" ", "");
            listContentID[i] = listContentID[i].Replace("\r\n", " ");
            listContentID[i] = listContentID[i].Replace("\n", " ");
            tempStr = "" + listID[i] + " " + listX[i] + " " + listY[i] + " " + listContentID[i];
            sw.WriteLine(tempStr);
        }

        sw.WriteLine("#");

        for (int i = 0; i < listContentEquation.Count; i++)
        {
            sw.WriteLine("" + listFromID[i] + " " + listToID[i] + " " + listContentEquation[i]);
        }

        sw.Close();

        FSMConvertTXTtoJSON.Convert(outputFile, outputFile);
    }
}