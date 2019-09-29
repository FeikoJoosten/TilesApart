using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerPrefsResetter : MonoBehaviour {
    [MenuItem("PlayerPrefs/Reset player prefs")]
    private static void ResetPlayerPrefs() {
        PlayerPrefs.DeleteAll();
    }
}
