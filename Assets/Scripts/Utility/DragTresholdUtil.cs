using UnityEngine;
using UnityEngine.EventSystems;

public class DragTresholdUtil : MonoBehaviour {
	private void Start() {
		int defaultValue = EventSystem.current.pixelDragThreshold;

		EventSystem.current.pixelDragThreshold = Mathf.Max(defaultValue, (int)(defaultValue * Screen.dpi / 160f));
	}
}
