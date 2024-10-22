using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class GetCurrentPos : MonoBehaviour
{
    [SerializeField] ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] private TextMeshProUGUI positionOutput;
    [SerializeField] private TextMeshProUGUI currentNode;
    [SerializeField] private Transform userCamera;

    private string newNodeName = "";
    [SerializeField] private int pointer = 0;

    [SerializeField] private TextMeshProUGUI relativePositionOutput;
    [SerializeField] private GameObject measurePoint;
    private Vector3 relativePoint = Vector3.zero;

    XDocument document;
    private XMLParser parser;

    private void Start()
    {
        parser = new XMLParser();
        //parser.ParseXML("", document);
    }

    private void Update()
    {
        //Measure part
        Vector3 relPos = relativePoint - userCamera.position;
        relativePositionOutput.SetText($"{relPos.x.ToString("n2")} \n {relPos.y.ToString("n2")} \n {relPos.z.ToString("n2")}");
    }

    public void AddConnectionToSeletedNode()
    {
        if(pointer>= parser.NodeList.Count)return;

        positionOutput.SetText($"{newNodeName} \n {userCamera.position.x} \n {userCamera.position.y} \n {userCamera.position.z}");
        parser.NodeList[pointer].AddConnection(newNodeName,new Vector3( userCamera.position.x , userCamera.position.y , userCamera.position.z ));
    }
   
    public void AddNewNode()
    {
        Node newNode = new Node("New Node");
        parser.NodeList.Add(newNode);
    }
  
    public void OnNameChanged(string newName)
    {
        newNodeName = newName;
    }
  
    public void ResetRelativePos()
    {
        relativePoint = userCamera.position;
        if (measurePoint != null) 
        {
            measurePoint.SetActive(true);
            measurePoint.transform.position = relativePoint;
        } 
    }

    void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var newImage in eventArgs.added)
        {
            int c = 0;
            foreach (Node node in parser.NodeList)
            {
                if (node.Name == newImage.referenceImage.name)
                {
                    node.isTracked = true;
                    pointer = c;
                    currentNode.SetText($"Node: {parser.NodeList[pointer].Name}");
                    break;
                }
                c++;
            }
            Debug.Log($"Detected: {newImage.referenceImage.name}");
        }

        foreach(var image in eventArgs.updated)
        {
            int c = 0;
            foreach (Node node in parser.NodeList)
            {
                if (node.Name == image.referenceImage.name)
                {
                    node.isTracked = true;
                    pointer = c;
                    currentNode.SetText($"Node: {parser.NodeList[pointer].Name}");
                    break;
                }
                c++;
            }
        }

        foreach (var lostImage in eventArgs.removed)
        {
            foreach (Node node in parser.NodeList)
            {
                if (node.Name == lostImage.Value.referenceImage.name)
                {
                    node.isTracked = false;
                    break;
                }
            }
            Debug.Log($"Lost: {lostImage.Value.referenceImage.name}");
        }
    } 
    
    public void SaveData()
    {
        string relPath = Application.persistentDataPath + "/Saves/saveData.txt";
        XMLParser.ToXmlFile(parser.NodeList,relPath);
    }
}
