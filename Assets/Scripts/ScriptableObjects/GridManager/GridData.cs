using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Grid Data")]
public class GridData : ScriptableObject {

	[Header("Movement")]
	[SerializeField]
	private AnimationCurve tileVerticalMovement = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
	public AnimationCurve TileVerticalMovement => tileVerticalMovement;
	[SerializeField]
	private AnimationCurve tileHorizontalMovement = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
	public AnimationCurve TileHorizontalMovement => tileHorizontalMovement;
	[SerializeField]
	private AnimationCurve tileFreeMovement = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
	public AnimationCurve TileFreeMovement => tileFreeMovement;
	[SerializeField]
	private float tileDownMovementOffset = -3.5f;
	public float TileDownMovementOffset => tileDownMovementOffset;

	[Header("Player settings")]
	[SerializeField]
	private Vector3 playerSpawnOffset = new Vector3(0, 5.205f, 0);
	public Vector3 PlayerSpawnOffset => playerSpawnOffset;
    public float playerMoveWaitTime = 0.1f;
	[SerializeField]
	private float playerFastMoveSpeed = 10.0f;
	public float PlayerFastMoveSpeed => playerFastMoveSpeed;
	[SerializeField]
	private float playerWrappingAnimationSpeed = 1.0f;
	public float PlayerWrappingAnimationSpeed => playerWrappingAnimationSpeed;

	[SerializeField]
	private Player playerPrefab = null;
	public Player PlayerPrefab => playerPrefab;

    [Header("Input settings")]
	[Tooltip("This value is used to define the maximum amount of moves the player can undo")]
	[SerializeField]
	private float maxUndos = 5;
	public float MaxUndos => maxUndos;
    [SerializeField]
    [Tooltip("This value is used to define the minimum range the user needs to drag a tile before it moves if they release their finger")]
    public float minimumDraggingRange = 70;
    [Tooltip("Offset from tile center to top of center")]
    [SerializeField]
    public Vector3 tileTopOffset = new Vector3(0.0f, 5.0f, 0.0f);

    [Header("Tile highlighting settings")]
    [Tooltip("Color fade time")]
    [SerializeField]
    public float dragFadeTime = 0.6f;
    [Tooltip("Speed at which the gradient fades to the start stage")]
    [SerializeField]
    public float gradientStartLerpDuration = 0.2f;
    [Tooltip("How much the tiles are faded when mouse hasn't moved yet (0.0 to 1.0 as percentage)")]
    [SerializeField]
    public float gradientStartStage = 0.4f;
	[SerializeField]
	public float gradientMinLength = 50.0f;
    [Tooltip("Drag length over which the gradient changes")]
    [SerializeField]
    public float gradientMaxLength = 300.0f;
    [Tooltip("Whether or not start and end tiles fade")]
    [SerializeField]
    public bool startEndTilesFading = false;

    [Header("Tile wrap animation settings")]
    [Tooltip("Duration of fade out of tile when wrapping")]
    [SerializeField]
    public float tileWrapFadeOutDuration = 0.8f;
    [Tooltip("Duration of fade in of tile when wrapping")]
    [SerializeField]
    public float tileWrapFadeInDuration = 0.8f;
}