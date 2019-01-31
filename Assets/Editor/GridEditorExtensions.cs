using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

static class GridEditorExtensions {

	[MenuItem("Grid Editor/Update All Tile Mesh Information")]
	private static void UpdateAllTileMeshInformation() {
		CallFunctionOnGridManager(new[] { "UpdateGridMeshes" });
	}

	[MenuItem("Grid Editor/Update All player models")]
	private static void UpdateAllPlayerModels() {
		CallFunctionOnGridManager(new[] { "UpdatePlayerModel" });
	}

	[MenuItem("Grid Editor/Update everything")]
	private static void UpdateEverything() {
		CallFunctionOnGridManager(new [] { "UpdateGridMeshes", "UpdatePlayerModel" });
	}

	private static bool IsValidScene(Scene sceneToCheck, out GridManager gridManager) {
		gridManager = null;

		if (sceneToCheck.isLoaded == false) {
			Debug.Log("Please load this scene first");
			return false;
		}

		GameObject[] rootObjects = sceneToCheck.GetRootGameObjects();

		for (int j = 0; j < rootObjects.Length; j++) {
			if (rootObjects[j].GetComponent<GridManager>()) {
				gridManager = rootObjects[j].GetComponent<GridManager>();
				return true;
			}
		}
		return false;
	}

	private static void CallFunctionOnGridManager(string[] methodsToCall, object[] parameters = null) {
		Scene originalScene = SceneManager.GetActiveScene();
		string originalScenePath = originalScene.path;

		string[] sceneGUIDs = AssetDatabase.FindAssets("t:scene", new[] { "Assets/Scenes" });

		for (int i = 0; i < sceneGUIDs.Length; i++) {
			Scene sceneToCheck = EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(sceneGUIDs[i]), OpenSceneMode.Single);
			GridManager gridManagerToUse;
			if (IsValidScene(sceneToCheck, out gridManagerToUse) == false) continue;
			if (gridManagerToUse == null) continue;

			for (int j = 0; j < methodsToCall.Length; j++) {
				MethodInfo methodInfo = gridManagerToUse.GetType().GetMethod(methodsToCall[j], BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
				if (methodInfo != null) {
					methodInfo.Invoke(gridManagerToUse, parameters);
				}
			}
			EditorSceneManager.SaveScene(sceneToCheck);
		}
		EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
		Debug.Log("Done updating all scenes");
	}
}
