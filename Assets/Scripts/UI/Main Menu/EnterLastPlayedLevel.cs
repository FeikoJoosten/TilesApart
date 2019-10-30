using UnityEngine;

public class EnterLastPlayedLevel : MonoBehaviour {
    private string lastPlayedLevelName = null;

    public void OpenLastPlayedLevel() {
        lastPlayedLevelName = LevelManager.Instance.GetLevelName(LevelManager.Instance.LastPlayedLevel.x, LevelManager.Instance.LastPlayedLevel.y);

        if (string.IsNullOrEmpty(lastPlayedLevelName)) {
            lastPlayedLevelName = LevelManager.Instance.GetLevelName(0, 0);
        }

        LevelManager.Instance.LoadGameLevel(lastPlayedLevelName, true, true, true, true);
    }
}
