using UnityEngine;

public class UrlOpener : MonoBehaviour {
    public void OpenURL(string urlToOpen) {
        if (urlToOpen.Length == 0) return;

        Application.OpenURL(urlToOpen);
    }
}
