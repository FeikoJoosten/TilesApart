using UnityEngine;

public class UrlOpener : MonoBehaviour {
    public void OpenURL(string urlToOpen) {
        if (string.IsNullOrEmpty(urlToOpen)) return;

        Application.OpenURL(urlToOpen);
    }
}
