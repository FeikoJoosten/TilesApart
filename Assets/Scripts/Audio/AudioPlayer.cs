using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AudioType {
	SFX,
	Music
}

[RequireComponent(typeof(AudioSource))]
public abstract class AudioPlayer : MonoBehaviour {
	[SerializeField]
	protected AudioType audioType = AudioType.SFX;

	[Range(0, 1)]
	protected float audioVolume;

	protected AudioSource audioSource;

	protected virtual void Start() {
		audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

		switch (audioType) {
			case AudioType.SFX:
				AudioManager.OnSFXChange += OnSFXChange;
				break;
			case AudioType.Music:
				AudioManager.OnMusicChange += OnMusicChange;
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		SetDefaultSettings();
	}

	protected virtual void OnDestroy() {
		switch (audioType) {
			case AudioType.SFX:
				AudioManager.OnSFXChange -= OnSFXChange;
				break;
			case AudioType.Music:
				AudioManager.OnMusicChange -= OnMusicChange;
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void OnSFXChange(bool isEnabled, float newVolume) {
		HandleAudioChange(isEnabled, newVolume);
	}

	private void OnMusicChange(bool isEnabled, float newVolume) {
		HandleAudioChange(isEnabled, newVolume);
	}

	protected virtual void HandleAudioChange(bool isEnabled, float newVolume) {
		audioSource.enabled = isEnabled;
		audioSource.volume = newVolume;
	}

	protected virtual void SetDefaultSettings() {
		switch (audioType) {
			case AudioType.SFX:
				HandleAudioChange(AudioManager.Instance.SfxEnabled, AudioManager.Instance.SfxVolume);
				break;
			case AudioType.Music:
				HandleAudioChange(AudioManager.Instance.MusicEnabled, AudioManager.Instance.MusicVolume);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	protected virtual void OnAudioEnd(AudioClip lastPlayedSong) {
		
	}

	public virtual void StopAllSounds() {
		audioSource.Stop();
	}

	public virtual void PlaySound(AudioClip clipToPlay, bool playOnce = true, bool playOneShot = false) {
		// In case the game object has been disabled, return to prevent a warning
		if (audioSource.enabled == false || gameObject.activeInHierarchy == false) return;

		audioSource.loop = !playOnce;
		if (playOneShot) {
			audioSource.PlayOneShot(clipToPlay, audioVolume);
		}
		else {
			audioSource.clip = clipToPlay;
			audioSource.Play();
		}
		StartCoroutine(AudioTracker());
	}

	// Save/load playerprefs for whenever we pause the application
	private void OnApplicationPause(bool pauseStatus) {
		if (pauseStatus)
			StopAllCoroutines();
		else
			StartCoroutine(AudioTracker());
	}

	private IEnumerator AudioTracker() {
		if (audioSource == null) yield break;

		AudioClip currentTrack = audioSource.clip;

		while (audioSource.isPlaying) {
			yield return null;
		}

		if (Application.isFocused)
			OnAudioEnd(currentTrack);
	}

	// Allow for more random sounds support in the future
	public virtual void PlayRandomSound(ICollection<AudioClip> filesToPlay, bool playOnce = true, bool playOneShot = false) {
		// Get a random audio file from the collection and play it once.
		int fileToPlay = UnityEngine.Random.Range(0, filesToPlay.Count - 1);

		if (fileToPlay < 0 || fileToPlay >= filesToPlay.Count) return;

		if (filesToPlay.ElementAt(fileToPlay) == null) return;

		PlaySound(filesToPlay.ElementAt(fileToPlay), playOnce);
	}
}
