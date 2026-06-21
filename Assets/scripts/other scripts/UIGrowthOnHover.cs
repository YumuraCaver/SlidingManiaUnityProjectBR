using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIGrowthOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    //Script simples para aumentar o tamanho do objeto quando o jogador passar o mouse em cima, e tocar um evento tbm
{
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private UnityEvent onHoverEvent;
    [SerializeField] private UnityEvent onHoverOffEvent;

    private Vector3 originalScale;
    private bool hasHovered;

    private void Awake()
    {
        originalScale = transform.localScale;
        transform.localScale = originalScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * hoverScale;

        if (!hasHovered)
        {
            onHoverEvent?.Invoke();
            hasHovered = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
        hasHovered = false;
        onHoverOffEvent?.Invoke();
    }
}
