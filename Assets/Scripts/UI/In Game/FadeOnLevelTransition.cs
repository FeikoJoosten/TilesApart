using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FadeOnLevelTransition : MonoBehaviour {
    [SerializeField]
    private AnimationCurve fadeInCurve = new AnimationCurve(new Keyframe(0, 0.65f), new Keyframe(0.5f, 1));
    [SerializeField]
    private AnimationCurve fadeOutCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 0.65f));

    private int fadeInCurveKeyCount;
    private int fadeOutCurveKeyCount;
    private Button buttonToUpdate;

    private void Awake() {
        fadeInCurveKeyCount = fadeInCurve.length;
        fadeOutCurveKeyCount = fadeOutCurve.length;
        buttonToUpdate = GetComponent<Button>();

        Player.OnPlayerWillWin += OnPlayerWillWin;
        LevelManager.OnCurrentGameSceneChanged += OnLevelLoaded;
        LevelManager.OnCorruptedLoadDetected += OnLevelLoaded;
    }

    private void OnDestroy() {
        Player.OnPlayerWillWin -= OnPlayerWillWin;
        LevelManager.OnCurrentGameSceneChanged -= OnLevelLoaded;
        LevelManager.OnCorruptedLoadDetected -= OnLevelLoaded;
    }

    private void OnPlayerWillWin() {
        StopAllCoroutines();
        StartCoroutine(FadeButton(fadeOutCurve, true, fadeOutCurveKeyCount));
    }

    private void OnLevelLoaded() {
        StopAllCoroutines();
        StartCoroutine(FadeButton(fadeInCurve, false, fadeInCurveKeyCount));
    }

    private IEnumerator FadeButton(AnimationCurve fadeCurve, bool shouldDisable, int curveLength) {
        ColorBlock savedColorBlock = buttonToUpdate.colors;
        Color savedColor = savedColorBlock.normalColor;
        savedColorBlock.disabledColor = savedColor;
        buttonToUpdate.colors = savedColorBlock;
        buttonToUpdate.interactable = !shouldDisable;

        float startTime = Time.time;
        float endTime = Time.time + fadeCurve[curveLength - 1].time;

        while (Time.time < endTime) {
            savedColor.a = fadeCurve.Evaluate(Time.time - startTime);

            savedColorBlock.disabledColor = savedColor;
            buttonToUpdate.colors = savedColorBlock;

            yield return null;
        }

        savedColor.a = fadeCurve[curveLength - 1].value;
        savedColorBlock.disabledColor = savedColor;
        buttonToUpdate.colors = savedColorBlock;
    }
}
