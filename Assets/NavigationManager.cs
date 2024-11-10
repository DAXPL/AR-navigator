using System;
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

            // ZnajdŸ aktualny wêze³ na podstawie nazwy obrazu
            Node currentNode = xmlParser.NodeList.FirstOrDefault(n => n.Name == image.referenceImage.name);
            if (currentNode == null) continue;

            // Pozycja g³ównego wêz³a
            Vector3 currentPosition = image.transform.position;

            // PrzejdŸ przez po³¹czone wêz³y
            foreach (NodeConnection connection in currentNode.ConnectedNodes)
            {
                // Oblicz wzglêdn¹ pozycjê po³¹czonego wêz³a (podwêz³a) w przestrzeni AR
                Vector3 relativePosition = new Vector3(connection.Relative_x, connection.Relative_y, connection.Relative_z);
                Vector3 connectedPosition = currentPosition + relativePosition;

                // SprawdŸ, czy po³¹czony wêze³ istnieje w `NodeList` jako g³ówny wêze³
                bool isMainNode = xmlParser.NodeList.Any(n => n.Name == connection.To);

                // Wybierz kolor linii:
                // - Czerwona, jeœli po³¹czony wêze³ to g³ówny wêze³
                // - ¯ó³ta, jeœli to podwêze³
                Color lineColor = isMainNode ? Color.red : Color.yellow;

                // Narysuj liniê od currentPosition do connectedPosition
                Debug.DrawRay(currentPosition, connectedPosition - currentPosition, lineColor, 1f);
            }
        }
    }
}
