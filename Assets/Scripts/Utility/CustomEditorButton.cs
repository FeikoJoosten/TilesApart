using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif
using Object = UnityEngine.Object;

[System.AttributeUsage(System.AttributeTargets.Method)]
public class EditorButton : PropertyAttribute {
	public object[] parameters;

	public EditorButton(object[] parameters = null) {
		this.parameters = parameters;
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(MonoBehaviour), true)]
[CanEditMultipleObjects]
public class EditorButtonEditor : Editor {
	public override void OnInspectorGUI() {
		if (!target)
			return;

		base.OnInspectorGUI();
		Dictionary<string, List<KeyValuePair<MethodInfo, MonoBehaviour>>> savedMethods = new Dictionary<string, List<KeyValuePair<MethodInfo, MonoBehaviour>>>();

		foreach (Object currentTarget in targets) {
			MonoBehaviour mono = currentTarget as MonoBehaviour;

			if (mono == null) continue;

			IEnumerable<MemberInfo> methods = mono.GetType()
				.GetMembers(BindingFlags.Instance | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
							BindingFlags.NonPublic)
				.Where(o => Attribute.IsDefined(o, typeof(EditorButton)));


			foreach (MemberInfo memberInfo in methods) {
				MethodInfo method = memberInfo as MethodInfo;
				if (method == null) continue;

				if (savedMethods.ContainsKey(memberInfo.Name)) {
					savedMethods[memberInfo.Name].Add(new KeyValuePair<MethodInfo, MonoBehaviour>(method, mono));
					continue;
				}

				savedMethods.Add(memberInfo.Name, new List<KeyValuePair<MethodInfo, MonoBehaviour>>());
				savedMethods[memberInfo.Name].Add(new KeyValuePair<MethodInfo, MonoBehaviour>(method, mono));
			}
		}

		foreach (KeyValuePair<string, List<KeyValuePair<MethodInfo, MonoBehaviour>>> method in savedMethods) {
			if (!GUILayout.Button(method.Key)) continue;

			foreach (KeyValuePair<MethodInfo, MonoBehaviour> pair in method.Value) {

				EditorButton[] editorButton = pair.Key.GetCustomAttributes(typeof(EditorButton), true) as EditorButton[];
				if (editorButton == null) continue;
				if (editorButton.Length == 0) continue;

				pair.Key.Invoke(pair.Value, editorButton[0].parameters);
			}
		}

		savedMethods.Clear();
	}
}
#endif