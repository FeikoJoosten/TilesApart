﻿using UnityEngine;
using UnityEngine.UI;

public class LevelDisplayer : MonoBehaviour {
	[SerializeField]
	private Text levelText = null;
	[SerializeField]
	private string displayFormat = "Chapter {0} level {1}";

	private void Awake() {
		PreLoader.OnLevelTransition += OnLevelTransition;

		OnLevelTransition();
	}

	private void OnDestroy() {
		PreLoader.OnLevelTransition -= OnLevelTransition;
	}

	private void OnLevelTransition () {
		if (levelText == null) return;
		
		levelText.text = string.Format(displayFormat, LevelManager.Instance.CurrentLevel.y + 1, LevelManager.Instance.GetCurrentChapterLevelCount());
	}
}
