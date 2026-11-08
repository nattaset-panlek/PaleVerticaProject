using System.Collections.Generic;
using System.Linq;
using CoreSystem.Data;
using CoreSystem.Dialogue;
using UnityEditor;
using UnityEngine;

public partial class GameDataWorkbench : EditorWindow
{
    #region [Variables/Fields]
    private const string LeaveLabel = "Leave (quest does not finish)";
    private string _dialogueJsonPath = "Assets/Resources/Data/Dialogue.json";
    private int _dialogueSelectedIndex = -1;
    private DialogueSession _dialoguePreview;
    private DialogueModel _dialoguePreviewSource;
    private bool _dialoguePreviewEnded;
    private readonly HashSet<int> _collapsedNodes = new HashSet<int>();
    private const float NodeSpacing = 12f;
    private const float NodeHeaderHeight = 22f;
    private const float AnswerIndent = 18f;
    private static readonly Color NodeHeaderColor = new Color(0.17f, 0.17f, 0.17f);
    #endregion

    #region [Private Methods]
    private void DrawDialogueEditor()
    {
        DrawEditor(_dialogueData.Dialogues, _dialogueSelectedIndex, idx => _dialogueSelectedIndex = idx, DrawDialogueFields, DescribeDialogue, CreateDialogue);
    }

    private void DrawDialogueFields(DialogueModel entry)
    {
        if (entry.Nodes == null) entry.Nodes = new List<DialogueNode>();
        foreach (string problem in DialogueDataValidator.Validate(entry, _dialogueData.Dialogues))
        {
            EditorGUILayout.HelpBox(problem, MessageType.Warning);
        }

        if (_dialoguePreviewSource != entry) _collapsedNodes.Clear();

        BeginInspectorColumns();
        if (BeginGroup("Nodes", ThemeAccent, entry.Nodes.Count.ToString())) DrawDialogueNodes(entry);
        EndGroup();

        NextInspectorColumn();
        if (BeginGroup("Dialogue Info", ThemeAccent))
        {
            entry.Id = EditorGUILayout.IntField("ID", entry.Id);
            DrawNodePicker("Start Node", entry, entry.StartNode, false, id => entry.StartNode = id);
        }
        EndGroup();

        if (BeginGroup("Preview", ThemeAccent)) DrawDialoguePreview(entry);
        EndGroup();
        EndInspectorColumns();
    }

    private void DrawDialogueNodes(DialogueModel entry)
    {
        int removeIndex = -1;
        for (int i = 0; i < entry.Nodes.Count; i++)
        {
            DialogueNode node = entry.Nodes[i];
            if (node.Text == null) node.Text = new LocalizedString();
            if (node.Choices == null) node.Choices = new List<DialogueChoice>();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (DrawNodeHeader(entry, node)) removeIndex = i;
            if (!_collapsedNodes.Contains(node.Id)) DrawNodeBody(entry, node);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(NodeSpacing);
        }

        if (removeIndex >= 0)
        {
            entry.Nodes.RemoveAt(removeIndex);
            MarkDirty();
        }

        if (GUILayout.Button("+ Node", GUILayout.Height(26f)))
        {
            entry.Nodes.Add(CreateNode(entry.Nodes.Count > 0 ? entry.Nodes.Max(n => n.Id) + 1 : 0));
            MarkDirty();
        }
    }

    private bool DrawNodeHeader(DialogueModel entry, DialogueNode node)
    {
        Rect bar = EditorGUILayout.BeginHorizontal(GUILayout.Height(NodeHeaderHeight));
        EditorGUI.DrawRect(bar, NodeHeaderColor);

        bool collapsed = _collapsedNodes.Contains(node.Id);
        bool open = EditorGUILayout.Foldout(!collapsed, NodeTitle(entry, node), true, EditorStyles.foldoutHeader);
        if (open == collapsed)
        {
            if (open) _collapsedNodes.Remove(node.Id);
            else _collapsedNodes.Add(node.Id);
        }

        GUILayout.FlexibleSpace();
        bool remove = GUILayout.Button("Remove Node", EditorStyles.miniButton, GUILayout.Width(90f));
        EditorGUILayout.EndHorizontal();
        return remove;
    }

    private static string NodeTitle(DialogueModel entry, DialogueNode node)
    {
        string tag = node.Id == entry.StartNode ? "  START" : string.Empty;
        if (node.Choices.Count == 0) tag += "  END";
        return $"NODE {node.Id}{tag}";
    }

    private void DrawNodeBody(DialogueModel entry, DialogueNode node)
    {
        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("NPC Says (EN)", EditorStyles.miniBoldLabel);
        node.Text.EN = EditorGUILayout.TextArea(node.Text.EN, GUILayout.MinHeight(36f));
        EditorGUILayout.LabelField("NPC Says (TH)", EditorStyles.miniBoldLabel);
        node.Text.TH = EditorGUILayout.TextArea(node.Text.TH, GUILayout.MinHeight(36f));
        EditorGUILayout.Space(6f);
        DrawDialogueChoices(entry, node);
    }

