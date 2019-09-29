using System.Collections.Generic;
using UnityEngine;

public class MainMenuHelper : AudioPlayer {
    [SerializeField]
    private GameObject mainMenu = null;
    [SerializeField]
    private GameObject settingsMenu = null;
    [SerializeField]
    private GameObject chapterSelectionMenu = null;
    [SerializeField]
    private GameObject levelSelectionMenu = null;
    [SerializeField]
    private GameObject creditsMenu = null;

    [Header("Sounds")]
    [SerializeField]
    private List<AudioClip> buttonSounds = null;

#if UNITY_ANDROID
    private void Awake() {
        if (mainMenu == null || settingsMenu == null || chapterSelectionMenu == null ||
            levelSelectionMenu == null || creditsMenu == null) return;

        AndroidBackButton.AndroidBackButtonPressed += OnAndroidButtonPressed;
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        AndroidBackButton.AndroidBackButtonPressed -= OnAndroidButtonPressed;
    }

    private void OnAndroidButtonPressed() {
        if (mainMenu.activeInHierarchy) {
            PlayRandomButtonSound();
            Application.Quit();
        }

        if (settingsMenu.activeInHierarchy) {
            settingsMenu.SetActive(false);
            mainMenu.SetActive(true);
            PlayRandomButtonSound();
        } else if (creditsMenu.activeInHierarchy) {
            creditsMenu.SetActive(false);
            settingsMenu.SetActive(true);
            PlayRandomButtonSound();
        } else if (chapterSelectionMenu.activeInHierarchy) {
            chapterSelectionMenu.SetActive(false);
            mainMenu.SetActive(true);
            PlayRandomButtonSound();
        } else if (levelSelectionMenu.activeInHierarchy) {
            levelSelectionMenu.SetActive(false);
            chapterSelectionMenu.SetActive(true);
            PlayRandomButtonSound();
        }
    }
#else
	private void Awake() {
		// Just to get rid of a warning because backButton isn't being used.
		mainMenu.name = mainMenu.name;
		settingsMenu.name = settingsMenu.name;
		chapterSelectionMenu.name = chapterSelectionMenu.name;
		levelSelectionMenu.name = levelSelectionMenu.name;
		creditsMenu.name = creditsMenu.name;
	}
#endif

    /// <summary> Play a random button click sound </summary>
    public void PlayRandomButtonSound(bool playOnce = true) {
        if (buttonSounds == null) return;

        PlayRandomSound(buttonSounds, playOnce);
    }
}
