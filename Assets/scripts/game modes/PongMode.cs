using UnityEngine;
using UnityEngine.Events;

public class PongMode : MonoBehaviour
    // Script que gerencia o modo pong, disparando uma bola pong e controlando uma barra inimiga que a rebate
{
    [Header("Dependencies")]
    public GameObject ballPrefab;
    public Transform spawnPoint;
    public Vector2 launchForce;
    public TileManager manager;
    [Tooltip("a barra inimiga que irá bater na bola")]
    public Transform bar;
    [Tooltip("o ponto em que a barra irá começar a se mover se a bola passar por ela (a barra deve estar do lado direito)")]
    public Transform reactingArea;
    [Tooltip("o quão rápido a barra se move")]
    public float barSmoothSpeed = 5f;

    [Header("Events")]
    public UnityEvent pongDefeatEvent;
    public UnityEvent pongStartEvent;
    public UnityEvent pongHideEvent;

    private GameObject currentBall;
    private float lastBallPos;//usado para saber se está indo para a esquerda ou não

    private void Start()
    {
        hideMode();
    }
    public void SpawnAndLaunchBall()
    {
        pongStartEvent.Invoke();

        if (currentBall != null)
        {
            Destroy(currentBall);
        }

        // Cria e lança a bola
        currentBall = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
        currentBall.transform.SetParent(this.gameObject.transform, false);
        currentBall.transform.position = spawnPoint.position;
        Rigidbody2D rb = currentBall.GetComponent<Rigidbody2D>();
        Animator anim = currentBall.GetComponent<Animator>();
        anim.Play("bounce", 0, 0f);

        if (rb != null)
        {
            rb.AddForce(launchForce, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogError("faltou o Rigidbody2D na bola!");
        }

        lastBallPos = currentBall.transform.position.x;

    }

    private void Update()
    {
        if (currentBall != null)
        {
            // checa se a bola passou do ponto de reação da barra inimiga para ela reagir e a move
            if (currentBall.transform.position.x > reactingArea.position.x && lastBallPos < currentBall.transform.position.x)
            {
                Vector3 targetPos = new Vector3(bar.position.x, currentBall.transform.position.y, bar.position.z);
                bar.position = Vector3.Lerp(bar.position, targetPos, barSmoothSpeed * Time.deltaTime);
            }
            lastBallPos = currentBall.transform.position.x;

        }
    }

    //rebate a bola
    public void pongBack(Transform tilepos)
    {
        if (currentBall != null)
        {
            Rigidbody2D rb = currentBall.GetComponent<Rigidbody2D>();
            Animator anim = currentBall.GetComponent<Animator>();
            anim.Play("bounce", 0, 0f);
            Vector2 velocity = rb.velocity;

            if (velocity.x < 0)
            {
                velocity.x = launchForce.x;
            }

            if (tilepos.position.y < currentBall.transform.position.y)
            {
                velocity.y = -launchForce.y;
            }
            else
            {
                velocity.y = launchForce.y;
            }

            rb.velocity = velocity;

        }
    }


    public void hideMode()
    {
        pongHideEvent.Invoke();
        if (currentBall != null)
        {
            Destroy(currentBall);
        }
    }

    //chamado quando a bola entra no gol do jogador
    public void defeatPong()
    {
        if (currentBall != null)
        {
            Destroy(currentBall);
        }

        pongDefeatEvent?.Invoke();
        manager.Defeat();
        Debug.Log("defeat pong mode");

    }
}