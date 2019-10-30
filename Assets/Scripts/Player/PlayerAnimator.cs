using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerAnimator : MonoBehaviour {
    public static event System.Action OnPlayerRespawned = delegate { };

    [SerializeField]
    private Animator controller;
    [ReadOnly]
    [SerializeField]
    private RuntimeAnimatorController tileAnimatorController = null;
    [SerializeField]
    private GameObject playerRenderer = null;

    private Vector3 startScreenPosition;
    private Vector2Int currentOrientation;
    private Player player;
    private IEnumerator currentRotationAnimation;
    private float defaultPlaybackSpeed;
    private bool inputEnabled = true;
    private Vector3 vecY90 = new Vector3(0, 90, 0);
    private Vector3 vecY180 = new Vector3(0, 180, 0);
    private Vector3 vecY270 = new Vector3(0, 270, 0);

    public bool IsRotating { get; private set; }
    public GameObject PlayerRenderer => playerRenderer;

    public void Awake() {
        if (controller == null) {
            controller = gameObject.AddComponent<Animator>();
            controller.runtimeAnimatorController = tileAnimatorController;
        }

        controller.applyRootMotion = true;

        PlayerDeathAnimation.OnPlayerDeathAnimationExit += OnPlayerDeathAnimationExit;
        PlayerWalkAnimation.OnPlayerWalkAnimationExit += OnPlayerWalkAnimationExit;
        GameMenus.OnPauseMenuOpened += DisableInput;
        GameMenus.OnPauseMenuClosed += EnableInput;
        GameMenus.OnRestartPressed += OnRestartPressed;
        player = GetComponent<Player>();

        defaultPlaybackSpeed = controller.speed;
    }

    private void OnDestroy() {
        PlayerDeathAnimation.OnPlayerDeathAnimationExit -= OnPlayerDeathAnimationExit;
        PlayerWalkAnimation.OnPlayerWalkAnimationExit -= OnPlayerWalkAnimationExit;
        GameMenus.OnPauseMenuOpened -= DisableInput;
        GameMenus.OnPauseMenuClosed -= EnableInput;
        GameMenus.OnRestartPressed -= OnRestartPressed;
    }

    private void Update() {
        if (controller == null || controller.runtimeAnimatorController == null) {
            enabled = false;
            return;
        }

        if (inputEnabled == false
            || player.GridManager.IsLevelRisingUp
            || player.GridManager.IsGridMoving
            || player.isMoving
            || player.isDead
            || player.hasWon
            || LevelManager.Instance.PreLoader.isTransitioning
            || Application.isFocused == false) {
            return;
        }

        if (Input.GetMouseButtonDown(0)) {
            startScreenPosition = Input.mousePosition;
            currentOrientation.x = Mathf.RoundToInt(-transform.forward.x);
            currentOrientation.y = Mathf.RoundToInt(-transform.forward.z);
        }

        if (Input.GetMouseButton(0) == false) {
            return;
        }

        Vector3 inputDirection = new Vector3(Input.mousePosition.x - startScreenPosition.x, 0, Input.mousePosition.y - startScreenPosition.y);

        if (inputDirection.sqrMagnitude == 0) {
            return;
        }

        inputDirection = Camera.main.transform.rotation * inputDirection;

        //Lock to axis
        if (Mathf.Abs(inputDirection.x) > Mathf.Abs(inputDirection.z)) {
            inputDirection = inputDirection.x >= 0 ? Vector3.right : Vector3.left;
        } else {
            inputDirection = inputDirection.z >= 0 ? Vector3.forward : Vector3.back;
        }

        Vector2Int aimDirection = new Vector2Int(Mathf.RoundToInt(-inputDirection.x), Mathf.RoundToInt(-inputDirection.z));
        Debug.DrawLine(transform.position, transform.position + (inputDirection.normalized * 10), Color.blue, 0.1f, false);
        Debug.DrawLine(transform.position, transform.position + (transform.forward * 10), Color.green, 0.1f, false);

        if (aimDirection != currentOrientation) {
            //North
            if (aimDirection.y == 1) {
                // We need to do a 180 rotation, because of an inverse direction
                if (currentOrientation.y == -1) {
                    RotateCharacter(player.PlayerData.BackTrigger, aimDirection);
                }
                //We want north facing, but we are facing east. Rotate left
                else if (currentOrientation.x == 1) {
                    RotateCharacter(player.PlayerData.LeftTrigger, aimDirection);
                }
                //We want north facing, but we are facing west. Rotate right
                else if (currentOrientation.x == -1) {
                    RotateCharacter(player.PlayerData.RightTrigger, aimDirection);
                }
            }
            //East
            else if (aimDirection.x == 1) {
                //We want east facing, but we are facing north. Rotate right
                if (currentOrientation.y == 1) {
                    RotateCharacter(player.PlayerData.RightTrigger, aimDirection);
                }
                //We want east facing, but we are facing south. Rotate left
                else if (currentOrientation.y == -1) {
                    RotateCharacter(player.PlayerData.LeftTrigger, aimDirection);
                }
                // We need to do a 180 rotation, because of an inverse direction
                else if (currentOrientation.x == -1) {
                    RotateCharacter(player.PlayerData.BackTrigger, aimDirection);
                }
            }
            //South
            else if (aimDirection.y == -1) {
                // We need to do a 180 rotation, because of an inverse direction
                if (currentOrientation.y == 1) {
                    RotateCharacter(player.PlayerData.BackTrigger, aimDirection);
                }
                //We want south facing, but we are facing east. Rotate right
                else if (currentOrientation.x == 1) {
                    RotateCharacter(player.PlayerData.RightTrigger, aimDirection);
                }
                //We want south facing, but we are facing west. Rotate left
                else if (currentOrientation.x == -1) {
                    RotateCharacter(player.PlayerData.LeftTrigger, aimDirection);
                }
            }
            //West
            else if (aimDirection.x == -1) {
                //We want west facing, but we are facing north. Rotate left
                if (currentOrientation.y == 1) {
                    RotateCharacter(player.PlayerData.LeftTrigger, aimDirection);
                }
                //We want west facing, but we are facing south. Rotate right
                else if (currentOrientation.y == -1) {
                    RotateCharacter(player.PlayerData.RightTrigger, aimDirection);
                }
                // We need to do a 180 rotation, because of an inverse direction
                else if (currentOrientation.x == 1) {
                    RotateCharacter(player.PlayerData.BackTrigger, aimDirection);
                }
            }
        }
    }

    private void EnableInput() {
        inputEnabled = true;
    }

    private void DisableInput() {
        inputEnabled = false;
    }

    public void ActivateTrigger(string triggerToActivate, Vector2Int newOrientation) {
        ResetPlaybackSpeed();

        if (controller != null) {
            controller.SetTrigger(triggerToActivate);
        }

        currentOrientation = newOrientation;
    }

    private void RotateCharacter(string triggerToActivate, Vector2Int newOrientation) {
        if (currentRotationAnimation != null) {
            StopCoroutine(currentRotationAnimation);
            currentRotationAnimation = null;
        }

        // Some animations move the root of the player, need to fix this.
        transform.position = player.GridManager.GetTileAtIndex(player.CurrentTileIndex).transform.position + player.GridManager.GridData.PlayerSpawnOffset;

        Vector3 aimRotation = Vector3.zero;

        // North
        // North is already vector3.zero
        //if (newOrientation.y == -1) {
        //	
        //}
        // East
        if (newOrientation.x == -1) {
            aimRotation = vecY90;
        }
        // South
        else if (newOrientation.y == 1) {
            aimRotation = vecY180;
        }
        // West
        else if (newOrientation.x == 1) {
            aimRotation = vecY270;
        }

        currentRotationAnimation = RotatePlayerModel(aimRotation);
        StartCoroutine(currentRotationAnimation);

        ActivateTrigger(triggerToActivate, newOrientation);
    }

    private IEnumerator RotatePlayerModel(Vector3 targetRotationEuler) {
        IsRotating = true;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(targetRotationEuler);

        float startTime = Time.time;
        float endTime = player.PlayerData.ShortRotationTime;

        if (Quaternion.Angle(startRotation, targetRotation) > 90) {
            endTime = player.PlayerData.LongRotationTime;
        }

        endTime += Time.time;

        while (Time.time < endTime) {
            if (player.GridManager.IsGridMoving) {
                transform.rotation = targetRotation;
                currentRotationAnimation = null;
                IsRotating = false;
                yield break;
            }

            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, (Time.time - startTime) / (endTime - startTime));

            yield return null;
        }

        transform.rotation = targetRotation;
        currentRotationAnimation = null;
        IsRotating = false;
    }

    public void OnPlayerStartWalking(Vector2Int movementDirection) {
        if (player.gameObject.activeInHierarchy == false) {
            return;
        }

        // Some animations move the root of the player, this is to fix that.
        transform.position = player.GridManager.GetTileWorldPosition(player.CurrentTileIndex) + player.GridManager.GridData.PlayerSpawnOffset;

        Vector3 aimRotation = Vector3.zero;

        // North
        // North is already vector3.zero
        //if (newOrientation.y == 1) {
        //	
        //}
        // East
        if (movementDirection.x == 1) {
            aimRotation = vecY90;
        }
        // South
        else if (movementDirection.y == -1) {
            aimRotation = vecY180;
        }
        // West
        else if (movementDirection.x == -1) {
            aimRotation = vecY270;
        }

        transform.rotation = Quaternion.Euler(aimRotation);

        if (player.DiesOnMovement(player.CurrentTileIndex, movementDirection)) {
            Tile tileToReach = player.GridManager.GetTileAtIndex(player.CurrentTileIndex + movementDirection);

            if (tileToReach == null || tileToReach.tileType == TileType.Pathless || tileToReach.tileType == TileType.Empty || tileToReach.tileType == TileType.Border || (tileToReach.TileAnimator != null && tileToReach.TileAnimator.IsDown)) {
                ActivateTrigger(player.PlayerData.DeathFarTrigger, currentOrientation - movementDirection);
            } else {
                ActivateTrigger(player.PlayerData.DeathCloseTrigger, currentOrientation - movementDirection);
            }
        } else {
            if (currentRotationAnimation != null) {
                StopCoroutine(currentRotationAnimation);

                player.GridManager.AlignPlayerRotation();
            }

            ActivateTrigger(player.PlayerData.WalkTrigger, currentOrientation);
        }
    }

    private void OnRestartPressed(int currentMoveCount) {
        if (gameObject.activeInHierarchy == false) {
            return;
        }

        if (currentMoveCount == 0) return;

        StartCoroutine(DeathAnimationExit());
    }

    private void OnPlayerDeathAnimationExit() {
        if (gameObject.activeInHierarchy == false) {
            return;
        }

        StartCoroutine(DeathAnimationExit());
    }

    private IEnumerator DeathAnimationExit() {
        player.GridManager.SinkLevel(false, true, player.GridManager.startTile.TileAnimator.IsDown, true);

        float startTime = Time.time;
        float endTime = Time.time + player.GridManager.GridData.tileWrapFadeOutDuration;

        SkinnedMeshRenderer meshRenderer = playerRenderer.GetComponent<SkinnedMeshRenderer>();
        Material[] originalMaterials = meshRenderer.sharedMaterials;
        int materialLength = meshRenderer.materials.Length;

        while (Time.time < endTime) {
            for (int i = 0; i < materialLength; i++) {
                meshRenderer.materials[i].SetFloat(player.GridManager.TileData.playerFragmentationName,((Time.time - startTime) / (endTime - startTime)));
            }

            transform.localPosition -= Vector3.one * Time.deltaTime;

            yield return null;
        }

        for(int i = 0; i < materialLength; i++) {
            meshRenderer.materials[i].SetFloat(player.GridManager.TileData.playerFragmentationName, 1);
        }

        player.GridManager.AlignPlayerRotation();

        while(player.GridManager.IsLevelDown() == false)
            yield return null;

        player.GridManager.ResetLevel();

        yield return null;

        StartCoroutine(player.GridManager.RiseLevel(false, true, false, true));

        while (player.GridManager.IsLevelRisingUp) {
            yield return null;
        }

        transform.localPosition = player.GridManager.startTile.transform.position + player.GridManager.GridData.PlayerSpawnOffset;
        startTime = Time.time;
        endTime = Time.time + player.GridManager.GridData.tileWrapFadeOutDuration;

        while (Time.time < endTime) {
            for(int i = 0; i < materialLength; i++) {
                meshRenderer.materials[i].SetFloat(player.GridManager.TileData.playerFragmentationName, 1.0f - ((Time.time - startTime) / (endTime - startTime)));
            }

            yield return null;
        }

        meshRenderer.sharedMaterials = originalMaterials;

        player.ResetPlayer();
        Physics.SyncTransforms();
        OnPlayerRespawned();
    }

    public void ResetController() {
        if (controller == null) return;

        controller.SetTrigger(player.PlayerData.ResetTrigger);
    }

    public void ResetPlaybackSpeed() {
        if (controller == null) return;

        controller.speed = defaultPlaybackSpeed;
    }

    private void OnPlayerWalkAnimationExit() {
        ResetPlaybackSpeed();
        player.PlayerMoveEnd();
    }
}