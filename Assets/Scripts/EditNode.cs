using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class EditNode : MonoBehaviour
{
    [SerializeField] ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] private TextMeshProUGUI positionText;
    [SerializeField] private TextMeshProUGUI currentNodeText;
    [SerializeField] private Transform userCamera;

    private string newNodeName = "";
    private int pointer = 0;
    private Vector3 relativePoint = Vector3.zero;
    Vector3 relPos = Vector3.zero;//relative position

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
        relPos = relativePoint - userCamera.position;
        parser.NodeList[pointer].AddConnection(newNodeName, new Vector3(relPos.x, relPos.y, relPos.z));
    }

    public void OnNameChanged(string newName)
    {
        newNodeName = newName;
    }

    void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        XMLParser parser = MeasureManger.parser;
        if (parser == null) return;

        foreach (var image in eventArgs.updated)
        {
            int c = 0;
            foreach (Node node in parser.NodeList)
            {
                if (node.Name == image.referenceImage.name)
                {
                    pointer = c;
                    currentNodeText.SetText($"Node: {parser.NodeList[pointer].Name} | {image.transform.position}");
                    relativePoint = image.transform.position;
                    break;
                }
                c++;
            }
        }

        foreach (var newImage in eventArgs.added)
        {
            int c = 0;
            foreach (Node node in parser.NodeList)
            {
                if (node.Name == newImage.referenceImage.name)
                {
                    pointer = c;
                    currentNodeText.SetText($"Node: {parser.NodeList[pointer].Name} | {newImage.transform.position}");
                    relativePoint = newImage.transform.position;
                    break;
                }
                c++;
            }
        }
    }
}
