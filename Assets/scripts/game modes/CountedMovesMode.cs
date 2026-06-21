using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CountedMovesMode : MonoBehaviour
    // script para contar os moves do jogador no modo Counted moves
    // representa os moves através de um contador e também atravéz de uma barra, estilo bateria
{
    public int movesLeft;

    [Header("Dependencies")]
    public TileManager manager;
    public Slider slider;
    public Image fillSlider;
    public int dangerThreshold;
    public Animator anim;
    public TextMeshProUGUI text;

    [Header("Events")]
    public UnityEvent OnCounterStartEvent;
    public UnityEvent OnCounterHideEvent;

    // Começa a contagem
    public void setMovesCount(int i)
    {
        movesLeft = i;
        if (i < 20)
        {
            slider.maxValue = 20;
        }
        else
        { 
            slider.maxValue = i;
        
        }
        slider.minValue = 0;
        slider.value = i;
        text.text = i.ToString();
        UpdateColor(i);

        OnCounterStartEvent.Invoke();
    }

    // Gasto de um move, chamado pelos Tiles
    public void move()
    {
        if (manager.alreadyGotResults) return;

        movesLeft--;
        if (movesLeft <= 0)
        {
            manager.Defeat();
            Debug.Log("defeat by counted moves mode");
        }


        slider.value = movesLeft;
        text.text = movesLeft.ToString();


        UpdateColor(movesLeft);
        anim.Play("move",0, 0f);
    }

    // Avermelha a barra conforme acaba
    private void UpdateColor(float currentValue)
    {
        if (currentValue >= dangerThreshold)
        {
            // Safe zone: Keep it pure white
            fillSlider.color = Color.white;
        }
        else
        {
            // Danger zone: Blend between Red (at 0) and White (at the threshold)
            // Example: If value is 15 and threshold is 30, percentage is 0.5 (halfway red)
            float percentage = currentValue / dangerThreshold;

            fillSlider.color = Color.Lerp(Color.red, Color.white, percentage);
        }
    }

    // Esconde o objetos relacionados ao modo quando não está ativo, chamado pelo tile manager
    public void hideMode()
    {
        OnCounterHideEvent.Invoke();
    }

}
