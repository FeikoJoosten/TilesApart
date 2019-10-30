using UnityEngine;
using UnityEngine.SceneManagement;

public class ChapterArtCameraUpdater : MonoBehaviour {
    private Canvas renderCanvas = null;

    private void Awake() {
        renderCanvas = GetComponent<Canvas>();

        if (renderCanvas == null) {
            Destroy(this);
            return;
        }

        PreLoader.OnNextCameraActived += UpdateCanvasCamera;
        //LevelManager.OnCurrentGameSceneChanged += UpdateCanvasCamera;
        //LevelManager.OnCorruptedLoadDetected += UpdateCanvasCamera;

        UpdateCanvasCamera();
    }

    private void OnDestroy() {
        //LevelManager.OnCurrentGameSceneChanged -= UpdateCanvasCamera;
        //LevelManager.OnCorruptedLoadDetected -= UpdateCanvasCamera;
        PreLoader.OnNextCameraActived -= UpdateCanvasCamera;
    }

    private void OnApplicationFocus(bool hasFocus) {
        if (hasFocus == false) return;

        //UpdateCanvasCamera();
    }

    private void OnApplicationPause(bool pauseStatus) {
        if (pauseStatus) return;

        //UpdateCanvasCamera();
    }

    private void Update() {
        if (renderCanvas.worldCamera == null) {
            renderCanvas.worldCamera = Camera.current;
            //LevelManager.Instance.TriggerCorruptedLoadDetected();
            Debug.Log("Updating because worldcam == null");
        }

        UpdateCanvasCamera(SceneManager.GetActiveScene());
    }

    private void UpdateCanvasCamera() {
        Scene sceneToUse = LevelManager.Instance.CurrentGameScene;

        if (renderCanvas.worldCamera != null) {
            if (sceneToUse.buildIndex == renderCanvas.worldCamera.gameObject.scene.buildIndex) {
                UpdateCanvasCamera(LevelManager.Instance.CurrentGameScene);
            }
        } else
            UpdateCanvasCamera(sceneToUse);
    }

    private void UpdateCanvasCamera(Scene sceneToUse) {
        if (sceneToUse.IsValid() == false) return;

        GameObject[] rootObjects = sceneToUse.GetRootGameObjects();
        for (int i = 0, length = rootObjects.Length; i < length; i++) {
            Camera cameraToUse = rootObjects[i].GetComponent<Camera>();

            if (cameraToUse == null) continue;

            if (cameraToUse.gameObject.activeSelf == false) {
                cameraToUse.gameObject.SetActive(true);
            }

            renderCanvas.worldCamera = cameraToUse;
            break;
        }
    }
}
