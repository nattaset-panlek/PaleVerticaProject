using System.Collections.Generic;
using System.Linq;
using CoreSystem.Events;
using UnityEditor;
using UnityEngine;

public partial class GameDataWorkbench : EditorWindow
{
    #region [Variables/Fields]
    private string _eventJsonPath = "Assets/Resources/Data/Event.json";
    private EventDataList _eventData;
    private int _eventSelectedIndex = -1;
    #endregion

    #region [Private Methods]
    private void DrawEventEditor()
    {
        DrawEditor(_eventData.Events, _eventSelectedIndex, idx => _eventSelectedIndex = idx, DrawEventFields, DescribeEvent, CreateEvent);
    }

    private void DrawEventFields(EventModel entry)
    {
        if (entry.Trigger == null) entry.Trigger = new EventTrigger();
        if (entry.Actions == null) entry.Actions = new List<EventAction>();
        foreach (string problem in EventDataValidator.Validate(entry, _eventData.Events, BuildEventLookups()))
        {
            EditorGUILayout.HelpBox(problem, MessageType.Warning);
        }

        BeginInspectorColumns();
        if (BeginGroup("When", ThemeAccent))
        {
            entry.Id = EditorGUILayout.IntField("ID", entry.Id);
            entry.Name = EditorGUILayout.TextField("Name", entry.Name);
            entry.Once = EditorGUILayout.Toggle(new GUIContent("Only Once", "Once per save file."), entry.Once);
            DrawEventTrigger(entry.Trigger);
        }
        EndGroup();

        NextInspectorColumn();
        if (BeginGroup("Then Do", ThemeAccent, entry.Actions.Count.ToString())) DrawEventActions(entry);
        EndGroup();
        EndInspectorColumns();
    }

    private void DrawEventTrigger(EventTrigger trigger)
    {
        trigger.Type = (EventTriggerType)EditorGUILayout.EnumPopup("Trigger", trigger.Type);
        switch (trigger.Type)
        {
            case EventTriggerType.EnterZone:
                trigger.ZoneId = EditorGUILayout.TextField("Zone Id", trigger.ZoneId);
                DrawObjectiveHint("Matches the Zone Id on an EventZone in the scene.");
                break;
            case EventTriggerType.QuestAccepted:
            case EventTriggerType.QuestCompleted:
                DrawQuestPicker(null, "Quest", trigger.QuestId, id => trigger.QuestId = id);
                break;
            case EventTriggerType.QuestObjectiveDone:
                DrawQuestPicker(null, "Quest", trigger.QuestId, id => trigger.QuestId = id);
                trigger.ObjectiveIndex = EditorGUILayout.IntField("Objective Number (from 0)", trigger.ObjectiveIndex);
                break;
            case EventTriggerType.KillCount:
                DrawCharacterPicker("Monster", trigger.CharacterId, id => trigger.CharacterId = id);
                trigger.Count = EditorGUILayout.IntField("Kill Count", trigger.Count);
                break;
            case EventTriggerType.DialogueEnded:
                DrawDialoguePicker("Dialogue", trigger.DialogueId, id => trigger.DialogueId = id, false);
                break;
        }
    }

    private void DrawEventActions(EventModel entry)
    {
        int removeIndex = -1;
        for (int i = 0; i < entry.Actions.Count; i++)
        {
            EventAction action = entry.Actions[i];
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            action.Type = (EventActionType)EditorGUILayout.EnumPopup("Action", action.Type);
            DrawEventAction(action);
            if (GUILayout.Button("Remove Action", EditorStyles.miniButton)) removeIndex = i;
            EditorGUILayout.EndVertical();
        }

        if (removeIndex >= 0)
        {
            entry.Actions.RemoveAt(removeIndex);
            MarkDirty();
        }

        if (GUILayout.Button("+ Action"))
        {
            entry.Actions.Add(new EventAction());
            MarkDirty();
        }
    }

    private void DrawEventAction(EventAction action)
    {
        switch (action.Type)
        {
            case EventActionType.PlayDialogue:
                DrawDialoguePicker("Dialogue", action.DialogueId, id => action.DialogueId = id, false);
                DrawCharacterPicker("Speaker", action.SpeakerCharacterId, id => action.SpeakerCharacterId = id);
                break;
            case EventActionType.SpawnMonster:
                DrawCharacterPicker("Monster", action.CharacterId, id => action.CharacterId = id);
                action.Count = EditorGUILayout.IntField("How Many", action.Count);
                action.SpawnPointId = EditorGUILayout.TextField("Spawn Point Id", action.SpawnPointId);
                DrawObjectiveHint("Matches the Id on a MapSpawnPoint in the current scene.");
                break;
            case EventActionType.GiveReward:
                DrawItemPicker("Item", action.ItemId, id => action.ItemId = id);
                action.Quantity = EditorGUILayout.IntField("Item Quantity", action.Quantity);
                action.BaseExp = EditorGUILayout.LongField("Base EXP", action.BaseExp);
                action.JobExp = EditorGUILayout.LongField("Job EXP", action.JobExp);
                break;
            case EventActionType.SetObjectActive:
                action.ObjectId = EditorGUILayout.TextField("Object Id", action.ObjectId);
                action.Active = EditorGUILayout.Toggle("Show Object", action.Active);
                DrawObjectiveHint("Matches the Object Id on an EventObject in the scene. The state is saved.");
                break;
        }
    }

    private EventDataLookups BuildEventLookups()
    {
        return new EventDataLookups
        {
            CharacterExists = id => _characterData.Characters.Any(c => c.Id == id),
            ItemExists = ItemExists,
            QuestExists = id => _questData.Quests.Any(q => q.Id == id),
            DialogueExists = id => _dialogueData.Dialogues.Any(d => d.Id == id)
        };
    }

    private EntryView DescribeEvent(EventModel entry)
    {
        int problems = EventDataValidator.Validate(entry, _eventData.Events, BuildEventLookups()).Count;
        return new EntryView
        {
            Id = entry.Id.ToString(),
            Name = entry.Name,
            Meta = problems > 0 ? $"{problems} issue(s)" : string.Empty,
            MetaColor = ThemeMuted,
            Accent = ThemeAccent,
            Subtitle = entry.Trigger != null ? entry.Trigger.Type.ToString() : string.Empty
        };
    }

    private EventModel CreateEvent()
    {
        return new EventModel
        {
            Id = _eventData.Events.Count > 0 ? _eventData.Events.Max(e => e.Id) + 1 : 1,
            Name = "New Event"
        };
    }
    #endregion
}
