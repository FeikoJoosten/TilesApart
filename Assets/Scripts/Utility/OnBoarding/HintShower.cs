using UnityEngine;

public class HintShower : MonoBehaviour {
    [Tooltip("This is the required amount of deaths, before this script will use the `Number Of Turns This Session` variable")]
    [SerializeField]
    private int numberOfDeaths = 3;
    [Tooltip("Once the number of deaths has been reached, this script will wait till the player has done these amount of turns before it starts playing an animation on the `Animation Object`")]
    [SerializeField]
    private int numberOfTurnsThisSession = 2;

    [SerializeField]
    private Animation animationObject = null;

    private static int currentDeathCount;
    private int currentTurnCount;

    private void Awake() {
        Player.OnPlayerEndedMoving += OnPlayerMove;
        Player.OnPlayerDied += OnPlayerDeath;
        Player.OnPlayerWon += OnPlayerWin;
        LevelManager.OnMainMenuLoading += OnMainMenuLoading;
    }

    private void OnDestroy() {
        Player.OnPlayerEndedMoving -= OnPlayerMove;
        Player.OnPlayerDied -= OnPlayerDeath;
        Player.OnPlayerWon -= OnPlayerWin;
        LevelManager.OnMainMenuLoading -= OnMainMenuLoading;
    }

    private void OnPlayerDeath(string diedLevel) {
        currentDeathCount++;
    }

    private void OnPlayerWin(string wonLevel) {
        ResetDeathCounter();
    }

    private void OnMainMenuLoading() {
        ResetDeathCounter();
    }

    private void OnPlayerMove() {
        currentTurnCount++;

        // Only play an animation if the requirements are met
        if (CheckRequirements() == false) return;

        PlayAnimation();
    }

    private bool CheckRequirements() {
        if (currentDeathCount < numberOfDeaths) return false;
        if (currentTurnCount < numberOfTurnsThisSession) return false;
        return true;
    }

    private void PlayAnimation() {
        if (animationObject == null) return;

        animationObject.Play();
    }

    private void ResetDeathCounter() {
        currentDeathCount = 0;
    }
}
