using System.Collections.Generic;
using UnityEngine;

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

	//public void AddConnection(string to, List<float> relativePositions)
	//{
	//	ConnectedNodes.Add(new NodeConnection(to, relativePositions));
	//}

	public void AddConnection(string to, Vector3 relativePosition)
	{
		ConnectedNodes.Add(new NodeConnection(to, relativePosition));
	}

	string Name { get; private set; }
	List<NodeConnection> ConnectedNodes { get; set; }
}
