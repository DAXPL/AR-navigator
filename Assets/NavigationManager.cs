using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

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
    private SearchDestinationUI searchDestinationUI;

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

        if (searchDestinationUI == null) searchDestinationUI = FindFirstObjectByType<SearchDestinationUI>();

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
            if(searchDestinationUI)searchDestinationUI.SetAnimatorBooleanValue("ShowArrivalInfo", true);
        } 
    }

    public void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        if(Application.isEditor) DrawMesh(eventArgs);

        foreach (var image in eventArgs.updated)
        {
            if (image.trackingState != UnityEngine.XR.ARSubsystems.TrackingState.Tracking) continue;
            // Znajd� aktualny w�ze� na podstawie nazwy obrazu
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

    public void NavigateTo(string destinationNode, bool wheelChair)
    {
        if (pathFinder == null) return;
        Debug.Log($"Finding path from: {lastTrackedPoint} to {destinationNode}"); 

        destination = destinationNode;
        isTraveling = true;

        pathFinder.ToggleAccessibility(wheelChair);
        Path path = pathFinder.FindShortestPath(lastTrackedPoint, destinationNode);

        if (lastTrackedPoint != "") DrawPath(path);
        else if (searchDestinationUI) searchDestinationUI.SetAnimatorBooleanValue("ShowScanEnviroUI", true);
    }
    
    //update on scan or when close enough
    private void UpdatePath()
    {
        if (searchDestinationUI) searchDestinationUI.SetAnimatorBooleanValue("ShowScanEnviroUI", lastTrackedPoint == "");
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

        int linePosCount = 1;

        Vector3 pos = lastTrackedPosition;
        for (int i = 0; i < path.Nodes.Count; i++)
        {
            if (Vector3.Distance(pos,userCamera.position)>0.5f)
            {
                lineRenderer.SetPosition(linePosCount, pos);
                linePosCount++;
            }
            
            if (path.Nodes[i].ConnectedNodes.Count > 0)
                pos += path.Nodes[i].ConnectedNodes[0].GetVector();
        }
        lineRenderer.positionCount = linePosCount;
        destinationPosition = pos;
    }

    public void EndNavigation()
    {
        destination = "";
        isTraveling = false;
        lineRenderer.positionCount = 0;
        destinationPosition = Vector3.zero;
    }
}
