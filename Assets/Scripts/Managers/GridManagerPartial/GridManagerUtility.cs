using UnityEngine;

public partial class GridManager : MonoBehaviour {

    [EditorButton]
    private void ResetGrid() {
        gridSize = new Vector2Int(1, 1);
        startTile = null;
        endTile = null;
        grid.rows[0].row.Clear();
        grid.rows.RemoveRange(1, GridSize.x - 1);
        CleanupScene();

        HandleGridUpdate();
        RecenterGrid();
    }

    [EditorButton]
    public void UpdateGridMeshes() {
        for (int x = 0; x < GridSize.x; x++) {
            for (int y = 0; y < GridSize.y; y++) {
                if (!grid.rows[x].row[y])
                    continue;

                ChangeTile(grid.rows[x].row[y], grid.rows[x].row[y].tileType, false, true);
            }
        }
    }

    [EditorButton]
    private void UpdatePlayerModel() {
        if (playerObject == null) {
            CheckForPlayer();
        }

        if (PlayerObject != null) {
            DestroyImmediate(playerObject.gameObject);
        }

        CheckForPlayer();
#if UNITY_EDITOR
        if (PlayerObject == null) return;

        UnityEditor.EditorUtility.SetDirty(playerObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(playerObject.gameObject.scene);
#endif
    }

    public void CheckForPlayer() {
        if (startTile == null) {
            if (Application.isPlaying) // We can assume we are currently editing the grid, no need to spit out an error if we aren't in play mode.
                Debug.LogError("No start tile was assigned");
            return;
        }

        GameObject[] gameObjects = gameObject.scene.GetRootGameObjects();
        for (int i = 0, length = gameObjects.Length; i < length; i++) {
            Player player = gameObjects[i].GetComponent<Player>();

            if (player == null) continue;

            playerObject = player;
            playerObject.transform.position = startTile.transform.position + GridData.PlayerSpawnOffset;
            return;
        }

        if (playerObject == false && GridData.PlayerPrefab != null) {
            playerObject = Instantiate(GridData.PlayerPrefab, startTile.transform.position + GridData.PlayerSpawnOffset, Quaternion.identity);
        }
    }

    private void RecenterGrid() {
        if (defaultTile == null) {
            return;
        }

        Vector3 prefabScale = defaultTile.GetComponent<Transform>().localScale;

        for (int x = 0; x < GridSize.x; x++) {
            for (int y = 0; y < GridSize.y; y++) {
                if (grid.rows[x].row[y] == null)
                    continue;

                grid.rows[x].row[y].transform.localPosition = GetTileLocalPosition(x, y);
            }
        }

        if (tileHolder) {
            float xWidth = (((gridSize.x * prefabScale.x) + (gridSize.x * gridSpacing.x)));
            if (gridSize.x == 1) {
                xWidth = 0;
            }

            float zWidth = (((gridSize.y * prefabScale.z) + (gridSize.y * gridSpacing.y)));
            if (gridSize.y == 1) {
                zWidth = 0;
            }

            tileHolder.transform.position = transform.localPosition + tileHolderOffset;

            Vector3 originalPosition = tileHolder.transform.localPosition;
            originalPosition.x = -xWidth + xWidth * 0.5f + (gridSize.x > 1 ? (prefabScale.x * 0.5f + gridSpacing.x * 0.5f) : 0);
            originalPosition.z = -zWidth + zWidth * 0.5f + (gridSize.y > 1 ? (prefabScale.z * 0.5f + gridSpacing.y * 0.5f) : 0);
            tileHolder.transform.localPosition = originalPosition;
        }
    }

    public void ChangeTile(Tile tile, TileType type, bool ignoreRotation = false, bool checkDirty = false) {
        if (!TileData) {
            Debug.LogWarning("No TileData set, cannot change tile");
            return;
        }

#if UNITY_EDITOR
        Mesh tileOriginalMesh = null;
        Material[] tileOriginalMaterials = null;
        bool[] tileOriginalDirections = tile.directions;
        Quaternion tileOriginalRoation = tile.transform.rotation;
        if (checkDirty) {
            tileOriginalMesh = tile.MeshFilter == null ? null : tile.MeshFilter.sharedMesh;
            tileOriginalMaterials = tile.MeshRenderer == null ? null : tile.MeshRenderer.sharedMaterials;
        }
#endif

        Mesh meshToUse = null;
        Material[] materialsToUse = null;
        bool[] directionsToUse = null;

        switch (type) {
            case TileType.TCrossing:
                meshToUse = TileData.crossingTileMesh;
                materialsToUse = TileData.crossingTileMaterials;
                directionsToUse = TileData.crossingDirections.GetDirections();
                break;
            case TileType.Curve:
                meshToUse = TileData.curveTileMesh;
                materialsToUse = TileData.curveTileMaterials;
                directionsToUse = TileData.curveDirections.GetDirections();
                break;
            case TileType.Pathless:
                meshToUse = TileData.pathlessTileMesh;
                materialsToUse = TileData.pathlessTileMaterials;
                directionsToUse = TileData.pathlessDirection.GetDirections();
                break;
            case TileType.End:
                meshToUse = TileData.endTileMesh;
                materialsToUse = TileData.endTileMaterials;
                directionsToUse = TileData.endDirections.GetDirections();
                endTile = tile;
                break;
            case TileType.Start:
                meshToUse = TileData.startTileMesh;
                materialsToUse = TileData.startTileMaterials;
                directionsToUse = TileData.startDirections.GetDirections();
                startTile = tile;
                break;
            case TileType.Straight:
                meshToUse = TileData.straightTileMesh;
                materialsToUse = TileData.straightTileMaterials;
                directionsToUse = TileData.straightDirections.GetDirections();
                break;
            case TileType.Border:
                materialsToUse = new Material[] { };
                directionsToUse = new[] { false, false, false, false };
                break;
            case TileType.Empty:
                DestroyTile(tile);
                return;
        }

#if UNITY_EDITOR
        bool objectsDoNotMatch = false;
        if (checkDirty) {

            if (directionsToUse != null && directionsToUse.Length != tileOriginalDirections.Length) {
                UnityEditor.EditorUtility.SetDirty(tile);
                objectsDoNotMatch = true;
            }

            if (objectsDoNotMatch == false && directionsToUse != null) {
                for (int i = 0; i < directionsToUse.Length; i++) {
                    if (directionsToUse[i] == tileOriginalDirections[i]) continue;

                    tile.directions = directionsToUse;
                    UnityEditor.EditorUtility.SetDirty(tile);
                    objectsDoNotMatch = true;
                    break;
                }
            }

            if (tileOriginalMaterials != null && materialsToUse != null) {
                objectsDoNotMatch = tileOriginalMaterials.Length != materialsToUse.Length;
                if (objectsDoNotMatch == true) {
                    tile.MeshRenderer.sharedMaterials = materialsToUse;
                    UnityEditor.EditorUtility.SetDirty(tile.MeshRenderer);
                }
            }

            if (objectsDoNotMatch == false && tileOriginalMaterials != null && materialsToUse != null) {
                for (int i = 0; i < tileOriginalMaterials.Length; i++) {
                    if (tileOriginalMaterials[i] == materialsToUse[i]) continue;

                    tile.MeshRenderer.sharedMaterials = materialsToUse;
                    UnityEditor.EditorUtility.SetDirty(tile.MeshRenderer);
                    objectsDoNotMatch = true;
                    break;
                }
            }

            if (tile.MeshFilter != null) {
                if (meshToUse != tileOriginalMesh) {
                    objectsDoNotMatch = true;
                    tile.MeshFilter.sharedMesh = meshToUse;
                    UnityEditor.EditorUtility.SetDirty(tile.MeshFilter);
                }
            }
        }
#else
		tile.MeshFilter.sharedMesh = meshToUse;
		tile.MeshRenderer.sharedMaterials = materialsToUse;
		tile.directions = directionsToUse;
#endif

        if (ignoreRotation == false) {
            float yRot = tile.transform.localEulerAngles.y;
            tile.transform.rotation = new Quaternion(0, 0, 0, 0);

            if (Mathf.Approximately(yRot, 270.0f)) {
                tile.RotateLeft();
            } else if (Mathf.Approximately(yRot, 90.0f)) {
                tile.RotateRight();
            } else if (Mathf.Approximately(yRot, 180.0f)) {
                tile.RotateLeft();
                tile.RotateLeft();
            }

#if UNITY_EDITOR
            Quaternion tileRotationToUse = tile.transform.rotation;
            if (tileRotationToUse != tileOriginalRoation) {
                UnityEditor.EditorUtility.SetDirty(tile);
                objectsDoNotMatch = true;
            }
#endif
        }

#if UNITY_EDITOR
        if (objectsDoNotMatch) {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(tile.gameObject.scene);
        }
#endif

        tile.tileType = type;
        tile.previousTileType = type;
    }
}