using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DelayiedEventor : MonoBehaviour
    // Um script simples para tocar um evento ao ter o objeto ativo ou chamar um evento com um delay
{
    public float delay;

    public UnityEvent StartEvent;
    public UnityEvent DelayiedEvent;
    void Start()
    {
        StartEvent.Invoke();
    }

    public void TriggerDelayiedEvent()
    {
        StartCoroutine(delayiedEventCoroutine());
    }

    IEnumerator delayiedEventCoroutine()
    { 
        float endtime = Time.time + delay;

        while(Time.time < endtime) 
        {
            yield return null;
        }
        DelayiedEvent.Invoke();

    }
}
