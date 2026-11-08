using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Dialogue;
using CoreSystem.Quests;
using UnityEditor;
using UnityEngine;

public partial class GameDataWorkbench : EditorWindow
{
    #region [Variables/Fields]
    private string _questJsonPath = "Assets/Resources/Data/Quest.json";
    private QuestDataList _questData;
    private DialogueDataList _dialogueData;
    private int _questSelectedIndex = -1;
    #endregion

    #region [Private Methods]
    private void DrawQuestEditor()
    {
        DrawEditor(_questData.Quests, _questSelectedIndex, idx => _questSelectedIndex = idx, DrawQuestFields, DescribeQuest, CreateQuest);
    }

    private void DrawQuestFields(QuestModel entry)
    {
        EnsureQuestFields(entry);
        DrawQuestProblems(entry);

        BeginInspectorColumns();
        if (BeginGroup("Quest Info", ThemeAccent))
        {
            entry.Id = EditorGUILayout.IntField("ID", entry.Id);
            entry.Name.EN = EditorGUILayout.TextField("Name (EN)", entry.Name.EN);
            entry.Name.TH = EditorGUILayout.TextField("Name (TH)", entry.Name.TH);
            EditorGUILayout.LabelField("Description (EN)");
            entry.Description.EN = EditorGUILayout.TextArea(entry.Description.EN, GUILayout.MinHeight(44f));
            EditorGUILayout.LabelField("Description (TH)");
            entry.Description.TH = EditorGUILayout.TextArea(entry.Description.TH, GUILayout.MinHeight(44f));
            DrawCharacterPicker("Giver NPC", entry.GiverCharacterId, id => entry.GiverCharacterId = id);
            DrawCharacterPicker("Turn-in NPC", entry.TurnInCharacterId, id => entry.TurnInCharacterId = id);
            entry.Sequential = EditorGUILayout.Toggle(new GUIContent("Objectives In Order", "Only the current objective counts and is shown."), entry.Sequential);
            DrawDialoguePicker("Accept Dialogue", entry.AcceptDialogueId, id => entry.AcceptDialogueId = id);
            DrawDialoguePicker("Turn-in Dialogue", entry.TurnInDialogueId, id => entry.TurnInDialogueId = id);
            DrawRequiredQuests(entry);
        }
        EndGroup();

        if (BeginGroup("Reward", ThemeAccent))
        {
            entry.Reward.BaseExp = EditorGUILayout.LongField("Base EXP", entry.Reward.BaseExp);
            entry.Reward.JobExp = EditorGUILayout.LongField("Job EXP", entry.Reward.JobExp);
            DrawRewardItems(entry.Reward);
        }
        EndGroup();

        NextInspectorColumn();
        if (BeginGroup("Objectives", ThemeAccent, entry.Objectives.Count.ToString())) DrawObjectives(entry);
        EndGroup();
        EndInspectorColumns();
    }

    private static void EnsureQuestFields(QuestModel entry)
    {
        if (entry.Name == null) entry.Name = new LocalizedString();
        if (entry.Description == null) entry.Description = new LocalizedString();
        if (entry.Objectives == null) entry.Objectives = new List<QuestObjectiveModel>();
        if (entry.Reward == null) entry.Reward = new QuestRewardModel();
        if (entry.RequiresQuestIds == null) entry.RequiresQuestIds = new List<int>();
        if (entry.Reward.Items == null) entry.Reward.Items = new List<QuestRewardItem>();
    }

    private void DrawQuestProblems(QuestModel entry)
    {
        foreach (string problem in QuestDataValidator.Validate(entry, _questData.Quests, BuildQuestLookups()))
        {
            EditorGUILayout.HelpBox(problem, MessageType.Warning);
        }
    }

    private QuestDataLookups BuildQuestLookups()
    {
        return new QuestDataLookups
        {
            CharacterExists = id => _characterData.Characters.Any(c => c.Id == id),
            CharacterHasDialogue = id => _characterData.Characters.Any(c => c.Id == id && c.DialogueId > 0),
            ItemExists = id => _itemData.Items.Any(i => i.ItemId == id),
            SceneExists = name => BuildSceneNames().Contains(name),
            DialogueExists = id => _dialogueData.Dialogues.Any(d => d.Id == id)
        };
    }

