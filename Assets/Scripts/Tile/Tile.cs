using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TileType {
    Empty,
    TCrossing,
    Curve,
    Pathless,
    Start,
    End,
    Straight,
    Border
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(TileAnimator))]
public partial class Tile : MonoBehaviour {
    public static Vector2Int originalTileMover = new Vector2Int(-1, -1);
    private static Vector2Int lastMovementDirection;
    public static event System.Action<Vector2Int, Vector2Int> OnTileStartedMoving = delegate { };
    public static event System.Action<Vector2Int> OnTileEndedMoving = delegate { };

    public TileType tileType;
    [ReadOnly]
    public Vector2Int tileIndex;

    private GridManager tileOwner;
    public GridManager TileOwner {
        get {
            if (tileOwner != null) return tileOwner;

            GameObject[] gameObjects = gameObject.scene.GetRootGameObjects();
            for (int i = 0; i < gameObjects.Length; i++) {
                if (gameObjects[i].GetComponent<GridManager>() != null) {
                    tileOwner = gameObjects[i].GetComponent<GridManager>();
                    break;
                }
            }
            return tileOwner;
        }
        set { tileOwner = value; }
    }

    [ReadOnly]
    public TileType previousTileType;
    [ReadOnly]
    public bool[] directions = new bool[4];

    public bool moving { get; private set; }
    private MeshFilter meshFilter;
    public MeshFilter MeshFilter => meshFilter ?? (meshFilter = GetComponent<MeshFilter>());
    private MeshRenderer meshRenderer;
    public MeshRenderer MeshRenderer => meshRenderer ?? (meshRenderer = GetComponent<MeshRenderer>());
    private TileAnimator tileAnimator;
    public TileAnimator TileAnimator => tileAnimator ?? (tileAnimator = GetComponent<TileAnimator>() ?? gameObject.AddComponent<TileAnimator>());
    private Material[] originalSharedMaterials;
    public Material[] OriginalSharedMaterials => originalSharedMaterials;
    private MaterialPropertyBlock propertyBlock = null;
    public MaterialPropertyBlock PropertyBlock => propertyBlock;
    private string tileColor1ShaderName = "Color_22D9D809";
    public string TileColor1ShaderName => tileColor1ShaderName;
    private string tileColor2ShaderName = "Color_B82826F4";
    public string TileColor2ShaderName => tileColor2ShaderName;
    private string tileColor3ShaderName = "Color_2F00DD01";
    public string TileColor3ShaderName => tileColor3ShaderName;
    private string tileColor4ShaderName = "Color_DBB74A21";
    public string TileColor4ShaderName => tileColor4ShaderName;

    // Used to save the tile's default colors
    public List<Color> startColors { get; private set; } = new List<Color>();

    [SerializeField]
    public Attributes attribute = Attributes.None;
    [ReadOnly]
    public TileAttribute attributeTile = null;

    private Vector2Int startingTileIndex;

    private void Awake() {
        previousTileType = tileType;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        LevelManager.OnCurrentGameSceneChanged += OnLevelStart;
        LevelManager.OnCorruptedLoadDetected += OnLevelStart;
        GameMenus.OnRestartPressed += OnRestartLevel;
        LevelManager.OnMainMenuLoading += OnBackToMainMenu;

        if (attribute == Attributes.Immobile)
            attributeTile = new TileAttributeImmobile(null);

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        originalSharedMaterials = meshRenderer.sharedMaterials;

        startingTileIndex = tileIndex;

        if (meshRenderer.sharedMaterial == null) return;

        if (propertyBlock == null) {
            propertyBlock = new MaterialPropertyBlock();
        }

        meshRenderer.GetPropertyBlock(propertyBlock);

        if (meshRenderer.sharedMaterial.HasProperty(tileColor1ShaderName))
            propertyBlock.SetColor(tileColor1ShaderName, meshRenderer.sharedMaterial.GetColor(tileColor1ShaderName));
        if (meshRenderer.sharedMaterial.HasProperty(tileColor2ShaderName))
            propertyBlock.SetColor(tileColor2ShaderName, meshRenderer.sharedMaterial.GetColor(tileColor2ShaderName));
        if (meshRenderer.sharedMaterial.HasProperty(tileColor3ShaderName))
            propertyBlock.SetColor(tileColor3ShaderName, meshRenderer.sharedMaterial.GetColor(tileColor3ShaderName));
        if (meshRenderer.sharedMaterial.HasProperty(tileColor4ShaderName))
            propertyBlock.SetColor(tileColor4ShaderName, meshRenderer.sharedMaterial.GetColor(tileColor4ShaderName));

        meshRenderer.SetPropertyBlock(propertyBlock);

        FixTileColor();

        for (int i = 0; i < TileOwner.TileData.materialColorNames.Length; i++) {
            startColors.Add(propertyBlock.GetColor(TileOwner.TileData.materialColorNames[i]));
        }
    }

