using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeCamera : MonoBehaviour {
    public static event System.Action<bool> OnFadeStarted = delegate { };
    public static event System.Action<bool> OnFadeCompleted = delegate { };

    [SerializeField]
    private Image fadeImage = null;
    [SerializeField]
    private FadeData fadeData = null;

    private float currentFadeLevel = 0;
    public float CurrentFadeLevel => currentFadeLevel;

    private void Awake() {
        if (fadeImage == null || fadeData == null) return;

        fadeImage.gameObject.SetActive(false);
    }

    public void FadeIn(bool shouldDisableObject = true) {
        if (fadeData == null) return;

        StopAllCoroutines();
        StartCoroutine(FadeImage(fadeImage, fadeData.FadeInCurve, shouldDisableObject, true, fadeData.FadeInCurveKeyCount));
    }

    public void FadeOut(bool shouldDisableObject = false) {
        if (fadeData == null) return;

        StopAllCoroutines();
        StartCoroutine(FadeImage(fadeImage, fadeData.FadeOutCurve, shouldDisableObject, false, fadeData.FadeOutCurveKeyCount));
    }

    public void ChangeFadeImageStatus(bool shouldBeEnabled) {
        if (fadeImage == null) return;

        fadeImage.gameObject.SetActive(shouldBeEnabled);
    }

    private IEnumerator FadeImage(Image imageToFade, AnimationCurve fadeCurve, bool disableObject, bool isFadingIn, int fadeCurveLength) {
        if (imageToFade == null) yield break;

        currentFadeLevel = 0;
        OnFadeStarted(isFadingIn);

        Color startColor = imageToFade.color;
        startColor.a = fadeCurve[0].value;

        imageToFade.color = startColor;
        imageToFade.gameObject.SetActive(true);

        float startTime = Time.time;
        float endTime = Time.time + fadeCurve[fadeCurveLength - 1].time;
        Color currentColor = startColor;
        currentFadeLevel = 0;

        while (Time.time < endTime) {
            currentColor.a = fadeCurve.Evaluate(Time.time - startTime);

            imageToFade.color = currentColor;

            currentFadeLevel = (Time.time - startTime) / (endTime - startTime);

            yield return null;
        }

        currentFadeLevel = endTime;
        currentColor.a = fadeCurve[fadeCurveLength - 1].value;
        imageToFade.color = currentColor;

        if (disableObject)
            imageToFade.gameObject.SetActive(false);

        OnFadeCompleted(disableObject);
    }
}
