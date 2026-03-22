using UnityEngine;
using UnityEditor;
using CoreSystem.Controllers;
using CoreSystem.Agent;
using CoreSystem.Entity;

namespace CoreSystem.EditorScripts
{
    [CustomEditor(typeof(AgentCore))]
    public class AgentCoreEditor : UnityEditor.Editor
    {
        private AgentCore _agent;

        private void OnEnable()
        {
            _agent = (AgentCore)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Custom Title
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                normal = { textColor = new Color(0.3f, 0.8f, 1f) }
            };
            EditorGUILayout.Space(4);
            GUILayout.Label($"Agent Core  [{_agent.CurrentState}]", titleStyle);
            EditorGUILayout.Space(4);

            // Full default inspector (without script field)
            DrawPropertiesExcluding(serializedObject, "m_Script");

            EditorGUILayout.Space(10);

            // ── Tools Section ──────────────────────────────
            EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            if (GUILayout.Button("Auto-Find References  (Model + Animator)", GUILayout.Height(28)))
                AutoFindReferences();

            if (GUILayout.Button("Open Agent Monitor Window", GUILayout.Height(28)))
                AgentCoreWindow.OpenWindow();

            if (Application.isPlaying)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox($"Play Mode  |  State: {_agent.CurrentState}  |  Target: {(_agent.Target != null ? _agent.Target.name : "None")}", MessageType.Info);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Force Idle", GUILayout.Height(28)))
                    _agent.Editor_ForceReset();
                if (GUILayout.Button("Force Think", GUILayout.Height(28)))
                    _agent.Editor_ForceThink();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Play mode controls available during runtime.", MessageType.None);
            }

            EditorGUILayout.EndVertical();

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_agent);
            }
        }

        private void AutoFindReferences()
        {
            Undo.RecordObject(_agent, "Auto-Find Agent References");

            if (_agent.ModelTransform == null)
            {
                var model = _agent.transform.Find("Model");
                if (model != null) { _agent.ModelTransform = model; Debug.Log($"[AgentCore] Assigned ModelTransform → {_agent.name}"); }
            }

            if (_agent.Anim == null)
            {
                var anim = _agent.GetComponentInChildren<Animator>();
                if (anim != null) { _agent.Anim = anim; Debug.Log($"[AgentCore] Assigned Animator → {_agent.name}"); }
            }

            if (_agent.GetComponent<Character>() == null)
                Debug.LogWarning($"[AgentCore] Missing Character component on {_agent.name}!");

            EditorUtility.SetDirty(_agent);
        }
    }
}
