using System;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class MeasureDistanceBeetweenTags : MonoBehaviour
{
    [SerializeField] ARTrackedImageManager m_TrackedImageManager;
    private NodeData referenceNode;
    private NodeData targetNode;

    [SerializeField] private GameObject startIndicator;
    [SerializeField] private GameObject endIndicator;

    [SerializeField] private Button saveButton;

    [SerializeField] private TextMeshProUGUI referenceNodeText;
    [SerializeField] private TextMeshProUGUI targetNodeText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI lockButtonText;

    [SerializeField] private TextMeshProUGUI imagesText;

    private bool referenceLocked = false;

    private void OnEnable()
    {
        referenceLocked = false;
        referenceNode = null;
        targetNode = null;

        startIndicator.SetActive(false);
        endIndicator.SetActive(false);
    }

    public void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        UpdateNodesReferences(eventArgs.added);
        UpdateNodesReferences(eventArgs.updated);
    }

    private void UpdateNodesReferences(ReadOnlyList<ARTrackedImage> images, bool first = false)
    {
        if(!this.isActiveAndEnabled) return;
        // Reset referenceNode if it's not locked
        if (!referenceLocked) referenceNode = null;
        targetNode=null;
        imagesText.SetText("");

        foreach (var newImage in images)
        {
            string imageID = newImage.referenceImage.name;
            imagesText.text += $"{imageID} | {newImage.trackingState}\n";

            // Continue only if the image is being tracked
            if (newImage.trackingState != UnityEngine.XR.ARSubsystems.TrackingState.Tracking) continue;

            if (!referenceLocked)
            {
                // Set the reference node if not locked
                if (referenceNode == null)
                {
                    referenceNode = new NodeData(imageID, imageID);
                    startIndicator.transform.position = newImage.transform.position;
                    startIndicator.SetActive(true);
                }
            }
            else if (targetNode == null || targetNode.tagId != imageID)
            {
                if(imageID == referenceNode.tagId) return;
                // Set the target node if reference is locked
                targetNode = new NodeData(imageID, imageID);
                endIndicator.transform.position = newImage.transform.position;
                endIndicator.SetActive(true);
            }
        }
    }

    public void ToggleLock()
    {
        referenceLocked = !referenceLocked;
        lockButtonText.SetText(referenceLocked ? "Unlock" : "Lock");

        if (!referenceLocked)
        {
            targetNode = null;
            endIndicator.SetActive(false);
        }
        else
        {
            startIndicator.SetActive(true);
            endIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        saveButton.interactable = (referenceNode!=null && targetNode!=null);

        XMLParser parser = MeasureManger.parser;
        if (parser == null) return;

        startIndicator.SetActive(referenceNode!=null);
        endIndicator.SetActive(targetNode!=null);

        referenceNodeText.SetText(referenceNode!=null ? referenceNode.tagId : "No ref node");
        targetNodeText.SetText(targetNode != null ? targetNode.tagId : "No target node");

        float dist = (referenceNode != null && targetNode != null)
            ? Vector3.Distance(startIndicator.transform.position, endIndicator.transform.position)
            : 0;

        distanceText.SetText($"{dist:F2}m");
    }

    public void SaveConnection()
    {
        if(referenceNode==null || targetNode==null)return;
        XMLParser parser = MeasureManger.parser;
        if (parser == null) return;


        Node start = null;
        Node end = null;
        for (int i = 0; i < parser.NodeList.Count; i++) 
        {
            Node node = parser.NodeList[i];
            if(node.TagId == referenceNode.tagId) start = node;
            if(node.TagId == targetNode.tagId) end = node;
            if (start != null && end != null) break; 
        }

        if(start == null)
        {
            parser.NodeList.Add(new Node(referenceNode.nodeName,referenceNode.tagId));
            start = parser.NodeList[parser.NodeList.Count - 1];
        }
        if (end == null) 
        {
            parser.NodeList.Add(new Node(targetNode.nodeName, targetNode.tagId));
            end = parser.NodeList[parser.NodeList.Count - 1];
        }

        if(start == null || end == null) return;

        start.AddConnection(targetNode.nodeName, endIndicator.transform.position - startIndicator.transform.position);
        end.AddConnection(referenceNode.nodeName, startIndicator.transform.position - endIndicator.transform.position);

    }
}
public class NodeData
{
    public string nodeName = "";
    public string tagId = "";

    public NodeData()
    {
        nodeName = "";
        tagId = "";
    }
    public NodeData(string nd, string id)
    {
        nodeName = nd;
        tagId = id;
    }
}
