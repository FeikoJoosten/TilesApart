using UnityEngine;

public class LevelSelector : MonoBehaviour {
	[SerializeField]
	private UnityEngine.UI.Button buttonToUse = null;
	[SerializeField]
	private UnityEngine.UI.Text displayText = null;
	[SerializeField]
	private UnityEngine.UI.Image lockIcon = null;

	[SerializeField]
	private Color unlockedColor = Color.magenta;
	[SerializeField]
	private Color alreadyCompletedColor = Color.white;
	[SerializeField]
	private Color lockedColor = Color.black;

	private string levelToLoad;
	private UnityEngine.UI.ColorBlock savedColorBlock;

	public void Initialize(string displayName, string levelName, bool isUnlocked, bool isCompleted, MainMenuHelper mainMenuHelper) {
		levelToLoad = levelName;

		if (displayText != null) {
			if (isUnlocked || Debug.isDebugBuild) {
				displayText.text = displayName;
				displayText.enabled = true;
				if (lockIcon != null) {
					lockIcon.enabled = false;
				}
			}
			else {
				displayText.enabled = false;
				if (lockIcon != null) {
					lockIcon.enabled = true;
				}
			}
		}

		if (buttonToUse != null) {
			buttonToUse.interactable = Debug.isDebugBuild || isUnlocked;

			savedColorBlock = buttonToUse.colors;
			savedColorBlock.disabledColor = lockedColor;
			buttonToUse.colors = savedColorBlock;

			if (buttonToUse.image != null) {
				buttonToUse.image.color = isCompleted ? alreadyCompletedColor : unlockedColor;
			}
		}

		if (mainMenuHelper != null && buttonToUse != null) {
			buttonToUse.onClick.AddListener(() => mainMenuHelper.PlayRandomButtonSound(true));
		}
	}

	public void LoadLevel() {
		if (levelToLoad.Length == 0) {
			Debug.LogError("You tried to load a level that doesn't excist");
			return;
		}

		LevelManager.Instance.LoadGameLevel(levelToLoad, true, true, true, true);
	}
}
