using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GridManager : MonoBehaviour {

	private void OnValidate() {
		transform.position = Vector3.zero;

		if (GridData == null) {
			Debug.LogWarning("Please assign a grid data object");
			return;
		}

		if (gameObject.activeInHierarchy)
			StartCoroutine(WaitForEndOfFrame());
	}

	private IEnumerator WaitForEndOfFrame() {
#if UNITY_EDITOR
		bool shouldHandleTileChange = UnityEditor.EditorApplication.isPlaying ||
		                              UnityEditor.EditorApplication.isCompiling ||
		                              UnityEditor.EditorApplication.isUpdating ||
		                              IsGridMoving == false ||
		                              playerObject.isMoving == false;
#endif

		yield return null;

		HandleGridUpdate();

		RecenterGrid();

#if UNITY_EDITOR
		// Added should update overwrite, to disallow updating while recompiling, scene loading/unloading, etc.
		HandleTileChange(shouldHandleTileChange);
#else
		HandleTileChange();
#endif

		CleanupScene();

		CheckForPlayer();
	}

	private void HandleGridUpdate() {
		if (grid == null) {
			grid = new ArrayLayout();
		}

		if (grid.rows == null) {
			grid.rows = new List<ArrayLayout.RowData>();
			ArrayLayout.RowData column = new ArrayLayout.RowData { row = new List<Tile> { EditorCreateNewTileObject(0, 0) } };
			grid.rows.Add(column);
		}

		// In case we are editing data, not the grid size, return
		if (gridSize.x == grid.rows.Count && gridSize.x > 0) {
			if (gridSize.y == grid.rows[0].row.Count && gridSize.y > 0) {
				return;
			}
		}

		if (gridSize.x <= 0) {
			// Always make sure we have at least 1 value
			gridSize.x = 1;
			int endIndex = grid.rows.Count - gridSize.x;

			DestroyTileRow(gridSize.x, endIndex);
			grid.rows.RemoveRange(gridSize.x, endIndex);
			return;
		}

		if (gridSize.y <= 0) {
			// Always make sure we have at least 1 value
			gridSize.y = 1;
			for (int i = 0; i < grid.rows.Count; i++) {
				int endIndex = grid.rows[i].row.Count - gridSize.y;

				DestroyTileColumn(grid.rows[i].row, gridSize.y, endIndex);
				grid.rows[i].row.RemoveRange(gridSize.y, endIndex);
			}
			return;
		}

		// Make the rows bigger in case of a resize
		if (gridSize.x > grid.rows.Count) {
			int difference = gridSize.x - grid.rows.Count;

			// Update the rows and make sure the columns are filled up with the correct size
			for (int i = grid.rows.Count, size = grid.rows.Count + difference; i < size; i++) {
				List<Tile> columns = new List<Tile>();
				if (columns.Count < gridSize.y) {
					for (int y = 0; y < gridSize.y; y++) {
						columns.Add(EditorCreateNewTileObject(i, y));
					}
				}
				ArrayLayout.RowData rowData = new ArrayLayout.RowData { row = columns };
				grid.rows.Add(rowData);
			}
		}
		// Make the rows smaller in case of a resize
		else if (gridSize.x < grid.rows.Count) {
			int endIndex = grid.rows.Count - gridSize.x;

			DestroyTileRow(gridSize.x, endIndex);
			grid.rows.RemoveRange(gridSize.x, endIndex);
		}
		// In case only the gridSize.y has been updated
		else {
			// Fill up the columns
			for (int x = 0; x < gridSize.x; x++) {
				if (gridSize.y > grid.rows[x].row.Count) {
					int difference = gridSize.y - grid.rows[x].row.Count;

					for (int y = grid.rows[x].row.Count, size = grid.rows[x].row.Count + difference; y < size; y++) {
						grid.rows[x].row.Add(EditorCreateNewTileObject(x, y));
					}
				}
				else if (gridSize.y < grid.rows[x].row.Count) {
					int endIndex = grid.rows[x].row.Count - gridSize.y;

					DestroyTileColumn(grid.rows[x].row, gridSize.y, endIndex);
					grid.rows[x].row.RemoveRange(gridSize.y, endIndex);
				}
			}
		}
	}

	public void HandleTileChange(bool shouldUpdate = true, bool ignorePreviousTileType = false) {
#if UNITY_EDITOR

		// Added should update overwrite, to disallow updating while recompiling, scene loading/unloading, etc.
		if (shouldUpdate == false) {
			return;
		}

		for (int x = 0; x < grid.rows.Count; x++) {
			for (int y = 0; y < grid.rows[x].row.Count; y++) {
				if (!grid.rows[x].row[y])
					continue;

				if (grid.rows[x].row[y].tileType == grid.rows[x].row[y].previousTileType && ignorePreviousTileType == false)
					continue;

				ChangeTile(grid.rows[x].row[y], grid.rows[x].row[y].tileType, false, true);

				if (Application.isPlaying == false && grid.rows[x].row[y] != null && ignorePreviousTileType == false)
					UnityEditor.Selection.activeGameObject = grid.rows[x].row[y].gameObject;
			}
		}
#endif
	}

	private Tile EditorCreateNewTileObject(int x, int y) {
		return EditorCreateNewTileObject(new Vector2Int(x, y));
	}

	private Tile EditorCreateNewTileObject(Vector2Int tileindex) {
		if (defaultTile == null) {
			Debug.LogWarning("If you want the grid manager to automatically fill the grid for you, please make sure you fill in the defaultTile variable.");
			return null;
		}

		Tile tileToReturn = Instantiate(defaultTile, tileHolder == null ? transform : tileHolder);
		tileToReturn.Initialize(this, tileindex);
		ChangeTile(tileToReturn, tileToReturn.tileType);
		return tileToReturn;
	}

	private void DestroyTileRow(int rowIndex) {
		if (rowIndex >= grid.rows.Count) {
			return;
		}

		DestroyTileColumn(grid.rows[rowIndex].row);
	}

	private void DestroyTileRow(int startIndex, int endIndex) {
		if (startIndex < 0 || startIndex > endIndex) {
			return;
		}

		if (endIndex >= grid.rows.Count) {
			return;
		}

		for (int i = startIndex; i <= endIndex; i++) {
			DestroyTileColumn(grid.rows[i].row);
		}
	}

	private void DestroyTileColumn(List<Tile> columnToDestroy) {
		for (int i = 0; i < columnToDestroy.Count; i++) {
			DestroyTile(columnToDestroy[i]);
		}
	}

	private void DestroyTileColumn(List<Tile> columnToDestroy, int startIndex, int endIndex) {
		if (startIndex < 0 || startIndex > endIndex) {
			return;
		}

		if (endIndex >= columnToDestroy.Count) {
			return;
		}

		for (int i = startIndex; i <= endIndex; i++) {
			DestroyTile(columnToDestroy[i]);
		}
	}

	private void DestroyTile(Tile tileToDestroy) {
		if (tileToDestroy == null)
			return;

		if (Application.isPlaying) {
			Destroy(tileToDestroy.gameObject);
		}
		else {
#if UNITY_EDITOR
			UnityEditor.EditorApplication.delayCall += () => {
				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				if (tileToDestroy != null)
					DestroyImmediate(tileToDestroy.gameObject);
			};
#endif
		}
	}

	public void CleanupScene() {
		Tile[] allTiles = FindObjectsOfType<Tile>();
		GridManager[] allManagers = FindObjectsOfType<GridManager>();

		for (int i = 0; i < allTiles.Length; i++) {
			bool shouldDestroy = true;

			for (int j = 0; j < allManagers.Length; j++) {
				for (int x = 0; x < allManagers[j].grid.rows.Count; x++) {
					if (allManagers[j].grid.rows[x].row.Contains(allTiles[i])) {
						shouldDestroy = false;
						break;
					}
				}
				if (shouldDestroy == false) {
					break;
				}
			}

			if (shouldDestroy) {
				DestroyTile(allTiles[i]);
			}
		}

		if (startTile != null) {
			if (startTile.tileType != TileType.Start) {
				startTile = null;
			}
		}

		if (endTile != null) {
			if (endTile.tileType != TileType.End) {
				endTile = null;
			}
		}

		if (playerObject != null && startTile == null) {
			if (Application.isPlaying) {
				Destroy(playerObject.gameObject);
			}
			else {
#if UNITY_EDITOR
				UnityEditor.EditorApplication.delayCall += () => {
					// ReSharper disable once ConditionIsAlwaysTrueOrFalse
					if (playerObject != null)
						DestroyImmediate(playerObject.gameObject);
				};
#endif
			}
		}
	}
}