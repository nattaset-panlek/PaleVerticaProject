using UnityEngine;
using UnityEditor;
using CoreSystem.Skills;

namespace CoreSystem.Editor
{
    [CustomEditor(typeof(ActiveSkill))]
    public class ActiveSkillEditor : UnityEditor.Editor
    {
        private ActiveSkill skill;

        private void OnEnable()
        {
            skill = (ActiveSkill)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox("Active Skill Configuration", MessageType.Info);
            
            // Draw default Inspector (handles SubclassSelector automatically)
            DrawDefaultInspector();

            EditorGUILayout.Space();
            GUILayout.Label("Preview & Utilities", EditorStyles.boldLabel);
            
            if (skill.BaseTargeting != null)
            {
                if (GUILayout.Button("Force Update Hitbox Preview", GUILayout.Height(30)))
                {
                    SceneView.RepaintAll();
                }
                EditorGUILayout.HelpBox("Select this asset to see Hitbox preview in the Scene View.\nPosition is at Origin (0,0) or at the currently Selected GameObject in the Hierarchy.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Select a BaseTargeting logic above to enable Hitbox preview.", MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (skill == null || skill.BaseTargeting == null) return;

            Vector3 origin = Vector3.zero;
            Vector2 direction = Vector2.up;

            // If a GameObject is selected in the hierarchy, use its position and rotation
            if (Selection.activeGameObject != null && !AssetDatabase.Contains(Selection.activeGameObject))
            {
                origin = Selection.activeGameObject.transform.position;
                direction = Selection.activeGameObject.transform.up;
            }

            // Create mock payload for preview
            SkillPayload payload = new SkillPayload 
            { 
                OriginPosition = origin, 
                AOERadius = skill.BaseAOERadius 
            };

            // Call TargetingLogic's DrawGizmos (which actually uses UnityEditor.Handles internally)
            skill.BaseTargeting.DrawGizmos(origin, direction, payload);

            GUIStyle labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.white;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.fontStyle = FontStyle.Bold;

            Handles.Label(origin + new Vector3(0, skill.BaseAOERadius + 0.5f, 0), $"{skill.Name} Hitbox Preview", labelStyle);

            // Force repaint to make interactions smooth during changes
            if (Event.current.type == EventType.Repaint)
            {
                SceneView.RepaintAll();
            }
        }
    }
}
