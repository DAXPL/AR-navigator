using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class EditNode : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TextMeshProUGUI positionText;
    [SerializeField] private TextMeshProUGUI currentNodeText;
    [SerializeField] private Transform userCamera;
    [SerializeField] private GameObject cube;
    [SerializeField] private Toggle[] Toggles;

    private string newNodeName = "";
    private int pointer = 0;
    private Vector3 relativePoint = Vector3.zero;
    private Vector3 relPos = Vector3.zero;

    private void Update()
    {
        UpdateNodeReferences(m_TrackedImageManager.trackables);
        relPos = relativePoint - userCamera.position;
        positionText.SetText($"{relPos.x:n2} \n{relPos.y:n2} \n{relPos.z:n2}");
    }

    public void AddConnectionToSelectedNode()
    {
        XMLParser parser = MeasureManger.parser;
        if (parser == null || pointer >= parser.NodeList.Count)
            return;

        Vector3 relPos = userCamera.position - relativePoint;

        // Apply mirroring based on toggles
        if (Toggles.Length > 0 && Toggles[0].isOn) relPos.x = -relPos.x;
        if (Toggles.Length > 1 && Toggles[1].isOn) relPos.y = -relPos.y;
        if (Toggles.Length > 2 && Toggles[2].isOn) relPos.z = -relPos.z;

        // Add connection to the selected node
        parser.NodeList[pointer].AddConnection(newNodeName, relPos);

        // Create new node and connect back
        Node newNode = new Node(newNodeName);
        newNode.AddConnection(parser.NodeList[pointer].Name, -relPos);
        parser.NodeList.Add(newNode);

        // Reset input field
        nameInput.Select();
        nameInput.text = "";
    }

    public void OnNameChanged(string newName)
    {
        newNodeName = newName;
    }

    private void UpdateNodeReferences(TrackableCollection<ARTrackedImage> images)
    {
        XMLParser parser = MeasureManger.parser;
        if (parser == null)
        {
            currentNodeText.SetText("NULL PARSER!");
            return;
        }

        foreach (var image in images)
        {
            if (image.trackingState != TrackingState.Tracking)
                continue;

            for (int i = 0; i < parser.NodeList.Count; i++)
            {
                Node node = parser.NodeList[i];
                if (node.Name != image.referenceImage.name)
                    continue;

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
}
