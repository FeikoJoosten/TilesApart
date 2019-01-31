using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Tile Audio Data")]
public class TileAudioData : ScriptableObject {
	[Header("Movement")]
	[SerializeField]
	private List<AudioClip> movementSounds = null;
	public List<AudioClip> MovementSounds => movementSounds;
}