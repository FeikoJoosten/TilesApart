using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerAudioPlayer))]
public class Player : MonoBehaviour {
    public static event System.Action<Vector2Int> OnPlayerStartedMoving = delegate { };
    public static event System.Action OnPlayerEndedMoving = delegate { };
    public static event System.Action<string> OnPlayerWon = delegate { };
    public static event System.Action<string> OnPlayerDied = delegate { };
    public static event System.Action OnPlayerActivated = delegate { };

    private GridManager gridManager;
    public GridManager GridManager {
        get {
            if (gridManager != null) return gridManager;

            GameObject[] gameObjects = gameObject.scene.GetRootGameObjects();
            foreach (GameObject obj in gameObjects) {
                if (obj.GetComponent<GridManager>() == null) continue;

                gridManager = obj.GetComponent<GridManager>();
                break;
            }
            return gridManager;
        }
        set => gridManager = value;
    }

    [ReadOnly]
    public bool isMoving;
    [ReadOnly]
    public bool isDead;
    public bool hasWon { get; private set; }

    [SerializeField]
    private PlayerData playerData = null;
    public PlayerData PlayerData => playerData ?? ScriptableObject.CreateInstance<PlayerData>();

    public Vector2Int CurrentTileIndex { get; private set; }
    public PlayerAnimator PlayerAnimtor { get; private set; }
    public PlayerAudioPlayer PlayerAudioPlayer { get; private set; }

    private Vector2Int originalStartingIndex;
    private Vector3 originalStartingPosition;
    private GameObject tempPlayer;
    private Material[] playerMaterials;
    private float currentWrappingTime;
    private float playerWrappingAnimationSpeed;
    private bool pauseMenuEnabled;

    private void Awake() {
        if (GridManager.startTile != null) {
            CurrentTileIndex = GridManager.startTile.tileIndex;
            GridManager.AlignPlayerRotation();

            originalStartingIndex = GridManager.startTile.tileIndex;
            originalStartingPosition = transform.position;

            return;
        }

        Tile[] allTiles = FindObjectsOfType<Tile>();

        foreach (Tile tile in allTiles) {
            if (tile.tileType != TileType.Start) continue;

            CurrentTileIndex = tile.tileIndex;
            GridManager.AlignPlayerRotation();

            originalStartingIndex = GridManager.startTile.tileIndex;
            originalStartingPosition = transform.position;

            return;
        }

        Debug.LogError("Could not find the start tile, defaulting to start index of 0, 0");
        CurrentTileIndex = new Vector2Int(0, 0);
    }

    private void Start() {
        PlayerAnimtor = GetComponent<PlayerAnimator>();
        PlayerAudioPlayer = GetComponent<PlayerAudioPlayer>() ?? gameObject.AddComponent<PlayerAudioPlayer>();

        GameMenus.OnPauseMenuOpened += OnPauseMenuOpened;
        GameMenus.OnPauseMenuClosed += OnPauseMenuClosed;
        Tile.OnTileStartedMoving += OnMovementStartTriggered;
        Tile.OnTileEndedMoving += OnMovementEndTriggered;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

        playerWrappingAnimationSpeed = GridManager.GridData.PlayerWrappingAnimationSpeed;

        OnPlayerActivated();
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene loadedScene, UnityEngine.SceneManagement.LoadSceneMode sceneLoadMode) {
        isMoving = false;
    }

