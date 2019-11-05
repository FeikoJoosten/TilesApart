using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct GridHolder {
    public List<Transform> tiles;
}

public partial class GridManager : MonoBehaviour {
    public static event System.Action<GridManager> OnResetTiles = delegate { };

    [Header("Grid data settings")]
    [SerializeField]
    private GridData gridData = null;

    [SerializeField]
    private Tile defaultTile = null;
    [SerializeField]
    private TileData defaultTileData = null;

    [Header("Layout")]
    [SerializeField]
    private Transform tileHolder = null;
    [SerializeField]
    private Vector3 tileHolderOffset = new Vector3();
    [SerializeField]
    private Vector2Int gridSize = new Vector2Int(3, 3);
    [SerializeField]
    private Vector2 gridSpacing = new Vector2(2f, 2f);
    public Vector2 GridSpacing => gridSpacing;
    [ReadOnly]
    public Tile startTile = null;
    [ReadOnly]
    public Tile endTile = null;
    [SerializeField]
    private ArrayLayout grid = null;
    public ArrayLayout Grid => grid;

    public Vector2Int GridSize => gridSize;
    public GridData GridData => gridData;
    public TileData TileData => defaultTileData;
    public GameObject TempTile { get; private set; }
    public bool IsLevelRisingUp { get; private set; }
    public bool IsGridMoving {
        get {
            {
                if (grid.rows == null) {
                    return false;
                }

                for (int x = 0; x < GridSize.x; x++) {
                    for (int y = 0; y < GridSize.y; y++) {
                        if (grid.rows[x].row[y] == null)
                            continue;

                        if (grid.rows[x].row[y].moving) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }

    private Player playerObject = null;
    public Player PlayerObject {
        get {
            if (playerObject == null) {
                CheckForPlayer();
            }

            return playerObject;
        }
    }

    private void Start() {
        StopAllCoroutines();
        SinkLevel(true, true, true, false);
        StartCoroutine(RiseLevel(false, true, true, false));
    }

    public IEnumerator RiseLevel(bool instant = false, bool ignoreUpCheck = false, bool ignoreStartTile = true, bool ignoreEndTile = true) {
        IsLevelRisingUp = true;

        for (int x = 0; x < GridSize.x; x++) {
            for (int y = 0; y < GridSize.y; y++) {
                if (grid.rows[x].row[y] == null)
                    continue;

                if (grid.rows[x].row[y].TileAnimator == false ||
                    (grid.rows[x].row[y].tileType == TileType.Start && ignoreStartTile) ||
                    (grid.rows[x].row[y].tileType == TileType.End && ignoreEndTile))
                    continue;

                if (instant) {
                    //grid.rows[x].row[y].tileAnimator.MoveUpInstant(); Nothing yet, just copied SinkLevel
                    continue;
                }

                grid.rows[x].row[y].TileAnimator.MoveUp(ignoreUpCheck, true);
            }
        }

        // Wait for the entire level to raise back up
        while (true) {
            bool shouldWait = false;
            for (int x = 0; x < GridSize.x; x++) {
                if (shouldWait == true) {
                    break;
                }

                for (int y = 0; y < GridSize.y; y++) {
                    if (grid.rows[x].row[y] == null) {
                        continue;
                    }

                    if (grid.rows[x].row[y].TileAnimator.IsUp == false) {
                        shouldWait = true;
                        break;
                    }
                }
            }

            if (shouldWait == false) {
                break;
            }

            yield return null;
        }

        IsLevelRisingUp = false;
        Physics.SyncTransforms();
    }

    public bool IsLevelDown(bool ignoreStartTile = true, bool ignoreEndTile = true) {
        for (int x = 0; x < GridSize.x; x++) {
            for (int y = 0; y < GridSize.y; y++) {
                if (grid.rows[x].row[y] == null) continue;
                if (grid.rows[x].row[y].tileType == TileType.Start && ignoreStartTile) continue;
                if (grid.rows[x].row[y].tileType == TileType.End && ignoreEndTile) continue;

                if (grid.rows[x].row[y].TileAnimator.IsDown == false) return false;
            }
        }

        return true;
    }

    public void AlignPlayerRotation() {
        if (startTile)
            PlayerObject.transform.rotation = startTile.transform.rotation;
    }

    public Tile GetTileAtIndex(int x, int y) {
        return GetTileAtIndex(new Vector2Int(x, y));
    }

    public Tile GetTileAtIndex(Vector2Int tileIndex) {
        if (tileIndex.x >= GridSize.x || tileIndex.x < 0 || tileIndex.y < 0) {
            return null;
        }

        if (tileIndex.y >= GridSize.y) {
            return null;
        }

        return grid.rows[tileIndex.x].row[tileIndex.y];
    }

    public Vector3 GetTileLocalPosition(int x, int y) {
        return GetTileLocalPosition(new Vector2Int(x, y));
    }

    public Vector3 GetTileLocalPosition(Vector2Int tileIndex) {
        if (defaultTile == null) {
            Debug.Log("The default tile has not been assigned");
            return new Vector3(0, -5, 0);
        }

        Vector3 prefabScale = defaultTile.GetComponent<Transform>().localScale;

        return new Vector3(
            (tileIndex.x * prefabScale.x) + (tileIndex.x * gridSpacing.x),
            transform.position.y,
            (tileIndex.y * prefabScale.z) + (tileIndex.y * gridSpacing.y));
    }

    public Vector3 GetTileWorldPosition(int x, int y) {
        return GetTileWorldPosition(new Vector2Int(x, y));
    }

    public Vector3 GetTileWorldPosition(Vector2Int tileIndex) {
        Vector3 localPosition = GetTileLocalPosition(tileIndex);

        return tileHolder.transform.TransformPoint(localPosition);
    }

    public void InitializeTempTile() {
        TempTile = new GameObject("TileOwner.TempTile");
        TempTile.AddComponent<MeshRenderer>();
        TempTile.AddComponent<MeshFilter>();
    }

    public void UpdateTileIndexLocation(Vector2Int oldIndex, Vector2Int newIndex, Tile tileToUpdate, bool ignoreOldIndex = false, bool isResetting = false) {
        if (newIndex.x >= grid.rows.Count || newIndex.x < 0 || newIndex.y < 0) {
            return;
        }

        if (newIndex.y >= grid.rows[newIndex.x].row.Count) {
            return;
        }

        if (grid.rows[newIndex.x].row[newIndex.y] == null) {
            grid.rows[newIndex.x].row[newIndex.y] = tileToUpdate;

            if (ignoreOldIndex == false && isResetting == false)
                grid.rows[oldIndex.x].row[oldIndex.y] = null;
        } else {
            grid.rows[newIndex.x].row[newIndex.y] = tileToUpdate;
        }
    }

    public void EmptyTileIndexLocation(int x, int y) {
        EmptyTileIndexLocation(new Vector2Int(x, y));
    }

    public void EmptyTileIndexLocation(Vector2Int tileIndex) {
        if (tileIndex.x >= GridSize.x || tileIndex.x < 0 || tileIndex.y < 0) {
            return;
        }

        if (tileIndex.y >= GridSize.y) {
            return;
        }

        grid.rows[tileIndex.x].row[tileIndex.y] = null;
    }

    public void ForceTileToSlot(Vector2Int newIndex, Tile tileToForce) {
        if (newIndex.x < 0 || newIndex.x >= GridSize.x || newIndex.y < 0 || newIndex.y >= GridSize.y)
            return;

        Grid.rows[newIndex.x].row[newIndex.y] = tileToForce;
    }

    public void SinkLevel(bool instant = false, bool fadeLevel = false, bool ignoreStartTile = true, bool ignoreEndTile = true) {
        for (int x = 0; x < GridSize.x; x++) {
            for (int y = 0; y < GridSize.y; y++) {
                if (grid.rows[x].row[y] == null)
                    continue;

                if (grid.rows[x].row[y].TileAnimator == false)
                    continue;

                if (ignoreStartTile && grid.rows[x].row[y].tileType == TileType.Start) {
                    continue;
                }

                if (instant) {
                    grid.rows[x].row[y].TileAnimator.MoveDownInstant(fadeLevel);
                    continue;
                }

                if (ignoreEndTile && grid.rows[x].row[y].tileType == TileType.End)
                    continue;

                grid.rows[x].row[y].TileAnimator.MoveDown(true, fadeLevel);
            }
        }
    }

    public void ResetLevel() {
        for (int x = 0; x < GridSize.x; x++) {
            for (int y = 0; y < GridSize.y; y++) {
                grid.rows[x].row[y] = null;
            }
        }

        OnResetTiles(this);
    }
}