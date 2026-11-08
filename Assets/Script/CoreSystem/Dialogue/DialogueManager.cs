using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.Entity;
using CoreSystem.Interaction;
using CoreSystem.Player;
using CoreSystem.UI.Dialogue;
using CoreSystem.Utility;
using UnityEngine;

namespace CoreSystem.Dialogue
{
    public sealed class DialogueManager : MonoBehaviour, IInteractionProvider
    {
        #region [Variables/Fields]
        public const string OptionId = "dialogue.talk";
        private const string OptionLabel = "Talk";
        private const float TalkRange = 1.5f;
        private const int TalkPriority = 220;
        private static DialogueManager _instance;
        #endregion

        #region [Properties]
        public static DialogueManager Instance => _instance;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _instance = this;
            InteractionRegistry.Register(this);
        }

        private void OnDestroy()
        {
            InteractionRegistry.Unregister(this);
            if (_instance == this) _instance = null;
        }
        #endregion

        #region [Public Methods]
        public static DialogueManager Create()
        {
            return RuntimeRoot.CreateChild("DialogueManager").AddComponent<DialogueManager>();
        }

        public static DialogueManager Ensure()
        {
            return _instance != null ? _instance : Create();
        }

        public void Collect(InteractionContext context, List<InteractionOption> options)
        {
            Character target = context.Target;
            if (context.Actor == null || target == null || context.IsSelf || target.IsDead) return;
            if (!(context.Holder is PlayerCore)) return;

            CharacterModel model = Database.GetCharacter(target.CharacterId);
            if (model == null || model.DialogueId <= 0) return;

            DialogueModel dialogue = DialogueDatabase.Get(model.DialogueId);
            if (dialogue == null) return;

            options.Add(new InteractionOption(OptionId, OptionLabel, _ => DialogueWindow.Open(model.Name, dialogue), TalkRange, TalkPriority));
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }
        #endregion
    }
}
