using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Camera Fade Data")]
public class FadeData : ScriptableObject {
	[SerializeField]
	private AnimationCurve fadeInCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 0));
	public AnimationCurve FadeInCurve => fadeInCurve;
	[SerializeField]
	private AnimationCurve fadeOutCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1));
	public AnimationCurve FadeOutCurve => fadeOutCurve;
}
