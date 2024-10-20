using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class NavigationManager : MonoBehaviour
{
    [SerializeField] private GameObject nodeIndicator;
    private XMLParser parser;
    private Vector3 offset;
    private void Start()
    {
        parser = new XMLParser();

    }
    public void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        List<Vector3> estimations = new List<Vector3>();
        foreach (var newImage in eventArgs.added)
        {
            Debug.Log($"Detected: {newImage.referenceImage.name}");

            estimations.Add(EstimatePosition());
        }

        foreach(var image in eventArgs.updated)
        {
            estimations.Add(EstimatePosition());
        }
        offset = AverageOfMeasurements(estimations);
    }
    private void UpdateNodesIndicators()
    {
        //iterate through list, and based on image set all points
    }
    private Vector3 EstimatePosition()
    {
        //Estimates position based on current cam position and detected marker distance
        return new Vector3(0, 0, 0);
    }
    private Vector3 AverageOfMeasurements(List<Vector3> estimations)
    {
        Vector3 avg = new Vector3(0, 0, 0);
        foreach (var estimation in estimations) 
        {
            avg += estimation;
        }
        return avg/estimations.Count;
    }
}
