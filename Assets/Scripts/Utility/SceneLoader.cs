using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {
    [SerializeField]
    [Scene]
    private string sceneToLoad = null;
    [SerializeField]
    private bool autoLoad = true;
    [SerializeField]
    private bool useFadeEffect = false;

    private void Awake() {
        if (autoLoad == false) return;
        if (Application.isEditor && SceneManager.GetActiveScene().buildIndex != gameObject.scene.buildIndex) return;
        if (string.IsNullOrEmpty(sceneToLoad)) {
            return;
        }

        if (useFadeEffect) {
            StartCoroutine(LoadWithFade());
        } else {
            LoadScene();
        }
    }

    public void LoadSceneWithFade() {
        if (string.IsNullOrEmpty(sceneToLoad)) {
            return;
        }

        StartCoroutine(LoadWithFade());
    }

    private IEnumerator LoadWithFade() {
        if (string.IsNullOrEmpty(sceneToLoad)) {
            yield break;
        }

        LevelManager.Instance.FadeCamera.FadeOut();

        while (LevelManager.Instance.FadeCamera.CurrentFadeLevel < 1f) {
            yield return null;
        }

        LoadScene();
    }

    public void LoadScene() {
        if (string.IsNullOrEmpty(sceneToLoad)) {
            return;
        }
        if (SceneUtility.GetBuildIndexByScenePath(sceneToLoad) == -1) {
            Debug.LogWarning("Please add the scene " + sceneToLoad + " to the build path");
            return;
        }
        SceneManager.LoadScene(sceneToLoad);
    }
}