    private void OnDestroy() {
        GameMenus.OnPauseMenuOpened -= OnPauseMenuOpened;
        GameMenus.OnPauseMenuClosed -= OnPauseMenuClosed;
        Tile.OnTileStartedMoving -= OnMovementStartTriggered;
        Tile.OnTileEndedMoving -= OnMovementEndTriggered;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SetCurrentTileIndex(Vector2Int newTileIndex) {
        CurrentTileIndex = newTileIndex;
    }

    private void OnMovementStartTriggered(Vector2Int startingIndex, Vector2Int movementDirection) {
        PlayerAnimtor.ResetPlaybackSpeed();

        if (WillMoveWithTile(startingIndex, movementDirection) == false) return;

        if (transform == null)
            return;

        transform.SetParent(GridManager.GetTileAtIndex(CurrentTileIndex).transform, true);

        Vector2Int tileStartIndex = CurrentTileIndex;

        if (GridManager.GetTileAtIndex(tileStartIndex).IsMovableTile()) {
            CurrentTileIndex = GridManager.GetTileAtIndex(tileStartIndex).GetNextTileIndex(tileStartIndex, movementDirection);
        }
    }

    public IEnumerator AnimatePlayerWrapping() {
        SkinnedMeshRenderer playerRenderer = PlayerAnimtor.PlayerRenderer.GetComponent<SkinnedMeshRenderer>();

        if (playerMaterials == null) {
            playerMaterials = playerRenderer.sharedMaterials;
        }

        // Fade the player as well when the player wraps around
        if (tempPlayer == null) {
            tempPlayer = new GameObject("TempPlayer");
            tempPlayer.AddComponent<MeshRenderer>();
            tempPlayer.AddComponent<MeshFilter>();
        }

        MeshRenderer tempPlayerRenderer = tempPlayer.GetComponent<MeshRenderer>();
        MeshFilter tempPlayerFilter = tempPlayer.GetComponent<MeshFilter>();

        tempPlayerRenderer.materials = playerRenderer.materials;
        tempPlayerFilter.sharedMesh = playerRenderer.sharedMesh;
        // The original player model uses a skinned mesh renderer, which has a different pivot point.
        // Need to manually adjust the offset.
        tempPlayer.transform.position = new Vector3(transform.position.x, transform.position.y - 0.7f, transform.position.z);
        tempPlayer.transform.localScale = playerRenderer.transform.localScale;

        // Find temporary tile to parent temporary player to
        while (GridManager.tempTile == null) {
            yield return null;
        }

        tempPlayer.transform.parent = GridManager.tempTile.transform;

        // Hide player 
        foreach (Material material in playerRenderer.materials) {
            material.SetFloat(GridManager.TileData.fragmentationName, 1.0f);
        }

        // Show temp player 
        foreach (Material material in tempPlayerRenderer.materials) {
            material.SetFloat(GridManager.TileData.fragmentationName, 0.0f);
        }

        // Setup loop timer
        currentWrappingTime = 0.0f;
        float wait = Mathf.Max(GridManager.GridData.tileWrapFadeInDuration, GridManager.GridData.tileWrapFadeOutDuration);

        float playerMaterialsLength = playerRenderer.materials.Length;

        // Loops for fading
        while (currentWrappingTime < wait) {
            currentWrappingTime += Time.deltaTime * playerWrappingAnimationSpeed;

            // Need to make sure we are following the original player model's rotation along
            if (tempPlayer == null) {
                yield break;
            }

            tempPlayer.transform.rotation = transform.rotation;

            if (currentWrappingTime < GridManager.GridData.tileWrapFadeOutDuration) {
                // Fading out effect
                for (int i = 0; i < playerMaterialsLength; i++) {
                    tempPlayerRenderer.materials[i].SetFloat(GridManager.TileData.playerFragmentationName, currentWrappingTime / GridManager.GridData.tileWrapFadeOutDuration);
                }
            }

            if (currentWrappingTime < GridManager.GridData.tileWrapFadeInDuration) {
                // Fading in effect
                for (int i = 0; i < playerMaterialsLength; i++) {
                    playerRenderer.materials[i].SetFloat(GridManager.TileData.playerFragmentationName, 1.0f - (currentWrappingTime / GridManager.GridData.tileWrapFadeInDuration));
                }
            }

            yield return null;
        }

        playerRenderer.sharedMaterials = playerMaterials;

        // Destroy temporary object
        Destroy(tempPlayer);

        // Mark as movement ended
        isMoving = false;
    }

    private bool WillMoveWithTile(Vector2Int startTile, Vector2Int movementDirection) {
        if (CurrentTileIndex == startTile && GridManager.GetTileAtIndex(startTile).IsMovableTile()) {
            return true;
        }

        // Start from first tile
        Vector2Int groupStart = startTile;

        // Loop backwards
        while (true) {
            Vector2Int next = GridManager.GetTileAtIndex(groupStart).GetNextTileIndex(groupStart, movementDirection);

            Tile nextTile = GridManager.GetTileAtIndex(next);

            // Stop when you reach an empty tile or border
            if (nextTile == null || next == startTile) {
                return false;
            }
            if (nextTile.IsMovableTile() == false && (nextTile.tileType != TileType.End && nextTile.tileType != TileType.Start)) {
                return false;
            }
            if (next == CurrentTileIndex) {
                return true;
            }

            // Otherwise continue looping
            groupStart = next;
        }
    }

    private void OnMovementEndTriggered(Vector2Int movementDirection) {
        if (isMoving) return;
        if (GridManager == null) return;
        if (GridManager.startTile == null) return;
        if (gameObject.activeInHierarchy == false) return;

        transform.SetParent(null, true);

        isMoving = true;
        OnPlayerStartedMoving(movementDirection);
        PlayerAnimtor.OnPlayerStartWalking(movementDirection);

        Vector2Int newIndex = GetNextPlayerLocation(CurrentTileIndex, movementDirection);

        transform.position = GridManager.GetTileWorldPosition(CurrentTileIndex) + GridManager.GridData.PlayerSpawnOffset;

        if (GridManager.GetTileAtIndex(newIndex) == null) {
            Death();
            return;
        }

        // Only update player position when he's alive
        if (isDead == false) {
            CurrentTileIndex = newIndex;
        }
    }

    public void PlayerMoveEnd() {
        isMoving = false;
        playerWrappingAnimationSpeed = GridManager.GridData.PlayerWrappingAnimationSpeed;
        OnPlayerEndedMoving();
        // Performance thing, only sync transform changes when we are done moving
        Physics.SyncTransforms();

        if (CheckForWin()) {
            Win();
        }
    }

    private Vector2Int GetNextPlayerLocation(Vector2Int currentIndex, Vector2Int movementDirection) {
        Vector2Int newIndex;

        if (currentIndex.x + movementDirection.x > GridManager.GridSize.x - 1 && currentIndex.x + movementDirection.x > 0) {
            newIndex = new Vector2Int(0, currentIndex.y);
            Death();
        } else if (currentIndex.x + movementDirection.x < GridManager.GridSize.x - 1 && currentIndex.x + movementDirection.x < 0) {
            newIndex = new Vector2Int(GridManager.GridSize.x - 1, currentIndex.y);
            Death();
        } else if (currentIndex.y + movementDirection.y > GridManager.GridSize.y - 1 && currentIndex.y + movementDirection.y > 0) {
            newIndex = new Vector2Int(currentIndex.x, 0);
            Death();
        } else if (currentIndex.y + movementDirection.y < GridManager.GridSize.y - 1 && currentIndex.y + movementDirection.y < 0) {
            newIndex = new Vector2Int(currentIndex.x, GridManager.GridSize.y - 1);
            Death();
        } else {
            if (DiesOnMovement(currentIndex, movementDirection)) {
                Death();
            }
            newIndex = currentIndex + movementDirection;
        }

        return newIndex;
    }

    private void Death() {
        isDead = true;
        PlayerAudioPlayer.PlayRandomDeathSound();
        PlayerAnimtor.ResetPlaybackSpeed();
        OnPlayerDied(LevelManager.Instance.GetCurrentLevelName());
    }

    public bool DiesOnMovement(Vector2Int currentIndex, Vector2Int movementDirection) {
        if (currentIndex.x + movementDirection.x < 0 || currentIndex.y + movementDirection.y < 0)
            return true;

        if (!GridManager.GetTileAtIndex(currentIndex))
            return true;
        TileType typeCurrent = GridManager.GetTileAtIndex(currentIndex).tileType;
        bool[] d1 = GridManager.GetTileAtIndex(currentIndex).GetDirections();

        if (!GridManager.GetTileAtIndex(currentIndex + movementDirection))
            return true;

        if (GridManager.GetTileAtIndex(currentIndex + movementDirection).TileAnimator.isUp == false) {
            return true;
        }

        TileType typeNew = GridManager.GetTileAtIndex(currentIndex + movementDirection).tileType;
        bool[] d2 = GridManager.GetTileAtIndex(currentIndex + movementDirection).GetDirections();

        bool currentTile;
        bool newTile;

        switch (typeCurrent) {
            case TileType.Pathless:
            case TileType.Empty:
            case TileType.Border:
                currentTile = true;
                break;
            case TileType.Curve:
            case TileType.Straight:
            case TileType.TCrossing:
            case TileType.End:
            case TileType.Start:
                currentTile = (movementDirection.x <= 0 || !d1[1]) && (movementDirection.x >= 0 || !d1[3]) && (movementDirection.y <= 0 || !d1[0]) && (movementDirection.y >= 0 || !d1[2]);

                break;
            default:
                throw new System.ArgumentOutOfRangeException();
        }

        switch (typeNew) {
            case TileType.End:
                newTile = false;
                break;
            case TileType.Pathless:
            case TileType.Empty:
            case TileType.Border:
                newTile = true;
                break;
            case TileType.Curve:
            case TileType.Straight:
            case TileType.TCrossing:
            case TileType.Start:
                newTile = (movementDirection.x <= 0 || !d2[3]) && (movementDirection.x >= 0 || !d2[1]) && (movementDirection.y <= 0 || !d2[2]) && (movementDirection.y >= 0 || !d2[0]);

                break;
            default:
                throw new System.ArgumentOutOfRangeException();
        }

        return newTile || currentTile;
    }

    public bool CheckForWin() {
        if (GridManager.GetTileAtIndex(CurrentTileIndex) == null) return false;
        return GridManager.GetTileAtIndex(CurrentTileIndex).tileType == TileType.End;
    }

    [EditorButton]
    private void Win() {
        // Don't allow a win in non playmode
        if (Application.isPlaying == false) return;

        if (pauseMenuEnabled) {
            StartCoroutine(WaitForClosingOfPauseMenu());
            return;
        }

        hasWon = true;
        transform.position = GridManager.endTile.transform.position + GridManager.GridData.PlayerSpawnOffset;

        GridManager.SinkLevel(false, true, false);

        OnPlayerWon(LevelManager.Instance.GetCurrentLevelName());
    }

    private void OnPauseMenuOpened() {
        pauseMenuEnabled = true;
    }

    private void OnPauseMenuClosed() {
        pauseMenuEnabled = false;
    }

    private IEnumerator WaitForClosingOfPauseMenu() {
        while (pauseMenuEnabled) {
            yield return null;
        }

        Win();
    }

    public void ResetPlayer() {
        CurrentTileIndex = originalStartingIndex;
        transform.position = originalStartingPosition;
        isDead = false;
        isMoving = false;

        if (playerMaterials != null)
            PlayerAnimtor.PlayerRenderer.GetComponent<SkinnedMeshRenderer>().sharedMaterials = playerMaterials;
    }
}
