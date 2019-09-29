using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor {
    private ReorderableList chapterList;
    private ReorderableList bonusChapterList;

    private void OnEnable() {
        SerializedProperty chapterInfo = serializedObject.FindProperty("chapters");

        chapterList = new ReorderableList(serializedObject, chapterInfo, true, true, true, true);

        chapterList.drawHeaderCallback = rect => {
            EditorGUI.LabelField(rect, "Chapters");
        };

        chapterList.drawElementCallback = (rect, index, active, focused) => {
            Rect originalRect = rect;
            rect.x += 10;
            rect.width = 70;

            EditorGUI.LabelField(rect, "Chapter: " + (index + 1));

            SerializedProperty chapterData = chapterInfo.GetArrayElementAtIndex(index);
            if (chapterData.isExpanded) {
                SerializedProperty chapterName = chapterData.FindPropertyRelative("chapterName");
                if (chapterName != null) {
                    Rect chapterNameRect = rect;
                    chapterNameRect.x += rect.width + 10;
                    chapterNameRect.width = originalRect.width - (rect.width + rect.x + 10);
                    chapterNameRect.height = EditorGUIUtility.singleLineHeight;

                    chapterName.stringValue = EditorGUI.TextField(chapterNameRect, "Chapter name", chapterName.stringValue);
                }

                SerializedProperty chapterArt = chapterData.FindPropertyRelative("chapterArt");
                if (chapterArt != null) {
                    Rect chapterArtRect = rect;
                    chapterArtRect.y += EditorGUIUtility.singleLineHeight * 1.25f;
                    chapterArtRect.height = EditorGUIUtility.singleLineHeight;
                    chapterArtRect.width = originalRect.width - (rect.width) + rect.x - 18;

                    EditorGUI.PropertyField(chapterArtRect, chapterArt, new GUIContent("Chapter Art "));
                }

                SerializedProperty chapterTileData = chapterData.FindPropertyRelative("chapterTileData");
                if (chapterTileData != null) {
                    Rect chapterTileDataRect = rect;
                    chapterTileDataRect.y += EditorGUIUtility.singleLineHeight * 1.25f * 2;
                    chapterTileDataRect.height = EditorGUIUtility.singleLineHeight;
                    chapterTileDataRect.width = originalRect.width - (rect.width) + rect.x - 18;

                    EditorGUI.PropertyField(chapterTileDataRect, chapterTileData, new GUIContent("Chapter Tile Data "));
                }
            }

            rect.height = EditorGUI.GetPropertyHeight(chapterData, GUIContent.none, false);

            SerializedProperty levelList = chapterData.FindPropertyRelative("levels");
            if (levelList != null && chapterData.isExpanded) {
                Rect levelListRect = rect;
                levelListRect.x += 10;
                levelListRect.width = rect.width;
                levelListRect.y += (EditorGUIUtility.singleLineHeight * 1.25f) * 3;

                EditorGUI.LabelField(levelListRect, "Levels");
                EditorGUI.PropertyField(levelListRect, levelList, GUIContent.none, false);

                if (levelList.isArray && levelList.isExpanded) {
                    Rect levelListSizeRect = levelListRect;
                    levelListSizeRect.x = levelListRect.x + levelListRect.width;
                    levelListSizeRect.width = originalRect.width - levelListSizeRect.x;
                    levelList.arraySize = EditorGUI.IntField(levelListSizeRect, levelList.arraySize);

                    for (int i = 0; i < levelList.arraySize; i++) {
                        Rect levelItemRect = levelListSizeRect;
                        levelItemRect.x = levelListRect.x + 10;
                        levelItemRect.y += 20 + (EditorGUIUtility.singleLineHeight * i);
                        levelItemRect.width = originalRect.width - levelItemRect.x;
                        SerializedProperty levelObject = levelList.GetArrayElementAtIndex(i);
                        EditorGUI.PropertyField(levelItemRect, levelObject, new GUIContent("Level: " + (i + 1)));
                    }
                }
            }
            EditorGUI.PropertyField(rect, chapterData, GUIContent.none, false);

        };

        chapterList.elementHeightCallback = (index) => {
            Repaint();

            return EditorGUI.GetPropertyHeight(chapterInfo.GetArrayElementAtIndex(index), true) + 1;
        };
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren)) {
            if (iterator.propertyPath == "chapters") {
                chapterList.DoLayoutList();
                continue;
            }

            using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath)) {
                EditorGUILayout.PropertyField(iterator, true);
            }

            enterChildren = false;
        }

        serializedObject.ApplyModifiedProperties();
    }
}