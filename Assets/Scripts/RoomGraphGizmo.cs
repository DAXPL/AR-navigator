using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace Uninav.Preview
{

    [ExecuteAlways]
    public class RoomGraphGizmo : MonoBehaviour
    {
        [Header("Ustawienia")]
        [Tooltip("Plik XML z danymi w�z��w (drag & drop z Assets).")]
        public TextAsset xmlFile;

        [Tooltip("Kolor linii mi�dzy w�z�ami.")]
        public Color lineColor = Color.yellow;

        [Tooltip("Kolor g��wnych w�z��w (tag�w AR).")]
        public Color mainNodeColor = Color.cyan;

        [Tooltip("Kolor w�z��w pomniejszych.")]
        public Color subNodeColor = Color.green;

        [Tooltip("Promie� sfery gizmo.")]
        public float nodeRadius = 0.1f;

        private List<Node> nodes;

        private void OnDrawGizmos()
        {
            if (xmlFile == null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(transform.position, "Brak pliku XML!");
#endif
                return;
            }

            if (nodes == null || nodes.Count == 0)
            {
                LoadXML();
            }

            DrawNetwork();
        }

        private void LoadXML()
        {
            if (xmlFile == null) return;

            XmlSerializer serializer = new XmlSerializer(typeof(List<Node>));
            using (StringReader reader = new StringReader(xmlFile.text))
            {
                nodes = (List<Node>)serializer.Deserialize(reader);
            }
        }

        private void DrawNetwork()
        {
            if (nodes == null || nodes.Count == 0) return;

            Dictionary<string, Vector3> nodePositions = new Dictionary<string, Vector3>();

            // Ustal pozycj� startow� (pierwszy w�ze� przyjmujemy za (0,0,0))
            nodePositions[nodes[0].Name] = transform.position;

            foreach (Node node in nodes)
            {
                Vector3 basePos = nodePositions.ContainsKey(node.Name)
                    ? nodePositions[node.Name]
                    : transform.position;

                if (node.ConnectedNodes == null) continue;

                foreach (NodeConnection conn in node.ConnectedNodes)
                {
                    Vector3 targetPos = basePos + new Vector3(conn.Relative_x, conn.Relative_y, conn.Relative_z);

                    if (!nodePositions.ContainsKey(conn.To))
                        nodePositions[conn.To] = targetPos;

                    Gizmos.color = lineColor;
                    Gizmos.DrawLine(basePos, targetPos);
                }
            }

            foreach (var kvp in nodePositions)
            {
                string name = kvp.Key;
                Vector3 pos = kvp.Value;

                bool isMainNode = nodes.Exists(n => n.TagId == name);
                Gizmos.color = isMainNode ? mainNodeColor : subNodeColor;
                Gizmos.DrawSphere(pos, nodeRadius);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(pos + Vector3.up * 0.1f, name);
#endif
            }
        }
    }
    // ----------------------
    // Klasy odwzorowuj�ce XML
    // ----------------------

    [System.Serializable]
    public class Node
    {
        public string Name;
        public bool isTracked;
        public string TagId;
        public List<NodeConnection> ConnectedNodes;
    }

    [System.Serializable]
    public class NodeConnection
    {
        public string To;
        public bool IsWheelchairAccessible;
        public float Relative_x;
        public float Relative_y;
        public float Relative_z;
    }

}