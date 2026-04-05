#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DiamondSweepPlayer))]
public class DiamondSweepPlayerEditor : Editor
{
    private SerializedProperty _playOnEnable;
    private SerializedProperty _steps;
    private SerializedProperty _onEnd;

    private void OnEnable()
    {
        _playOnEnable = serializedObject.FindProperty("PlayOnEnable");
        _steps        = serializedObject.FindProperty("Steps");
        _onEnd        = serializedObject.FindProperty("OnSequenceEnd");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_playOnEnable);
        EditorGUILayout.Space(6);

        EditorGUILayout.LabelField("Steps", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        for (int i = 0; i < _steps.arraySize; i++)
        {
            SerializedProperty step = _steps.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Step {i + 1}", EditorStyles.miniBoldLabel);

            EditorGUILayout.PropertyField(step.FindPropertyRelative("TargetProgress"));
            EditorGUILayout.PropertyField(step.FindPropertyRelative("Duration"));

            // Colors
            SerializedProperty overrideColors = step.FindPropertyRelative("OverrideColors");
            EditorGUILayout.PropertyField(overrideColors, new GUIContent("Override Colors"));
            if (overrideColors.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(step.FindPropertyRelative("Color1"), new GUIContent("Color 1"));
                EditorGUILayout.PropertyField(step.FindPropertyRelative("Color2"), new GUIContent("Color 2"));
                EditorGUILayout.PropertyField(step.FindPropertyRelative("Color3"), new GUIContent("Color 3"));
                EditorGUI.indentLevel--;
            }

            // Direction
            SerializedProperty overrideDir = step.FindPropertyRelative("OverrideDirection");
            EditorGUILayout.PropertyField(overrideDir, new GUIContent("Override Direction"));
            if (overrideDir.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(step.FindPropertyRelative("Direction"), new GUIContent("Direction"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(
                step.FindPropertyRelative("OnStepBegin"),
                new GUIContent("On Step Begin"),
                true
            );
            EditorGUILayout.PropertyField(
                step.FindPropertyRelative("OnStepEnd"),
                new GUIContent("On Step End"),
                true
            );

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                _steps.DeleteArrayElementAtIndex(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        EditorGUI.indentLevel--;

        EditorGUILayout.Space(4);
        if (GUILayout.Button("+ Add Step"))
        {
            _steps.InsertArrayElementAtIndex(_steps.arraySize);
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.PropertyField(_onEnd);

        // Preview in Play Mode
        if (Application.isPlaying)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            if (GUILayout.Button("▶ Play Sequence"))
            {
                ((DiamondSweepPlayer)target).Play();
            }
            if (GUILayout.Button("■ Stop"))
            {
                ((DiamondSweepPlayer)target).Stop();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
