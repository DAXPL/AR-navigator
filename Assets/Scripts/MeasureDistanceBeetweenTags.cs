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
    private string referenceNode="";
    private string targetNode="";

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
        referenceNode = "";
        targetNode = "";

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
            string imageName = newImage.referenceImage.name;
            imagesText.text += $"{imageName} | {newImage.trackingState}\n";

            // Continue only if the image is being tracked
            if (newImage.trackingState != UnityEngine.XR.ARSubsystems.TrackingState.Tracking) continue;

            if (!referenceLocked)
            {
                // Set the reference node if not locked
                if (referenceNode == null || referenceNode == "")
                {
                    referenceNode = imageName;
                    startIndicator.transform.position = newImage.transform.position;
                    startIndicator.SetActive(true);
                }
            }
            else if (targetNode != imageName)
            {
                if(imageName == referenceNode) return;
                // Set the target node if reference is locked
                targetNode = imageName;
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
            targetNode = "";
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
        saveButton.interactable = (!string.IsNullOrEmpty(referenceNode) && !string.IsNullOrEmpty(targetNode));

        XMLParser parser = MeasureManger.parser;
        if (parser == null) return;

        startIndicator.SetActive(!string.IsNullOrEmpty(referenceNode));
        endIndicator.SetActive(!string.IsNullOrEmpty(targetNode));

        referenceNodeText.SetText(!string.IsNullOrEmpty(referenceNode) ? referenceNode : "No ref node");
        targetNodeText.SetText(!string.IsNullOrEmpty(targetNode) ? targetNode : "No target node");

        float dist = (!string.IsNullOrEmpty(referenceNode) && !string.IsNullOrEmpty(targetNode))
            ? Vector3.Distance(startIndicator.transform.position, endIndicator.transform.position)
            : 0;

        distanceText.SetText($"{dist:F2}m");
    }

    public void SaveConnection()
    {
        if(string.IsNullOrEmpty(referenceNode) || string.IsNullOrEmpty(targetNode))return;
        XMLParser parser = MeasureManger.parser;
        if (parser == null) return;


        Node start = null;
        Node end = null;
        for (int i = 0; i < parser.NodeList.Count; i++) 
        {
            Node node = parser.NodeList[i];
            if(node.Name == referenceNode) start = node;
            if(node.Name == targetNode) end = node;
            if (start != null && end != null) break; 
        }

        if(start == null)
        {
            parser.NodeList.Add(new Node(referenceNode));
            start = parser.NodeList[parser.NodeList.Count - 1];
        }
        if (end == null) 
        {
            parser.NodeList.Add(new Node(targetNode));
            end = parser.NodeList[parser.NodeList.Count - 1];
        }

        if(start == null || end == null) return;

        start.AddConnection(targetNode, endIndicator.transform.position - startIndicator.transform.position);
        end.AddConnection(referenceNode, startIndicator.transform.position - endIndicator.transform.position);

    }
}
