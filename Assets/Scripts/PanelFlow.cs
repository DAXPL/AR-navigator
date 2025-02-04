using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PanelFlow : MonoBehaviour
{
    [SerializeField] private UnityEvent FlowInStartEvent;
    [SerializeField] private UnityEvent FlowInEndEvent;
    [SerializeField] private UnityEvent FlowOutStartEvent;
    [SerializeField] private UnityEvent FlowOutEndEvent;
    public void FlowIn()
    {
        StopAllCoroutines();
        StartCoroutine(FlowUp());
    }

    public void FlowOut()
    {
        StopAllCoroutines();
        StartCoroutine(FlowUp(false));
    }

    private IEnumerator FlowUp(bool flowUp = true)
    {
        if(flowUp) FlowInStartEvent.Invoke();
        else FlowOutStartEvent.Invoke();

        RectTransform rectTransform = GetComponent<RectTransform>();
        float start = flowUp ? -rectTransform.rect.height:0 ;
        float end = flowUp ? 0: -rectTransform.rect.height;
        float time = 0;
        float transTime = 1;

        rectTransform.position = new Vector3(rectTransform.position.x, start, rectTransform.position.z);
        while (time <= transTime) 
        {
            rectTransform.position = new Vector3(rectTransform.position.x, Mathf.Lerp(start,end,(time/ transTime)), rectTransform.position.z);
            time += Time.deltaTime;
            yield return null;
        }
        rectTransform.position = new Vector3(rectTransform.position.x, end, rectTransform.position.z);
        if (flowUp) FlowInEndEvent.Invoke();
        else FlowOutEndEvent.Invoke();
    }
}
