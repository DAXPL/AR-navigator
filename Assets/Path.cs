using System.Collections.Generic;
using UnityEngine;

public class PathNode : Node
{
	public PathNode() { }

	public PathNode(Node node)
	{
		SetName(node.Name);
		ConnectedNodes = node.ConnectedNodes;
		Distance = float.MaxValue;
	}
	public float Distance { get; set; }
}
public class Path
{
	public Path()
	{
		Nodes = new List<PathNode>();
		Length = 0;
	}

	public List<PathNode> Nodes { get; set; }
	public float Length { get; set; }

	void AddDistance(NodeConnection connection)
	{
		Length += connection.GetDistance();
	}
}
