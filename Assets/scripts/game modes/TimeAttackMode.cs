using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class TimeAttackMode : MonoBehaviour
    //Script para gerenciar o timer do modo timeAttack
{
    [Header("Dependencies")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TileManager manager;
    [SerializeField] private Animator anim;


    
    private float timeRemaining = 60f;//(60 = 1 minuto)
    private bool timerIsRunning = false;

    [Header("Events")]
    public UnityEvent OnTimerStartEvent;
    public UnityEvent OnTimerEndEvent;
    public UnityEvent OnTimerHideEvent;



    private void OnEnable()
    {
        manager.defeatEvent.AddListener(StopTimer);
        manager.victoryEvent.AddListener(StopTimer);
    }

    private void OnDisable()
    {
        manager.defeatEvent.RemoveListener(StopTimer);
        manager.victoryEvent.RemoveListener(StopTimer);
    }



    private void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                // Subtrai o tempo que passou desde o último frame
                timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay(timeRemaining);
            }
            else
            {
                // Tempo acaba
                timeRemaining = 0;
                timerIsRunning = false;
                UpdateTimerDisplay(timeRemaining);

                TimerEnded();
            }
        }
    }

    // Formata o tempo para o formato MM:SS
    private void UpdateTimerDisplay(float currentTime)
    {
        if (currentTime < 0) currentTime = 0;

        // Converte os segundos totais em minutos e segundos
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        // formata em rológio "{0:00}"
        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void TimerEnded()
    {
        Debug.Log("O tempo acabou!");

        OnTimerEndEvent?.Invoke();
        manager.Defeat();
        Debug.Log("defeat by time attack  mode");


    }


    public void SetTimer(float amount)
    {
        timeRemaining = amount +1;
        UpdateTimerDisplay(timeRemaining);
        timerIsRunning = true;
        OnTimerStartEvent?.Invoke();
        anim.Play("start timer", 0, 0f);

    }

    public void StopTimer()
    {
        timerIsRunning = false;
    }

    public void hideMode()
    {
        OnTimerHideEvent.Invoke();
        StopTimer();
    }
}