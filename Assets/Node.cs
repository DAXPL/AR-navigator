using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
class Node
{
	Node() { }

	public Node(string name, List<NodeConnection> connectedNodes)
	{
		Name = name;
		ConnectedNodes = connectedNodes;
	}
    public Node(string name)
    {
        Name = name;
        ConnectedNodes = new List<NodeConnection>();
    }
    public void AddConnection(string to, List<float> relativePositions)
	{
		ConnectedNodes.Add(new NodeConnection(to, relativePositions));

    }
	public string Name { get; private set; }
	List<NodeConnection> ConnectedNodes { get; set; }
}
