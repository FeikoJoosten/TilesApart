using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class AnalyticsManager : Singleton<AnalyticsManager> {
	[SerializeField]
	private bool outputDebugMessages = false;

	private void OnApplicationPause(bool isPaused) {
		if (isPaused == true) {
			RecordCustomEvent("OnAppliationFocusLost", new Dictionary<string, object> {
				{ "TotalPlayTime", Time.realtimeSinceStartup },
				{ "TotalLevelsCompletedThisSession", LevelManager.Instance.SessionPlayedLevelCount }
			});
		}
	}

	private void OnApplicationQuit() {
		RecordCustomEvent("OnAppliationQuit", new Dictionary<string, object> {
			{ "TotalPlayTime", Time.realtimeSinceStartup },
			{ "TotalLevelsCompletedThisSession", LevelManager.Instance.SessionPlayedLevelCount }
		});
	}

	public void RecordCustomEvent(string eventName, Dictionary<string, object> eventInformation) {
		Analytics.CustomEvent(eventName, eventInformation);

		if (outputDebugMessages == false) return;

		string customEvent = "Event name: " + eventName;

		foreach (KeyValuePair<string, object> eventInfo in eventInformation) {
			customEvent += "\n    " + eventInfo.Key + ": " + eventInfo.Value;
		}

		Debug.Log(customEvent);
	}
}
