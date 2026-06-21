using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TileManager : MonoBehaviour
// Script que gerencia e embaralha Tiles, prepara a grid e modos de jogo

{
    [Header("Dependencies")]
    [Tooltip("a grid deve organizar os objetos da esquerda para a direita e de cima para baixo")]
    [SerializeField] private GameObject gridGO;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject tileSlotPrefab;
    [SerializeField] public GameSettings settings;
    public int linhas;
    public int colunas;

    [Header("mode's scripts")]
    [SerializeField] public TimeAttackMode timeAttackMode;
    [SerializeField] public CountedMovesMode countedMovesMode;
    [SerializeField] public PongMode pongMode;

    [Header("Config")]
    [Tooltip("Quantos movimentos aleatórios ele deve fazer para embaralhar?")]
    public int shuffleAmount = 50;

    [Header("Events")]
    public UnityEvent victoryEvent;
    public UnityEvent defeatEvent;


    [HideInInspector] public bool locked; // impede que o jogador jogue
    [HideInInspector] public int tileAmount = 8;
    [HideInInspector] public Transform[] slots = new Transform[0];
    [HideInInspector] public List<GameObject> spawnedTiles = new List<GameObject>();
    [HideInInspector] public int[,] tileMatrix;
    [HideInInspector] public bool alreadyGotResults;

    private GameObject lastMovedTile = null; // guarda qual foi a última peça movida e evita um "vai-e-volta" durante o embaralhamento
    private GridLayoutGroup gridLayout;

    private void Awake()
    {
        LoadSettings();

        gridLayout = gridGO.GetComponent<GridLayoutGroup>();

        if (gridLayout == null)
        {
            Debug.LogError("missing GridLayoutGroup component");
            return;
        }

        if (linhas <= 0 || colunas <= 0)
        {
            Debug.LogError("linhas e colunas precisam ser maiores que 0");
            return;
        }

        RecreateGrid();

        // Garante uma posição vazia
        if (tileAmount > (linhas * colunas) - 1)
        {
            tileAmount = (linhas * colunas) - 1;
        }

        // Garante pelo menos um tile
        if (tileAmount <= 0)
        {
            tileAmount = 1;
        }
    }

    private void RecreateGrid()
    {
        // SAFEST CLEANUP: Destroy all physical child UI slots inside gridGO directly
        if (gridGO != null)
        {
            for (int i = gridGO.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(gridGO.transform.GetChild(i).gameObject);
            }
        }

        // Configure GridLayoutGroup
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = colunas;

        // Initialize tracking arrays
        int totalPositions = linhas * colunas;
        slots = new Transform[totalPositions];
        tileMatrix = new int[colunas, linhas];

        // SINGLE LOOP: Create positions AND save direct references simultaneously
        for (int i = 0; i < totalPositions; i++)
        {
            GameObject posObject = Instantiate(tileSlotPrefab, gridGO.transform);
            posObject.name = $"Pos {i}";
            slots[i] = posObject.transform;
        }

        // Force UI updates so Canvas handles layout positioning cleanly
        Canvas.ForceUpdateCanvases();
        gridLayout.CalculateLayoutInputHorizontal();
        gridLayout.CalculateLayoutInputVertical();
        gridLayout.SetLayoutHorizontal();
        gridLayout.SetLayoutVertical();
    }

    public void SetGridHeight(int Linhas)
    {
        linhas = Linhas;
        CleanGrid();
        RecreateGrid();
    }

    public void SetGridWidth(int Colunas)
    {
        colunas = Colunas;
        CleanGrid();
        RecreateGrid();
    }

    private void LoadSettings()
    {
        tileAmount = settings.maxSlotAmount - settings.emptySlotAmount;
    }




    // Prepara os modos
    void setupModes()
    {
        settings.checkAmounts();


        if (settings.TimedMode)
        {
            timeAttackMode.SetTimer(settings.startingTimer);
        }
        else
        {
            timeAttackMode.hideMode();
        }

        if (settings.countedMode)
        {
            countedMovesMode.setMovesCount(settings.countedMovesAmount);
        }
        else
        {
            countedMovesMode.hideMode();
        }

        if (settings.blindMode)
        {
            List<int> randomList = GetRandomOrder(0, tileAmount - 1);
            for (int i = 0; i < settings.blindTileAmount; i++)
            {
                spawnedTiles[randomList[i]].GetComponent<TileScript>().blindTile();

            }
        }
        if (settings.pongMode)
        {
            List<int> randomList = GetRandomOrder(0, tileAmount - 1);
            for (int i = 0; i < settings.pongableTileAmount; i++)
            {
                spawnedTiles[randomList[i]].GetComponent<TileScript>().turnTilePongable();
                Debug.Log("pongged");
            }
            for (int i = settings.pongableTileAmount; i < tileAmount; i++)
            {
                spawnedTiles[randomList[i]].GetComponent<TileScript>().turnTileUnpongable();
                Debug.Log("unpongged");
            }

            pongMode.SpawnAndLaunchBall();
        }
        else
        {
            pongMode.hideMode();
        }
    }

    // Esconde os objetos relacionados aos modos enquanto o jogo não está ativo
    void hideModes()
    {
        timeAttackMode.hideMode();
        countedMovesMode.hideMode();
        pongMode.hideMode();
    }

    public void RestartGame()
    {
        LoadSettings();
        CleanGrid();
        CreateTiles();
        UnlockGame();

        setupModes();

        alreadyGotResults = false;

        if (CheckWinCondition())
        {
            FixInstantWin();
            Debug.Log("Um Tile foi movido para evitar vitória imediata!");
        }
    }



    // Previne vitória imediata movendo duas peças, resulta em um puzzle fácil mas garante que não esteja solucionado e funciona em qualquer grid
    private void FixInstantWin()
    {
        // Executa 2 movimentos válidos respeitando a regra de "não vai-e-volta"
        DoRandomLegalMove();
        DoRandomLegalMove();
    }




    public void CreateTiles()
    {
        if (slots == null || slots.Length == 0)
        {
            Debug.LogWarning("Grid array positions not initialized yet!");
            return;
        }

        // Spawna tudo organizado na posição de vitória
        for (int i = 0; i < slots.Length; i++)
        {
            int value = i + 1; // Ordem perfeita

            if (value > tileAmount) continue; // free slot

            GameObject tile = SpawnTile(value);
            Assign(tile, i, true); // true evita dar trigger de vitória durante o setup
        }

        // simula o jogador bagunçando a grid fazendo movimentos legais
        lastMovedTile = null;
        for (int i = 0; i < shuffleAmount; i++)
        {

            DoRandomLegalMove();
        }
    }

    private void DoRandomLegalMove()
    {
        // lista todos os espaços vazios
        List<Vector2Int> emptySlots = new List<Vector2Int>();
        for (int y = 0; y < linhas; y++)
        {
            for (int x = 0; x < colunas; x++)
            {
                if (tileMatrix[x, y] == 0) emptySlots.Add(new Vector2Int(x, y));
            }
        }

        if (emptySlots.Count == 0) return;

        // Sorteia um slot vazio
        Vector2Int chosenEmpty = emptySlots[UnityEngine.Random.Range(0, emptySlots.Count)];

        // checa os vizinhos
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        List<GameObject> validNeighbors = new List<GameObject>(); // vizinhos que não foram movidos no move passado
        List<GameObject> allNeighbors = new List<GameObject>();   // Todos os vizinhos (backup)

        foreach (var dir in directions)
        {
            int nx = chosenEmpty.x + dir.x;
            int ny = chosenEmpty.y + dir.y;
            if (nx >= 0 && nx < colunas && ny >= 0 && ny < linhas)
            {

                int tileNum = tileMatrix[nx, ny];
                if (tileNum != 0)
                {

                    GameObject neighborTile = GetTileByNumber(tileNum);
                    if (neighborTile != null)
                    {
                        allNeighbors.Add(neighborTile);

                        // Regra de não ir e voltar, Só adiciona nos válidos se não for a última peça mexida
                        if (neighborTile != lastMovedTile)
                        {
                            validNeighbors.Add(neighborTile);
                        }
                    }




                }

            }
        }

        //  Escolhe qual vizinho vai ser puxado para o buraco vazio
        GameObject tileToMove = null;
        if (validNeighbors.Count > 0)
        {
            tileToMove = validNeighbors[UnityEngine.Random.Range(0, validNeighbors.Count)];
        }
        else if (allNeighbors.Count > 0)
        {
            // caso ele fique preso no canto e a única opção seja voltar a peça
            tileToMove = allNeighbors[UnityEngine.Random.Range(0, allNeighbors.Count)];
            Debug.LogWarning("peça teve que ir e voltar no embaralhamento");
        }

        // Faz o move
        if (tileToMove != null)
        {
            int numToMove = tileToMove.GetComponent<TileScript>().number;
            Vector2Int oldPos = findTilePosByNumber(numToMove);

            ClearPosition(oldPos.x, oldPos.y); // Esvazia o antigo espaço
            Assign(tileToMove, GetSlotIndex(chosenEmpty.x, chosenEmpty.y), true); // Move pro novo

            lastMovedTile = tileToMove; // Salva para o próximo turno
        }
    }


    // Código obsoleto de embaralhamento aleatório dos tiles, pois embaralhamento realmente aleatório pode resultar em um puzzle insolucionável
    private void CreateTilesObsolete()

    {
        if (slots == null || slots.Length == 0)
        {
            Debug.LogWarning("Grid array positions not initialized yet!");
            return;
        }

        List<int> slotValues = GetRandomOrder(1, slots.Length);

        for (int i = 0; i < slots.Length; i++)
        {
            int value = slotValues[i];

            if (value > tileAmount)
                continue; // free slot

            GameObject tile = SpawnTile(value);
            Assign(tile, i, true);// o true é para sinalizar que está sendo feito durante o embaralhamento das peças e evitar dar triger de vitória
        }
    }

    // Ao detectar vitória instantânea, este método irá mover uma peça aleatória a um espaço livre aleatório de preferencia não vizinho à peça escolhida
    // Código obsoleto, pois embaralhamento realmente aleatório pode resultar em um puzzle insolucionável
    private void FixInstantWinObsolete()
    {
        // segurança
        if (spawnedTiles.Count == 0 || spawnedTiles.Count == linhas * colunas) return;

        // Lista todos os espaços vazios e sorteia um
        List<Vector2Int> emptySlots = new List<Vector2Int>();
        for (int y = 0; y < linhas; y++)
            for (int x = 0; x < colunas; x++)
                if (tileMatrix[x, y] == 0) emptySlots.Add(new Vector2Int(x, y));
        Vector2Int chosenEmpty = emptySlots[UnityEngine.Random.Range(0, emptySlots.Count)];

        // Filtra tiles não vizinhos
        List<GameObject> nonNeighbors = new List<GameObject>();
        foreach (GameObject tile in spawnedTiles)
        {
            int num = tile.GetComponent<TileScript>().number;
            Vector2Int tPos = Vector2Int.zero;
            tPos = findTilePosByNumber(num);

            int dist = Mathf.Abs(tPos.x - chosenEmpty.x) + Mathf.Abs(tPos.y - chosenEmpty.y);
            if (dist > 1) nonNeighbors.Add(tile);
        }

        // Escolhe o tile para mover (um não-vizinho, se não existir, pega qualquer tile).
        GameObject tileToMove;
        if (nonNeighbors.Count > 0)
        {
            tileToMove = nonNeighbors[UnityEngine.Random.Range(0, nonNeighbors.Count)];
        }
        else
        {
            tileToMove = spawnedTiles[UnityEngine.Random.Range(0, spawnedTiles.Count)];

        }

        // Encontra e move o tile
        int numToMove = tileToMove.GetComponent<TileScript>().number;
        for (int y = 0; y < linhas; y++)
        {
            for (int x = 0; x < colunas; x++)
            {
                if (tileMatrix[x, y] == numToMove)
                {
                    ClearPosition(x, y);
                    Assign(tileToMove, GetSlotIndex(chosenEmpty.x, chosenEmpty.y), true);
                    return;
                }
            }
        }
    }






    GameObject SpawnTile(int number)
    {
        GameObject tile = Instantiate(tilePrefab);

        TileScript tileScript = tile.GetComponent<TileScript>();
        if (tileScript == null) Debug.LogError("Tile não tem TileScript!");
        tileScript.setup(this, number);

        spawnedTiles.Add(tile);
        return tile;
    }

    // Posiciona um tile em uma nova posição, registrando na matriz e deixando como filho de um objeto slot
    public void Assign(GameObject tile, int posIndex, bool isDuringSetup)
    {
        if (slots == null || posIndex >= slots.Length || slots[posIndex] == null)
        {
            Debug.LogError($"posição inválida! {posIndex}");
            return;
        }

        Vector2Int matrixPos = GetMatrixPosition(posIndex);

        if (!IsPosFree(matrixPos.x, matrixPos.y))
        {
            Debug.LogError("posição não está livre!");
            return;
        }

        // Deixa como filho do objeto slot
        tile.transform.SetParent(slots[posIndex], false);
        tile.transform.localPosition = Vector3.zero;
        tile.transform.localScale = Vector3.one;

        // Salva eu numero na matriz
        TileScript tileScript = tile.GetComponent<TileScript>();
        if (tileScript == null) Debug.LogError("Tile não tem TileScript!");

        tileMatrix[matrixPos.x, matrixPos.y] = tileScript.number;
        tileScript.savePos(matrixPos.x, matrixPos.y);

        // Dá trigger de vitória se estiverem ordenado e não for durante o embaralhamento do das peças
        if (!isDuringSetup && CheckWinCondition())
        {
            Victory();
        }
    }

    // Deleta os tiles, limpa a matriz e esconde objetos relacionados aos modos, usado antes de resetar o jogo e também para deixar a grid vazia
    public void CleanGrid()
    {
        tileMatrix = new int[colunas, linhas];
        foreach (GameObject tile in spawnedTiles)
        {
            if (tile != null) Destroy(tile);
        }
        spawnedTiles.Clear();
        hideModes();
        alreadyGotResults = true;
    }

    // Helpers -------------------------------------------------------------------------------------
    public bool IsPosFree(int x, int y)
    {
        if (x < 0 || x >= colunas || y < 0 || y >= linhas) return false;
        return tileMatrix[x, y] == 0;
    }

    public void ClearPosition(int x, int y)// Libera uma posição na matriz
    {
        if (x < 0 || x >= colunas || y < 0 || y >= linhas) return;
        tileMatrix[x, y] = 0;
    }

    public Vector2Int findTilePosByNumber(int number)
    {
        for (int y = 0; y < linhas; y++)
            for (int x = 0; x < colunas; x++)
                if (tileMatrix[x, y] == number) return new Vector2Int(x, y);

        Debug.Log("Tile não encontrado");
        return Vector2Int.zero;
    }

    private GameObject GetTileByNumber(int number)
    {
        foreach (GameObject tile in spawnedTiles)
        {
            if (tile.GetComponent<TileScript>().number == number) return tile;
        }
        return null;
    }

    public Vector2Int GetMatrixPosition(int posIndex)
    {
        return new Vector2Int(posIndex % colunas, posIndex / colunas);
    }

    public int GetSlotIndex(int x, int y)
    {
        return y * colunas + x;
    }
    public Transform GetSlot(int x, int y)
    {
        return slots[GetSlotIndex(x, y)];
    }

    // Retorna um lista de numeros int com ordem aleatória, min e max determinam seu tamanho e quais numeros estarão presentes
    List<int> GetRandomOrder(int min, int max)
    {
        List<int> numbers = new List<int>();
        for (int i = min; i <= max; i++) numbers.Add(i);

        for (int i = 0; i < numbers.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, numbers.Count);
            int temp = numbers[i];
            numbers[i] = numbers[randomIndex];
            numbers[randomIndex] = temp;
        }
        return numbers;
    }

    public int GetTileNumber(int x, int y)
    {
        if (x < 0 || x >= colunas || y < 0 || y >= linhas) return -1;
        return tileMatrix[x, y];
    }




    public bool CheckWinCondition()
    {
        int expectedNumber = 1;

        for (int y = 0; y < linhas; y++)
        {
            for (int x = 0; x < colunas; x++)
            {
                if (expectedNumber > tileAmount)
                {
                    if (tileMatrix[x, y] != 0) return false;
                }
                else
                {
                    if (tileMatrix[x, y] != expectedNumber) return false;
                }
                expectedNumber++;
            }
        }
        Debug.Log("Puzzle Solucionado!");
        return true;
    }

    // ---------------------------------------------------------------------------------------

    public void Victory()
    {
        if (!alreadyGotResults)
        {
            victoryEvent.Invoke();
            alreadyGotResults = true;
            Debug.Log("Victory!");
        }
    }
    public void Defeat()
    {
        if (!alreadyGotResults)
        {
            defeatEvent.Invoke();
            alreadyGotResults = true;
            Debug.Log("Defeat...");
        }
    }

    public void LockGame() { locked = true; }
    public void UnlockGame() { locked = false; }


}