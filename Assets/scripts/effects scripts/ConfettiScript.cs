using UnityEngine;

public class ConfettiScript : MonoBehaviour
{
    [Header("Tempo de vida")]
    public float lifespan = 5f;


    private Vector3 spinVector;

    private void Start()
    {
        spinVector = new Vector3(500, 500, 500);

        Destroy(gameObject, lifespan);
    }

    private void Update()
    {
        // Rotaciona
        transform.Rotate(spinVector * Time.deltaTime);
    }

   
}