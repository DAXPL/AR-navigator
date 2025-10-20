using System;
using TMPro;
using Unity.XR.CoreUtils.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class EditNode : MonoBehaviour
{
    [SerializeField] ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] TMP_InputField nameInput;
    [SerializeField] private TextMeshProUGUI positionText;
    [SerializeField] private TextMeshProUGUI currentNodeText;
    [SerializeField] private Transform userCamera;
    [SerializeField] private GameObject cube;
    private string newNodeName = "";
    private int pointer = 0;
    private Vector3 relativePoint = Vector3.zero;
    Vector3 relPos = Vector3.zero;//relative position

    private void OnEnable()
    {
        pointer = 0;
        //m_TrackedImageManager.trackablesChanged.AddListener(OnImageChanged);
        
    }
    
    private void OnDisable()
    {
        //m_TrackedImageManager.trackablesChanged.RemoveListener(OnImageChanged);
    }

    private void Update()
    {
        ContinousUpdateNodesReferences(m_TrackedImageManager.trackables);
        relPos = relativePoint - userCamera.position;
        positionText.SetText($"{relPos.x.ToString("n2")} \n {relPos.y.ToString("n2")} \n {relPos.z.ToString("n2")}");
    }

    public void AddConnectionToSeletedNode()
    {
        XMLParser parser = MeasureManger.parser;
        if (parser == null) return;

        if (pointer >= parser.NodeList.Count) return;
        relPos =  userCamera.position - relativePoint;
        parser.NodeList[pointer].AddConnection(newNodeName, new Vector3(relPos.x, relPos.y, relPos.z));

        Vector3 relPosSecond = relativePoint - userCamera.position;
        Node newNode = new Node(newNodeName);
        newNode.AddConnection(parser.NodeList[pointer].Name, new Vector3(relPosSecond.x, relPosSecond.y, relPosSecond.z));
        parser.NodeList.Add(newNode);

        nameInput.Select();
        nameInput.text = "";
    }

    public void OnNameChanged(string newName)
    {
        newNodeName = newName;
    }

    public void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        //UpdateNodesReferences(eventArgs.added);
        //UpdateNodesReferences(eventArgs.updated);
    }
   
    private void ContinousUpdateNodesReferences(TrackableCollection<ARTrackedImage> images)
    {
        XMLParser parser = MeasureManger.parser;
        if (parser == null)
        {
            currentNodeText.SetText($"NULL PARSER!");
            return;
        }
        foreach (var image in images)
        {
            if(image.trackingState != UnityEngine.XR.ARSubsystems.TrackingState.Tracking) continue;
            for (int i = 0; i < parser.NodeList.Count; i++)
            {
                Node node = parser.NodeList[i];
                if (node.Name != image.referenceImage.name) continue;

                pointer = i;
                currentNodeText.SetText($"Node: {parser.NodeList[pointer].Name}");
                relativePoint = image.transform.position;

                cube.SetActive(true);
                cube.transform.position = image.transform.position;
                cube.transform.rotation = image.transform.rotation;
                return;
            }
        }
    }
    
    private void UpdateNodesReferences(ReadOnlyList<ARTrackedImage> images)
    {
        XMLParser parser = MeasureManger.parser;
        if (parser == null) 
        {
            currentNodeText.SetText($"NULL PARSER!");
            return; 
        }

        foreach (var image in images)
        {
            for (int i=0;i < parser.NodeList.Count;i++)
            {
                Node node = parser.NodeList[i];
                if (node.Name != image.referenceImage.name) continue;

                pointer = i;
                currentNodeText.SetText($"Node: {parser.NodeList[pointer].Name}");
                relativePoint = image.transform.position;

                cube.SetActive(true);
                cube.transform.position = image.transform.position;
                cube.transform.rotation = image.transform.rotation;
                return;
            }
        }
        cube.SetActive(false);
        currentNodeText.SetText($"No valid node");
    }
}
