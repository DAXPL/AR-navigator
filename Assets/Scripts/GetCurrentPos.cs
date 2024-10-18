using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GetCurrentPos : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI positionOutput;
    [SerializeField] private TMP_Dropdown selectNode;
    [SerializeField] private Transform userCamera;

    private string newNodeName = "";
    [SerializeField] private int pointer = 0;
    [SerializeField] private List<Node> nodes = new List<Node>();

    private void Start()
    {
        nodes.Add(new Node("test 1"));
        nodes.Add(new Node("test 2"));
        nodes.Add(new Node("test 3"));

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        foreach (Node n in nodes) 
        {
            options.Add(new TMP_Dropdown.OptionData(n.Name));
        }
        selectNode.AddOptions(options);
    }

    public void DisplayCurrentPosition()
    {
        if(pointer>=nodes.Count)return;

        positionOutput.SetText($"{newNodeName} \n {userCamera.position.x} \n {userCamera.position.y} \n {userCamera.position.z}");
        nodes[pointer].AddConnection(newNodeName,new List<float> { userCamera.position.x , userCamera.position.y , userCamera.position.z });
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
}
