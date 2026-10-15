using System.Collections.Generic;
using CoreSystem.Entity;
using CoreSystem.Utility;
using UnityEngine;

namespace CoreSystem.Agent
{
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public class AgentCore : MonoBehaviour
    {
        #region [Variables/Fields]
        private static AgentCore _instance;
        private readonly Dictionary<int, AgentHandle> _handles = new Dictionary<int, AgentHandle>();
        private readonly List<AgentHandle> _tickBuffer = new List<AgentHandle>();
        #endregion

        #region [Properties]
        public static AgentCore Instance => _instance;
        public static bool IsReady => _instance != null;
        public static event System.Action OnReady;
        public IReadOnlyCollection<AgentHandle> Handles => _handles.Values;
        public int Count => _handles.Count;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (GetComponent<Character>() != null)
            {
                Debug.LogWarning($"[AgentCore] Legacy AgentCore component found on character '{name}'. Characters no longer carry AgentCore; removing it.");
                Destroy(this);
                return;
            }

            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            OnReady?.Invoke();
        }

        private void Update()
        {
            _tickBuffer.Clear();
            _tickBuffer.AddRange(_handles.Values);
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < _tickBuffer.Count; i++) _tickBuffer[i].Tick(deltaTime);
        }

        private void OnDestroy()
        {
            if (_instance != this) return;
            foreach (var handle in _handles.Values) handle.Dispose();
            _handles.Clear();
            _instance = null;
        }
        #endregion

        #region [Public Methods]
        public static AgentCore Create()
        {
            return RuntimeRoot.CreateChild("AgentCore").AddComponent<AgentCore>();
        }

        public static AgentCore Ensure()
        {
            return _instance != null ? _instance : Create();
        }

        public AgentHandle Register(Character character)
        {
            if (character == null || character.RuntimeId <= 0) return null;
            if (_handles.TryGetValue(character.RuntimeId, out var existing)) return existing;

            if (character.Control.TransferOwnership(this) == null)
            {
                Debug.LogWarning($"[AgentCore] Could not take ownership of '{character.name}'.");
                return null;
            }

            var handle = new AgentHandle(this, character);
            _handles[character.RuntimeId] = handle;
            handle.Start();
            return handle;
        }

        public void Unregister(Character character)
        {
            if (character == null) return;
            if (!_handles.TryGetValue(character.RuntimeId, out var handle)) return;

            handle.Dispose();
            _handles.Remove(character.RuntimeId);
            character.Control.Release(this);
        }

        public AgentHandle GetHandle(Character character)
        {
            return character != null && _handles.TryGetValue(character.RuntimeId, out var handle) ? handle : null;
        }

        public bool TryGetHandle(int runtimeId, out AgentHandle handle)
        {
            return _handles.TryGetValue(runtimeId, out handle);
        }

        public void CommandMoveTo(Character character, Vector3 destination)
        {
            GetHandle(character)?.SetCommandMove(destination);
        }

        public void CommandFollow(Character character, Transform target)
        {
            GetHandle(character)?.SetCommandFollow(target);
        }

        public void ClearCommand(Character character)
        {
            GetHandle(character)?.ClearCommand();
        }

        public void Revive(Character character, float hp)
        {
            GetHandle(character)?.Revive(hp);
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            OnReady = null;
        }
        #endregion
    }
}
