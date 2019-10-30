using UnityEngine;

public class BackgroundMusicPlayer : AudioPlayer {
    [SerializeField] private AudioClip[] backgroundMusicSongs = null;

    private int backgroundMusicSongsCount = 0;

    protected override void Start() {
        backgroundMusicSongsCount = backgroundMusicSongs.Length;

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
        if (backgroundMusicSongsCount == 0) return null;
        if (backgroundMusicSongsCount == 1) return backgroundMusicSongs[0];

        AudioClip clipToReturn = backgroundMusicSongs[Random.Range(0, backgroundMusicSongsCount)];

        while (clipToReturn == lastPlayedClip) {
            clipToReturn = backgroundMusicSongs[Random.Range(0, backgroundMusicSongsCount)];
        }

        return clipToReturn;
    }
}
