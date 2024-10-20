using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public class NodeConnection
{
	NodeConnection() { }

	public NodeConnection(string to, Vector3 relativePosition)
	{
		To = to;
		Relative_x = relativePosition.x;
		Relative_y = relativePosition.y;
		Relative_z = relativePosition.z;
		RelativePosition = relativePosition;
		Distance = calculateDistance();
	}

	public string To { get; set; }
	public float Relative_x { get; set; }
	public float Relative_y { get; set; }
	public float Relative_z { get; set; }
	Vector3 RelativePosition { get; set; }
	float Distance { get; set; }

	public Vector3 GetVector()
	{ return RelativePosition; }

	public float GetDistance()
	{ return Distance; }

	float calculateDistance()
	{
		float distance = (float)Math.Sqrt(RelativePosition.x * RelativePosition.x + RelativePosition.y * RelativePosition.y);
		distance = (float)Math.Sqrt(distance * distance + RelativePosition.z * RelativePosition.z);
		return distance;
	}
}
