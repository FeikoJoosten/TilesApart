using UnityEngine;

public class EnterLastPlayedLevel : MonoBehaviour {
	private string lastPlayedLevelName = null;

	public void OpenLastPlayedLevel() {
		lastPlayedLevelName = LevelManager.Instance.GetLevelName(LevelManager.Instance.LastPlayedLevel.x, LevelManager.Instance.LastPlayedLevel.y);

		if (lastPlayedLevelName.Length == 0) {
			lastPlayedLevelName = LevelManager.Instance.GetLevelName(0, 0);
		}

		LevelManager.Instance.LoadGameLevel(lastPlayedLevelName, true, true, true, true);
	}
}
