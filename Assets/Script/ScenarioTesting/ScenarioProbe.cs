#if UNITY_EDITOR || PV_SCENARIO_TESTS
using CoreSystem;
using CoreSystem.Data;
using CoreSystem.Dialogue;
using CoreSystem.Entity;
using CoreSystem.Player;
using CoreSystem.Quests;
using CoreSystem.UI.Loading;
using CoreSystem.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ScenarioTesting
{
    public static class ScenarioProbe
    {
        #region [Properties]
        public static Character Player => PlayerCore.GetPlayer();
        public static string SceneName => SceneManager.GetActiveScene().name;
        public static bool IsLoading => LoadingManager.IsVisible;
        public static DialogueConversation Conversation => DialogueConversation.Active;

        public static bool IsGameReady => Player != null && !IsLoading && GameCoreLoader.Core != null && SceneName != GameCoreLoader.TitleSceneName;
        #endregion

        #region [Public Methods]
        public static Character FindCharacter(int characterId, Vector3 from, bool aliveOnly = true, float radius = 60f)
        {
            Character best = null;
            float bestDistance = radius * radius;
            foreach (Character character in CharacterRegistry.All)
            {
                if (character == null || character.CharacterId != characterId) continue;
                if (aliveOnly && character.IsDead) continue;
                float distance = (character.transform.position - from).sqrMagnitude;
                if (distance > bestDistance) continue;
                bestDistance = distance;
                best = character;
            }
            return best;
        }

        public static float Read(IEntity entity, AttributeType type)
        {
            return AttributeProbe.Read(entity, type);
        }

        public static QuestStatus QuestStatusOf(int questId)
        {
            QuestModel quest = QuestDatabase.Get(questId);
            return quest != null ? GameData.Player.Quests.GetStatus(quest) : QuestStatus.Locked;
        }

        public static int QuestCount(int questId, int objectiveIndex)
        {
            return GameData.Player.Quests.GetCount(questId, objectiveIndex);
        }

        public static Key HotbarKey(int slot)
        {
            return CoreSystem.Settings.Keybinds.KeybindSetting.GetHotbarKey(slot, 0);
        }
        #endregion
    }
}
#endif
