using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioPlayer : AudioPlayer {
    [SerializeField]
    private List<AudioClip> deathSounds = new List<AudioClip>();

    /// <summary> Play a random button click sound </summary>
    public void PlayRandomDeathSound(bool playOnce = true) {
        PlayRandomSound(deathSounds, playOnce);
    }
}
