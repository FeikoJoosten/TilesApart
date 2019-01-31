using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FadeOnLevelTransition : MonoBehaviour {
	[SerializeField]
	private AnimationCurve fadeInCurve = new AnimationCurve(new Keyframe(0, 0.65f), new Keyframe(0.5f, 1));
	[SerializeField]
	private AnimationCurve fadeOutCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 0.65f));

	private Button buttonToUpdate;

	private void Awake() {
		buttonToUpdate = GetComponent<Button>();

		Player.OnPlayerWon += OnPlayerOnOnPlayerWon;
		PreLoader.OnLevelTransition += OnLevelLoaded;
	}

	private void OnDestroy() {
		Player.OnPlayerWon -= OnPlayerOnOnPlayerWon;
		PreLoader.OnLevelTransition -= OnLevelLoaded;
	}

	private void OnPlayerOnOnPlayerWon(string wonLevel) {
		StopAllCoroutines();
		StartCoroutine(FadeButton(fadeOutCurve, true));
	}

	private void OnLevelLoaded() {
		StopAllCoroutines();
		StartCoroutine(FadeButton(fadeInCurve, false));
	}

	private IEnumerator FadeButton(AnimationCurve fadeCurve, bool shouldDisable) {
		ColorBlock savedColorBlock = buttonToUpdate.colors;
		Color savedColor = savedColorBlock.normalColor;
		savedColorBlock.disabledColor = savedColor;
		buttonToUpdate.colors = savedColorBlock;
		buttonToUpdate.interactable = !shouldDisable;

		float currentTime = 0;
		float endTime = fadeCurve.keys[fadeCurve.keys.Length - 1].time;

		while (currentTime < endTime) {
			savedColor.a = fadeCurve.Evaluate(currentTime);

			savedColorBlock.disabledColor = savedColor;
			buttonToUpdate.colors = savedColorBlock;

			currentTime += Time.deltaTime;
			yield return null;
		}

		savedColor.a = fadeCurve[fadeCurve.length - 1].value;
		savedColorBlock.disabledColor = savedColor;
		buttonToUpdate.colors = savedColorBlock;
	}
}
