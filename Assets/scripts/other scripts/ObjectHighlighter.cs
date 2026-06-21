using Unity.VisualScripting;
using UnityEngine;

public class ObjectHighlighter : MonoBehaviour
    // Destaca objetos salvando-os em uma lista e posicionando um deles em uma posição destacada, enquanto que deixa os outros de fora
    // Destacar um objeto diferente irá mover o atual para fora e o novo para a posição.
{
    [Tooltip("on screen")]
    [SerializeField] private Transform onPosition;
    [Tooltip("off screen, para o objeto vai ao sair")]
    [SerializeField] private Transform outPositionEnd;
    [Tooltip("off screen, de onde o objeto virá ao entrar na tela")]
    [SerializeField] private Transform outPositionStart;

    [SerializeField] private GameObject[] objects;

    [SerializeField] private float moveSpeed = 10f;

    private int selectedIndex = -1;

    public void Show(int index)
    {
        if (index < 0 || index >= objects.Length)
        {
            Debug.LogWarning("Index out of range.");
            return;
        }

        if (index != selectedIndex)
        {
            objects[index].transform.position = outPositionStart.position;
        }

        selectedIndex = index;
    }

    public void Hide()
    {
        selectedIndex = -1;
    }

    private void Start()
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.transform.position = outPositionEnd.position;
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null)
                continue;

            Vector3 targetPosition;
            bool diactivates;

            if (i == selectedIndex)
            {
                targetPosition = onPosition.position;
                diactivates = false;
                objects[i].SetActive(true);
            }
            else
            {
                targetPosition = outPositionEnd.position;
                diactivates = true;

            }

            objects[i].transform.position = Vector3.Lerp(objects[i].transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if(diactivates)
            { 
                float distance = Vector3.Distance(objects[i].transform.position, targetPosition);
                if (distance < 1f)
                { 
                objects[i].SetActive(false);
                
                }
            }
            


        }
    }
}