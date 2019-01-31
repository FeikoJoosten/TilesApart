using System;
using UnityEngine;

public class AudioManager : Singleton<AudioManager> {
	public static event Action<bool, float> OnSFXChange = delegate { };
	public static event Action<bool, float> OnMusicChange = delegate { };
	[Header("Player Prefs")]
	[SerializeField]
	private string sfxEnabledLocation = "SFXEnabled";
	[SerializeField]
	private string sfxVolumeLocation = "SFXVolume";
	[SerializeField]
	private string musicEnabledLocation = "MusicEnabled";
	[SerializeField]
	private string musicVolumeLocation = "MusicVolumeLocation";

	[Header("SFX")]
	private bool sfxEnabled = true;
	public bool SfxEnabled => sfxEnabled;
	private float sfxVolume = 1;
	public float SfxVolume => sfxVolume;
	[Header("Music")]
	private bool musicEnabled = true;
	public bool MusicEnabled => musicEnabled;
	private float musicVolume = 1;
	public float MusicVolume => musicVolume;

	protected override void Awake() {
		base.Awake();

		LoadSettings();
	}

	public void ToggleSFX() {
		sfxEnabled = !sfxEnabled;
		OnSFXChange(sfxEnabled, sfxVolume);
	}

	/// <summary>
	/// This method will allow you to set the volume of the SFX sounds
	/// </summary>
	/// <param name="newVolume">Please assign a value between 0 and 1.</param>
	public void SetSFXVolume(float newVolume) {
		if (newVolume > 1) {
			newVolume = 1;
		}

		if (newVolume == sfxVolume) return;

		sfxVolume = newVolume;
		OnSFXChange(sfxEnabled, sfxVolume);
	}

	public void ToggleMusic() {
		musicEnabled = !musicEnabled;
		OnMusicChange(musicEnabled, musicVolume);
	}

	/// <summary>
	/// This method will allow you to set the volume of the music sounds
	/// </summary>
	/// <param name="newVolume">Please assign a value between 0 and 1.</param>
	public void SetMusicVolume(float newVolume) {
		if (newVolume > 1) {
			newVolume = 1;
		}

		if (newVolume == musicVolume) return;

		musicVolume = newVolume;
		OnMusicChange(musicEnabled, musicVolume);
	}

	private void LoadSettings() {
		bool sfxSavedEnabled = sfxEnabled;

		if (PlayerPrefs.HasKey(sfxEnabledLocation)) {
			sfxSavedEnabled = PlayerPrefs.GetInt(sfxEnabledLocation) == 1 ? true : false;
		}

		float sfxSavedVolume = sfxVolume;

		if (PlayerPrefs.HasKey(sfxVolumeLocation)) {
			sfxSavedVolume = PlayerPrefs.GetFloat(sfxVolumeLocation);
		}

		if (sfxEnabled != sfxSavedEnabled || sfxSavedVolume != sfxVolume) {
			sfxEnabled = sfxSavedEnabled;
			sfxVolume = sfxSavedVolume;
			OnSFXChange(sfxEnabled, sfxVolume);
		}

		bool musicSavedEnabled = musicEnabled;

		if (PlayerPrefs.HasKey(sfxEnabledLocation)) {
			musicSavedEnabled = PlayerPrefs.GetInt(musicEnabledLocation) == 1 ? true : false;
		}

		float musicSavedVolume = musicVolume;

		if (PlayerPrefs.HasKey(sfxVolumeLocation)) {
			musicSavedVolume = PlayerPrefs.GetFloat(musicVolumeLocation);
		}

		if (musicEnabled != musicSavedEnabled || musicSavedVolume != musicVolume) {
			musicEnabled = musicSavedEnabled;
			musicVolume = musicSavedVolume;
			OnSFXChange(musicEnabled, musicVolume);
		}
	}

	private void SaveSettings() {
		PlayerPrefs.SetInt(sfxEnabledLocation, sfxEnabled == true ? 1 : 0);
		PlayerPrefs.SetFloat(sfxVolumeLocation, sfxVolume);
		PlayerPrefs.SetInt(musicEnabledLocation, musicEnabled == true ? 1 : 0);
		PlayerPrefs.SetFloat(musicVolumeLocation, musicVolume);
	}

	// Save/load playerprefs for whenever we lose focus to the application
	private void OnApplicationFocus(bool hasFocus) {
		if (hasFocus)
			LoadSettings();
		else
			SaveSettings();
	}

	// Save/load playerprefs for whenever we pause the application
	private void OnApplicationPause(bool pauseStatus) {
		if (pauseStatus)
			SaveSettings();
		else
			LoadSettings();
	}

	// Save/load playerprefs for whenever we quit the application
	private void OnApplicationQuit() {
		SaveSettings();
	}

	// Safety measure, to make sure we save all player prefs on destroy
	private void OnDestroy() {
		SaveSettings();
	}
}
