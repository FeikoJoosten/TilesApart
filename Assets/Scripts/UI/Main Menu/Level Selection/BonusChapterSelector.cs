﻿using System.Collections.Generic;
using UnityEngine;

public class BonusChapterSelector : ChapterSelector {
	[SerializeField]
	private string textToUse = "BONUS";

	// Use this for initialization
	protected override void Awake () {
		if (chapterText != null) {
			chapterText.text = LevelManager.Instance.GetChapterCompletionStatus(LevelManager.Instance.Chapters.Count) || Debug.isDebugBuild ? textToUse : "LOCKED";
		}

		if (buttonToUse != null && Debug.isDebugBuild == false) {
			buttonToUse.interactable = LevelManager.Instance.GetChapterCompletionStatus(LevelManager.Instance.Chapters.Count);
		}
	}

	public override void InitializeLevels() {
		if (levelSelectorPrefab == null || levelSelectionHolder == null || isActiveAndEnabled == false) return;

		if (levelSelectionHolder.childCount > 0) {
			for (int i = levelSelectionHolder.childCount - 1; i >= 0; i--) {
				Destroy(levelSelectionHolder.GetChild(i).gameObject);
			}
		}

		List<string> levelNames = LevelManager.Instance.GetBonusLevels();
		// Cleanup the levelNames list, to make sure we do not accidentally create a button for a unloadable level
		for (int i = levelNames.Count - 1; i >= 0; i--) {
			if (levelNames[i].Length > 0) continue;

			levelNames.RemoveAt(i);
		}

		for (int i = 0; i < levelNames.Count; i++) {
			LevelSelector selector = Instantiate(levelSelectorPrefab, levelSelectionHolder);
			bool isCompleted = LevelManager.Instance.GetLevelCompletionStatus(levelNames[i]);

			selector.Initialize((i + 1).ToString(), levelNames[i], true, isCompleted, mainMenuHelper);
		}
	}
}
