using System.Collections.Generic;
using UnityEngine;

public class GameMenus : AudioPlayer {
    public static event System.Action OnPauseMenuOpened = delegate { };
    public static event System.Action OnPauseMenuClosed = delegate { };
    public static event System.Action OnUndoPressed = delegate { };
    public static event System.Action<int> OnRestartPressed = delegate { };

    [SerializeField]
    private GameObject gameMenu = null;
    [SerializeField]
    private GameObject pauseMenu = null;
    [SerializeField]
    private GameObject settingsMenu = null;
    [SerializeField]
    private UnityEngine.UI.Button restartButton = null;

    [Header("Sounds")]
    [SerializeField]
    private List<AudioClip> buttonSounds = null;

    public GameObject PauseMenu => pauseMenu;

    protected override void Start() {
        base.Start();

        DontDestroyOnLoad(gameObject);
        Player.OnPlayerDied += OnPlayerDied;
        PlayerAnimator.OnPlayerRespawned += OnPlayerRespawned;
#if UNITY_ANDROID
        AndroidBackButton.AndroidBackButtonPressed += OnAndroidBackButtonPressed;
#endif
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        Player.OnPlayerDied -= OnPlayerDied;
        PlayerAnimator.OnPlayerRespawned -= OnPlayerRespawned;
#if UNITY_ANDROID
        AndroidBackButton.AndroidBackButtonPressed -= OnAndroidBackButtonPressed;
#endif
    }

    private void OnPlayerDied(string diedAtLevel) {
        if (restartButton == null) return;

        restartButton.interactable = false;
    }

    private void OnPlayerRespawned() {
        if (restartButton == null) return;

        restartButton.interactable = true;
    }

#if UNITY_ANDROID
    private void OnAndroidBackButtonPressed() {
        if (pauseMenu == null || gameMenu == null || settingsMenu == null) return;

        if (pauseMenu.activeInHierarchy) {
            pauseMenu.SetActive(false);
            gameMenu.SetActive(true);
            PlayRandomButtonSound();
            OnPauseMenuClosed();
        } else {
            pauseMenu.SetActive(true);
            gameMenu.SetActive(false);
            PlayRandomButtonSound();
            OnPauseMenuOpened();
        }

        if (settingsMenu.activeInHierarchy) {
            settingsMenu.SetActive(false);
            pauseMenu.SetActive(true);
            PlayRandomButtonSound();
        }
    }
#endif

    /// <summary> Pause button of the game UI menu </summary>
    public void PauseButton() {
        // Toggle UI menus
        pauseMenu.SetActive(true);
        gameMenu.SetActive(false);
        OnPauseMenuOpened();
    }

    /// <summary> Continue button of the game UI menu </summary>
    public void ContinueButton() {
        // Toggle UI menus
        pauseMenu.SetActive(false);
        gameMenu.SetActive(true);
        OnPauseMenuClosed();
    }

    /// <summary> Main Menu button of the UI menu </summary>
    public void MainMenuButton() {
        FadeCamera.OnFadeCompleted += OnFadeCompleted;
        LevelManager.Instance.FadeCamera.FadeOut();
    }

    private void OnFadeCompleted(bool isFadeIn) {
        if (isFadeIn) return;

        FadeCamera.OnFadeCompleted -= OnFadeCompleted;
        LevelManager.Instance.LoadMainMenu();
        Destroy(gameObject);
    }

    /// <summary> Reset button of the UI menu </summary>
    public void ResetButton() {
        // Change UI menus
        pauseMenu.SetActive(false);
        gameMenu.SetActive(true);

        OnRestartPressed(LevelManager.Instance.GetCurrentMoveCount());
    }

    /// <summary> Button to go to settings from main menu </summary>
    public void SettingsButton() {
        pauseMenu.SetActive(false);
        gameMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    /// <summary> Button to go back to main menu from settings </summary>
    public void SettingsBackButton() {
        pauseMenu.SetActive(true);
        gameMenu.SetActive(false);
        settingsMenu.SetActive(false);
    }

    /// <summary> Undo button of the UI menu </summary>
    public void UndoButton() {
        OnUndoPressed();
    }

    /// <summary> Play a random button click sound </summary>
    public void PlayRandomButtonSound(bool playOnce = true) {
        if (buttonSounds == null) return;

        PlayRandomSound(buttonSounds, playOnce);
    }

    public void ToggleSFX() {
        AudioManager.Instance.ToggleSFX();
    }

    public void ToggleMusic() {
        AudioManager.Instance.ToggleMusic();
    }
}
