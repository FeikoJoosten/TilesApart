using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreLoader : MonoBehaviour {
    //public static event Action OnLevelTransition = delegate { };
    public static event Action OnEndSceneLoaded = delegate { };
    public static event Action OnNextCameraActivated = delegate { };

    public AsyncOperation async;

    public bool isTransitioning = false;
    private Scene nextScene;
    private WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();

    private Vector3 newStartPosition = new Vector3(0, 0, 0);
    Quaternion newStartRotation = new Quaternion(0, 0, 0, 0);
    private bool chapterTransition = false;
    public float waitAfterMoveCoroutine = 0.3f;

    private readonly AnimationCurve cameraAnimationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    private int cameraAnimationCurveKeyCount;
    private int currentSceneRootObjectCount;
    private int nextSceneRootObjectCount;
    private GameObject[] nextSceneRootGameObjects;

    private void Awake() {
        cameraAnimationCurveKeyCount = cameraAnimationCurve.length;

        LevelManager.OnMainMenuLoading += OnBackToMainMenu;
    }

    private void OnDestroy() {
        LevelManager.OnMainMenuLoading -= OnBackToMainMenu;
    }

    private void OnBackToMainMenu() {
        StopAllCoroutines();
        Destroy(this);
    }

    public IEnumerator PreLoadNextLevel() {
        // We're already loading no need to load again
        if (async != null) {
            yield break;
        }

        string nextSceneName = LevelManager.Instance.GetNextLevelName(false, false);

        if (string.IsNullOrEmpty(nextSceneName)) {
            Debug.Log("The next level doesn't exsist, no need to preload it");
            yield break;
        }

        if (LevelManager.Instance.IsValidLevel(nextSceneName) == false && LevelManager.Instance.IsEndScene(nextSceneName) == false) {
            Debug.Log("Stopping the preloader, since the next level is not a valid level");
            yield break;
        }

        chapterTransition = LevelManager.Instance.WillAdvanceChapter();

        if (SceneManager.GetSceneByName(nextSceneName).isLoaded) {
            Debug.Log("Next scene is already loaded: " + nextSceneName);
            SaveNextLevelInformationAndDisableObject();
            yield break;
        }

        async = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);

        yield return async;

        async = null;
        nextScene = SceneManager.GetSceneByName(nextSceneName);
        nextSceneRootGameObjects = nextScene.GetRootGameObjects();
        nextSceneRootObjectCount = nextSceneRootGameObjects.Length;

        SaveNextLevelInformationAndDisableObject();
    }

    private void SaveNextLevelInformationAndDisableObject() {
        if (string.IsNullOrEmpty(nextScene.name)) return;

        // Disabling GameObjects of next scene, to fit this in the same frame (Async does not allow this)
        for (int index = 0; index < nextSceneRootObjectCount; index++) {
            GameObject rootObject = nextSceneRootGameObjects[index];
            GridManager nextManager = rootObject.GetComponent<GridManager>();

            if (nextManager != null) {
                if (nextManager) {
                    nextManager.SinkLevel(true);
                }

                newStartPosition = nextManager.startTile.transform.position;
                newStartRotation = nextManager.startTile.transform.rotation;
            }

            rootObject.SetActive(false);
        }
    }

    public IEnumerator ActivateNextLevel() {
        if (isTransitioning) {
            yield break;
        }

        // Loading an end level, so stopping early.
        if (LevelManager.Instance.IsEndScene(nextScene.name)) {
            // We do need to inform the level manager we completed our level, but not increment the level index.
            LevelManager.Instance.SaveProgressionForLevel(SceneManager.GetActiveScene().name, true);

            yield return ActivateEndScene();

            OnEndSceneLoaded();
            yield break;
        }

        if (LevelManager.Instance.IsValidLevel(nextScene.name) == false) {
            Debug.Log("Stopping the preloader, since the next level is not a valid level");
            yield break;
        }

        isTransitioning = true;

        if (chapterTransition) {
            LevelManager.Instance.FadeCamera.FadeOut();
        }

        GridManager gridManager = null;
        Camera currentCamera = null;

        Scene currentScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = currentScene.GetRootGameObjects();

        for (int i = 0, length = rootObjects.Length; i < length; i++) {
            if (currentCamera != null && gridManager != null) break;

            if (currentCamera == null) {
                currentCamera = rootObjects[i].GetComponent<Camera>();
            }

            if (gridManager != null) continue;

            GridManager foundManager = rootObjects[i].GetComponent<GridManager>();

            if (foundManager == null) continue;

            gridManager = foundManager;
        }

        if (gridManager == null) {
            Debug.LogError("No Grid manager found!");
            yield break;
        }

        float endTime = Time.time + gridManager.GridData.TileHorizontalMovement[gridManager.GridData.TileHorizontalMovementKeyCount - 1].time;

        while (Time.time < endTime) {
            yield return null;
        }

        string nextLevel = LevelManager.Instance.GetNextLevelName(true, true);

        // Not adding this line causes the level manager to NOT update the level progression as it's compiled away...
        // This only happens in case we lose focus while transitioning
        if (nextLevel == "") {
            Debug.Log("Something went wrong while trying to get the next level");
        }

        gridManager.PlayerObject.transform.SetParent(gridManager.endTile.transform);

        GridManager nextGridManager = null;
        Camera nextCamera = null;

        for (int i = 0; i < nextSceneRootObjectCount; i++) {
            if(nextCamera != null && nextGridManager != null) break;

            if(nextCamera == null) {
                nextCamera = nextSceneRootGameObjects[i].GetComponent<Camera>();
            }

            if (nextGridManager != null) continue;

            GridManager manager = nextSceneRootGameObjects[i].GetComponent<GridManager>();
            if (manager == null) continue;

            nextGridManager = manager;
        }

        if (chapterTransition == false)
            StartCoroutine(MoveCamera(currentCamera, nextCamera.orthographicSize, nextCamera.transform.position));

        StartCoroutine(RotatePlayer(newStartRotation, gridManager));
        yield return gridManager.endTile.StartCoroutine(gridManager.endTile.MoveFreeToIndex(newStartPosition, newStartRotation));

        SceneManager.SetActiveScene(nextScene);

        endTime = Time.time + waitAfterMoveCoroutine;

        while (Time.time < endTime) {
            yield return null;
        }

        for(int i = 0; i < nextSceneRootObjectCount; i++) {
            nextSceneRootGameObjects[i].SetActive(true);
        }

        OnNextCameraActivated();

        if (nextGridManager != null) {
            nextGridManager.startTile.TileAnimator.ForceUp();
            nextGridManager.AlignPlayerRotation();
        }

        if (chapterTransition)
            LevelManager.Instance.FadeCamera.FadeIn();

        yield return StartCoroutine(EnableRootObjects(nextSceneRootGameObjects));

        LevelManager.Instance.SetCurrentGameScene(nextScene);

        SceneManager.UnloadSceneAsync(gridManager.gameObject.scene);

        isTransitioning = false;
        Destroy(this);
    }

    private IEnumerator ActivateEndScene() {
        isTransitioning = true;
        LevelManager.Instance.FadeCamera.FadeOut();

        SceneManager.SetActiveScene(nextScene);

        LevelManager.Instance.FadeCamera.FadeIn();

        float endTime = Time.time + waitAfterMoveCoroutine;

        while(Time.time < endTime) {
            yield return null;
        }

        yield return EnableRootObjects(nextScene.GetRootGameObjects());
        yield return SceneManager.UnloadSceneAsync(LevelManager.Instance.CurrentGameScene);

        isTransitioning = false;
        Destroy(this);
    }

    private IEnumerator EnableRootObjects(IEnumerable<GameObject> rootObjects) {
        yield return endOfFrame;

        foreach (GameObject rootObject in rootObjects) {
            rootObject.SetActive(true);
        }

        yield return endOfFrame;
    }

    private IEnumerator MoveCamera(Camera cameraToMove, float newSize, Vector3 newPosition) {
        float startTime = Time.time;
        float endTime = Time.time + cameraAnimationCurve[cameraAnimationCurveKeyCount - 1].time;

        float startSize = cameraToMove.orthographicSize;
        Vector3 startPosition = cameraToMove.transform.position;

        while (Time.time < endTime) {
            cameraToMove.orthographicSize = Mathf.LerpUnclamped(startSize, newSize, cameraAnimationCurve.Evaluate(Time.time - startTime));

            cameraToMove.transform.position = Vector3.LerpUnclamped(startPosition, newPosition, cameraAnimationCurve.Evaluate(Time.time - startTime));

            yield return null;
        }

        cameraToMove.orthographicSize = newSize;
        cameraToMove.transform.position = newPosition;
    }

    private IEnumerator RotatePlayer(Quaternion newRotation, GridManager gridManager) {
        float startTime = Time.time;
        float endTime = Time.time + gridManager.GridData.TileFreeMovement[gridManager.GridData.TileFreeMovementKeyCount - 1].time;

        Quaternion startRotation = gridManager.PlayerObject.transform.rotation;

        while (Time.time < endTime) {
            if (gridManager.PlayerObject == null)
                yield break;

            gridManager.PlayerObject.transform.rotation = Quaternion.LerpUnclamped(startRotation, newRotation, gridManager.GridData.TileFreeMovement.Evaluate(Time.time - startTime));

            yield return null;
        }

        gridManager.PlayerObject.transform.rotation = newRotation;
    }

    //Super sketch, but it allows us to load the next level incase of a corrupt level transition
    private void Update() {
        if(async != null || isTransitioning) return;
        string nextLevelName = LevelManager.Instance.GetNextLevelName(false, false);

        if(string.IsNullOrEmpty(nextLevelName)) return;
        if(LevelManager.Instance.IsEndScene(nextLevelName)) return;

        if(nextLevelName == SceneManager.GetActiveScene().name) {
            nextLevelName = LevelManager.Instance.GetNextLevelName();

            if(string.IsNullOrEmpty(nextLevelName)) return;
            nextLevelName = LevelManager.Instance.GetNextLevelName(false, false);
        }

        Scene nextLevel = SceneManager.GetSceneByName(nextLevelName);

        if(nextLevel.isLoaded == false) {
            StartCoroutine(PreLoadNextLevel());
        }
    }
}
