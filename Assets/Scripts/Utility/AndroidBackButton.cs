
using UnityEngine;

public class AndroidBackButton : MonoBehaviour {
#if UNITY_ANDROID
    public static event System.Action AndroidBackButtonPressed = delegate { };

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            AndroidBackButtonPressed();
        }
    }
#endif
}