    private void OnSceneLoaded(Scene loadedScene, LoadSceneMode sceneLoadMode) {
        moving = false;
    }

    private void OnSceneUnloaded(Scene loadedScene) {
        //Make sure that no weird movement behaviour is still running
        StopAllCoroutines();
    }

    private void OnLevelStart() {
        Tile.originalTileMover = new Vector2Int(-1, -1);
        Tile.lastMovementDirection = new Vector2Int();
    }

    private void OnRestartLevel(int moveCount) {
        Tile.originalTileMover = new Vector2Int(-1, -1);
        Tile.lastMovementDirection = new Vector2Int();
    }

    private void OnBackToMainMenu() {
        Tile.originalTileMover = new Vector2Int(-1, -1);
        Tile.lastMovementDirection = new Vector2Int();
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        LevelManager.OnCurrentGameSceneChanged -= OnLevelStart;
        LevelManager.OnCorruptedLoadDetected -= OnLevelStart;
        GameMenus.OnRestartPressed -= OnRestartLevel;
        LevelManager.OnMainMenuLoading -= OnBackToMainMenu;
    }

    private void SetTileIndex(int x, int y, bool useEditorCode = false) {
        SetTileIndex(new Vector2Int(x, y), useEditorCode);
    }

    private void SetTileIndex(Vector2Int newTileIndex, bool useEditorCode = false) {
        TileOwner.UpdateTileIndexLocation(tileIndex, newTileIndex, this, useEditorCode);

        if (useEditorCode) {
            Vector3 newLocalPosition = TileOwner.GetTileLocalPosition(newTileIndex);
            newLocalPosition.y = transform.localPosition.y;
            transform.localPosition = newLocalPosition;
        } else {
            TileOwner.StartCoroutine(UpdatePosition(newTileIndex));
        }
        tileIndex = newTileIndex;
        UpdateObjectName();
    }

    private IEnumerator UpdatePosition(Vector2Int newTileIndex) {
        moving = true;
        Vector2Int originalTileIndex = tileIndex;
        bool willTeleport = WillTeleport(originalTileIndex, lastMovementDirection);

        TileAudioPlayer.PlayRandomMovementSound();

        Vector3 moveTarget = TileOwner.GetTileLocalPosition(newTileIndex.x, newTileIndex.y);
        Vector3 movementStartPosition = transform.localPosition;
        float endTime = TileOwner.GridData.TileVerticalMovement[TileOwner.GridData.TileVerticalMovement.length - 1].time;
        float currentTime = 0;

        if (willTeleport) {
            StartCoroutine(AnimateTileWrap(newTileIndex, movementStartPosition));
        } else {
            while (currentTime < endTime) {

                transform.localPosition = Vector3.LerpUnclamped(movementStartPosition, moveTarget,
                    TileOwner.GridData.TileVerticalMovement.Evaluate(currentTime));

                currentTime += Time.deltaTime;

                yield return null;
            }

            transform.localPosition = moveTarget;
        }

        if (willTeleport == false) {
            moving = false;
        }

        if (originalTileIndex == Tile.originalTileMover) {
            while (moving) {
                yield return null;
            }

            if (TileOwner.GetTileAtIndex(TileOwner.PlayerObject.CurrentTileIndex) != null) {
                OnTileEndedMoving(
                    TileOwner.GetTileAtIndex(TileOwner.PlayerObject.CurrentTileIndex).attribute == Attributes.Reverse
                        ? lastMovementDirection * -1
                        : lastMovementDirection);

                Tile.originalTileMover = new Vector2Int(-1, -1);
            } else {
                OnTileEndedMoving(lastMovementDirection);

                Tile.originalTileMover = new Vector2Int(-1, -1);
            }
            // Performance thing, only sync transform changes when we are done moving
            Physics.SyncTransforms();
        }
    }

