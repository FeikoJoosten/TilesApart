using UnityEngine;

public class BackgroundMusicPlayer : AudioPlayer {
    [SerializeField] private AudioClip[] backgroundMusicSongs = null;

    protected override void Start() {
        base.Start();

        StartBackGroundMusic();
    }

    protected override void HandleAudioChange(bool isEnabled, float newVolume) {
        base.HandleAudioChange(isEnabled, newVolume);

        if (isEnabled == false) return;

        StartBackGroundMusic();
    }

    private void StartBackGroundMusic() {
        PlaySound(GetRandomAudioClip());
    }

    protected override void OnAudioEnd(AudioClip lastPlayedSong) {
        PlaySound(GetRandomAudioClip(lastPlayedSong));
    }

    private AudioClip GetRandomAudioClip(AudioClip lastPlayedClip = null) {
        if (backgroundMusicSongs.Length == 0) return null;
        if (backgroundMusicSongs.Length == 1) return backgroundMusicSongs[0];

        AudioClip clipToReturn = backgroundMusicSongs[Random.Range(0, backgroundMusicSongs.Length)];

        while (clipToReturn == lastPlayedClip) {
            clipToReturn = backgroundMusicSongs[Random.Range(0, backgroundMusicSongs.Length)];
        }

        return clipToReturn;
    }
}
