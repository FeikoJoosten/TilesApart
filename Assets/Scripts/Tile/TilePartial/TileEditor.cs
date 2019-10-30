using System.Collections;
using UnityEngine;

public partial class Tile : MonoBehaviour {

    private void OnValidate() {
        if (Application.isPlaying)
            return;

        if (meshFilter == null) {
            meshFilter = GetComponent<MeshFilter>();
        }
        if (meshRenderer == null) {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        if (gameObject.activeInHierarchy)
            StartCoroutine(WaitForEndOfFrame());
    }

    private IEnumerator WaitForEndOfFrame() {
        yield return null;

        if (previousTileType == tileType) {
            yield break;
        }

        if (TileOwner) {
            TileOwner.HandleTileChange();
            TileOwner.CleanupScene();
            TileOwner.CheckForPlayer();
        }
    }

    public void Initialize(GridManager owner, Vector2Int originalTileIndex) {
        TileOwner = owner;
        SetTileIndex(originalTileIndex, true);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Only use this method in the editor itself, use the MoveTile method in runtime.
    /// </summary>
    [EditorButton, ContextMenu(nameof(MoveForwards))]
    public void MoveForwards() {
        if (Application.isPlaying) {
            Debug.LogWarning("This is a test method only, please use the MoveTile method instead");
            return;
        }

        MoveTile(Vector3.forward, true);
    }

    /// <summary>
    /// Only use this method in the editor itself, use the MoveTile method in runtime.
    /// </summary>
    [EditorButton, ContextMenu(nameof(MoveBackwards))]
    public void MoveBackwards() {
        if (Application.isPlaying) {
            Debug.LogWarning("This is a test method only, please use the MoveTile method instead");
            return;
        }

        MoveTile(Vector3.back, true);
    }

    /// <summary>
    /// Only use this method in the editor itself, use the MoveTile method in runtime.
    /// </summary>
    [EditorButton, ContextMenu(nameof(MoveLeft))]
    public void MoveLeft() {
        if (Application.isPlaying) {
            Debug.LogWarning("This is a test method only, please use the MoveTile method instead");
            return;
        }

        MoveTile(Vector3.left, true);
    }

    /// <summary>
    /// Only use this method in the editor itself, use the MoveTile method in runtime.
    /// </summary>
    [EditorButton, ContextMenu(nameof(MoveRight))]
    public void MoveRight() {
        if (Application.isPlaying) {
            Debug.LogWarning("This is a test method only, please use the MoveTile method instead");
            return;
        }

        MoveTile(Vector3.right, true);
    }
#endif

    [EditorButton]
    public void RotateLeft() {
        transform.Rotate(0, -90.0f, 0);

        bool[] db = directions;
        directions = new bool[4] { db[1], db[2], db[3], db[0] };
    }

    [EditorButton]
    public void RotateRight() {
        transform.Rotate(0, 90.0f, 0);

        bool[] db = directions;
        directions = new bool[4] { db[3], db[0], db[1], db[2] };
    }
}
