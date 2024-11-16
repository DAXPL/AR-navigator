using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using static UnityEditor.PlayerSettings;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager Instance;
    public XMLParser xmlParser;
    private string dataPath ="";
    private PathFinder pathFinder;
    NodeList list;

    private LineRenderer lineRenderer;

    private string lastTrackedPoint = "";
    private Vector3 lastTrackedPosition = Vector3.zero;
    private Vector3 destinationPosition = Vector3.zero;
    [SerializeField] private Transform userCamera;
    [SerializeField] private Transform travelEndUI;
    [SerializeField] private Transform scanEnviroUI;

    private bool isTraveling = false;
    private string destination = "";

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

        list = new NodeList(xmlParser.NodeList);
        pathFinder = new PathFinder(list);

        //DEBUG
        //lastTrackedPoint = "0";
        //lastTrackedPosition = new Vector3(-0.693f,1.374f,1.401f);
        //NavigateTo("hanger");
    }

    private void FixedUpdate()
    {
        if(!isTraveling) return;
        if(lineRenderer.positionCount>0) lineRenderer.SetPosition(0, userCamera.transform.position - Vector3.up);
        if (Vector3.Distance(userCamera.position, destinationPosition) < 1)
        {
            travelEndUI.gameObject.SetActive(true);
        } 
    }

    public void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        if(Application.isEditor) DrawMesh(eventArgs);

        foreach (var image in eventArgs.updated)
        {
            if (image.trackingState != UnityEngine.XR.ARSubsystems.TrackingState.Tracking) continue;
            // ZnajdŸ aktualny wêze³ na podstawie nazwy obrazu
            Node currentNode = xmlParser.NodeList.FirstOrDefault(n => n.TagId == image.referenceImage.name);
            if (currentNode == null) continue;
            lastTrackedPoint = currentNode.Name;
            lastTrackedPosition = image.transform.position;

            if (isTraveling) UpdatePath();
            break;
        }
    }

    private void DrawMesh(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var image in eventArgs.updated)
        {
            if (image.trackingState != UnityEngine.XR.ARSubsystems.TrackingState.Tracking) continue;

            // ZnajdŸ aktualny wêze³ na podstawie nazwy obrazu
            Node currentNode = xmlParser.NodeList.FirstOrDefault(n => n.TagId == image.referenceImage.name);
            if (currentNode == null) continue;

            // Pozycja g³ównego wêz³a
            Vector3 currentPosition = image.transform.position;

            foreach (NodeConnection connection in currentNode.ConnectedNodes)
            {
				// Oblicz wzglêdn¹ pozycjê po³¹czonego wêz³a (podwêz³a) w przestrzeni AR
				//replaceable with Vector3 relativePosition = connection.GetVector()
				Vector3 relativePosition = new Vector3(connection.Relative_x, connection.Relative_y, connection.Relative_z);
                Vector3 connectedPosition = currentPosition + relativePosition;

                // SprawdŸ, czy po³¹czony wêze³ istnieje w `NodeList` jako g³ówny wêze³
                bool isMainNode = xmlParser.NodeList.Any(n => n.Name == connection.To);

                // - Czerwona, jeœli po³¹czony wêze³ to g³ówny wêze³
                // - ¯ó³ta, jeœli to podwêze³
                Color lineColor = isMainNode ? Color.red : Color.yellow;

                Debug.DrawRay(currentPosition, connectedPosition - currentPosition, lineColor, 1f);
            }
        }
    }

    public void NavigateTo(string destinationNode)
    {
        if (pathFinder == null) return;
        Debug.Log($"Finding path from: {lastTrackedPoint} to {destinationNode}");
        Path path = pathFinder.FindShortestPath(lastTrackedPoint, destinationNode);

        if(path.Nodes.Count <= 0) return;

        destination = destinationNode;
        isTraveling = true;

        if (lastTrackedPoint != "") DrawPath(path);
        else scanEnviroUI.gameObject.SetActive(true);
    }
    
    //update on scan or when close enough
    private void UpdatePath()
    {
        scanEnviroUI.gameObject.SetActive(lastTrackedPoint == "");
        if (lastTrackedPoint == "") return;
        Path path = pathFinder.FindShortestPath(lastTrackedPoint, destination);
        DrawPath(path);  
    }
   
    private void DrawPath(Path path)
    {
        if (path == null || path.Nodes.Count == 0)
        {
            Debug.LogWarning("No path found.");
            return;
        }

        lineRenderer.positionCount = path.Nodes.Count + 1;
        lineRenderer.SetPosition(0, userCamera.transform.position - Vector3.up);

        Vector3 pos = lastTrackedPosition;
        for (int i = 0; i < path.Nodes.Count; i++)
        {
            Debug.Log($"Step {i}: {path.Nodes[i].Name}");
            lineRenderer.SetPosition(i + 1, pos);
            if (path.Nodes[i].ConnectedNodes.Count > 0)
                pos += path.Nodes[i].ConnectedNodes[0].GetVector();

        }
        destinationPosition = pos;
    }

    public void EndNavigation()
    {
        destination = "";
        isTraveling = true;
        lineRenderer.positionCount = 0;
        destinationPosition = Vector3.zero;
    }
}
