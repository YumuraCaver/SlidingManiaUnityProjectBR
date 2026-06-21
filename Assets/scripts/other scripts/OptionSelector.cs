using UnityEngine;

public class OptionSelector: MonoBehaviour
    // Script simples para mover um objeto a um filho de uma grid, usado apenas para destacá-lo, como se estivesse ativo/selecionado
{
    [SerializeField] private Transform parent;
    [SerializeField] private Transform objectToMove;

    public void MoveToIndex(int index)
    {
        if (index < 0 || index >= parent.childCount)
        {
            Debug.LogWarning("Child index out of range!");
            return;
        }

        objectToMove.position = parent.GetChild(index).position;
    }
}