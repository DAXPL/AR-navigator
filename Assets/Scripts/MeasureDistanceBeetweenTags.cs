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
    private Node referenceNode;
    private Node targetNode;

    [SerializeField] private GameObject startIndicator;
    [SerializeField] private GameObject endIndicator;

    [SerializeField] private Button saveButton;

    [SerializeField] private TextMeshProUGUI referenceNodeText;
    [SerializeField] private TextMeshProUGUI targetNodeText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI lockButtonText;

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
        UpdateNodesReferences(eventArgs.added, true);
        UpdateNodesReferences(eventArgs.updated);
    }

    private void UpdateNodesReferences(ReadOnlyList<ARTrackedImage> images, bool first = false)
    {
        XMLParser parser = MeasureManger.parser;
        if (parser == null) return;

        foreach (var newImage in images)
        {
            for (int i = 0; i < parser.NodeList.Count; i++)
            {
                Node node = parser.NodeList[i];
                if (node.Name == newImage.referenceImage.name)
                {
                    if (referenceLocked == false)
                    {
                        referenceNode = node;
                        startIndicator.SetActive(true);
                        startIndicator.transform.position = newImage.transform.position;
                        return;
                    }
                    else if (targetNode != node)
                    {
                        targetNode = node;
                        endIndicator.SetActive(true);
                        endIndicator.transform.position = newImage.transform.position;
                        return;
                    }
                    else
                    {
                        targetNode = null;
                        endIndicator.SetActive(true);
                        return;
                    }
                }
            }
            if (first == true && referenceNode == null)
            {
                parser.NodeList.Add(new Node(newImage.referenceImage.name));
                referenceNode = parser.NodeList[parser.NodeList.Count - 1];
            }

        }
    }

    public void ToggleLock()
    {
        referenceLocked = !referenceLocked;
        lockButtonText.SetText(referenceLocked ? $"Unlock" : $"Lock");
        if (referenceLocked == false)
        {
            targetNode = null;
            startIndicator.SetActive(false);
            endIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        saveButton.interactable = (referenceNode != null && targetNode != null);

        referenceNodeText.SetText(referenceNode != null ? referenceNode.Name : "No ref node");
        targetNodeText.SetText(targetNode != null ? targetNode.Name : "No target node");

        float dist = referenceNodeText != null && targetNode != null ? Vector3.Distance(startIndicator.transform.position, endIndicator.transform.position) : 0;
        distanceText.SetText($"{dist}m");
    }

    public void SaveConnection()
    {
        if (referenceNode == null || targetNode == null) return;

        referenceNode.AddConnection(targetNode.Name, endIndicator.transform.position - startIndicator.transform.position);
        targetNode.AddConnection(referenceNode.Name, startIndicator.transform.position - endIndicator.transform.position);

    }
}
