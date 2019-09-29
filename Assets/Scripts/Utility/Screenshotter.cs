using UnityEngine;

public class Screenshotter : MonoBehaviour {
    private int screenshotcount = 0;
    [EditorButton]
    private void CaptureScreenshot() {
        ScreenCapture.CaptureScreenshot(Application.dataPath + "/Screenshots/" + screenshotcount + ".png");
        screenshotcount++;
    }
}