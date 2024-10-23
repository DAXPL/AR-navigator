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
    [SerializeField] private TextMeshProUGUI nodesOutput;

    void Start()
    {
        parser = new XMLParser();
    }

    private void OnEnable()
    {
        if(parser == null)return;

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

}