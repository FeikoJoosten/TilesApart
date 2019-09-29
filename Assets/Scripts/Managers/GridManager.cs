using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct GridHolder {
    public List<Transform> tiles;
}

public partial class GridManager : MonoBehaviour {
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
    public GameObject tempTile;
    public bool IsLevelRisingUp { get; private set; }
    public bool IsGridMoving {
        get {
            {
                if (grid.rows == null) {
                    return false;
                }

                for (int x = 0; x < grid.rows.Count; x++) {
                    for (int y = 0; y < grid.rows[x].row.Count; y++) {
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

    private void OnEnable() {
        StopAllCoroutines();
        StartCoroutine(RiseLevel());
    }

    public IEnumerator RiseLevel(bool instant = false) {
        IsLevelRisingUp = true;

        for (int x = 0; x < grid.rows.Count; x++) {
            for (int y = 0; y < grid.rows[x].row.Count; y++) {
                if (grid.rows[x].row[y] == null)
                    continue;

                if (grid.rows[x].row[y].tileType == TileType.Start || grid.rows[x].row[y].TileAnimator == false)
                    continue;

                if (instant) {
                    //grid.rows[x].row[y].tileAnimator.MoveDownInstant(); Nothing yet, just copied SinkLevel
                    continue;
                }

                grid.rows[x].row[y].TileAnimator.MoveUp(false, true);
            }
        }

        // Wait for the entire level to raise back up
        while (true) {
            bool shouldWait = false;
            for (int x = 0; x < grid.rows.Count; x++) {
                if (shouldWait == true) {
                    break;
                }

                for (int y = 0; y < grid.rows[x].row.Count; y++) {
                    if (grid.rows[x].row[y] == null) {
                        continue;
                    }

                    if (grid.rows[x].row[y].TileAnimator.isUp == false) {
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

    public void AlignPlayerRotation() {
        if (startTile)
            PlayerObject.transform.rotation = startTile.transform.rotation;
    }

    public Tile GetTileAtIndex(int x, int y) {
        return GetTileAtIndex(new Vector2Int(x, y));
    }

    public Tile GetTileAtIndex(Vector2Int tileIndex) {
        if (tileIndex.x >= grid.rows.Count || tileIndex.x < 0 || tileIndex.y < 0) {
            return null;
        }

        if (tileIndex.y >= grid.rows[tileIndex.x].row.Count) {
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

    public void UpdateTileIndexLocation(Vector2Int oldIndex, Vector2Int newIndex, Tile tileToUpdate, bool ignoreOldIndex = false) {
        if (newIndex.x >= grid.rows.Count || newIndex.x < 0 || newIndex.y < 0) {
            return;
        }

        if (newIndex.y >= grid.rows[newIndex.x].row.Count) {
            return;
        }

        if (grid.rows[newIndex.x].row[newIndex.y] == null) {
            grid.rows[newIndex.x].row[newIndex.y] = tileToUpdate;

            if (ignoreOldIndex == false)
                grid.rows[oldIndex.x].row[oldIndex.y] = null;
        } else {
            grid.rows[newIndex.x].row[newIndex.y] = tileToUpdate;
        }
    }

    public void EmptyTileIndexLocation(int x, int y) {
        EmptyTileIndexLocation(new Vector2Int(x, y));
    }

    public void EmptyTileIndexLocation(Vector2Int tileIndex) {
        if (tileIndex.x >= grid.rows.Count || tileIndex.x < 0 || tileIndex.y < 0) {
            return;
        }

        if (tileIndex.y >= grid.rows[tileIndex.x].row.Count) {
            return;
        }

        grid.rows[tileIndex.x].row[tileIndex.y] = null;
    }

    public void SinkLevel(bool instant = false, bool fadeLevel = false, bool ignoreStartTile = true, bool ignoreEndTile = true) {
        for (int x = 0; x < grid.rows.Count; x++) {
            for (int y = 0; y < grid.rows[x].row.Count; y++) {
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
        // We need to save the tiles to reset in a separate list, otherwise they can overwrite each other
        List<Tile> tilesToReset = new List<Tile>();

        for (int x = 0; x < grid.rows.Count; x++) {
            for (int y = 0; y < grid.rows[x].row.Count; y++) {
                if (grid.rows[x].row[y] == null) continue;

                tilesToReset.Add(grid.rows[x].row[y]);
            }
        }

        for (int x = 0; x < grid.rows.Count; x++) {
            for (int y = 0; y < grid.rows[x].row.Count; y++) {
                grid.rows[x].row[y] = null;
            }
        }

        tilesToReset.ForEach(tile => tile.ResetTile());
    }
}