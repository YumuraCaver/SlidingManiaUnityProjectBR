using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfettiMaker : MonoBehaviour
    // Cria confeti para a vitória do jogador
    // dispara várias imagens UI para uma direção com variação de angulo
{
    [Header("Dependencies")]
    public GameObject confettiPrefab;
    public Transform spawnPoint;


    [Header("Config")]
    public List<Color> confettiColors;
    public int numberOfConfetti = 50;
    [Tooltip("intervalo entre confetis")]
    public float spawnSpeed = 0.02f;
    [Tooltip("quantos spawns até decidir randomizar uma nova variação de angulo do disparo do confeti")]
    public int randomizeEveryNSpawns = 3;
    [Tooltip("Direção do disparo do confetti")]
    public Vector2 direction = Vector2.up;
    [Tooltip("Força do disparo")]
    public float force = 800f;
    [Tooltip("variação máxima do angulo do disparo")]
    public float maxDeviationAngle = 45f;
    [Tooltip("A gravidade base do confete. Será escalada junto com a força para manter o visual consistente em qualquer resolução.")]
    public float baseGravityScale = 15f;


    private Canvas parentCanvas;

    private void Start()
    {
        if (spawnPoint != null)
        {
            parentCanvas = spawnPoint.GetComponentInParent<Canvas>();
        }
    }

    public void Celebrate()
    {
        if (confettiPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("Confetti prefab or spawn point is missing!");
            return;
        }

        StartCoroutine(SpawnConfettiRoutine());
    }

    private IEnumerator SpawnConfettiRoutine()
    {
        Vector2 currentDirection = direction.normalized;

        // Pega o fator de escala do canvas (se não achar, usa 1 como padrão)
        float currentScaleFactor = parentCanvas != null ? parentCanvas.scaleFactor : 1f;

        for (int i = 0; i < numberOfConfetti; i++)
        {
            // randomiza um novo angulo para atirar o confeti, mas somente depois de um numero de spawn desde a última randomizada
            if (i % randomizeEveryNSpawns == 0)
            {
                currentDirection = GetDeviatedDirection(direction, maxDeviationAngle);
            }

            // Cria o confeti
            GameObject confetti = Instantiate(confettiPrefab, spawnPoint.position, Quaternion.identity, spawnPoint);

            // escolhe a cor de forma sequencial
            if (confettiColors != null && confettiColors.Count > 0)
            {
                Image image = confetti.GetComponent<Image>();
                if (image != null)
                {
                    int colorIndex = i % confettiColors.Count;
                    image.color = confettiColors[colorIndex];
                }
            }

            // aplica força compensando o tamanho da tela
            Rigidbody2D rb = confetti.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = baseGravityScale * currentScaleFactor;
                rb.AddForce(currentDirection * (force * currentScaleFactor));
            }

            // intervalo de spawn
            if (spawnSpeed > 0)
            {
                yield return new WaitForSeconds(spawnSpeed);
            }
        }
    }

    // randomiza o angulo
    private Vector2 GetDeviatedDirection(Vector2 baseDir, float maxAngle)
    {
        float randomAngle = Random.Range(-maxAngle, maxAngle);
        float radians = randomAngle * Mathf.Deg2Rad;

        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            baseDir.x * cos - baseDir.y * sin,
            baseDir.x * sin + baseDir.y * cos
        ).normalized;
    }
}