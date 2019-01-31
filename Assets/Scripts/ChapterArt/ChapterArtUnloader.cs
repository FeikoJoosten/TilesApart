using System.Collections;
using UnityEngine;

public class ChapterArtUnloader : MonoBehaviour {
	private void Awake() {
		ChapterArtUnloader[] otherChapterArt = FindObjectsOfType<ChapterArtUnloader>();

		for (int i = 0; i < otherChapterArt.Length; i++) {
			if (otherChapterArt[i].gameObject == gameObject) continue;
			if (otherChapterArt[i].gameObject.name != gameObject.name) continue;

			Destroy(gameObject);
			return;
		}

		LevelManager.OnChapterTransition += OnChapterTransition;
		LevelManager.OnMainMenuLoading += OnChapterTransition;
		PreLoader.OnEndSceneLoaded += OnEndSceneLoading;
		DontDestroyOnLoad(gameObject);
	}

	private void OnDestroy() {
		LevelManager.OnChapterTransition -= OnChapterTransition;
		LevelManager.OnMainMenuLoading -= OnChapterTransition;
		PreLoader.OnEndSceneLoaded -= OnEndSceneLoading;
	}

	private void OnEndSceneLoading() {
		StartCoroutine(WaitForFadeEffect());
	}

	private IEnumerator WaitForFadeEffect() {
		while (LevelManager.Instance.FadeCamera.CurrentFadeLevel < 1f) {
			yield return null;
		}

		OnChapterTransition();
	}

	private void OnChapterTransition() {
		Destroy(gameObject);
	}
}
