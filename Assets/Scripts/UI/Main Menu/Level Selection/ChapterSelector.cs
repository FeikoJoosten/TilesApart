using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChapterSelector : MonoBehaviour {
	[SerializeField]
	private int chapterNumber = 0;
	[SerializeField]
	protected Text chapterText = null;
	[SerializeField]
	protected Button buttonToUse = null;
	[SerializeField]
	protected Transform levelSelectionHolder = null;
	[SerializeField]
	protected LevelSelector levelSelectorPrefab = null;
	[SerializeField]
	protected MainMenuHelper mainMenuHelper = null;

	protected virtual void Awake() {
		if (chapterText != null && chapterNumber < LevelManager.Instance.Chapters.Count) {
			chapterText.text = LevelManager.Instance.GetChapterCompletionStatus(chapterNumber) || Debug.isDebugBuild ? LevelManager.Instance.Chapters[chapterNumber].chapterName : "LOCKED";
		}

		if (buttonToUse != null && Debug.isDebugBuild == false) {
			buttonToUse.interactable = LevelManager.Instance.GetChapterCompletionStatus(chapterNumber);
		}
	}

	public virtual void InitializeLevels() {
		if (levelSelectorPrefab == null || levelSelectionHolder == null || isActiveAndEnabled == false) return;

		if (levelSelectionHolder.childCount > 0) {
			for (int i = levelSelectionHolder.childCount - 1; i >= 0; i--) {
				Destroy(levelSelectionHolder.GetChild(i).gameObject);
			}
		}

		List<string> levelNames = LevelManager.Instance.GetChapterLevels(chapterNumber);
		// Cleanup the levelNames list, to make sure we do not accidentally create a button for a unloadable level
		for (int i = levelNames.Count - 1; i >= 0; i--) {
			if (levelNames[i].Length > 0) continue;

			levelNames.RemoveAt(i);
		}

		for (int i = 0; i < levelNames.Count; i++) {
			LevelSelector selector = Instantiate(levelSelectorPrefab, levelSelectionHolder);
			bool isUnlocked = true;
			bool isCompleted = LevelManager.Instance.GetLevelCompletionStatus(levelNames[i]);

			if (i > 0) {
				if (LevelManager.Instance.GetLevelCompletionStatus(levelNames[i - 1]) == false) {
					isUnlocked = false;
				}
			}

			selector.Initialize((i + 1).ToString(), levelNames[i], isUnlocked, isCompleted, mainMenuHelper);
		}
	}
}
