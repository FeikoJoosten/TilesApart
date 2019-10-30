using System.Collections;
using UnityEngine;

public class OnboardHelper : MonoBehaviour {
    [SerializeField]
    private Animation animationObject = null;
    [SerializeField]
    private float startupDelay = 2;
    [SerializeField]
    private float repeatingDelay = 5;
    [SerializeField]
    private float resetDelay = 10;

    private float currentTiming;
    private float targetTiming;

    private void Start() {
        Tile.OnTileStartedMoving += OnTileStartedMoving;
        GameMenus.OnPauseMenuOpened += OnPauseMenuOpened;
        GameMenus.OnPauseMenuClosed += OnPauseMenuClosed;

        targetTiming = startupDelay;

        // Only start playing if we actually can start playing
        if (animationObject == null) return;

        StartCoroutine(RepeatOnboardingAnimation());
    }

    private void OnDestroy() {
        Tile.OnTileStartedMoving -= OnTileStartedMoving;
        GameMenus.OnPauseMenuOpened -= OnPauseMenuOpened;
        GameMenus.OnPauseMenuClosed -= OnPauseMenuClosed;
    }

    private void OnTileStartedMoving(Vector2Int startingIndex, Vector2Int movementDirection) {
        // Add a delay to the repetition of the animation in case the player moved a tile.
        targetTiming += resetDelay;
    }

    private void OnPauseMenuOpened() {
        if (gameObject.activeInHierarchy == false) {
            return;
        }

        StopAllCoroutines();
    }

    private void OnPauseMenuClosed() {
        if (gameObject.activeInHierarchy == false) {
            return;
        }

        StartCoroutine(RepeatOnboardingAnimation());
    }

    private IEnumerator RepeatOnboardingAnimation() {
        while (Application.isPlaying) {
            // If the animation should play again, play it and reset the current timing value
            if (Time.time > targetTiming) {
                animationObject.Play();
                targetTiming = Time.time + repeatingDelay;
            }

            yield return null;
        }
    }
}
