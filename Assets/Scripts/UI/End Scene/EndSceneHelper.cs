using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndSceneHelper : AudioPlayer {
    [SerializeField]
    private Button endMenuButton = null;
    [SerializeField]
    private Button creditsMenuBackButton = null;

    [Header("Sounds")]
    [SerializeField]
    private List<AudioClip> buttonSounds = null;

#if UNITY_ANDROID
    private void Awake() {
        AndroidBackButton.AndroidBackButtonPressed += OnAndroidBackButtonPressed;
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        AndroidBackButton.AndroidBackButtonPressed -= OnAndroidBackButtonPressed;
    }

    private void OnAndroidBackButtonPressed() {
        if (endMenuButton == null || creditsMenuBackButton == null) return;

        if (endMenuButton.gameObject.activeInHierarchy) {
            endMenuButton.onClick.Invoke();
        } else if (creditsMenuBackButton.gameObject.activeInHierarchy) {
            creditsMenuBackButton.onClick.Invoke();
        }
    }
#else
	private void Awake() {
		// Just to get rid of a warning because backButton isn't being used.
		endMenuButton.name = endMenuButton.name;
		creditsMenuBackButton.name = creditsMenuBackButton.name;
	}
#endif

    /// <summary> Play a random button click sound </summary>
    public void PlayRandomButtonSound(bool playOnce = true) {
        if (buttonSounds == null) return;

        PlayRandomSound(buttonSounds, playOnce);
    }
}
