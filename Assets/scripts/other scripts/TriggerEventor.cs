using UnityEngine;
using UnityEngine.Events;

public class TriggerDetector2D : MonoBehaviour
    // Script simples para disparar um evento quando algo entra em seu collider 2d trigger
{
    [Tooltip("Se ativado, apenas objetos com a Tag especificada vão ativar o gatilho.")]
    public bool filterByTag = false;
    public string targetTag;

    [Header("Eventos")]
    public UnityEvent OnTriggerEnter;

    public UnityEvent OnTriggerExit;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (filterByTag && !collision.CompareTag(targetTag)) return;

        OnTriggerEnter?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (filterByTag && !collision.CompareTag(targetTag)) return;

        OnTriggerExit?.Invoke();
    }
}