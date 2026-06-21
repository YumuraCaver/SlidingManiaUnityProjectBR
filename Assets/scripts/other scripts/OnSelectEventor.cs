using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnSelectEventor : MonoBehaviour,    IPointerEnterHandler,    IPointerExitHandler
{
    // Script simples para aumentar o tamanho do objeto quando o jogador passar o mouse em cima, e tocar um evento tbm
    // Este script toca eventos ao passar o mouse por cima de elementos UI visuais do objto com este script
    // Facilita eu conectar certos métodos a estes eventos atravé do unity editor

    [SerializeField] private string sfxOnHover = "UIhover";
    [SerializeField] private string sfxOnClick = "UIclick";
    [SerializeField] private float hoverScale = 1.05f;

    public UnityEvent onHighlight;
    public UnityEvent onUnHighlight;

    private Vector3 originalScale;
    private bool hasHovered;
    private Button button;



    private void Awake()
    {
        originalScale = transform.localScale;
        transform.localScale = originalScale;

        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(ButtonClicked);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * hoverScale;
        if(sfxOnHover != null) 
        {
            AudioManager.Instance.Play(sfxOnHover);
        }

        if (!hasHovered)
        {
            onHighlight?.Invoke();
            hasHovered = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
        hasHovered = false;
        onUnHighlight?.Invoke();
    }

    void ButtonClicked()
    {
        if (sfxOnClick != null)
        {
            AudioManager.Instance.Play(sfxOnClick);
        }
    }
}