    private IEnumerator AnimateTileWrap(Vector2Int moveTargetIndex, Vector3 movementStartPosition) {
        // Calculate needed values
        Vector3 moveTarget = TileOwner.GetTileLocalPosition(moveTargetIndex);
        Vector3 diff = moveTarget - TileOwner.GetTileLocalPosition(tileIndex);

        // Keep in mind the proper grid spacing
        float offset;
        offset = (moveTargetIndex - tileIndex).x != 0 ? TileOwner.GridSpacing.x : TileOwner.GridSpacing.y;

        // Calculate movement direction
        Vector3 moveDirection = offset * 2.0f * new Vector3(-Mathf.Clamp(diff.x, -1, 1), 0, -Mathf.Clamp(diff.z, -1, 1));

        // Create temporary tile for tile fading
        if (TileOwner.tempTile == null) {
            TileOwner.tempTile = new GameObject("TileOwner.tempTile");
            TileOwner.tempTile.AddComponent<MeshRenderer>();
            TileOwner.tempTile.AddComponent<MeshFilter>();
        } else if (TileOwner.tempTile.activeSelf == false) {
            TileOwner.tempTile.SetActive(true);
        }

        MeshRenderer tempTileRenderer = TileOwner.tempTile.GetComponent<MeshRenderer>();
        tempTileRenderer.enabled = true;
        tempTileRenderer.sharedMaterials = MeshRenderer.sharedMaterials;

        TileOwner.tempTile.GetComponent<MeshFilter>().sharedMesh = MeshFilter.sharedMesh;
        TileOwner.tempTile.transform.parent = transform.parent;
        TileOwner.tempTile.transform.position = transform.position;
        TileOwner.tempTile.transform.localScale = transform.localScale;
        TileOwner.tempTile.transform.rotation = transform.rotation;

        MaterialPropertyBlock tempPropertyBlock = new MaterialPropertyBlock();
        tempTileRenderer.GetPropertyBlock(tempPropertyBlock);

        for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++) {
            PropertyBlock.SetFloat(TileOwner.TileData.fragmentationName, 1.0f);
        }

        if (TileOwner.PlayerObject.CurrentTileIndex == moveTargetIndex) {
            TileOwner.StartCoroutine(TileOwner.PlayerObject.AnimatePlayerWrapping());
        }

        // Setup loop timer
        float time = 0.0f;
        float endTime = TileOwner.GridData.TileVerticalMovement[TileOwner.GridData.TileVerticalMovement.length - 1].time;

        // Loops for fading
        while (time < endTime) {
            time += Time.deltaTime;

            //Resharper might be complaining, but trust me. It can be null
            if (tempTileRenderer != null) {
                if (time < TileOwner.GridData.tileWrapFadeOutDuration) {
                    // Fading out effect
                    for (int i = 0; i < tempTileRenderer.sharedMaterials.Length; i++) {
                        tempPropertyBlock.SetFloat(TileOwner.TileData.fragmentationName, time / TileOwner.GridData.tileWrapFadeOutDuration);

                        foreach (string materialColorName in TileOwner.TileData.materialColorNames) {
                            tempPropertyBlock.SetColor(materialColorName, PropertyBlock.GetColor(materialColorName));
                        }
                        tempTileRenderer.SetPropertyBlock(tempPropertyBlock);
                    }

                } else {
                    tempTileRenderer.enabled = false;
                }
            }

            if (time < TileOwner.GridData.tileWrapFadeInDuration) {
                // Fading in effect
                for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++) {
                    propertyBlock.SetFloat(TileOwner.TileData.fragmentationName, 1.0f - (time / TileOwner.GridData.tileWrapFadeInDuration));

                    foreach (string materialColorName in TileOwner.TileData.materialColorNames) {
                        tempPropertyBlock.SetColor(materialColorName, PropertyBlock.GetColor(materialColorName));
                    }
                    meshRenderer.SetPropertyBlock(propertyBlock);
                }
            } else {
                //meshRenderer.sharedMaterials = originalSharedMaterials;
                propertyBlock.SetFloat(TileOwner.TileData.fragmentationName, 0);

                foreach (string materialColorName in TileOwner.TileData.materialColorNames) {
                    tempPropertyBlock.SetColor(materialColorName, PropertyBlock.GetColor(materialColorName));
                }

                meshRenderer.SetPropertyBlock(propertyBlock);
            }

