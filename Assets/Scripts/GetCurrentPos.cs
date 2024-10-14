using TMPro;
using UnityEngine;

public class GetCurrentPos : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI output;
    [SerializeField] private Transform userCamera;
    public void DisplayCurrentPosition()
    {
        output.SetText($"Pos: {userCamera.position.x} | {userCamera.position.y} | {userCamera.position.z}");
    }
}
