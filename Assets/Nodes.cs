using System;
using System.Collections.Generic;
using UnityEngine;

public class NodeList
{
	public NodeList()
	{
		Nodes = new List<Node>();
	}
	public NodeList(List<Node> nodes)
	{
		Nodes = nodes;
	}

	public List<Node> Nodes { get; set; }

	public Dictionary<string, PathNode> ToPathNodeDictionary()
	{
		Dictionary<string, PathNode> PathNodeDictionary = new Dictionary<string, PathNode>();
		foreach (Node node in Nodes)
		{
			PathNodeDictionary.Add(node.Name, new PathNode(node));
		}

		return PathNodeDictionary;
	}

	public void Add(Node node)
	{
		Nodes.Add(node);
	}

	public NodeList Clone()
	{
		NodeList nodeList = new NodeList();

		foreach (Node node in Nodes)
		{
			List<NodeConnection> connections = new List<NodeConnection>();
			Vector3 relativePosition = new Vector3();
			foreach (NodeConnection connection in node.ConnectedNodes)
			{
				relativePosition = new Vector3();

				relativePosition.x = connection.Relative_x;
				relativePosition.y = connection.Relative_y;
				relativePosition.z = connection.Relative_z;

				connections.Add(new NodeConnection(connection.To, relativePosition));
			}
			nodeList.Nodes.Add(new Node(node.TagId, node.Name, connections));
		}
		return nodeList;
	}
}

[System.Serializable]
public class Node
{
	public Node() { }

	public Node(string name)
	{
		Name = name;
		ConnectedNodes = new List<NodeConnection>();
	}
    public Node(string name, string tagId)
    {
        Name = name;
        TagId = tagId;
        ConnectedNodes = new List<NodeConnection>();
    }

    public Node(string tagId, string name, List<NodeConnection> connectedNodes)
	{
		TagId = tagId;
		Name = name;
		ConnectedNodes = connectedNodes;
	}

	public string TagId { get; set; }
	public string Name;
	public List<NodeConnection> ConnectedNodes { get; set; }
	public bool isTracked = false;

	public void SetName(string name)
	{ Name = name; }

	public void AddConnection(string to, Vector3 relativePosition)
	{ ConnectedNodes.Add(new NodeConnection(to, relativePosition)); }

	public void ClearConnections()
	{ ConnectedNodes.Clear(); }
}

[System.Serializable]
public class NodeConnection
{
	NodeConnection() { }

	public NodeConnection(string to, Vector3 relativePosition)
	{
		To = to;
		Relative_x = relativePosition.x;
		Relative_y = relativePosition.y;
		Relative_z = relativePosition.z;
		RelativePosition = relativePosition;
		Distance = calculateDistance();
	}

	public string To { get; set; }
	public float Relative_x { get; set; }
	public float Relative_y { get; set; }
	public float Relative_z { get; set; }
	Vector3 RelativePosition { get; set; }
	float Distance { get; set; }

	public Vector3 GetVector()
	{ return RelativePosition; }

	public float GetDistance()
	{ return Distance; }

	float calculateDistance()
	{
		float distance = (float)Math.Sqrt(RelativePosition.x * RelativePosition.x + RelativePosition.y * RelativePosition.y);
		distance = (float)Math.Sqrt(distance * distance + RelativePosition.z * RelativePosition.z);
		return distance;
	}
}