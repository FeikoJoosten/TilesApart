using UnityEngine;

public partial class TileMover : MonoBehaviour {
    private bool isDragging;
    private bool hasPressed;
    private bool inputEnabled = true;
    private bool firstMove = true;

    private Vector3 startScreenPosition;
    private Tile selectedTile;
    private GridManager gridManager;
    private RaycastHit[] hits = new RaycastHit[10];
    public GridManager GridManager {
        get {
            if (gridManager != null) return gridManager;

            GameObject[] gameObjects = gameObject.scene.GetRootGameObjects();
            for (int i = 0, length = gameObjects.Length; i < length; i++) {
                GridManager manager = gameObjects[i].GetComponent<GridManager>();
                if (manager == null) continue;

                gridManager = manager;
                break;
            }
            return gridManager;
        }
        set => gridManager = value;
    }

    public void EnableInput() {
        inputEnabled = true;
    }

    public void DisableInput() {
        inputEnabled = false;
    }

    private void Start() {
        // Get grid manager reference for later use
        Player.OnPlayerWillWin += OnPlayerWillWin;
        GameMenus.OnPauseMenuOpened += DisableInput;
        GameMenus.OnPauseMenuClosed += EnableInput;
    }

    private void OnDestroy() {
        Player.OnPlayerWillWin -= OnPlayerWillWin;
        GameMenus.OnPauseMenuOpened -= DisableInput;
        GameMenus.OnPauseMenuClosed -= EnableInput;
    }

    private void OnPlayerWillWin() {
        DisableInput();
    }

    private void Update() {
        // Only update things when input is enabled
        if (inputEnabled == false
            || GridManager.IsLevelRisingUp
            || GridManager.IsGridMoving
            || GridManager.PlayerObject.isMoving
            || GridManager.PlayerObject.isDead
            || GridManager.PlayerObject.hasWon
            || GridManager.PlayerObject.PreLoader.isTransitioning
            || Application.isFocused == false) {
            return;
        }

        // In case we are still in a move and it's not detected properly
        if (GridManager.TempTile != null)
            if (GridManager.TempTile.activeSelf) return;

        // Make sure that the game never accidentally blocks input
        if (Input.GetMouseButton(0) == false && hasPressed) {
            hasPressed = false;
        }

        // If the player starts touching the screen
        if (Input.GetMouseButton(0) && hasPressed == false) {
            OnStartTouch();
        }

        // When the player is dragging
        if (isDragging && selectedTile != null) {
            Vector3 inputDirection = new Vector3(Input.mousePosition.x - startScreenPosition.x, 0, Input.mousePosition.y - startScreenPosition.y);
            float inputDirectionLength = inputDirection.magnitude;

            // Make sure the camera rotation is kept in mind
            inputDirection = transform.rotation * inputDirection;

            // Only highlight one line when movement is evident
            if (inputDirectionLength > GridManager.GridData.minimumDraggingRange) {
                UpdateNeighbours(inputDirection);
            }
            if (onlyDisplayLine) {
                if (inputDirectionLength < GridManager.GridData.minimumDraggingRange) {
                    //UpdateNeighboursBack(inputDirection);
                }
            }

            // Move tiles once you release the mouse button
            if (Input.GetMouseButtonUp(0) == false) return;

            if (inputDirectionLength > GridManager.GridData.minimumDraggingRange) {
                ActivateTileMovement(inputDirection);
            }
            ResetDragginInformation();
        }
    }

    /// <summary> When the player starts touching the screen </summary>
    private void OnStartTouch() {
        // Raycast to touched object
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

	    if (Physics.Raycast(ray, out RaycastHit hit)) {
            // If the drag starts at an object
            selectedTile = hit.collider.gameObject.GetComponent<Tile>();
		    if (selectedTile == null || selectedTile.tileType == TileType.End || selectedTile.tileType == TileType.Start) return;

            if (Application.isFocused == false) return;

            // Save drag start data
            startScreenPosition = Input.mousePosition;
            isDragging = true;
            hasPressed = true;

            // Recolor tiles
            GetCrossNeighbours(selectedTile);
            ColorFadeIn(crossNeighbours);
        }
    }

    /// <summary> Called when tile movement is triggered </summary>
    private void ActivateTileMovement(Vector3 inputDirection) {
        // Lock to axis
        if (Mathf.Abs(inputDirection.x) > Mathf.Abs(inputDirection.z)) {
            inputDirection = inputDirection.x >= 0 ? Vector3.right : Vector3.left;
        } else {
            inputDirection = inputDirection.z >= 0 ? Vector3.forward : Vector3.back;
        }

        // This is to make sure the player can't go back to start
        if (firstMove) {
            firstMove = false;

            //AnalyticsManager.Instance.RecordCustomEvent("PlayerFirstMove", new Dictionary<string, object> {
            //    { "PlayerFirstMoveSinceLevelLoaded", Time.timeSinceLevelLoad }
            //});
        }

        // Move tiles
        if (selectedTile != null) {
            selectedTile.MoveTile(inputDirection);
        }
    }

    /// <summary> Reset dragging information </summary>
    private void ResetDragginInformation() {
        // Reset variables
        isDragging = false;
        hasPressed = false;
        selectedTile = null;

        // Reset tile colors
        ColorFadeOut(crossNeighbours);
        onlyDisplayLine = false;
    }
}