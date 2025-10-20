using System;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class MeasureManger : MonoBehaviour
{
    [SerializeField] ARTrackedImageManager m_TrackedImageManager;
    public static XMLParser parser;
    private string dataPath;
    [SerializeField] private TextMeshProUGUI nodesOutput;
    [SerializeField] private TextMeshProUGUI debugOutput;
    [SerializeField] private GameObject[] cubes;

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

        debugOutput.SetText("");
        foreach (var image in m_TrackedImageManager.trackables)
        {
            string imageName = image.referenceImage.name;
            var trackingState = image.trackingState;

            debugOutput.text += $"{imageName} | {trackingState} \n";
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

    public void ClearCubes()
    {
        foreach (var c in cubes) 
        { 
            c.SetActive(false);
        }
    }
}
