using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class GetCurrentPos : MonoBehaviour
{
    [SerializeField] ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] private TextMeshProUGUI positionOutput;
    [SerializeField] private TMP_Dropdown selectNode;
    [SerializeField] private Transform userCamera;

    private string newNodeName = "";
    [SerializeField] private int pointer = 0;
    [SerializeField] private List<Node> nodes = new List<Node>();

    [SerializeField] private TextMeshProUGUI relativePositionOutput;
    [SerializeField] private GameObject measurePoint;
    private Vector3 relativePoint = Vector3.zero;

    private void Start()
    {
        //read nodes from XMLParser
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        foreach (Node n in nodes) 
        {
            options.Add(new TMP_Dropdown.OptionData(n.Name));
        }
        selectNode.AddOptions(options);
    }

    private void Update()
    {
        //Measure part
        Vector3 relPos = relativePoint - userCamera.position;
        relativePositionOutput.SetText($"{relPos.x} \n {relPos.y} \n {relPos.z}");
    }

    public void AddConnectionToSeletedNode()
    {
        if(pointer>=nodes.Count)return;

        positionOutput.SetText($"{newNodeName} \n {userCamera.position.x} \n {userCamera.position.y} \n {userCamera.position.z}");
        nodes[pointer].AddConnection(newNodeName,new List<float> { userCamera.position.x , userCamera.position.y , userCamera.position.z });
    }
    public void AddNewNode()
    {
        Node newNode = new Node("New Node");
        nodes.Add(newNode);

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        options.Add(new TMP_Dropdown.OptionData(newNode.Name));
        selectNode.AddOptions(options);
    }
    public void OnNameChanged(string newName)
    {
        newNodeName = newName;
    }
    public void OnPointerChanged(int newPointer)
    {
        if (pointer >= nodes.Count) return;
        pointer = newPointer;
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
            Debug.Log($"Detected: {newImage.referenceImage.name}");
        }
    }  
}
