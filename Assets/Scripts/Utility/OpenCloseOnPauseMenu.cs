using UnityEngine;

public class OpenCloseOnPauseMenu : MonoBehaviour {

    private void Start() {
        GameMenus.OnPauseMenuOpened += OnPauseMenuOpened;
        GameMenus.OnPauseMenuClosed += OnPauseMenuClosed;
    }

    private void OnDestroy() {
        GameMenus.OnPauseMenuOpened -= OnPauseMenuOpened;
        GameMenus.OnPauseMenuClosed -= OnPauseMenuClosed;
    }

    private void OnPauseMenuOpened() {
        gameObject.SetActive(false);
    }

    private void OnPauseMenuClosed() {
        gameObject.SetActive(true);
    }
}
