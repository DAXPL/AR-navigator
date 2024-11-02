using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class NavigationManager : MonoBehaviour
{
    private XMLParser xmlParser;
    private string dataPath ="";
    private void Start()
    {
        dataPath = $"{Application.persistentDataPath}/data.xml";
        Debug.Log(dataPath);
        xmlParser = new XMLParser();
        xmlParser.ParseXML(dataPath);

        foreach (Node n in xmlParser.NodeList)
        {
            Debug.Log($"Found {n.Name}");
        }

    }
    public void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach(var image in eventArgs.updated)
    {
            if (image.trackingState != UnityEngine.XR.ARSubsystems.TrackingState.Tracking) continue;
            Debug.Log($"Found {image.referenceImage.name}");
            // Znajd� aktualny w�ze� na podstawie nazwy obrazu
            Node currentNode = xmlParser.NodeList.FirstOrDefault(n => n.Name == image.referenceImage.name);
            if (currentNode == null) continue;

            // Pozycja w�z�a
            Vector3 currentPosition = image.transform.position;

            // Przejd� przez po��czone w�z�y
            foreach (NodeConnection connection in currentNode.ConnectedNodes)
            {
                // Znajd� po��czony w�ze� w li�cie NodeList
                Node connectedNode = xmlParser.NodeList.FirstOrDefault(n => n.Name == connection.To);
                if (connectedNode == null) continue;

                // Oblicz wzgl�dn� pozycj� w przestrzeni AR
                Vector3 relativePosition = new Vector3(connection.Relative_x, connection.Relative_y, connection.Relative_z);
                Vector3 connectedPosition = currentPosition + relativePosition;

                // Wybierz kolor w zale�no�ci od typu po��czenia
                Color lineColor = connectedNode.ConnectedNodes.Any(c => c.To == currentNode.Name) ? Color.red : Color.yellow;

                // Narysuj lini� od currentPosition do connectedPosition
                Debug.DrawRay(currentPosition, connectedPosition - currentPosition, lineColor, 0.5f);
            }
        }
    }

    private bool IsNodeInXML(XMLParser parser,string nodeName)
    {
        foreach (Node n in parser.NodeList)
        {
            if(n.Name == nodeName) return true;
        }
        return false;
    }
}
