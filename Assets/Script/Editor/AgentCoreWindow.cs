using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CoreSystem.Controllers;
using CoreSystem.Agent;
using CoreSystem.Attributes;

namespace CoreSystem.EditorScripts
{
    /// <summary>
    /// EditorWindow for monitoring all AgentCore instances in the scene.
    /// Open via: Window > Agent Monitor
    /// </summary>
    public class AgentCoreWindow : EditorWindow
    {
        private Vector2 _scrollPos;
        private string _filterName = "";
        private int _filterGroup = -1;

        // Tracking expanded foldouts and attribute search strings per agent instance ID
        private Dictionary<int, bool> _expandedState = new Dictionary<int, bool>();
        private Dictionary<int, string> _searchStrings = new Dictionary<int, string>();

        [MenuItem("Window/Agent Monitor")]
        public static void OpenWindow()
        {
            var win = GetWindow<AgentCoreWindow>("Agent Monitor");
            win.minSize = new Vector2(520, 300);
        }

        private void OnEnable()
        {
            EditorApplication.update += Repaint;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawHeader();
            DrawAgentList();
            DrawFooter();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Agent Monitor", EditorStyles.boldLabel, GUILayout.Width(120));
            GUILayout.Space(10);

            // Filter by name
            GUILayout.Label("Name:", GUILayout.Width(40));
            _filterName = EditorGUILayout.TextField(_filterName, GUILayout.Width(120));

            // Filter by group
            GUILayout.Label("Group:", GUILayout.Width(42));
            _filterGroup = EditorGUILayout.IntField(_filterGroup, GUILayout.Width(40));
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(40)))
            {
                _filterName = "";
                _filterGroup = -1;
            }

            GUILayout.FlexibleSpace();

