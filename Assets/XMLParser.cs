using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;

class XMLParser
{
	public XMLParser()
	{
		Nodes = new NodeList();
	}

	public NodeList Nodes { get; set; }

	public void ParseXML(string _xmlFilePath)
	{
		Nodes = new NodeList();
		XDocument xmlDoc = XDocument.Load(_xmlFilePath);

		//Foreach node
		foreach (var node in xmlDoc.Descendants("Node"))
		{
			List<NodeConnection> nodeConnections = new List<NodeConnection>();

			//Foreach node connection
			foreach (var nodeConnection in node.Descendants("NodeConnection"))
			{
				string to = nodeConnection.Element((XName)"To").Value.ToString().Trim();

				//Relative position of connected node 
				float x = float.Parse(nodeConnection.Element((XName)"Relative_x").Value);
				float y = float.Parse(nodeConnection.Element((XName)"Relative_y").Value);
				float z = float.Parse(nodeConnection.Element((XName)"Relative_z").Value);
				Vector3 relativePosition = new Vector3(x, y, z);

				nodeConnections.Add(new NodeConnection(to, relativePosition));
			}

			string name = node.Element((XName)"Name").Value.ToString().Trim();

			//Add node to node list
			Nodes.Add(new Node(name, nodeConnections));
		}
	}

	public static void ToXmlFile(object obj, string filePath)
	{
		var xs = new XmlSerializer(obj.GetType());
		var ns = new XmlSerializerNamespaces();
		var ws = new XmlWriterSettings { Indent = true, NewLineOnAttributes = false, OmitXmlDeclaration = true };
		ns.Add("", "");

		using (XmlWriter writer = XmlWriter.Create(filePath, ws))
		{
			xs.Serialize(writer, obj, ns);
		}
	}
}
