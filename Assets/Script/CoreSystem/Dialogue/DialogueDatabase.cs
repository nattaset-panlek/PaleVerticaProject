using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Dialogue
{
    public static class DialogueDatabase
    {
        #region [Variables/Fields]
        private const string ResourcePath = "Data/Dialogue";
        private static Dictionary<int, DialogueModel> _dialogues;
        #endregion

        #region [Public Methods]
        public static DialogueModel Get(int id)
        {
            if (_dialogues == null) Load();
            _dialogues.TryGetValue(id, out DialogueModel model);
            return model;
        }
        #endregion

        #region [Private Methods]
        private static void Load()
        {
            _dialogues = new Dictionary<int, DialogueModel>();
            TextAsset file = Resources.Load<TextAsset>(ResourcePath);
            if (file == null)
            {
                Debug.LogError("DialogueDatabase: Dialogue.json not found in Resources/Data/");
                return;
            }

            DialogueDataList data = JsonUtility.FromJson<DialogueDataList>(file.text);
            foreach (DialogueModel model in data.Dialogues) _dialogues[model.Id] = model;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _dialogues = null;
        }
        #endregion
    }
}