    private void DrawObjectives(QuestModel entry)
    {
        int removeIndex = -1;
        for (int i = 0; i < entry.Objectives.Count; i++)
        {
            QuestObjectiveModel objective = entry.Objectives[i];
            if (objective.Text == null) objective.Text = new LocalizedString();

            objective.Type = (QuestObjectiveType)EditorGUILayout.EnumPopup("Objective Type", objective.Type);
            DrawObjectiveTarget(objective);
            DrawObjectiveAmount(objective);
            objective.Text.EN = EditorGUILayout.TextField("Player Sees (EN)", objective.Text.EN);
            objective.Text.TH = EditorGUILayout.TextField("Player Sees (TH)", objective.Text.TH);
            if (GUILayout.Button("Remove Objective")) removeIndex = i;
            EditorGUILayout.Space(6f);
        }

        if (removeIndex >= 0)
        {
            entry.Objectives.RemoveAt(removeIndex);
            MarkDirty();
        }

        if (GUILayout.Button("+ Objective"))
        {
            entry.Objectives.Add(new QuestObjectiveModel { Text = new LocalizedString() });
            MarkDirty();
        }
    }

    private static void DrawObjectiveAmount(QuestObjectiveModel objective)
    {
        switch (objective.Type)
        {
            case QuestObjectiveType.Kill:
                objective.Required = EditorGUILayout.IntField("Kill Count", objective.Required);
                DrawObjectiveHint("Counts only this monster, and only if the player damaged it.");
                break;
            case QuestObjectiveType.Collect:
                objective.Required = EditorGUILayout.IntField("Item Count", objective.Required);
                DrawObjectiveHint("Counts what is in the inventory. This many are removed when the quest is completed.");
                break;
            case QuestObjectiveType.Talk:
                objective.Required = 1;
                DrawObjectiveHint(objective.DialogueId > 0
                    ? "Finishes when the player reaches the end of this dialogue. Choices that leave do not count."
                    : "Finishes when the player talks to this NPC.");
                break;
            case QuestObjectiveType.ReachMap:
                objective.Required = 1;
                DrawObjectiveHint("Finishes when the player is in this scene.");
                break;
        }
    }

    private static void DrawObjectiveHint(string text)
    {
        EditorGUILayout.LabelField(text, EditorStyles.wordWrappedMiniLabel);
    }

    private void DrawObjectiveTarget(QuestObjectiveModel objective)
    {
        switch (objective.Type)
        {
            case QuestObjectiveType.Kill:
                DrawCharacterPicker("Monster", objective.TargetId, id => objective.TargetId = id);
                break;
            case QuestObjectiveType.Talk:
                DrawCharacterPicker("NPC", objective.TargetId, id => objective.TargetId = id);
                DrawDialoguePicker("Quest Dialogue", objective.DialogueId, id => objective.DialogueId = id);
                break;
            case QuestObjectiveType.Collect:
                DrawItemPicker("Item To Collect", objective.TargetId, id => objective.TargetId = id);
                break;
            case QuestObjectiveType.ReachMap:
                DrawScenePicker(objective);
                break;
        }
    }

    private void DrawRewardItems(QuestRewardModel reward)
    {
        int removeIndex = -1;
        for (int i = 0; i < reward.Items.Count; i++)
        {
            QuestRewardItem item = reward.Items[i];
            DrawItemPicker("Item", item.ItemId, id => item.ItemId = id);
            item.Quantity = EditorGUILayout.IntField("Quantity", item.Quantity);
            if (GUILayout.Button("Remove Item")) removeIndex = i;
        }

        if (removeIndex >= 0)
        {
            reward.Items.RemoveAt(removeIndex);
            MarkDirty();
        }

        if (GUILayout.Button("+ Reward Item"))
        {
            reward.Items.Add(new QuestRewardItem());
            MarkDirty();
        }
    }

    private void DrawCharacterPicker(string label, int currentId, System.Action<int> onSelected)
    {
        CharacterModel current = _characterData.Characters.Find(c => c.Id == currentId);
        string text = current != null ? $"[{current.Id}] {current.Name}" : currentId > 0 ? $"Missing {currentId}" : "None";

        DrawPickerButton(label, text, menu =>
        {
            foreach (CharacterModel character in _characterData.Characters.OrderBy(c => c.Id))
            {
                int id = character.Id;
                menu.AddItem(new GUIContent($"[{id}] {character.Name}"), id == currentId, () => Pick(onSelected, id));
            }
        });
    }

    private void DrawItemPicker(string label, int currentId, System.Action<int> onSelected)
    {
        ItemEntry current = _itemData.Items.Find(i => i.ItemId == currentId);
        string text = current != null ? $"[{current.ItemId}] {current.Name}" : currentId > 0 ? $"Missing {currentId}" : "None";

        DrawPickerButton(label, text, menu =>
        {
            foreach (ItemEntry item in _itemData.Items.OrderBy(i => i.ItemId))
            {
                int id = item.ItemId;
                menu.AddItem(new GUIContent($"{item.ItemType}/[{id}] {item.Name}"), id == currentId, () => Pick(onSelected, id));
            }
        });
    }

