using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager> {
    public static event Action OnChapterTransition = delegate { };
    public static event Action OnMainMenuLoading = delegate { };
    public static event Action OnCurrentGameSceneChanged = delegate { };
    public static event Action OnCorruptedLoadDetected = delegate { };

    [Header("UI")]
    [SerializeField]
    private GameMenus inGameUI = null;
    public GameMenus InGameUI => inGameUI;
    private GameMenus loadedInGameUI;

    [Header("Main Menu")]
    [Scene]
    [SerializeField]
    private string mainMenu = null;

    [Header("Player Prefs")]
    [SerializeField]
    private string lastChapterLocation = "LastChapter";
    [SerializeField]
    private string lastLevelLocation = "LastLevel";
    [SerializeField]
    private string levelProgressionFormat = "LevelProgession{0}";
    [SerializeField]
    private string levelReplayCounterFormat = "LevelReplayCount{0}";
    [SerializeField]
    private string levelMinimalStepsRequiredFormat = "LevelMinimalStep{0}";

    [Header("Chapter transition")]
    [SerializeField]
    private FadeCamera fadeCamera = null;
    private FadeCamera fadeCameraInstance;
    public FadeCamera FadeCamera => fadeCameraInstance ?? (fadeCameraInstance = Instantiate(fadeCamera, transform));
    private PreLoader preLoader = null;
    public PreLoader PreLoader => preLoader ?? (preLoader = gameObject.AddComponent<PreLoader>());

    [SerializeField]
    private List<Chapter> chapters = new List<Chapter>();
    public List<Chapter> Chapters => chapters;
    [SerializeField]
    [Scene]
    private string endScene = null;
    [SerializeField]
    [Scene]
    private string bonusEndScene = null;

    [Header("Bonus Chapters")]
    [SerializeField]
    [Scene]
    private List<string> bonusChapters = null;
    [SerializeField]
    [Scene]
    private string bonusChapterArt = null;

    public bool IsLoadingScene { get; private set; }
    public bool IsLoadingChapterArt { get; private set; }
    private float timeSinceLevelStarted;
    public float TimeSinceLevelStarted => timeSinceLevelStarted;
    private int sessionPlayedLevelCount;
    public int SessionPlayedLevelCount => sessionPlayedLevelCount;
    public Scene CurrentGameScene { get; private set; }

    private Vector2Int currentLevel = Vector2Extensions.Minus1Int;
    public Vector2Int CurrentLevel => currentLevel;
    private Vector2Int lastPlayedLevel = Vector2Extensions.Minus1Int;
    public Vector2Int LastPlayedLevel => lastPlayedLevel;
    private Dictionary<string, bool> levelCompletionInfo = new Dictionary<string, bool>();
    private Dictionary<string, int> levelReplayCounter = new Dictionary<string, int>();
    private Dictionary<string, int> levelMinimalStepsRequiredCounter = new Dictionary<string, int>();
    private Dictionary<string, int> levelTotalStepsRequiredCounter = new Dictionary<string, int>();

    public int ChaptersCount { get; private set; }
    public int BonusChaptersCount { get; private set; }

    protected override void Awake() {
        base.Awake();

        ChaptersCount = chapters.Count;
        BonusChaptersCount = bonusChapters.Count;

        LoadProgress();

        sessionPlayedLevelCount = 0;

        Player.OnPlayerWon += OnLevelWon;
        Player.OnPlayerDied += OnLevelLost;
        Player.OnPlayerActivated += OnPlayerActivated;
        // Temporary required to define the current game level name
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        Tile.OnTileStartedMoving += OnTileMoved;
        GameMenus.OnRestartPressed += OnRestartPressed;
    }

    public void SelectCurrentLevel(int buildIndex, bool useFadeEffect = false) {
        if (chapters == null) return;

        SelectCurrentLevel(SceneManager.GetSceneByBuildIndex(buildIndex).name, useFadeEffect);
    }

    public void SelectCurrentLevel(string levelName, bool useFadeEffect = false) {
        if (IsBonusLevel(levelName)) {
            int bonusLevelIndex = -1;

            for (int i = 0; i < BonusChaptersCount; i++) {
                if (bonusChapters[i] != levelName) continue;

                bonusLevelIndex = i;
            }

            if (bonusLevelIndex == -1) return;

            SelectCurrentLevel(-100, bonusLevelIndex, useFadeEffect);
            return;
        }

        for (int i = 0; i < ChaptersCount; i++) {
            for (int y = 0; y < chapters[i].LevelsCount; y++) {
                if (string.Equals(chapters[i].levels[y], levelName, StringComparison.CurrentCultureIgnoreCase)) {
                    SelectCurrentLevel(i, y, useFadeEffect);
                    return;
                }
            }
        }
    }

    public void SelectCurrentLevel(int chapterIndex, int levelIndex, bool useFadeEffect = false) {
        currentLevel.x = chapterIndex;
        currentLevel.y = levelIndex;
        timeSinceLevelStarted = Time.time;

        // We know this is a bonus level, because we are the only one sending -100 as a value for the chapter index
        StartCoroutine(chapterIndex == -100 ? LoadBonusChapterArt() : LoadNextChapterArt(currentLevel));

        if (useFadeEffect) {
            FadeCamera.FadeIn();
        }

        SetLastPlayedLevel();
    }

    public string GetCurrentLevelName() {
        if (currentLevel.x == -100) {
            return bonusChapters[currentLevel.y];
        }

        // Try and select a game scene in case this values hasn't been assigned yet
        if (currentLevel == Vector2Extensions.Minus1) {
            SelectCurrentLevel(SceneManager.GetActiveScene().buildIndex);
        }

        // If we cannot retrieve a game scene, return an empty string
        if (currentLevel == Vector2Extensions.Minus1) {
            return "";
        }

        if (currentLevel.x >= ChaptersCount) {
            return "";
        }

        if (currentLevel.y >= chapters[currentLevel.x].LevelsCount) {
            return "";
        }

        return chapters[currentLevel.x].levels[currentLevel.y];
    }

    public string GetLevelName(int chapterIndex, int levelIndex) {
        if (chapterIndex == -100) {
            return bonusChapters[levelIndex];
        }

        if (chapterIndex > ChaptersCount || chapterIndex < 0) {
            return "";
        }

        if (levelIndex > chapters[chapterIndex].LevelsCount || levelIndex < 0) {
            return "";
        }

        return chapters[chapterIndex].levels[levelIndex];
    }

    public string GetNextLevelName(bool hasCompletedLevel = true, bool incrementCurrentLevelIndex = true) {
        if (IsBonusLevel(GetCurrentLevelName())) {
            return GetNextBonusLevel(GetCurrentLevelName());
        }

        if (currentLevel == Vector2Extensions.Minus1) {
            SelectCurrentLevel(SceneManager.GetActiveScene().buildIndex);
        }

        if (hasCompletedLevel) {
            sessionPlayedLevelCount++;
        }

        if (currentLevel.x >= ChaptersCount) {
            Debug.Log("There are no more levels available");
            return "";
        }

        if (currentLevel.x < 0) {
            Debug.Log("Something went wrong with detecting which level we are currently in.");
            return "";
        }

        if (currentLevel.y >= chapters[currentLevel.x].LevelsCount) {
            Debug.Log("There are no more levels available");
            return "";
        }

        if (incrementCurrentLevelIndex) {
            SaveProgressionForLevel(chapters[currentLevel.x].levels[currentLevel.y], hasCompletedLevel);
        }

        Vector2Int nextLevel = currentLevel;
        string selectedLevel = "";

        while (selectedLevel == "") {
            if (nextLevel.y + 1 >= chapters[currentLevel.x].LevelsCount) {
                nextLevel.x += 1;
                nextLevel.y = 0;
            } else {
                nextLevel.y++;
            }

            if (nextLevel.x >= ChaptersCount) {
                break;
            }

            selectedLevel = chapters[nextLevel.x].levels[nextLevel.y] ?? "";
        }

        if (incrementCurrentLevelIndex) {
            if (nextLevel.x > currentLevel.x) {
                OnChapterTransition();
                StartCoroutine(LoadNextChapterArt(nextLevel));
            }

            currentLevel = nextLevel;
        }

        if (nextLevel.x < ChaptersCount) {
            return chapters[nextLevel.x].levels[nextLevel.y];
        }

        return endScene;
    }

    public bool WillAdvanceChapter() {
        if (currentLevel.x == -100) return false;

        if (currentLevel == Vector2Extensions.Minus1 || currentLevel.x < 0) {
            SelectCurrentLevel(SceneManager.GetActiveScene().buildIndex);
        }

        Vector2Int nextLevel = currentLevel;
        string selectedLevel = "";

        while (selectedLevel == "") {
            if (nextLevel.y + 1 >= chapters[currentLevel.x].LevelsCount) {
                nextLevel.x += 1;
                nextLevel.y = 0;
            } else {
                nextLevel.y++;
            }

            if (nextLevel.x >= ChaptersCount) {
                break;
            }

            selectedLevel = chapters[nextLevel.x].levels[nextLevel.y] ?? "";
        }

        if (nextLevel.x > currentLevel.x) {
            return true;
        }

        return false;
    }

    public List<string> GetChapterLevels(int chapterIndex) {
        if (chapterIndex < ChaptersCount) return chapters[chapterIndex].levels;

        Debug.LogError("The chapter index you assigned is not available");
        return null;
    }

    public List<string> GetBonusLevels() {
        return bonusChapters;
    }

    public bool GetChapterCompletionStatus(int chapterIndex) {
        if (chapterIndex < 0 || chapterIndex > ChaptersCount) return false;

        // The first chapter is always enabled
        if (chapterIndex == 0) return true;

        // If the last level from the previous chapter has been completed, return true. Else return false
        return GetLevelCompletionStatus(chapters[chapterIndex - 1].levels[chapters[chapterIndex - 1].LevelsCount - 1]);
    }

    public bool GetLevelCompletionStatus(string levelToCheck) {
        if (levelCompletionInfo.ContainsKey(levelToCheck) == false) return false;

        return levelCompletionInfo[levelToCheck];
    }

    public bool IsBonusLevel(string levelToCheck) {
        if (bonusChapters == null) return false;

        return bonusChapters.Contains(levelToCheck);
    }

    public bool IsRegularLevel(string levelToCheck) {
        if (string.IsNullOrEmpty(levelToCheck)) {
            Debug.LogWarning("You tried to check an empty string, ignore this is if it was on purpose.");
            return false;
        }

        for (int i = 0; i < ChaptersCount; i++) {
            for (int j = 0; j < Chapters[i].LevelsCount; j++) {
                if (string.Equals(Chapters[i].levels[j].ToLower(), levelToCheck.ToLower())) return true;
            }
        }

        return false;
    }

    public bool IsValidLevel(string levelToCheck) {
        if (IsBonusLevel(levelToCheck)) return true;
        if (IsRegularLevel(levelToCheck)) return true;

        return false;
    }

    public bool IsEndScene(string levelToCheck) {
        if (string.IsNullOrEmpty(levelToCheck)) return false;
        if (string.Equals(levelToCheck.ToLower(), mainMenu.ToLower())) return true;
        if (string.Equals(levelToCheck.ToLower(), endScene.ToLower())) return true;
        if (string.Equals(levelToCheck.ToLower(), bonusEndScene.ToLower())) return true;

        return false;
    }

    public int GetCurrentMoveCount() {
        string currentLevelName = GetCurrentLevelName();

        if (string.IsNullOrEmpty(currentLevelName)) return 0;
        if (levelTotalStepsRequiredCounter.ContainsKey(currentLevelName) == false) return 0;

        return levelTotalStepsRequiredCounter[currentLevelName];
    }

    public int GetCurrentChapterLevelCount() {
        // If we are a bonus level
        if (currentLevel.x == -100) {
            if (bonusChapters == null) return 0;

            return BonusChaptersCount;
        }

        if (chapters == null) return 0;
        if (currentLevel.x >= ChaptersCount || currentLevel.x < 0) return 0;
        if (chapters[currentLevel.x].levels == null) return 0;

        return chapters[currentLevel.x].LevelsCount;
    }

    public void LoadMainMenu() {
        SaveProgress();
        IsLoadingScene = true;
        SceneManager.LoadScene(mainMenu);

        OnMainMenuLoading();
    }

    public void LoadEndScene(string endSceneToUse, bool useFadeEffect) {
        if (string.Equals(endSceneToUse.ToLower(), mainMenu.ToLower())) {
            LoadMainMenu();
            return;
        }

        if (useFadeEffect) {
            SaveProgress();
            StartCoroutine(LoadGameLevelWithFade(endSceneToUse, false, true, true, true));

            return;
        }

        SaveProgress();
        IsLoadingScene = true;
        SceneManager.LoadScene(endSceneToUse);
    }

    public void LoadGameLevel(string levelToLoad, bool setAsCurrentLevel, bool setAsActiveLevel, bool useFadeEffect = false, bool unloadOtherScenes = false) {
        if (IsLoadingScene) return;

        IsLoadingScene = true;
        StartCoroutine(LoadGameLevelWithFade(levelToLoad, setAsCurrentLevel, setAsActiveLevel, useFadeEffect, unloadOtherScenes));
    }

    public void SetCurrentGameScene(Scene currentGameScene) {
        if (currentGameScene.buildIndex == CurrentGameScene.buildIndex) return;

        CurrentGameScene = currentGameScene;
        OnCurrentGameSceneChanged();
    }

    public void TriggerCorruptedLoadDetected() {
        if (Application.isFocused == false) return;

        OnCorruptedLoadDetected();
        StartCoroutine(PreLoader.PreLoadNextLevel());

        GameObject[] rootObjects = CurrentGameScene.GetRootGameObjects();

        for (int i = 0, length = rootObjects.Length; i < length; i++) {
            rootObjects[i].SetActive(true);
        }
    }

    private IEnumerator LoadGameLevelWithFade(string levelToLoad, bool setAsCurrentLevel, bool setAsActiveLevel, bool useFadeEffect = false, bool unloadOtherScenes = false) {
        if (useFadeEffect) {
            FadeCamera.FadeOut();

            while (FadeCamera.CurrentFadeLevel < 1f) {
                yield return null;
            }
        }

        SceneManager.LoadScene(levelToLoad, LoadSceneMode.Additive);

        if (setAsCurrentLevel) {
            SelectCurrentLevel(levelToLoad);
        }

        Scene sceneToLoad = SceneManager.GetSceneByName(levelToLoad);

        if (setAsCurrentLevel) {
            CurrentGameScene = sceneToLoad;
        }

        while (sceneToLoad.isLoaded == false) {
            yield return null;
        }

        if (setAsActiveLevel) {
            SceneManager.SetActiveScene(sceneToLoad);
        }

        if (unloadOtherScenes) {
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                Scene sceneToUnload = SceneManager.GetSceneAt(i);

                if (sceneToUnload.buildIndex == sceneToLoad.buildIndex) continue;

                SceneManager.UnloadSceneAsync(sceneToUnload);
            }
        }

        yield return null;

        if (useFadeEffect) {
            FadeCamera.FadeIn();

            while (FadeCamera.CurrentFadeLevel > 0f) {
                yield return null;
            }
        }

        IsLoadingScene = false;
    }

    private void SaveProgress() {
        SetLastPlayedLevel();

        if (chapters != null) {
            for (int i = 0; i < ChaptersCount; i++) {
                for (int j = 0; j < chapters[i].LevelsCount; j++) {
                    if (levelCompletionInfo.ContainsKey(chapters[i].levels[j])) {
                        PlayerPrefs.SetInt(string.Format(levelProgressionFormat, chapters[i].levels[j]),
                            levelCompletionInfo[chapters[i].levels[j]] ? 1 : 0);
                    }

                    if (levelReplayCounter.ContainsKey(chapters[i].levels[j])) {
                        PlayerPrefs.SetInt(string.Format(levelReplayCounterFormat, chapters[i].levels[j]),
                            levelReplayCounter[chapters[i].levels[j]]);
                    }

                    if (levelMinimalStepsRequiredCounter.ContainsKey(chapters[i].levels[j])) {
                        PlayerPrefs.SetInt(string.Format(levelMinimalStepsRequiredFormat, chapters[i].levels[j]),
                            levelMinimalStepsRequiredCounter[chapters[i].levels[j]]);
                    }
                }
            }
        }

        if (bonusChapters == null) return;

        for (int i = 0; i < BonusChaptersCount; i++) {
            if (levelCompletionInfo.ContainsKey(bonusChapters[i])) {
                PlayerPrefs.SetInt(string.Format(levelProgressionFormat, bonusChapters[i]),
                    levelCompletionInfo[bonusChapters[i]] ? 1 : 0);
            }

            if (levelReplayCounter.ContainsKey(bonusChapters[i])) {
                PlayerPrefs.SetInt(string.Format(levelReplayCounterFormat, bonusChapters[i]),
                    levelReplayCounter[bonusChapters[i]]);
            }

            if (levelMinimalStepsRequiredCounter.ContainsKey(bonusChapters[i])) {
                PlayerPrefs.SetInt(string.Format(levelMinimalStepsRequiredFormat, bonusChapters[i]),
                    levelMinimalStepsRequiredCounter[bonusChapters[i]]);
            }
        }
    }

    private void SetLastPlayedLevel() {
        // Store the last played level, but only save if the player actually played a level
        if (currentLevel != Vector2Extensions.Minus1) {
            PlayerPrefs.SetInt(lastChapterLocation, currentLevel.x);
            PlayerPrefs.SetInt(lastLevelLocation, currentLevel.y);

            lastPlayedLevel = CurrentLevel;
        }
    }

    public void SaveProgressionForLevel(string levelName, bool hasCompleted) {
        if (levelCompletionInfo.ContainsKey(levelName) == false) {
            levelCompletionInfo.Add(levelName, hasCompleted);
        } else {
            levelCompletionInfo[levelName] = hasCompleted;
        }
    }

    private void LoadProgress() {
        // Check if we started from a level scene
        SelectCurrentLevel(SceneManager.GetActiveScene().buildIndex);

        // Retrieve the last played level, but only if we are not starting from a level scene
        if (lastPlayedLevel == Vector2Extensions.Minus1) {
            if (PlayerPrefs.HasKey(lastChapterLocation) && PlayerPrefs.HasKey(lastLevelLocation)) {
                lastPlayedLevel.x = PlayerPrefs.GetInt(lastChapterLocation);
                lastPlayedLevel.y = PlayerPrefs.GetInt(lastLevelLocation);
            }
        }

        if (chapters != null) {
            for (int i = 0; i < ChaptersCount; i++) {
                for (int j = 0; j < chapters[i].LevelsCount; j++) {
                    // Only load a player pref setting if it has actually been stored
                    if (PlayerPrefs.HasKey(string.Format(levelProgressionFormat, chapters[i].levels[j]))) {
                        if (levelCompletionInfo.ContainsKey(chapters[i].levels[j])) {
                            levelCompletionInfo[chapters[i].levels[j]] =
                                PlayerPrefs.GetInt(string.Format(levelProgressionFormat, chapters[i].levels[j])) == 1;
                        }
                        else {
                            levelCompletionInfo.Add(chapters[i].levels[j],
                                PlayerPrefs.GetInt(string.Format(levelProgressionFormat, chapters[i].levels[j])) == 1);
                        }
                    }

                    if (PlayerPrefs.HasKey(string.Format(levelReplayCounterFormat, chapters[i].levels[j]))) {
                        if (levelReplayCounter.ContainsKey(chapters[i].levels[j])) {
                            levelReplayCounter[chapters[i].levels[j]] =
                                PlayerPrefs.GetInt(string.Format(levelReplayCounterFormat, chapters[i].levels[j]));
                        }
                        else {
                            levelReplayCounter.Add(chapters[i].levels[j],
                                PlayerPrefs.GetInt(string.Format(levelReplayCounterFormat, chapters[i].levels[j])));
                        }
                    }

                    if (!PlayerPrefs.HasKey(string.Format(levelMinimalStepsRequiredFormat, chapters[i].levels[j]))) continue;

                    if (levelMinimalStepsRequiredCounter.ContainsKey(chapters[i].levels[j])) {
                        levelMinimalStepsRequiredCounter[chapters[i].levels[j]] =
                            PlayerPrefs.GetInt(string.Format(levelMinimalStepsRequiredFormat, chapters[i].levels[j]));
                    }
                    else {
                        levelMinimalStepsRequiredCounter.Add(chapters[i].levels[j],
                            PlayerPrefs.GetInt(string.Format(levelMinimalStepsRequiredFormat, chapters[i].levels[j])));
                    }
                }
            }
        }

        if (bonusChapters == null) return;

        for (int i = 0; i < BonusChaptersCount; i++) {
            // Only load a player pref setting if it has actually been stored
            if (PlayerPrefs.HasKey(string.Format(levelProgressionFormat, bonusChapters[i]))) {
                if (levelCompletionInfo.ContainsKey(bonusChapters[i])) {
                    levelCompletionInfo[bonusChapters[i]] =
                        PlayerPrefs.GetInt(string.Format(levelProgressionFormat, bonusChapters[i])) == 1;
                }
                else {
                    levelCompletionInfo.Add(bonusChapters[i],
                        PlayerPrefs.GetInt(string.Format(levelProgressionFormat, bonusChapters[i])) == 1);
                }
            }

            if (PlayerPrefs.HasKey(string.Format(levelReplayCounterFormat, bonusChapters[i]))) {
                if (levelReplayCounter.ContainsKey(bonusChapters[i])) {
                    levelReplayCounter[bonusChapters[i]] =
                        PlayerPrefs.GetInt(string.Format(levelReplayCounterFormat, bonusChapters[i]));
                }
                else {
                    levelReplayCounter.Add(bonusChapters[i],
                        PlayerPrefs.GetInt(string.Format(levelReplayCounterFormat, bonusChapters[i])));
                }
            }

            if (PlayerPrefs.HasKey(string.Format(levelMinimalStepsRequiredFormat, bonusChapters[i]))) {
                if (levelMinimalStepsRequiredCounter.ContainsKey(bonusChapters[i])) {
                    levelMinimalStepsRequiredCounter[bonusChapters[i]] =
                        PlayerPrefs.GetInt(string.Format(levelMinimalStepsRequiredFormat, bonusChapters[i]));
                }
                else {
                    levelMinimalStepsRequiredCounter.Add(bonusChapters[i],
                        PlayerPrefs.GetInt(string.Format(levelMinimalStepsRequiredFormat, bonusChapters[i])));
                }
            }
        }
    }

    private IEnumerator LoadNextChapterArt(Vector2Int nextChapter) {
        // We're already loading, why load again?
        if (IsLoadingChapterArt) yield break;

        if (nextChapter.x >= ChaptersCount) {
            yield break;
        }
        if (string.IsNullOrEmpty(chapters[nextChapter.x].chapterArt)) {
            yield break;
        }

        Scene sceneToLoad = SceneManager.GetSceneByName(chapters[nextChapter.x].chapterArt);

        if (sceneToLoad.isLoaded) {
            yield break;
        }

        IsLoadingChapterArt = true;
        AsyncOperation async = SceneManager.LoadSceneAsync(chapters[nextChapter.x].chapterArt, LoadSceneMode.Additive);

        yield return async;

        IsLoadingChapterArt = false;
    }

    private IEnumerator LoadBonusChapterArt() {
        if (IsLoadingChapterArt) yield break;
        if (string.IsNullOrEmpty(bonusChapterArt)) yield break;

        if (SceneManager.GetSceneByName(bonusChapterArt).isLoaded) {
            yield break;
        }

        IsLoadingChapterArt = true;
        AsyncOperation async = SceneManager.LoadSceneAsync(bonusChapterArt, LoadSceneMode.Additive);

        yield return async;

        IsLoadingChapterArt = false;
    }

    private void OnLevelWon(string levelName) {
        SaveProgress();
        StartCoroutine(PreLoader.ActivateNextLevel());

        // In case we force a win in editor with 0 moves required
        if (levelTotalStepsRequiredCounter.ContainsKey(levelName) == false) return;

        if (levelMinimalStepsRequiredCounter.ContainsKey(levelName)) {
            if (levelMinimalStepsRequiredCounter[levelName] > levelTotalStepsRequiredCounter[levelName]) {
                levelMinimalStepsRequiredCounter[levelName] = levelTotalStepsRequiredCounter[levelName];
            }
        } else {
            levelMinimalStepsRequiredCounter.Add(levelName, levelTotalStepsRequiredCounter[levelName]);
        }

        //AnalyticsManager.Instance.RecordCustomEvent("OnLevelWon", new Dictionary<string, object> {
        //    { "Level " + levelName + " SolvedInXSeconds", Time.time - TimeSinceLevelStarted },
        //    { "Level " + levelName + " MinimalStepsRequired", levelMinimalStepsRequiredCounter[levelName] },
        //    { "Level " + levelName + " TotalStepsRequired", levelTotalStepsRequiredCounter[levelName] },
        //    { "Level " + levelName + " ReplayCount", levelReplayCounter.ContainsKey(levelName) ? levelReplayCounter[levelName] : 0 }
        //});
    }

    private void OnLevelLost(string levelName) {
        if (levelReplayCounter.ContainsKey(levelName)) {
            levelReplayCounter[levelName]++;
        } else {
            levelReplayCounter.Add(levelName, 1);
        }

        //AnalyticsManager.Instance.RecordCustomEvent("OnLevelLost", new Dictionary<string, object> {
        //    { "Level " + levelName + " ReplayCount", levelReplayCounter[levelName] }
        //});
    }

    private void OnPlayerActivated() {
        StartCoroutine(PreLoader.PreLoadNextLevel());
    }

    private void OnTileMoved(Vector2Int startingIndex, Vector2Int movementDirection) {
        if (levelTotalStepsRequiredCounter.ContainsKey(GetCurrentLevelName())) {
            levelTotalStepsRequiredCounter[GetCurrentLevelName()]++;
        } else {
            levelTotalStepsRequiredCounter.Add(GetCurrentLevelName(), 1);
        }
    }

    private void OnRestartPressed(int currentMoveCount) {
        if (currentMoveCount == 0) return;

        string currentLevelName = GetCurrentLevelName();
        if (!levelTotalStepsRequiredCounter.ContainsKey(currentLevelName)) return;
        if (levelTotalStepsRequiredCounter[currentLevelName] <= 0) return;

        levelTotalStepsRequiredCounter[currentLevelName] = 0;
    }

    private string GetNextBonusLevel(string currentBonusLevel) {
        int currentIndex = -1;

        for (int i = 0; i < BonusChaptersCount; i++) {
            if (bonusChapters[i] != currentBonusLevel) continue;

            currentIndex = i;
            break;
        }

        return currentIndex == -1
            ? bonusEndScene
            : currentIndex + 1 >= BonusChaptersCount
                ? bonusEndScene
                : bonusChapters[currentIndex + 1];
    }

    // Required to define the current active scene for a reload, and required for the on start event
    private void OnSceneLoaded(Scene loadedScene, LoadSceneMode sceneLoadMode) {
        string currentLevelName = GetCurrentLevelName();

        if (currentLevelName != "") {
            if (loadedScene.name == currentLevelName) {
                // Load in the in game UI if needed so we only need to load it once
                if (loadedInGameUI == null && inGameUI != null) {
                    loadedInGameUI = Instantiate(inGameUI);
                }

                if (SceneManager.GetActiveScene().buildIndex != loadedScene.buildIndex) {
                    SceneManager.SetActiveScene(loadedScene);
                }

                // Reassign the starting time of this level
                timeSinceLevelStarted = Time.time;

                // Reset the total steps required for a level
                if (levelTotalStepsRequiredCounter.ContainsKey(currentLevelName)) {
                    levelTotalStepsRequiredCounter[currentLevelName] = 0;
                }

                // Keep track of the amount of times a level has been played
                if (levelReplayCounter.ContainsKey(currentLevelName)) {
                    levelReplayCounter[currentLevelName]++;
                } else {
                    levelReplayCounter.Add(currentLevelName, 1);
                }

                //AnalyticsManager.Instance.RecordCustomEvent("OnLevelStarted", new Dictionary<string, object> {
                //    {"Level " + GetCurrentLevelName() + " ReplayCount", levelReplayCounter[currentLevelName]}
                //});
            } else if (SceneManager.GetActiveScene().name == currentLevelName && Application.isEditor) {
                // Load in the in game UI if needed so we only need to load it once
                if (loadedInGameUI == null && inGameUI != null) {
                    loadedInGameUI = Instantiate(inGameUI);
                }
            }
        }
    }

    private void OnActiveSceneChanged(Scene currentScene, Scene nextScene) {
        IsLoadingScene = true;
        StartCoroutine(WaitSceneToLoad(nextScene, nextScene.name == mainMenu));

        if (nextScene.name == endScene && SceneManager.GetActiveScene().buildIndex == nextScene.buildIndex) {
            if (loadedInGameUI == null) return;

            Destroy(loadedInGameUI.gameObject);
        } else if (nextScene.name == bonusEndScene && SceneManager.GetActiveScene().buildIndex == nextScene.buildIndex) {
            if (loadedInGameUI == null) return;

            Destroy(loadedInGameUI.gameObject);
        }
    }

    private IEnumerator WaitSceneToLoad(Scene sceneToWaitFor, bool fadeIn) {
        while (sceneToWaitFor.isLoaded == false) {
            yield return null;
        }

        IsLoadingScene = false;

        if (fadeIn && fadeCameraInstance != null)
            FadeCamera.FadeIn();
    }

    // Save/load playerprefs for whenever we lose focus to the application
    private void OnApplicationFocus(bool hasFocus) {
        if (hasFocus)
            LoadProgress();
        else
            SaveProgress();
    }

    // Save/load playerprefs for whenever we pause the application
    private void OnApplicationPause(bool pauseStatus) {
        if (pauseStatus)
            SaveProgress();
        else
            LoadProgress();
    }

    // Save/load playerprefs for whenever we quit the application
    private void OnApplicationQuit() {
        SaveProgress();
    }

    // Safety measure, to make sure we save all player prefs on destroy
    private void OnDestroy() {
        SaveProgress();
        Player.OnPlayerWon -= OnLevelWon;
        Player.OnPlayerDied -= OnLevelLost;
        Player.OnPlayerActivated -= OnPlayerActivated;
        // Temporary required to define the current game level name
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        Tile.OnTileStartedMoving -= OnTileMoved;
        GameMenus.OnRestartPressed -= OnRestartPressed;
    }
}

[Serializable]
public struct Chapter {
    public string chapterName;
    [Scene]
    public string chapterArt;
    [Scene]
    public List<string> levels;
    public TileData chapterTileData;

    private int? levelsCount;

    public int LevelsCount {
        get {
            if (!levelsCount.HasValue)
                levelsCount = levels.Count;

            return levelsCount.Value;
        }
    }
}
