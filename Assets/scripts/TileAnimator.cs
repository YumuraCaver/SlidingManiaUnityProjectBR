using UnityEngine;
using UnityEngine.EventSystems;

public class TileAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler
    // Anima o tile conforme é arrastado, aumentando-o e esticando-o
    // aumenta o tamanho quando o mouse está sobre também
{
    [Header("Dependencies")]
    [Tooltip("O objeto que irá sofrer as deformações")]
    [SerializeField] private Transform visualTarget;


    [Header("Config")]
    [Tooltip("Aumento de escala quando o mouse está sobre o tile ou quando está sendo arrastando")]
    [SerializeField] private float hoverScale = 1.15f;
    [Tooltip("Velocidade do aumento")]
    [SerializeField] private float scaleSpeed = 15f;

    // Squash e stretch
    [Header("drag")]
    [Tooltip("O quanto ele estica baseado na velocidade")]
    [SerializeField] private float stretchMultiplier = 0.05f;
    [Tooltip("O máximo que ele pode esticar para não distorcer demais")]
    [SerializeField] private float maxStretch = 0.4f;


    [Header("Animação de gelatina (Drop)")]
    [SerializeField] private float jellyDuration = 0.5f;
    [SerializeField] private float jellyAmplitude = 0.3f;
    [SerializeField] private float jellyFrequency = 25f;

    [Header("sfx")]
    [SerializeField] private string SFXtileSlide = "tile slide";
    [SerializeField] private string SFXtileGrab = "tile grab";
    [SerializeField] private string SFXtileHover = "tile hover";
    [SerializeField] private string SFXtileRelease = "tile release";


    // estados
    private bool isHovering = false;
    private bool isDragging = false;

    // cálculo de velocidade
    private Vector3 lastPosition;
    private Vector3 currentVelocity;

    // Variáveis da gelatina
    private float jellyTimer = 0f;
    private Vector3 originalLocalScale;

    // sfx
    bool alreadyPlayiedSlideSFX = false;

    public void resetSlideSFX()
    {
        alreadyPlayiedSlideSFX = false;
    }


    private void Start()
    {
        lastPosition = transform.position;
        if (visualTarget != null)
        {
            originalLocalScale = visualTarget.localScale;
        }
        jellyTimer = jellyDuration;
    }

    private void Update()
    {
        if (visualTarget == null) return;

        // calculo de velocidade do drag
        currentVelocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        // A deformação
        HandleScaleAndDeformation();
    }

    private void HandleScaleAndDeformation()
    {
        // Define se deve estar maior (Hover ou Drag)
        float targetBaseScaleMult = (isHovering || isDragging) ? hoverScale : 1f;
        Vector3 targetScale = originalLocalScale * targetBaseScaleMult;

        // stretch & squash
        if (isDragging)
        {
            float speedX = Mathf.Abs(currentVelocity.x);
            float speedY = Mathf.Abs(currentVelocity.y);

            float stretchX = Mathf.Clamp(speedX * stretchMultiplier, 0, maxStretch);
            float stretchY = Mathf.Clamp(speedY * stretchMultiplier, 0, maxStretch);

            if (stretchY == maxStretch && !alreadyPlayiedSlideSFX || stretchX == maxStretch && !alreadyPlayiedSlideSFX)
            {
                AudioManager.Instance.Play(SFXtileSlide);
                alreadyPlayiedSlideSFX = true;

            }

            targetScale.x += stretchX - (stretchY * 0.5f);
            targetScale.y += stretchY - (stretchX * 0.5f);
        }

        // Efeito gelatina (Wobble) no drop
        if (jellyTimer > 0)
        {
            jellyTimer -= Time.deltaTime;

            float timeRatio = 1f - (jellyTimer / jellyDuration);
            float wobble = Mathf.Sin(timeRatio * jellyFrequency) * jellyAmplitude * Mathf.Exp(-timeRatio * 6f);

            targetScale.x += wobble;
            targetScale.y -= wobble;
        }

        // aumento de escala suavizada
        visualTarget.localScale = Vector3.Lerp(visualTarget.localScale, targetScale, Time.deltaTime * scaleSpeed);


    }



    // Funções UI Unity
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        if (!Input.GetMouseButton(0))
        {
            AudioManager.Instance.Play(SFXtileHover);
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        jellyTimer = 0f;
        //AudioManager.Instance.Play(SFXtileGrab);
        AudioManager.Instance.Play(SFXtileSlide);
        alreadyPlayiedSlideSFX = true;


    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        jellyTimer = jellyDuration;
        AudioManager.Instance.Play(SFXtileRelease);


    }
}