            // Move tiles (and player if present)
            if (time < endTime) {
                if (TileOwner.tempTile != null)
                    TileOwner.tempTile.transform.localPosition = Vector3.Lerp(movementStartPosition, movementStartPosition + moveDirection, TileOwner.GridData.TileVerticalMovement.Evaluate(time));

                transform.localPosition = Vector3.Lerp(moveTarget - moveDirection, moveTarget, TileOwner.GridData.TileVerticalMovement.Evaluate(time));
            }

            yield return null;
        }

        if (TileOwner.tempTile != null)
            TileOwner.tempTile.transform.localPosition = movementStartPosition + moveDirection;
        transform.localPosition = moveTarget;

        // Destroy temporary objects
        if (TileOwner.tempTile != null)
            TileOwner.tempTile.SetActive(false);

        // Mark as movement ended
        moving = false;
    }

    public IEnumerator MoveFreeToIndex(Vector3 newPos, Quaternion newRot) {
        Vector3 moveTarget = newPos;
        Vector3 movementStartPosition = transform.position;
        Quaternion movementStartRotation = transform.rotation;
        float endTime = TileOwner.GridData.TileFreeMovement[TileOwner.GridData.TileFreeMovement.length - 1].time;
        float currentTime = 0;

        while (currentTime < endTime && transform != null) {
            transform.position = Vector3.LerpUnclamped(movementStartPosition, moveTarget, TileOwner.GridData.TileFreeMovement.Evaluate(currentTime));

            transform.rotation = Quaternion.LerpUnclamped(movementStartRotation, newRot, TileOwner.GridData.TileFreeMovement.Evaluate(currentTime));

            currentTime += Time.deltaTime;

            yield return null;
        }

        transform.position = moveTarget;
        transform.rotation = newRot;
    }

    public void MoveTile(Vector3 direction, bool ignoreGroupStart = false) {
        if (TileOwner.IsGridMoving) {
            return;
        }

        Vector2Int movementDirection;

        if (direction == Vector3.right) {
            movementDirection = Vector2Int.right;
        } else if (direction == Vector3.left) {
            movementDirection = Vector2Int.left;
        } else if (direction == Vector3.forward) {
            movementDirection = Vector2Int.up;
        } else if (direction == Vector3.back) {
            movementDirection = Vector2Int.down;
        } else {
            // Invalid movement direction
            return;
        }

        if (Tile.originalTileMover == new Vector2Int(-1, -1)) {
            if (IsValidMove(tileIndex, movementDirection) == false) {
                return;
            }

            if (IsMovableTile() == false) {
                Vector2Int nextTile = GetNextTileIndex(tileIndex, movementDirection, true);
                if (TileOwner.GetTileAtIndex(nextTile) != null) {
                    TileOwner.GetTileAtIndex(nextTile).MoveTile(direction);
                    return;
                }
            }

            // Save movement starting position

            Vector2Int startTile = GetTileGroupStart(tileIndex, movementDirection);

            if (ignoreGroupStart) {
                startTile = tileIndex;
            }

            // If this is part of bigger chunk
            if (startTile != tileIndex) {

                TileOwner.GetTileAtIndex(startTile).MoveTile(direction);
                return;
            }

            Tile.originalTileMover = startTile;
            Tile.lastMovementDirection = movementDirection;
            OnTileStartedMoving(startTile, movementDirection);
        } else {
            if (Tile.originalTileMover == tileIndex) {
                // Stop the movement, to prevent looping and reset so we can continue correctly for the next iteration

                return;
            }
        }

        Vector2Int newIndex = GetNextTileIndex(tileIndex, movementDirection);

        if (TileOwner.GetTileAtIndex(newIndex) != null) {
            TileOwner.GetTileAtIndex(newIndex).MoveTile(direction);
        }

        if (IsMovableTile()) {
            SetTileIndex(newIndex, !Application.isPlaying);
        }
    }

    public void ResetTile() {
        SetTileIndex(startingTileIndex, true);
    }
}
