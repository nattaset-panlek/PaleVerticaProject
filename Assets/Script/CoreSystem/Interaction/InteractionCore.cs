using System;
using System.Collections.Generic;
using CoreSystem.Entity;
using CoreSystem.Utility;
using UnityEngine;

namespace CoreSystem.Interaction
{
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public class InteractionCore : MonoBehaviour
    {
        #region [Variables/Fields]
        private static InteractionCore _instance;

        private readonly Dictionary<Character, InteractionRequest> _pending = new Dictionary<Character, InteractionRequest>();
        private readonly List<Character> _actors = new List<Character>();
        #endregion

        #region [Properties]
        public static InteractionCore Instance => _instance;
        public static bool IsReady => _instance != null;
        public static event Action<InteractionCore> OnReady;
        public static event Action<InteractionRequest> OnApproachStarted;
        public static event Action<InteractionRequest> OnExecuted;
        public static event Action<InteractionRequest, InteractionCancelReason> OnCancelled;
        public int PendingCount => _pending.Count;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            CharacterRegistry.OnUnregistered += HandleCharacterUnregistered;
            OnReady?.Invoke(this);
        }

        private void OnDestroy()
        {
            if (_instance != this) return;
            CharacterRegistry.OnUnregistered -= HandleCharacterUnregistered;
            _pending.Clear();
            _instance = null;
        }

        private void Update()
        {
            if (_pending.Count == 0) return;

            _actors.Clear();
            _actors.AddRange(_pending.Keys);
            float deltaTime = Time.deltaTime;

            for (int i = 0; i < _actors.Count; i++)
            {
                var actor = _actors[i];
                if (!_pending.TryGetValue(actor, out var request)) continue;
                TickRequest(actor, request, deltaTime);
            }
        }
        #endregion

        #region [Public Methods]
        public static InteractionCore Create()
        {
            return RuntimeRoot.CreateChild("InteractionCore").AddComponent<InteractionCore>();
        }

        public static InteractionCore Ensure()
        {
            return _instance != null ? _instance : Create();
        }

        public static bool CanInteract(Character actor, Character target)
        {
            if (actor == null || target == null) return false;
            if (actor == target) return true;
            return target.IsInteractable && target.isActiveAndEnabled;
        }

        public static List<InteractionOption> GetOptions(Character actor, Character target, object holder)
        {
            if (!CanInteract(actor, target)) return new List<InteractionOption>();
            return InteractionRegistry.Collect(new InteractionContext(actor, target, holder));
        }

        public bool Request(Character actor, Character target, InteractionOption option, object holder)
        {
            if (option == null || !CanInteract(actor, target)) return false;

            Cancel(actor, InteractionCancelReason.Replaced);

            var request = new InteractionRequest(new InteractionContext(actor, target, holder), option);
            if (InteractionRequest.IsInRange(actor.transform.position, target.transform.position, option.Range))
            {
                Execute(request);
                return true;
            }

            if (!actor.Control.CanCommand(holder)) return false;

            _pending[actor] = request;
            OnApproachStarted?.Invoke(request);
            return true;
        }

        public bool Cancel(Character actor)
        {
            return Cancel(actor, InteractionCancelReason.Cancelled);
        }

        public bool IsApproaching(Character actor)
        {
            return actor != null && _pending.ContainsKey(actor);
        }

        public InteractionRequest GetPending(Character actor)
        {
            return actor != null && _pending.TryGetValue(actor, out var request) ? request : null;
        }
        #endregion

        #region [Private Methods]
        private void TickRequest(Character actor, InteractionRequest request, float deltaTime)
        {
            if (actor == null)
            {
                Cancel(actor, InteractionCancelReason.TargetInvalid);
                return;
            }

            var target = request.Context.Target;
            bool targetValid = CanInteract(actor, target) && !target.IsDead && !actor.IsDead;
            bool canCommand = actor.Control.CanCommand(request.Context.Holder);

            Vector2 actorPosition = actor.transform.position;
            Vector2 targetPosition = targetValid ? (Vector2)target.transform.position : Vector2.zero;

            switch (request.Step(actorPosition, targetPosition, targetValid, canCommand, deltaTime))
            {
                case InteractionStep.Approach:
                    Vector3 direction = (targetPosition - actorPosition).normalized;
                    actor.Control.CommandMove(direction, request.Context.Holder);
                    actor.Presenter.Face(direction);
                    actor.Presenter.SetMoving(true);
                    break;

                case InteractionStep.Execute:
                    _pending.Remove(actor);
                    actor.Presenter.SetMoving(false);
                    Execute(request);
                    break;

                case InteractionStep.Cancel:
                    var reason = !targetValid ? InteractionCancelReason.TargetInvalid
                        : !canCommand ? InteractionCancelReason.ControlLost
                        : InteractionCancelReason.TimedOut;
                    Cancel(actor, reason);
                    break;
            }
        }

        private void Execute(InteractionRequest request)
        {
            try
            {
                request.Option.Execute(request.Context);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            OnExecuted?.Invoke(request);
        }

        private bool Cancel(Character actor, InteractionCancelReason reason)
        {
            if (ReferenceEquals(actor, null) || !_pending.TryGetValue(actor, out var request)) return false;
            _pending.Remove(actor);
            if (actor != null) actor.Presenter.SetMoving(false);
            OnCancelled?.Invoke(request, reason);
            return true;
        }

        private void HandleCharacterUnregistered(Character character)
        {
            if (character == null || _pending.Count == 0) return;

            Cancel(character, InteractionCancelReason.TargetInvalid);

            _actors.Clear();
            foreach (var pair in _pending)
            {
                if (pair.Value.Context.Target == character) _actors.Add(pair.Key);
            }
            for (int i = 0; i < _actors.Count; i++) Cancel(_actors[i], InteractionCancelReason.TargetInvalid);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            OnReady = null;
            OnApproachStarted = null;
            OnExecuted = null;
            OnCancelled = null;
        }
        #endregion
    }
}
