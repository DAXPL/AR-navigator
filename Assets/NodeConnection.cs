using System.Collections.Generic;
using System;
using UnityEngine;

class NodeConnection
{
	NodeConnection() { }

	public NodeConnection(string to, List<float> relativePositions)
	{
		To = to;
		Relative_x = relativePositions[0];
		Relative_y = relativePositions[1];
		Relative_z = relativePositions[2];
		Distance = calculateDistance();
	}

	string To { get; set; }
	float Relative_x { get; set; }
	float Relative_y { get; set; }
	float Relative_z { get; set; }
	float Distance { get; set; }

	float calculateDistance()
	{
		float distance = (float)Math.Sqrt(Relative_x * Relative_x + Relative_y * Relative_y);
		distance = (float)Math.Sqrt(distance * distance + Relative_z * Relative_z);
		return distance;
	}
}
