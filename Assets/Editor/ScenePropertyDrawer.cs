using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
[CustomPropertyDrawer(typeof(SceneAttribute))]
public class ScenePropertyDrawer : PropertyDrawer {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		SceneAttribute sceneAttribute = (SceneAttribute)attribute;

		if (property.propertyType == SerializedPropertyType.String) {
			var sceneObject = GetSceneObject(property.stringValue, sceneAttribute);
			var scene = EditorGUI.ObjectField(position, label, sceneObject, typeof(SceneAsset), true);

			if (scene == null) {
				property.stringValue = "";
			}
			else if (scene.name != property.stringValue) {
				var sceneObj = GetSceneObject(scene.name, sceneAttribute);

				if (sceneObj == null) {
					Debug.LogWarningFormat("The scene {0} cannot be used. To use this scene add it to the build settings for the project", scene.name);
				}
				else {
					property.stringValue = scene.name;
				}
			}
		}
		else {
			EditorGUI.LabelField(position, label.text, "[Scene] attribute can only be used with strings");
		}
	}

	protected SceneAsset GetSceneObject(string sceneObjectName, SceneAttribute sceneAttribute) {
		if (string.IsNullOrEmpty(sceneObjectName)) {
			return null;
		}

		foreach (var editorScene in EditorBuildSettings.scenes) {
			if (editorScene.path.IndexOf(sceneObjectName) != -1) {
				if (Array.IndexOf(sceneAttribute.invalidScenes, sceneObjectName) != -1) {
					Debug.LogWarningFormat("The scene {0} may not be used.", sceneObjectName);
					return null;
				}

				return AssetDatabase.LoadAssetAtPath(editorScene.path, typeof(SceneAsset)) as SceneAsset;
			}
		}

		Debug.LogWarningFormat("The scene {0} cannot be used. To use this scene add it to the build settings for the project", sceneObjectName);
		return null;
	}
}