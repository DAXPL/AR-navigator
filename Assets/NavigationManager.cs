using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager Instance;
    public XMLParser xmlParser;
    private string dataPath ="";
    private PathFinder pathFinder;
    NodeList list;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

    }
    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        dataPath = $"{Application.persistentDataPath}/data.xml";
        Debug.Log(dataPath);
        xmlParser = new XMLParser();
        xmlParser.ParseXML(dataPath);

        // Tworzenie listy w�z��w na podstawie xmlParser
        list = new NodeList(xmlParser.NodeList);

        pathFinder = new PathFinder(list);
    }
    public void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        DrawMesh(eventArgs);
    }

    private void DrawMesh(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var image in eventArgs.updated)
        {
            if (image.trackingState != UnityEngine.XR.ARSubsystems.TrackingState.Tracking) continue;

            // Znajd� aktualny w�ze� na podstawie nazwy obrazu
            Node currentNode = xmlParser.NodeList.FirstOrDefault(n => n.TagId == image.referenceImage.name);
            if (currentNode == null) continue;

            // Pozycja g��wnego w�z�a
            Vector3 currentPosition = image.transform.position;

            foreach (NodeConnection connection in currentNode.ConnectedNodes)
            {
				// Oblicz wzgl�dn� pozycj� po��czonego w�z�a (podw�z�a) w przestrzeni AR
				//replaceable with Vector3 relativePosition = connection.GetVector()
				Vector3 relativePosition = new Vector3(connection.Relative_x, connection.Relative_y, connection.Relative_z);
                Vector3 connectedPosition = currentPosition + relativePosition;

                // Sprawd�, czy po��czony w�ze� istnieje w `NodeList` jako g��wny w�ze�
                bool isMainNode = xmlParser.NodeList.Any(n => n.Name == connection.To);

                // - Czerwona, je�li po��czony w�ze� to g��wny w�ze�
                // - ��ta, je�li to podw�ze�
                Color lineColor = isMainNode ? Color.red : Color.yellow;

                Debug.DrawRay(currentPosition, connectedPosition - currentPosition, lineColor, 1f);
            }
        }
    }

    public void NavigateTo(string destinationName)
    {
        Debug.Log($"Selected node: {destinationName}");

        if (pathFinder == null) return;

        Path path = pathFinder.FindShortestPath("Kettle", destinationName);

        if (path == null || path.Nodes.Count == 0)
        {
            Debug.LogWarning("No path found.");
            return;
        }

        Debug.Log($"Navigation log for {path.Nodes.Count} steps:");
        lineRenderer.positionCount = path.Nodes.Count;
        // Unity line renderer change points len
        for (int i = 1; i < path.Nodes.Count; i++)
        {
            //Vector3 relativePosition = new Vector3(connection.Relative_x, connection.Relative_y, connection.Relative_z);
            lineRenderer.SetPosition(i - 1, new Vector3(1,1,1));
            Debug.Log($"Step {i}: go from {path.Nodes[i - 1].Name} to {path.Nodes[i].Name}");
        }
    }
}
