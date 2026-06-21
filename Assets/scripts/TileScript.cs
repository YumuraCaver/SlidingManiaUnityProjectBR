using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using Unity.VisualScripting;

public class TileScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Dependencies")]
    public TextMeshProUGUI numberText;
    public Animator blinderAnimator;
    public TileAnimator tileAnimator;
    public Image tileImage;

    [Header("Config")]
    [Range(0.01f, 1.0f)]
    [Tooltip("Responsividade nas curvas (assumindo que há alguma), quanto maior mais fácil é de passar pelas curvas é, porém mais dará a impressão de teleportar")]
    public float curveAreaSize = 0.30f;
    public Color NonPongableColor;
    [SerializeField] private Collider2D pongCollider;




    // info
    [HideInInspector]public int number;

    // Working vars
    private TileManager manager;
    private int xPos;
    private int yPos;
    
    private Vector3 pointerOffset;
    private int pivotX, pivotY;
    private Vector2Int gridMin, gridMax;
    private Vector2 worldMin, worldMax;
    private bool isDragging = false; // evita null reference se estiver locked=true porque os eventos de drag ainda ativam

    public void setup(TileManager gm, int num)
    {
        manager = gm;
        numberText.text = num.ToString();
        number = num;
        manager.victoryEvent.AddListener(unBlindTile);
        manager.defeatEvent.AddListener(unBlindTile);

        pongCollider.enabled = false;
    }

    public void bouncePongBack(Transform t)// força a pong ball a ser lançada para a direita (ativado pelo triggerEventor script)
    {
        manager.pongMode.pongBack(t);
    }

    private void OnDisable()
    {
        manager.victoryEvent.RemoveListener(unBlindTile);
        manager.defeatEvent.RemoveListener(unBlindTile);
    }

    // cega o numero no modo blind tiles, e revela ao ganhar/perder
    public void blindTile(){blinderAnimator.Play("blind",0,0f);}
    public void unBlindTile(){blinderAnimator.Play("idle");}

    // ativa a hitbox ou muda de cor no modo pong
    public void turnTilePongable(){ pongCollider.enabled = true; }// ativa o colider para colidir com a pongball
    public void turnTileUnpongable(){ tileImage.color = NonPongableColor; }//deixa uma cor não branca para dizer que não colide com pong







    // Salva nova posição, chamado pelo assign, em tile manager
    public void savePos(int x, int y)
    {
        xPos = x;
        yPos = y;
    }

   



    public void OnBeginDrag(PointerEventData eventData)// começa a arrastar
    {
        if (manager.locked) return;
        isDragging = true; // confirma que o drag começou validamente

        pivotX = xPos;
        pivotY = yPos;
        RecalculateBounds(pivotX, pivotY);

        // Deixa de ser filho do slot para se renderizar por cima de tudo
        Transform gridContainer = manager.GetSlot(0, 0).parent;
        transform.SetParent(gridContainer.parent, true);
        transform.SetAsLastSibling();

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            (RectTransform)transform, eventData.position, eventData.pressEventCamera, out Vector3 worldPoint);
        pointerOffset = transform.position - worldPoint;
    }

    public void OnDrag(PointerEventData eventData)//arrasta
    {
        if (!isDragging) return;

        Transform gridContainer = manager.GetSlot(0, 0).parent;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)gridContainer.parent, eventData.position, eventData.pressEventCamera, out Vector3 worldPoint))
        {
            Vector3 rawTarget = worldPoint + pointerOffset;
            Vector3 pivotPos = manager.GetSlot(pivotX, pivotY).position; // Busca direto do Manager

            CheckForNewPivot();

            Vector3 hOption = new Vector3(Mathf.Clamp(rawTarget.x, worldMin.x, worldMax.x), pivotPos.y, pivotPos.z);
            Vector3 vOption = new Vector3(pivotPos.x, Mathf.Clamp(rawTarget.y, worldMin.y, worldMax.y), pivotPos.z);

            bool canMoveH = gridMin.x != gridMax.x;
            bool canMoveV = gridMin.y != gridMax.y;

            if (canMoveH && !canMoveV) transform.position = hOption;
            else if (canMoveV && !canMoveH) transform.position = vOption;
            else if (canMoveH && canMoveV)
            {
                transform.position = Vector3.Distance(rawTarget, hOption) < Vector3.Distance(rawTarget, vOption) ? hOption : vOption;
            }
        }
    }

    // Checa por possíveis pivôs nas casa vizinhas
    private void CheckForNewPivot()
    {
        if (pivotX > gridMin.x && TrySetPivot(pivotX - 1, pivotY)) return;
        if (pivotX < gridMax.x && TrySetPivot(pivotX + 1, pivotY)) return;
        if (pivotY > gridMin.y && TrySetPivot(pivotX, pivotY - 1)) return;
        if (pivotY < gridMax.y && TrySetPivot(pivotX, pivotY + 1)) return;
    }

    // Faz um pivô novo nas curvas dependendo de sua distancia, para então checar os vizinhos dessa posição e se mover em novo eixo
    private bool TrySetPivot(int targetX, int targetY)
    {
        // Calcula o snap dinamicamente para não precisar salvar a variável
        float snapThreshold = 50f; // Segurança
        if (manager.colunas > 1)
            snapThreshold = Vector3.Distance(manager.GetSlot(0, 0).position, manager.GetSlot(1, 0).position) * curveAreaSize;
        else if (manager.linhas > 1)
            snapThreshold = Vector3.Distance(manager.GetSlot(0, 0).position, manager.GetSlot(0, 1).position) * curveAreaSize;

        if (Vector3.Distance(transform.position, manager.GetSlot(targetX, targetY).position) < snapThreshold)
        {
            pivotX = targetX;
            pivotY = targetY;
            RecalculateBounds(pivotX, pivotY);
            tileAnimator.resetSlideSFX();
            return true;
        }
        return false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return; 
        isDragging = false;


        int bestX = pivotX;
        int bestY = pivotY;
        float minDistance = float.MaxValue;

        for (int x = gridMin.x; x <= gridMax.x; x++)
        {
            float dist = Vector3.Distance(transform.position, manager.GetSlot(x, pivotY).position);
            if (dist < minDistance) { minDistance = dist; bestX = x; bestY = pivotY; }
        }
        for (int y = gridMin.y; y <= gridMax.y; y++)
        {
            float dist = Vector3.Distance(transform.position, manager.GetSlot(pivotX, y).position);
            if (dist < minDistance) { minDistance = dist; bestX = pivotX; bestY = y; }
        }

        // conta como um move caso o modo countedMoves esteja ativado e caso esteja em um lugar diferente de antes
        if (manager.settings.countedMode)
        {
            if (manager.GetSlotIndex(bestX, bestY) != manager.GetSlotIndex(xPos, yPos))
            {
                manager.countedMovesMode.move();
            }
        }
        manager.ClearPosition(xPos, yPos);

        manager.Assign(this.gameObject, manager.GetSlotIndex(bestX, bestY), false);


    }


    // Checa vizinhos
    private void RecalculateBounds(int pX, int pY)
    {
        gridMin = new Vector2Int(pX, pY);
        gridMax = new Vector2Int(pX, pY);

        while (IsPosFree(gridMin.x - 1, pY)) gridMin.x--;
        while (IsPosFree(gridMax.x + 1, pY)) gridMax.x++;
        while (IsPosFree(pX, gridMin.y - 1)) gridMin.y--;
        while (IsPosFree(pX, gridMax.y + 1)) gridMax.y++;

        worldMin = new Vector2(
            Mathf.Min(manager.GetSlot(gridMin.x, pY).position.x, manager.GetSlot(gridMax.x, pY).position.x),
            Mathf.Min(manager.GetSlot(pX, gridMin.y).position.y, manager.GetSlot(pX, gridMax.y).position.y)
        );
        worldMax = new Vector2(
            Mathf.Max(manager.GetSlot(gridMin.x, pY).position.x, manager.GetSlot(gridMax.x, pY).position.x),
            Mathf.Max(manager.GetSlot(pX, gridMin.y).position.y, manager.GetSlot(pX, gridMax.y).position.y)
        );
    }
    private bool IsPosFree(int x, int y)
    {
        return manager.IsPosFree(x, y) || (x == xPos && y == yPos);
    }



}