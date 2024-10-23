using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class GetCurrentPos : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI positionOutput;
    [SerializeField] private Transform userCamera;

    [SerializeField] private GameObject measurePoint;

    private void Update()
    {
        Vector3 relPos = measurePoint.transform.position - userCamera.position;
        float distance = Vector3.Distance(measurePoint.transform.position, userCamera.position);
        positionOutput.SetText($"{relPos.x.ToString("n2")}\n{relPos.y.ToString("n2")}\n{relPos.z.ToString("n2")}\n{distance.ToString("n2")}m");
    }
  
    public void ResetRelativePos()
    {
        if (measurePoint == null) return;
        measurePoint.SetActive(true);
        measurePoint.transform.position = userCamera.position;
    }
}