    private void DrawScenePicker(QuestObjectiveModel objective)
    {
        string text = string.IsNullOrEmpty(objective.TargetScene) ? "None" : objective.TargetScene;
        DrawPickerButton("Scene To Reach", text, menu =>
        {
            foreach (string scene in BuildSceneNames())
            {
                string name = scene;
                menu.AddItem(new GUIContent(name), name == objective.TargetScene, () =>
                {
                    objective.TargetScene = name;
                    MarkDirty();
                });
            }
        });
    }

    private void DrawRequiredQuests(QuestModel entry)
    {
        EditorGUILayout.LabelField("Requires Quests (all must be completed)");
        int removeIndex = -1;
        for (int i = 0; i < entry.RequiresQuestIds.Count; i++)
        {
            int index = i;
            EditorGUILayout.BeginHorizontal();
            DrawQuestPicker(entry, $"Quest {i + 1}", entry.RequiresQuestIds[i], id => entry.RequiresQuestIds[index] = id);
            if (GUILayout.Button("X", GUILayout.Width(24f))) removeIndex = i;
            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex >= 0)
        {
            entry.RequiresQuestIds.RemoveAt(removeIndex);
            MarkDirty();
        }

        if (GUILayout.Button("+ Required Quest"))
        {
            entry.RequiresQuestIds.Add(0);
            MarkDirty();
        }
    }

    private void DrawQuestPicker(QuestModel entry, string label, int currentId, System.Action<int> onSelected)
    {
        QuestModel current = _questData.Quests.Find(q => q.Id == currentId);
        string text = current != null ? QuestLabel(current) : currentId > 0 ? $"Missing {currentId}" : "None";

        DrawPickerButton(label, text, menu =>
        {
            foreach (QuestModel quest in _questData.Quests.Where(q => q != entry).OrderBy(q => q.Id))
            {
                int id = quest.Id;
                menu.AddItem(new GUIContent(QuestLabel(quest)), id == currentId, () => Pick(onSelected, id));
            }
        });
    }

    private void DrawDialoguePicker(string label, int currentId, System.Action<int> onSelected, bool allowCreate = true)
    {
        DialogueModel current = _dialogueData.Dialogues.Find(d => d.Id == currentId);
        string text = current != null ? DialogueLabel(current) : currentId > 0 ? $"Missing {currentId}" : "None";

        DrawPickerButton(label, text, menu =>
        {
            menu.AddItem(new GUIContent("None"), currentId == 0, () => Pick(onSelected, 0));
            foreach (DialogueModel dialogue in _dialogueData.Dialogues.OrderBy(d => d.Id))
            {
                int id = dialogue.Id;
                menu.AddItem(new GUIContent(DialogueLabel(dialogue)), id == currentId, () => Pick(onSelected, id));
            }
        });
        if (allowCreate && currentId <= 0 && GUILayout.Button("Create New " + label)) CreateDialogueAndOpen(onSelected);
    }

    private static string DialogueLabel(DialogueModel dialogue)
    {
        DialogueNode first = dialogue.Nodes.Find(n => n.Id == dialogue.StartNode);
        string line = first != null && first.Text != null ? first.Text.EN : string.Empty;
        if (line.Length > 40) line = line.Substring(0, 40) + "...";
        return $"[{dialogue.Id}] {line.Replace('/', ' ')}";
    }

    private static void DrawPickerButton(string label, string text, System.Action<GenericMenu> fill)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(label);
        if (GUILayout.Button(text, EditorStyles.popup))
        {
            var menu = new GenericMenu();
            fill(menu);
            menu.ShowAsContext();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void Pick(System.Action<int> onSelected, int id)
    {
        onSelected(id);
        MarkDirty();
    }

    private static List<string> BuildSceneNames()
    {
        return EditorBuildSettings.scenes.Select(scene => Path.GetFileNameWithoutExtension(scene.path)).ToList();
    }

    private static string QuestLabel(QuestModel quest)
    {
        return $"[{quest.Id}] {(quest.Name != null ? quest.Name.EN : string.Empty)}";
    }

    private EntryView DescribeQuest(QuestModel entry)
    {
        int problems = QuestDataValidator.Validate(entry, _questData.Quests, BuildQuestLookups()).Count;
        return new EntryView
        {
            Id = entry.Id.ToString(),
            Name = entry.Name != null ? entry.Name.EN : string.Empty,
            Meta = problems > 0 ? $"{problems} issue(s)" : string.Empty,
            MetaColor = ThemeMuted,
            Accent = ThemeAccent,
            Subtitle = $"{entry.Objectives?.Count ?? 0} objective(s)"
        };
    }

    private QuestModel CreateQuest()
    {
        return new QuestModel
        {
            Id = _questData.Quests.Count > 0 ? _questData.Quests.Max(q => q.Id) + 1 : 1,
            Name = new LocalizedString { EN = "New Quest", TH = "" },
            Description = new LocalizedString()
        };
    }
    #endregion
}