            // Global Batch Actions
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Reset All", EditorStyles.toolbarButton, GUILayout.Width(70)))
                {
                    foreach (var a in FindAll()) a.Editor_ForceReset();
                }
                if (GUILayout.Button("Think All", EditorStyles.toolbarButton, GUILayout.Width(65)))
                {
                    foreach (var a in FindAll()) a.Editor_ForceThink();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            HeaderLabel("Name",    130);
            HeaderLabel("Group",   50);
            HeaderLabel("HP",      80);
            HeaderLabel("State",   70);
            HeaderLabel("Behavior",80);
            HeaderLabel("Combat",  70);
            HeaderLabel("Target",  100);
            GUILayout.FlexibleSpace();
            HeaderLabel("Actions", 120);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAgentList()
        {
            var agents = FindAll();
            var filtered = Filter(agents);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            bool evenRow = false;
            foreach (var agent in filtered)
            {
                // Row background color
                Color rowColor = evenRow ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.18f, 0.18f, 0.18f);
                DrawRow(agent, rowColor);
                evenRow = !evenRow;
            }

            if (filtered.Count == 0)
            {
                EditorGUILayout.HelpBox("No AgentCore instances found in the scene.", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawRow(AgentCore agent, Color bgColor)
        {
            int id = agent.GetInstanceID();
            if (!_expandedState.ContainsKey(id)) _expandedState[id] = false;
            if (!_searchStrings.ContainsKey(id)) _searchStrings[id] = "";

            EditorGUILayout.BeginVertical();

            Rect rowRect = EditorGUILayout.BeginHorizontal();
            EditorGUI.DrawRect(rowRect, bgColor);

            // Foldout toggle at the very left
            _expandedState[id] = GUILayout.Toggle(_expandedState[id], _expandedState[id] ? "▼" : "▶", EditorStyles.label, GUILayout.Width(20));

            GUI.color = GetStateColor(agent.CurrentState);

            RowLabel(agent.AgentName,                           106); // adjusted width for toggle
            RowLabel(agent.GroupID == 0 ? "Solo" : agent.GroupID.ToString(), 50);

            string hpText = "-";
            if (agent.Character != null && agent.Character.attributeSystem != null)
            {
                var hpAttr = agent.Character.attributeSystem.GetAttribute(AttributeType.HP);
                var maxHpAttr = agent.Character.attributeSystem.GetAttribute(AttributeType.MaxHP);
                if (hpAttr != null && maxHpAttr != null)
                {
                    hpText = $"{Mathf.CeilToInt(hpAttr.CurrentValue)} / {Mathf.CeilToInt(maxHpAttr.CurrentValue)}";
                }
            }
            RowLabel(hpText, 80);

            RowLabel(agent.CurrentState.ToString(),             70);

            GUI.color = Color.white;

            RowLabel(agent.Behavior.ToString(),                 80);
            RowLabel(agent.Combat.ToString(),                   70);
            RowLabel(agent.Target != null ? agent.Target.name : "—", 100);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Select", GUILayout.Width(50)))
            {
                Selection.activeGameObject = agent.gameObject;
                SceneView.lastActiveSceneView?.FrameSelected();
            }

            if (GUILayout.Button("Ping", GUILayout.Width(40)))
            {
                EditorGUIUtility.PingObject(agent.gameObject);
            }

            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("↺", GUILayout.Width(24))) agent.Editor_ForceReset();
            if (GUILayout.Button("⚡", GUILayout.Width(24))) agent.Editor_ForceThink();
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            // ─── Expanded Attributes View ──────────────────────────────
            if (_expandedState[id] && agent.Character != null && agent.Character.attributeSystem != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Search field for attributes
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("🔍 Search Attribute :", GUILayout.Width(130));
                _searchStrings[id] = EditorGUILayout.TextField(_searchStrings[id]);
                if (GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.Width(50)))
                    _searchStrings[id] = "";
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(2);

                string searchStr = _searchStrings[id].ToLower();
                var attrSystem = agent.Character.attributeSystem;

                if (string.IsNullOrEmpty(searchStr))
                {
                    // Show Basic Attributes
                    DrawAttributeLine(AttributeType.HP, attrSystem);
                    DrawAttributeLine(AttributeType.MaxHP, attrSystem);
                    DrawAttributeLine(AttributeType.ATK, attrSystem);
                    DrawAttributeLine(AttributeType.MATK, attrSystem);
                    DrawAttributeLine(AttributeType.Defense, attrSystem);
                    DrawAttributeLine(AttributeType.MoveSpeed, attrSystem);
                    DrawAttributeLine(AttributeType.BaseLevel, attrSystem);
                }
                else
                {
                    // Show Filtered Attributes
                    int foundCount = 0;
                    foreach (AttributeType type in System.Enum.GetValues(typeof(AttributeType)))
                    {
                        if (type.ToString().ToLower().Contains(searchStr))
                        {
                            DrawAttributeLine(type, attrSystem);
                            foundCount++;
                        }
                    }
                    if (foundCount == 0)
                        EditorGUILayout.HelpBox("No attributes match your search.", MessageType.None);
                }

                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawAttributeLine(AttributeType type, AttributeSystem attributeSystem)
        {
            var attr = attributeSystem.GetAttribute(type);
            string valueStr = attr != null ? attr.CurrentValue.ToString("F1") : "-";
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(type.ToString(), GUILayout.Width(150));
            GUILayout.Label(valueStr, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label($"Total Agents: {FindAll().Count}  |  Filtered: {Filter(FindAll()).Count}", EditorStyles.miniLabel);
            if (!Application.isPlaying)
                GUILayout.Label("Enter Play Mode to use real-time controls.", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        // ─── Helpers ─────────────────────────────────────────────

        private List<AgentCore> FindAll()
        {
            return new List<AgentCore>(FindObjectsOfType<AgentCore>());
        }

        private List<AgentCore> Filter(List<AgentCore> agents)
        {
            var result = new List<AgentCore>();
            foreach (var a in agents)
            {
                if (!string.IsNullOrEmpty(_filterName) && !a.AgentName.ToLower().Contains(_filterName.ToLower())) continue;
                if (_filterGroup >= 0 && a.GroupID != _filterGroup) continue;
                result.Add(a);
            }
            return result;
        }

        private Color GetStateColor(AIState state)
        {
            switch (state)
            {
                case AIState.Idle:   return new Color(0.7f, 0.7f, 0.7f);
                case AIState.Roam:   return new Color(0.4f, 0.8f, 1.0f);
                case AIState.Chase:  return new Color(1.0f, 0.8f, 0.2f);
                case AIState.Attack: return new Color(1.0f, 0.3f, 0.3f);
                case AIState.Flee:   return new Color(1.0f, 0.6f, 0.1f);
                case AIState.Dead:   return new Color(0.5f, 0.5f, 0.5f);
                default:             return Color.white;
            }
        }

        private void HeaderLabel(string text, int width)
        {
            GUILayout.Label(text, EditorStyles.boldLabel, GUILayout.Width(width));
        }

        private void RowLabel(string text, int width)
        {
            GUILayout.Label(text, GUILayout.Width(width));
        }
    }
}
