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
        StartCoroutine(FadeImage(fadeImage, fadeData.FadeInCurve, shouldDisableObject, true));
    }

    public void FadeOut(bool shouldDisableObject = false) {
        if (fadeData == null) return;

        StopAllCoroutines();
        StartCoroutine(FadeImage(fadeImage, fadeData.FadeOutCurve, shouldDisableObject, false));
    }

    public void ChangeFadeImageStatus(bool shouldBeEnabled) {
        if (fadeImage == null) return;

        fadeImage.gameObject.SetActive(shouldBeEnabled);
    }

    private IEnumerator FadeImage(Image imageToFade, AnimationCurve fadeCurve, bool disableObject, bool isFadingIn) {
        if (imageToFade == null) yield break;

        float endTime = fadeCurve[fadeCurve.length - 1].time;
        currentFadeLevel = 0;
        OnFadeStarted(isFadingIn);

        Color startColor = imageToFade.color;
        startColor.a = fadeCurve[0].value;

        imageToFade.color = startColor;
        imageToFade.gameObject.SetActive(true);

        float currentTime = 0;
        Color currentColor = startColor;
        currentFadeLevel = 0;

        while (currentTime < endTime) {
            currentColor.a = fadeCurve.Evaluate(currentTime);

            imageToFade.color = currentColor;

            currentTime += Time.deltaTime;
            currentFadeLevel = currentTime / endTime;

            yield return null;
        }

        currentFadeLevel = endTime;
        currentColor.a = fadeCurve[fadeCurve.length - 1].value;
        imageToFade.color = currentColor;

        if (disableObject)
            imageToFade.gameObject.SetActive(false);

        OnFadeCompleted(disableObject);
    }
}
