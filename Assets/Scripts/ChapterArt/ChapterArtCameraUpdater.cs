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

		PreLoader.OnLevelTransition += UpdateCanvasCamera;

		UpdateCanvasCamera();
	}

	private void OnDestroy() {
		PreLoader.OnLevelTransition -= UpdateCanvasCamera;
	}

	private void OnApplicationFocus(bool hasFocus) {
		if (hasFocus == false) return;

		UpdateCanvasCamera();
	}

	private void OnApplicationPause(bool pauseStatus) {
		if (pauseStatus) return;

		UpdateCanvasCamera();
	}

	private void LateUpdate() {
		if (renderCanvas.worldCamera == null) {
			renderCanvas.worldCamera = Camera.current;
		}
			//UpdateCanvasCamera(SceneManager.GetActiveScene());
	}

	private void UpdateCanvasCamera() {
		Scene sceneToUse = SceneManager.GetSceneByName(LevelManager.Instance.GetCurrentLevelName());

		if (renderCanvas.worldCamera != null) {
			if (sceneToUse.buildIndex == renderCanvas.worldCamera.gameObject.scene.buildIndex) return;
		}

		UpdateCanvasCamera(sceneToUse);
	}

	private void UpdateCanvasCamera(Scene sceneToUse) {
		if (sceneToUse.IsValid() == false) return;

		foreach (GameObject rootObject in sceneToUse.GetRootGameObjects()) {
			Camera cameraToUse = rootObject.GetComponent<Camera>();

			if (cameraToUse == null) continue;

			if (cameraToUse.gameObject.activeSelf == false) {
				continue;
			}

			renderCanvas.worldCamera = cameraToUse;
			break;
		}
	}
}
