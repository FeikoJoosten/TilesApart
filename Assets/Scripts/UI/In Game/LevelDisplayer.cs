using UnityEngine;
using UnityEngine.UI;

public class LevelDisplayer : MonoBehaviour {
    [SerializeField]
    private Text levelText = null;
    [SerializeField]
    private string displayFormat = "Chapter {0} level {1}";

    private void LateUpdate() {
        if (levelText == null) return;

        levelText.text = string.Format(displayFormat, LevelManager.Instance.CurrentLevel.y + 1, LevelManager.Instance.GetCurrentChapterLevelCount());
    }
}
