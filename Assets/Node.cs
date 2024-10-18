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

	string Name { get; set; }
	List<NodeConnection> ConnectedNodes { get; set; }
}