    private void DrawDialogueChoices(DialogueModel entry, DialogueNode node)
    {
        EditorGUILayout.LabelField("PLAYER ANSWERS", EditorStyles.boldLabel);
        if (node.Choices.Count == 0)
        {
            EditorGUILayout.LabelField("No answers: this node ends the conversation with a Continue button.", EditorStyles.wordWrappedMiniLabel);
        }

        int removeIndex = -1;
        for (int i = 0; i < node.Choices.Count; i++)
        {
            if (DrawDialogueChoice(entry, node.Choices[i], i)) removeIndex = i;
        }

        if (removeIndex >= 0)
        {
            node.Choices.RemoveAt(removeIndex);
            MarkDirty();
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(AnswerIndent);
        if (GUILayout.Button("+ Answer"))
        {
            node.Choices.Add(new DialogueChoice { Text = new LocalizedString(), Next = DialogueDataValidator.LeaveNode });
            MarkDirty();
        }
        EditorGUILayout.EndHorizontal();
    }

    private bool DrawDialogueChoice(DialogueModel entry, DialogueChoice choice, int index)
    {
        if (choice.Text == null) choice.Text = new LocalizedString();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(AnswerIndent);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Answer {index + 1}", EditorStyles.miniBoldLabel);
        GUILayout.FlexibleSpace();
        bool remove = GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(60f));
        EditorGUILayout.EndHorizontal();

        choice.Text.EN = EditorGUILayout.TextField("Text (EN)", choice.Text.EN);
        choice.Text.TH = EditorGUILayout.TextField("Text (TH)", choice.Text.TH);
        DrawNodePicker("Goes To", entry, choice.Next, true, id => choice.Next = id);

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        return remove;
    }

    private void DrawNodePicker(string label, DialogueModel entry, int currentId, bool allowLeave, System.Action<int> onSelected)
    {
        DialogueNode current = entry.Nodes.Find(n => n.Id == currentId);
        string text = currentId == DialogueDataValidator.LeaveNode && allowLeave ? LeaveLabel : current != null ? NodeLabel(current) : $"Missing {currentId}";

        DrawPickerButton(label, text, menu =>
        {
            if (allowLeave)
            {
                menu.AddItem(new GUIContent(LeaveLabel), currentId == DialogueDataValidator.LeaveNode, () => Pick(onSelected, DialogueDataValidator.LeaveNode));
            }
            foreach (DialogueNode node in entry.Nodes.OrderBy(n => n.Id))
            {
                int id = node.Id;
                menu.AddItem(new GUIContent(NodeLabel(node)), id == currentId, () => Pick(onSelected, id));
            }
        });
    }

    private static string NodeLabel(DialogueNode node)
    {
        string line = node.Text != null && node.Text.EN != null ? node.Text.EN.Replace('/', ' ') : string.Empty;
        if (line.Length > 40) line = line.Substring(0, 40) + "...";
        return $"[{node.Id}] {line}";
    }

    private void DrawDialoguePreview(DialogueModel entry)
    {
        if (_dialoguePreviewSource != entry || _dialoguePreview == null) RestartDialoguePreview(entry);

        if (_dialoguePreview.IsFinished)
        {
            EditorGUILayout.LabelField(_dialoguePreviewEnded ? "Reached the end: a quest objective would finish." : "Left early: a quest objective would not finish.", EditorStyles.wordWrappedLabel);
        }
        else
        {
            DrawPreviewNode(_dialoguePreview.Current);
        }

        if (GUILayout.Button("Restart Preview")) RestartDialoguePreview(entry);
    }

    private void DrawPreviewNode(DialogueNode node)
    {
        EditorGUILayout.LabelField(node.Text != null ? node.Text.EN : string.Empty, EditorStyles.wordWrappedLabel);

        if (node.Choices == null || node.Choices.Count == 0)
        {
            if (GUILayout.Button("Continue"))
            {
                _dialoguePreviewEnded = true;
                _dialoguePreview.End();
            }
            return;
        }

        for (int i = 0; i < node.Choices.Count; i++)
        {
            string label = node.Choices[i].Text != null ? node.Choices[i].Text.EN : string.Empty;
            if (GUILayout.Button(label)) _dialoguePreview.Choose(i);
        }
    }

    private void RestartDialoguePreview(DialogueModel entry)
    {
        _dialoguePreviewSource = entry;
        _dialoguePreview = new DialogueSession(entry);
        _dialoguePreviewEnded = false;
    }

    private EntryView DescribeDialogue(DialogueModel entry)
    {
        int problems = DialogueDataValidator.Validate(entry, _dialogueData.Dialogues).Count;
        DialogueNode first = entry.Nodes?.Find(n => n.Id == entry.StartNode);
        return new EntryView
        {
            Id = entry.Id.ToString(),
            Name = first?.Text != null ? first.Text.EN : string.Empty,
            Meta = problems > 0 ? $"{problems} issue(s)" : string.Empty,
            MetaColor = ThemeMuted,
            Accent = ThemeAccent,
            Subtitle = $"{entry.Nodes?.Count ?? 0} node(s)"
        };
    }

    private DialogueModel CreateDialogue()
    {
        return new DialogueModel
        {
            Id = _dialogueData.Dialogues.Count > 0 ? _dialogueData.Dialogues.Max(d => d.Id) + 1 : 1,
            StartNode = 0,
            Nodes = new List<DialogueNode> { CreateNode(0) }
        };
    }

    private static DialogueNode CreateNode(int id)
    {
        return new DialogueNode { Id = id, Text = new LocalizedString { EN = "New line", TH = "" }, Choices = new List<DialogueChoice>() };
    }

    private void CreateDialogueAndOpen(System.Action<int> onCreated)
    {
        DialogueModel dialogue = CreateDialogue();
        _dialogueData.Dialogues.Add(dialogue);
        onCreated(dialogue.Id);
        _dialogueSelectedIndex = _dialogueData.Dialogues.Count - 1;
        _currentTab = ToolTab.Dialogues;
        MarkDirty();
    }
    #endregion
}
