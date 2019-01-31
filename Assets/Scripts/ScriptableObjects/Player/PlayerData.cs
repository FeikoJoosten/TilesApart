using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Player Data")]
public class PlayerData : ScriptableObject {
	[Header("Animations")]
	[SerializeField]
	private string leftTrigger = "OnRotateLeft";
	public string LeftTrigger => leftTrigger;
	[SerializeField]
	private string rightTrigger = "OnRotateRight";
	public string RightTrigger => rightTrigger;
	[SerializeField]
	private string backTrigger = "OnRotateBack";
	public string BackTrigger => backTrigger;
	[SerializeField]
	private string walkTrigger = "OnWalkForward";
	public string WalkTrigger => walkTrigger;
	[SerializeField]
	private string deathFarTrigger = "OnDeathFar";
	public string DeathFarTrigger => deathFarTrigger;
	[SerializeField]
	private string deathCloseTrigger = "OnDeathClose";
	public string DeathCloseTrigger => deathCloseTrigger;
	[SerializeField]
	private string resetTrigger = "Reset";
	public string ResetTrigger => resetTrigger;

	[Header("Rotation")]
	[SerializeField]
	private float shortRotationTime = 0.483f;
	public float ShortRotationTime => shortRotationTime;
	[SerializeField]
	private float longRotationTime = 0.766f;
	public float LongRotationTime => longRotationTime;
}
