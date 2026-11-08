using UnityEngine;

namespace CoreSystem.Interaction
{
    public sealed class InteractionRequest
    {
        #region [Variables/Fields]
        public const float DefaultTimeout = 10f;

        private float _elapsed;
        #endregion

        #region [Properties]
        public InteractionContext Context { get; }
        public InteractionOption Option { get; }
        public float Timeout { get; }
        public float Elapsed => _elapsed;
        #endregion

        #region [Public Methods]
        public InteractionRequest(InteractionContext context, InteractionOption option, float timeout = DefaultTimeout)
        {
            Context = context;
            Option = option;
            Timeout = timeout;
        }

        public static bool IsInRange(Vector2 actorPosition, Vector2 targetPosition, float range)
        {
            if (range <= InteractionOption.AnyRange) return true;
            return (targetPosition - actorPosition).sqrMagnitude <= range * range;
        }

        public InteractionStep Step(Vector2 actorPosition, Vector2 targetPosition, bool targetValid, bool canCommand, float deltaTime)
        {
            if (Option == null || !targetValid) return InteractionStep.Cancel;
            if (IsInRange(actorPosition, targetPosition, Option.Range)) return InteractionStep.Execute;
            if (!canCommand) return InteractionStep.Cancel;

            _elapsed += deltaTime;
            if (Timeout > 0f && _elapsed > Timeout) return InteractionStep.Cancel;
            return InteractionStep.Approach;
        }
        #endregion
    }
}
