using System;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class MeasureManger : MonoBehaviour
{
    public static XMLParser parser;
    private string dataPath;
    [SerializeField] private TextMeshProUGUI nodesOutput;

    void Start()
    {
        dataPath = $"{Application.persistentDataPath}/data.xml";
        Debug.Log(dataPath);
        parser = new XMLParser();
    }

    private void FixedUpdate()
    {
        if (parser == null) 
        {
            nodesOutput.SetText("No parser");
            return;
        }
        

        nodesOutput.SetText("");
        int i =0;
        foreach (var node in parser.NodeList)
        {
            nodesOutput.text+=$"{i}) {node.Name}\n";
            foreach(var con in node.ConnectedNodes)
            {
                nodesOutput.text += $"{con.To} | ";
            }

            i++;
        }
    }

    public void LoadXML()
    {
        parser.ParseXML(dataPath);
    }

    public void SaveXML()
    {
        XMLParser.ToXmlFile(parser.NodeList, dataPath);
    }
}
