using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node
{
	Node() { }

	public Node(string name)
	{
		Name = name;
		ConnectedNodes = new List<NodeConnection>();
	}

	public Node(string name, List<NodeConnection> connectedNodes)
	{
		Name = name;
		ConnectedNodes = connectedNodes;
	}

	public string Name { get; private set; }
	public List<NodeConnection> ConnectedNodes { get; set; }

	public void AddConnection(string to, Vector3 relativePosition)
	{ ConnectedNodes.Add(new NodeConnection(to, relativePosition)); }
}
