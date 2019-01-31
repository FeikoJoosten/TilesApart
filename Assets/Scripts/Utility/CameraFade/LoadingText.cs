using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LoadingText : MonoBehaviour {
	[SerializeField]
	[Multiline]
	private string loadingTextFormat = "LOADING\n{0}";
	[SerializeField]
	private int maxDotCount = 4;
	[SerializeField]
	private float timeBetweenUpdates = 0.1f;

	private Text loadingText;
	private Coroutine loadingCoroutine = null;

	private void Awake() {
		loadingText = GetComponent<Text>();

		FadeCamera.OnFadeStarted += OnFadeStarted;
		FadeCamera.OnFadeCompleted += OnFadeCompleted;
	}

	private void OnDestroy() {
		FadeCamera.OnFadeStarted -= OnFadeStarted;
		FadeCamera.OnFadeCompleted -= OnFadeCompleted;
	}

	private void OnFadeStarted(bool isFadingIn) {
		LevelManager.Instance.FadeCamera.StartCoroutine(isFadingIn ? WhileFading(0, 1) : WhileFading(1, 0));

		if (loadingCoroutine == null)
			loadingCoroutine = LevelManager.Instance.FadeCamera.StartCoroutine(WhileLoading());
	}

	private IEnumerator WhileFading(float startAlpha, float targetAlpha) {
		float currentTime = LevelManager.Instance.FadeCamera.CurrentFadeLevel;

		Color currentColor = loadingText.color;
		currentColor.a = startAlpha;

		while (currentTime < 1) {
			currentColor.a = Mathf.Lerp(startAlpha, targetAlpha, 1 - currentTime);
			loadingText.color = currentColor;
			
			currentTime = LevelManager.Instance.FadeCamera.CurrentFadeLevel;
			yield return null;
		}

		loadingText.color = currentColor;
	}

	private IEnumerator WhileLoading() {
		loadingText.text = string.Format(loadingTextFormat, "");

		int i = 0;
		while (true) {
			string stringToAdd = "";

			for (int j = 0; j < i; j++) {
				stringToAdd += ".";
			}

			loadingText.text = string.Format(loadingTextFormat, stringToAdd);
			i++;

			if (i == maxDotCount)
				i = 0;

			yield return new WaitForSeconds(timeBetweenUpdates);
		}
	}

	private void OnFadeCompleted(bool isDisabled) {
		if (loadingCoroutine == null) return;

		LevelManager.Instance.FadeCamera.StopCoroutine(loadingCoroutine);
		loadingCoroutine = null;
		loadingText.text = string.Format(loadingTextFormat, "");
	}
}
