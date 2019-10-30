using System.Collections;
using UnityEngine;

public class TileAnimator : MonoBehaviour {

    private Tile tile;
    public Tile Tile {
        get => tile ?? (tile = GetComponent<Tile>());
        set => tile = value;
    }
    public bool IsUp => Mathf.Approximately(transform.localPosition.y, 0);
    public bool IsDown => Mathf.Approximately(transform.position.y, Tile.TileOwner.GridData.TileDownMovementOffset);

    public void Start() {
        Player.OnPlayerEndedMoving += OnPlayerEndedMoving;

        if (GetComponent<Animator>()) {
            Destroy(GetComponent<Animator>());
        }
    }

    private void OnDestroy() {
        if (Application.isPlaying == false) {
            return;
        }

        Player.OnPlayerEndedMoving -= OnPlayerEndedMoving;
    }

    private void OnPlayerEndedMoving() {
        if (gameObject.activeInHierarchy == false) {
            return;
        }

        // Move the starting tile downwards after the first move
        if (Tile.tileType == TileType.Start) {
            MoveDown(false, true);
        }
    }

    public void MoveUp(bool ignoreUpCheck = false, bool fadeIn = false) {
        if (IsUp == true && ignoreUpCheck == false) return;

        if (ignoreUpCheck) {
            transform.position = new Vector3(transform.position.x, Tile.TileOwner.GridData.TileDownMovementOffset, transform.position.z);
        }

        StartCoroutine(MoveHorizontally(new Vector3(transform.position.x, 0, transform.position.z)));

        if (fadeIn) {
            StartCoroutine(FadeIn());
        }
    }

    public void MoveDown(bool ignoreDownCheck = false, bool fadeOut = false) {
        if (IsUp == false && ignoreDownCheck == false) return;

        if (ignoreDownCheck) {
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        }

        StartCoroutine(MoveHorizontally(new Vector3(transform.position.x, Tile.TileOwner.GridData.TileDownMovementOffset, transform.position.z)));

        if (fadeOut) {
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator MoveHorizontally(Vector3 moveTarget) {
        Vector3 movementStartPosition = Tile.transform.position;
        float startTime = Time.time;
        float endTime = startTime + Tile.TileOwner.GridData.TileHorizontalMovement[Tile.TileOwner.GridData.TileHorizontalMovementKeyCount - 1].time;

        while (Time.time < endTime) {
            Tile.transform.position = Vector3.LerpUnclamped(movementStartPosition, moveTarget, Tile.TileOwner.GridData.TileHorizontalMovement.Evaluate((Time.time - startTime) / (endTime - startTime)));

            yield return null;
        }

        Tile.transform.position = moveTarget;
    }

    private IEnumerator FadeOut() {
        float startTime = Time.time;
        float endTime = Time.time + Tile.TileOwner.GridData.tileWrapFadeOutDuration;

        if (Tile.MeshRenderer == null) yield break;
        if (Tile.MeshRenderer.sharedMaterial == null) yield break;

        while (Time.time < endTime) {
            // Fading out effect
            Tile.MeshRenderer.GetPropertyBlock(Tile.PropertyBlock);
            Tile.PropertyBlock.SetFloat(Tile.TileOwner.TileData.fragmentationName, (Time.time - startTime) / (endTime - startTime));

            Tile.MeshRenderer.SetPropertyBlock(Tile.PropertyBlock);
            yield return null;
        }

        Tile.MeshRenderer.GetPropertyBlock(Tile.PropertyBlock);
        Tile.PropertyBlock.SetFloat(Tile.TileOwner.TileData.fragmentationName, 1);
        Tile.MeshRenderer.SetPropertyBlock(Tile.PropertyBlock);
    }

    private IEnumerator FadeIn() {
        float startTime = Time.time;
        float endTime = Time.time + Tile.TileOwner.GridData.tileWrapFadeOutDuration;

        if (Tile.MeshRenderer == null) yield break;
        if (Tile.MeshRenderer.sharedMaterial == null) yield break;

        while (Time.time < endTime) {
            // Fading out effect
            Tile.MeshRenderer.GetPropertyBlock(Tile.PropertyBlock);
            Tile.PropertyBlock.SetFloat(Tile.TileOwner.TileData.fragmentationName, 1.0f - (Time.time - startTime) / (endTime - startTime));

            Tile.MeshRenderer.SetPropertyBlock(Tile.PropertyBlock);
            yield return null;
        }

        Tile.MeshRenderer.GetPropertyBlock(Tile.PropertyBlock);
        Tile.PropertyBlock.SetFloat(Tile.TileOwner.TileData.fragmentationName, 0);
        Tile.MeshRenderer.SetPropertyBlock(Tile.PropertyBlock);
    }

    public void MoveDownInstant(bool fadeOut = false) {
        if (IsUp == false) return;

        if (transform == null)
            return;

        transform.position = new Vector3(transform.position.x, Tile.TileOwner.GridData.TileDownMovementOffset, transform.position.z);

        if (fadeOut) {
            Tile.MeshRenderer.GetPropertyBlock(Tile.PropertyBlock);
            Tile.PropertyBlock.SetFloat(Tile.TileOwner.TileData.fragmentationName, 1);
            Tile.MeshRenderer.SetPropertyBlock(Tile.PropertyBlock);
        }
    }

    // Temp function to 'fix' a bug
    public void ForceUp() {
        StopAllCoroutines();
        Tile.transform.position = new Vector3(Tile.transform.position.x, 0, Tile.transform.position.z);
    }
}