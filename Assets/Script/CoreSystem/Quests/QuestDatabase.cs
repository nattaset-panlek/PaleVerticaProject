using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Quests
{
    public static class QuestDatabase
    {
        #region [Variables/Fields]
        private const string ResourcePath = "Data/Quest";
        private static Dictionary<int, QuestModel> _quests;
        #endregion

        #region [Properties]
        public static IReadOnlyCollection<QuestModel> All
        {
            get
            {
                if (_quests == null) Load();
                return _quests.Values;
            }
        }
        #endregion

        #region [Public Methods]
        public static QuestModel Get(int id)
        {
            if (_quests == null) Load();
            _quests.TryGetValue(id, out QuestModel quest);
            return quest;
        }
        #endregion

        #region [Private Methods]
        private static void Load()
        {
            _quests = new Dictionary<int, QuestModel>();
            TextAsset file = Resources.Load<TextAsset>(ResourcePath);
            if (file == null)
            {
                Debug.LogError("QuestDatabase: Quest.json not found in Resources/Data/");
                return;
            }

            QuestDataList data = JsonUtility.FromJson<QuestDataList>(file.text);
            foreach (QuestModel quest in data.Quests) _quests[quest.Id] = quest;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _quests = null;
        }
        #endregion
    }
}
