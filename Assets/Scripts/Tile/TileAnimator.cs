using System.Collections;
using UnityEngine;

public class TileAnimator : MonoBehaviour {

    private Tile tile;
    public Tile Tile {
        get { return tile ?? (Tile = GetComponent<Tile>()); }
        set { tile = value; }
    }
    public bool isUp => Mathf.Approximately(transform.localPosition.y, 0);

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
        if (isUp == true && ignoreUpCheck == false) return;

        StopAllCoroutines();
        StartCoroutine(MoveHorizontally(new Vector3(transform.position.x, 0, transform.position.z)));

        if (fadeIn) {
            StartCoroutine(FadeIn());
        }
    }

    public void MoveDown(bool ignoreDownCheck = false, bool fadeOut = false) {
        if (isUp == false && ignoreDownCheck == false) return;

        StopAllCoroutines();
        StartCoroutine(MoveHorizontally(new Vector3(transform.position.x, Tile.TileOwner.GridData.TileDownMovementOffset, transform.position.z)));

        if (fadeOut) {
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator MoveHorizontally(Vector3 moveTarget) {
        Vector3 movementStartPosition = Tile.transform.position;
        float endTime = Tile.TileOwner.GridData.TileHorizontalMovement[Tile.TileOwner.GridData.TileHorizontalMovement.length - 1].time;

        float currentAnimationTime = 0;

        while (currentAnimationTime < endTime) {
            Tile.transform.position = Vector3.LerpUnclamped(movementStartPosition, moveTarget, Tile.TileOwner.GridData.TileHorizontalMovement.Evaluate(currentAnimationTime));

            currentAnimationTime += Time.deltaTime;

            yield return null;
        }

        Tile.transform.position = moveTarget;
    }

    private IEnumerator FadeOut() {
        float endTime = Tile.TileOwner.GridData.tileWrapFadeOutDuration;
        float currentFadeStep = 0;

        if (Tile.MeshRenderer == null) yield break;
        if (Tile.MeshRenderer.sharedMaterial == null) yield break;

        while (currentFadeStep < endTime) {
            // Fading out effect
            Tile.MeshRenderer.GetPropertyBlock(Tile.PropertyBlock);
            Tile.PropertyBlock.SetFloat(Tile.TileOwner.TileData.fragmentationName, currentFadeStep / Tile.TileOwner.GridData.tileWrapFadeOutDuration);

            currentFadeStep += Time.deltaTime;
            Tile.MeshRenderer.SetPropertyBlock(Tile.PropertyBlock);
            yield return null;
        }

        Tile.MeshRenderer.GetPropertyBlock(Tile.PropertyBlock);
        Tile.PropertyBlock.SetFloat(Tile.TileOwner.TileData.fragmentationName, 1);
        Tile.MeshRenderer.SetPropertyBlock(Tile.PropertyBlock);
    }

    private IEnumerator FadeIn() {
        float endTime = Tile.TileOwner.GridData.tileWrapFadeOutDuration;
        float currentFadeStep = 0;

        if (Tile.MeshRenderer == null) yield break;
        if (Tile.MeshRenderer.sharedMaterial == null) yield break;

        while (currentFadeStep < endTime) {
            // Fading out effect
            Tile.MeshRenderer.GetPropertyBlock(Tile.PropertyBlock);
            Tile.PropertyBlock.SetFloat(Tile.TileOwner.TileData.fragmentationName, 1.0f - currentFadeStep / Tile.TileOwner.GridData.tileWrapFadeOutDuration);

            currentFadeStep += Time.deltaTime;
            Tile.MeshRenderer.SetPropertyBlock(Tile.PropertyBlock);
            yield return null;
        }

        Tile.MeshRenderer.GetPropertyBlock(Tile.PropertyBlock);
        Tile.PropertyBlock.SetFloat(Tile.TileOwner.TileData.fragmentationName, 0);
        Tile.MeshRenderer.SetPropertyBlock(Tile.PropertyBlock);
    }

    public void MoveDownInstant(bool fadeOut = false) {
        if (isUp == false) return;

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