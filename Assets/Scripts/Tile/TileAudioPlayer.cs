using UnityEngine;

public class TileAudioPlayer : AudioPlayer {
	[SerializeField]
	private TileAudioData audioData = null;
	
	private static TileAudioPlayer instance;
	
	protected override void Start(){
		base.Start();
		
		TileAudioPlayer.instance = this;
		TileAudioPlayer.instance.audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
	}

	public static void PlayRandomMovementSound(bool playOnce = true) {
		if (TileAudioPlayer.instance == null) return;
		if (TileAudioPlayer.instance.audioData == null) return;

		TileAudioPlayer.instance.PlayRandomSound(TileAudioPlayer.instance.audioData.MovementSounds, playOnce, true);
	}
}