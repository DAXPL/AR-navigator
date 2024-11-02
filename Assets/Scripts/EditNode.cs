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

    private string newNodeName = "";
    private int pointer = 0;
    private Vector3 relativePoint = Vector3.zero;
    Vector3 relPos = Vector3.zero;//relative position

    private void OnEnable()
    {
        pointer = 0;
    }
   
    private void Update()
    {
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

        nameInput.Select();
        nameInput.text = "";
    }

    public void OnNameChanged(string newName)
    {
        newNodeName = newName;
    }

    public void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {

        UpdateNodesReferences(eventArgs.added);
        UpdateNodesReferences(eventArgs.updated);
    }
   
    private void UpdateNodesReferences(ReadOnlyList<ARTrackedImage> images, bool first = false)
    {
        XMLParser parser = MeasureManger.parser;
        if (parser == null) return;

        foreach (var image in images)
        {
            for (int i=0;i< parser.NodeList.Count;i++)
            {
                Node node = parser.NodeList[i];
                if (node.Name != image.referenceImage.name) continue;

                pointer = i;
                currentNodeText.SetText($"Node: {parser.NodeList[pointer].Name} | {image.transform.position}");
                relativePoint = image.transform.position;
                break;
            }
        }
    }
}
