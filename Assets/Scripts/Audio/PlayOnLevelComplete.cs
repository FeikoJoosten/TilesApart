using UnityEngine;

public class PlayOnLevelComplete : AudioPlayer {
    [Header("Audio")]
    [SerializeField]
    private AudioClip[] levelCompletionSouds = null;

    private void Awake() {
        Player.OnPlayerWon += OnPlayerWon;
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        Player.OnPlayerWon -= OnPlayerWon;
    }

    private void OnPlayerWon(string completedLevel) {
        PlayRandomSound(levelCompletionSouds);
    }
}
