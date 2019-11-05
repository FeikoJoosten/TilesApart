using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeOnLevelLoad : MonoBehaviour {
    [SerializeField]
    private AnimationCurve fadeInCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1));
    [SerializeField]
    private AnimationCurve fadeOutCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 0));
    private int imagesCount;
    private int textsCount;
    private int animationsCount;

    private Image[] imagesInChildren;
    private Text[] textsInChildren;
    private Animation[] animationsInChildren;

    private void Start() {
        imagesInChildren = GetComponentsInChildren<Image>();
        textsInChildren = GetComponentsInChildren<Text>();
        animationsInChildren = GetComponentsInChildren<Animation>();

        imagesCount = imagesInChildren.Length;
        textsCount = textsInChildren.Length;
        animationsCount = animationsInChildren.Length;

        StartCoroutine(HandleFading(fadeInCurve, fadeInCurve.length));

        Player.OnPlayerWon += OnPlayerWon;
    }

    private void OnDestroy() {
        Player.OnPlayerWon -= OnPlayerWon;
    }

    private void OnPlayerWon(string obj) {
        StopAllCoroutines();

        for (int i = 0; i < animationsCount; i++) {
            animationsInChildren[i].enabled = false;
        }

        StartCoroutine(HandleFading(fadeOutCurve, fadeOutCurve.length));
    }

    private IEnumerator HandleFading(AnimationCurve fadeCurve, int curveLength) {
        float startTime = Time.time;
        float endTime = Time.time + fadeCurve[curveLength - 1].time;

        while (Time.time <= endTime) {
            UpdateAlphas(fadeCurve.Evaluate(Time.time - startTime));
            yield return null;
        }

        UpdateAlphas(fadeCurve[curveLength - 1].value);
    }

    private void UpdateAlphas(float currentAlpha) {
        for(int i = 0; i < imagesCount; i++) {
            Color currentColor = imagesInChildren[i].color;
            currentColor.a = currentAlpha;
            imagesInChildren[i].color = currentColor;
        }

        for(int i = 0; i < textsCount; i++) {
            Color currentColor = textsInChildren[i].color;
            currentColor.a = currentAlpha;
            textsInChildren[i].color = currentColor;
        }
    }
}
