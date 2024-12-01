using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System;
using UnityEngine;

public class PathFinder
{
	public PathFinder()
	{
		Graph = new NodeList();
		WorkGraph = new Dictionary<string, PathNode>();
		shortestPath = new Path();
		SearchWheelchairAccessible = false;
	}

	public PathFinder(NodeList graph)
	{
		Graph = graph;
		WorkGraph = new Dictionary<string, PathNode>();
		shortestPath = new Path();
		SearchWheelchairAccessible = false;
	}

	NodeList Graph { get; set; }
	Dictionary<string, PathNode> WorkGraph { get; set; }
	Path shortestPath { get; set; }
	bool SearchWheelchairAccessible { get; set; }
	const float floatError = 0.001f;

	public void LoadGraph(NodeList graph)
	{
		Graph = graph;
	}
	public void ToggleAccessibility(bool? searchOnlyAccessible = null)
	{
		if(searchOnlyAccessible != null)
		{
			SearchWheelchairAccessible = searchOnlyAccessible.Value;
			return;
		}
		SearchWheelchairAccessible = !SearchWheelchairAccessible;
	}

	public Path FindShortestPath(string from, string to, [Optional] NodeList graph)
	{
		NodeList clone;
		shortestPath = new Path();

		//If optional graph parameter is specified then use it, otherwise use loaded Graph
		//If graph isn't specified and no graph is loaded return an empty path
		if (graph != null)
		{
			clone = (NodeList)graph.Clone();
		}
		else if (Graph != null)
		{
			clone = (NodeList)Graph.Clone();
		}
		else
			return new Path();

		WorkGraph = clone.ToPathNodeDictionary();

		//If starting point or destination isn't found return an empty path
		if (!WorkGraph.ContainsKey(from) || !WorkGraph.ContainsKey(to))
			return new Path();

		//Run Dijkstras algorithm on WorkGraph
		//From destination to starting point
		WalkWorkGraph(to, from);

		//If no path was found between start and destination
		if (shortestPath.Length == -1)
			return new Path();

		shortestPath.Length = WorkGraph[from].Distance;

		//Trace a path from the starting point to the destination
		TracePath(from);

		return shortestPath;
	}

	void WalkWorkGraph(string start, string destination)
	{
		WorkGraph[start].Distance = 0;

		//Check distance to connected nodes
		WalkConnections(WorkGraph[start]);

		if (WorkGraph[destination].Distance == float.MaxValue)
			shortestPath.Length = -1;
	}
	void WalkConnections(PathNode startNode)
	{
		foreach (NodeConnection connection in startNode.ConnectedNodes.ToList())
		{
			//If wheelchair accessible search enabled and connection isn't wheelchair accessible
			if(SearchWheelchairAccessible && !connection.IsWheelchairAccessible)
				continue;

			//If the distance to the connected node is smaller than assigned to that node, assign the distance from the current node
			if (startNode.Distance + connection.GetDistance() < WorkGraph[connection.To].Distance)
			{
				WorkGraph[connection.To].Distance = startNode.Distance + connection.GetDistance();
				//Check distance to connected nodes
				WalkConnections(WorkGraph[connection.To]);
			}
		}
	}

	void TracePath(string start)
	{
		PathNode startNode = WorkGraph[start];

		shortestPath.Nodes.Add(startNode);

		NextNode();
	}
	void NextNode()
	{
		string currentNode = shortestPath.Nodes[shortestPath.Nodes.Count - 1].Name;

		//If current node is destination node
		if (WorkGraph[currentNode].Distance == 0)
		{
			//Clear connections from destination node
			shortestPath.Nodes[shortestPath.Nodes.Count - 1].ClearConnections();
			return;
		}

		foreach (NodeConnection connection in WorkGraph[currentNode].ConnectedNodes)
		{
			//If distance between nodes is approximately the difference in distance from starting node
			if (Math.Abs(connection.GetDistance() - (WorkGraph[currentNode].Distance - WorkGraph[connection.To].Distance)) < floatError)
			{
				shortestPath.Nodes.Add(WorkGraph[connection.To]);

				//Clear connections from current node, add connection to the next node
				shortestPath.Nodes[shortestPath.Nodes.Count - 2].ClearConnections();
				shortestPath.Nodes[shortestPath.Nodes.Count - 2].ConnectedNodes.Add(connection);

				break;
			}
		}
		NextNode();
	}
}
