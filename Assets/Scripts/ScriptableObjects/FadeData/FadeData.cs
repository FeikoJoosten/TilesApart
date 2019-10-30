using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Camera Fade Data")]
public class FadeData : ScriptableObject {
    [SerializeField]
    private AnimationCurve fadeInCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 0));
    public AnimationCurve FadeInCurve => fadeInCurve;

    private int? fadeInCurveKeyCount;

    public int FadeInCurveKeyCount {
        get {
            if(!fadeInCurveKeyCount.HasValue)
                fadeInCurveKeyCount = fadeOutCurve.length;

            return fadeInCurveKeyCount.Value;
        }
    }

    [SerializeField]
    private AnimationCurve fadeOutCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1));
    public AnimationCurve FadeOutCurve => fadeOutCurve;

    private int? fadeOutCurveKeyCount;

    public int FadeOutCurveKeyCount {
        get {
            if (!fadeOutCurveKeyCount.HasValue)
                fadeOutCurveKeyCount = fadeOutCurve.length;

            return fadeOutCurveKeyCount.Value;
        }
    }
}
