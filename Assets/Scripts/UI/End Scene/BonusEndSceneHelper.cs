using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BonusEndSceneHelper : AudioPlayer {
	[SerializeField]
	private Button backButton = null;

	[Header("Sounds")]
	[SerializeField]
	private List<AudioClip> buttonSounds = null;

#if UNITY_ANDROID
	private void Awake() {
		AndroidBackButton.AndroidBackButtonPressed += OnAndroidBackButtonPressed;
	}

	protected override void OnDestroy() {
		base.OnDestroy();

		AndroidBackButton.AndroidBackButtonPressed -= OnAndroidBackButtonPressed;
	}

	private void OnAndroidBackButtonPressed() {
		if (backButton == null) return;

		backButton.onClick.Invoke();
	}
#else
	private void Awake() {
        // Just to get rid of a warning because backButton isn't being used.
        backButton.name = backButton.name;
    }
#endif

	/// <summary> Play a random button click sound </summary>
	public void PlayRandomButtonSound(bool playOnce = true) {
		if (buttonSounds == null) return;

		PlayRandomSound(buttonSounds, playOnce);
	}
}
