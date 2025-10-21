using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace Uninav.Preview
{

    [ExecuteAlways]
    public class RoomGraphGizmo : MonoBehaviour
    {
        
        [Header("XML")]
        public TextAsset xmlFile; // przypisz data.xml
        [Tooltip("Skaluje wartoœci Relative_x/y/z z XML do jednostek Unity")]
        public float scale = 0.01f;

        [Header("Drawing")]
        public float nodeSphereRadius = 0.25f;
        public Color anchorColor = Color.yellow;
        public Color roomColor = Color.cyan;
        public Color lineColor = Color.white;
        public bool drawLabels = true;
        public GUIStyle labelStyle;

        // internal cache
        private ArrayOfNode parsed;
        private string lastXmlText;

        [Header("Transform corrections")]
        [Tooltip("Odbicie osi — pozwala 'wywróciæ' uk³ad jeœli pozycje s¹ odwrócone.")]
        public bool flipX = false;
        public bool flipY = false;
        public bool flipZ = false;

        void OnValidate()
        {
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle();
                labelStyle.normal.textColor = Color.white;
                labelStyle.fontSize = 12;
            }
        }

        void OnDrawGizmos()
        {
            if (xmlFile == null) return;

            if (lastXmlText != xmlFile.text)
            {
                parsed = TryParseXml(xmlFile.text);
                lastXmlText = xmlFile.text;
            }

            if (parsed == null) return;

            // Build positions:
            // - anchoredNodes: nodes that have a numeric TagId we place on a circle
            // - For each connection from a source node, compute target position = sourcePos + relative*scale
            Dictionary<string, Vector3> positions = new Dictionary<string, Vector3>();
            HashSet<string> isAnchor = new HashSet<string>();
            List<Node> nodes = new List<Node>(parsed.Node);

            // Determine anchors (nodes with TagId)
            List<Node> anchors = new List<Node>();
            foreach (var n in nodes)
            {
                if (!string.IsNullOrEmpty(n.TagId))
                {
                    // TagId might be numeric (like "6") or missing for named rooms
                    anchors.Add(n);
                    isAnchor.Add(n.Name);
                }
            }

            // Place anchors on a circle so they are separated
            float radius = Mathf.Max(2f, anchors.Count * 1.5f);
            for (int i = 0; i < anchors.Count; i++)
            {
                var a = anchors[i];
                float angle = (i / (float)anchors.Count) * Mathf.PI * 2f;
                Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
                positions[a.Name] = pos;
            }

            // Now iterate connections and compute/add positions for connected targets.
            // If a target already has a position from another source, we average them (simple smoothing).
            Dictionary<string, int> positionCounts = new Dictionary<string, int>();
            // initialize counts for anchors
            foreach (var kv in positions) positionCounts[kv.Key] = 1;

            foreach (var src in nodes)
            {
                // ensure source has a position (if not anchor, give small random offset near origin)
                if (!positions.ContainsKey(src.Name))
                {
                    positions[src.Name] = Vector3.zero;
                    positionCounts[src.Name] = 1;
                }

                Vector3 srcPos = positions[src.Name];

                if (src.ConnectedNodes != null && src.ConnectedNodes.NodeConnection != null)
                {
                    foreach (var conn in src.ConnectedNodes.NodeConnection)
                    {
                        string targetName = conn.To;
                        if (string.IsNullOrEmpty(targetName)) continue;

                        // compute target position = srcPos + relative*scale
                        Vector3 rel = new Vector3(
    ToFloatSafe(conn.Relative_x),
    ToFloatSafe(conn.Relative_y),
    ToFloatSafe(conn.Relative_z)
) * scale;

                        // zastosuj odbicia osi
                        if (flipX) rel.x = -rel.x;
                        if (flipY) rel.y = -rel.y;
                        if (flipZ) rel.z = -rel.z;

                        Vector3 candidate = srcPos + rel;

                        if (positions.ContainsKey(targetName))
                        {
                            // average positions (running average)
                            positions[targetName] = (positions[targetName] * positionCounts[targetName] + candidate) / (positionCounts[targetName] + 1);
                            positionCounts[targetName] += 1;
                        }
                        else
                        {
                            positions[targetName] = candidate;
                            positionCounts[targetName] = 1;
                        }
                    }
                }
            }

            // Draw nodes
            foreach (var kv in positions)
            {
                string name = kv.Key;
                Vector3 pos = kv.Value + transform.position; // local offset by this GameObject
                bool anchor = isAnchor.Contains(name);

                Gizmos.color = anchor ? anchorColor : roomColor;
                Gizmos.DrawSphere(pos, nodeSphereRadius);

                // draw label via Handles (Editor only) — use Unity's built-in label method in runtime safe way
#if UNITY_EDITOR
                if (drawLabels)
                {
                    UnityEditor.Handles.BeginGUI();
                    var screenPoint = UnityEditor.HandleUtility.WorldToGUIPoint(pos);
                    var rect = new Rect(screenPoint.x + 8, screenPoint.y - 12, 300, 24);
                    GUI.Label(rect, name, labelStyle);
                    UnityEditor.Handles.EndGUI();
                }
#endif
            }

            // Draw connections (lines)
            Gizmos.color = lineColor;
            foreach (var src in nodes)
            {
                if (!positions.ContainsKey(src.Name)) continue;
                Vector3 srcPos = positions[src.Name] + transform.position;

                if (src.ConnectedNodes == null || src.ConnectedNodes.NodeConnection == null) continue;
                foreach (var conn in src.ConnectedNodes.NodeConnection)
                {
                    if (string.IsNullOrEmpty(conn.To)) continue;
                    if (!positions.ContainsKey(conn.To)) continue;

                    Vector3 targetPos = positions[conn.To] + transform.position;
                    Gizmos.DrawLine(srcPos, targetPos);

                    // optional: draw small arrow/point halfway
                    Vector3 mid = Vector3.Lerp(srcPos, targetPos, 0.5f);
                    Gizmos.DrawCube(mid, Vector3.one * nodeSphereRadius * 0.4f);
                }
            }
        }

        private ArrayOfNode TryParseXml(string xml)
        {
            try
            {
                var xs = new XmlSerializer(typeof(ArrayOfNode));
                using (StringReader sr = new StringReader(xml))
                {
                    var obj = (ArrayOfNode)xs.Deserialize(sr);
                    return obj;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("XML parse error: " + ex.Message);
                return null;
            }
        }

        private float ToFloatSafe(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0f;
            float v = 0f;
            float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v);
            return v;
        }
    }

    #region XML classes
    // generated lightweight classes matching your XML structure
    [XmlRoot("ArrayOfNode")]
    public class ArrayOfNode
    {
        [XmlElement("Node")]
        public Node[] Node { get; set; }
    }

    public class Node
    {
        public string Name { get; set; }
        public string isTracked { get; set; }
        public string TagId { get; set; }

        public ConnectedNodes ConnectedNodes { get; set; }
    }

    public class ConnectedNodes
    {
        [XmlElement("NodeConnection")]
        public NodeConnection[] NodeConnection { get; set; }
    }

    public class NodeConnection
    {
        public string To { get; set; }
        public string IsWheelchairAccessible { get; set; }
        public string Relative_x { get; set; }
        public string Relative_y { get; set; }
        public string Relative_z { get; set; }
    }
    #endregion

}