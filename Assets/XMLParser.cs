using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

class XMLParser
{
	public XMLParser()
	{
		NodeList = new List<Node>();
	}

	public List<Node> NodeList { get; set; }

	public void ParseXML(string _xmlFilePath)
	{
		NodeList.Clear();
		XDocument xmlDoc = XDocument.Load(_xmlFilePath);

		//Foreach node
		foreach (var node in xmlDoc.Descendants("node"))
		{
			List<NodeConnection> nodeConnections = new List<NodeConnection>();

			//Foreach node connection
			foreach (var nodeConnection in node.Descendants("nodeConnection"))
			{
				string to = nodeConnection.Element((XName)"to").Value.ToString().Trim();

				List<float> relativePositions = new List<float>();
				relativePositions.Add(float.Parse(nodeConnection.Element((XName)"relative_x").Value));
				relativePositions.Add(float.Parse(nodeConnection.Element((XName)"relative_y").Value));
				relativePositions.Add(float.Parse(nodeConnection.Element((XName)"relative_z").Value));

				//Add connection to connections list
				nodeConnections.Add(new NodeConnection(to, relativePositions));
			}

			string name = node.Element((XName)"name").Value.ToString().Trim();

			//Add node to node list
			NodeList.Add(new Node(name, nodeConnections));
		}
	}
}
