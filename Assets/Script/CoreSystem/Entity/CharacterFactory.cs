using CoreSystem.Agent;
using CoreSystem.Data;
using CoreSystem.Player;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CoreSystem.Entity
{
    public static class CharacterFactory
    {
        #region [Public Methods]
        public static void Spawn(int characterId, Vector3 position, Transform parent, System.Action<Character> onSpawned)
        {
            var data = Database.GetCharacter(characterId);
            if (data == null || string.IsNullOrEmpty(data.PrefabPath))
            {
                Debug.LogWarning($"[CharacterFactory] Character {characterId} has no valid PrefabPath.");
                onSpawned?.Invoke(null);
                return;
            }

            Addressables.InstantiateAsync(data.PrefabPath, position, Quaternion.identity, parent).Completed += handle =>
            {
                if (handle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded || handle.Result == null)
                {
                    Debug.LogError($"[CharacterFactory] Failed to load Addressable {data.PrefabPath}");
                    onSpawned?.Invoke(null);
                    return;
                }

                var character = handle.Result.GetComponent<Character>();
                if (character != null) character.faction = data.Faction;
                onSpawned?.Invoke(character);
            };
        }

        public static void AssignController(Character character)
        {
            if (character == null) return;

            switch (character.DefaultController)
            {
                case CharacterControllerType.Player:
                    PlayerCore.Ensure().Possess(character);
                    break;
                case CharacterControllerType.Agent:
                    AgentCore.Ensure().Register(character);
                    break;
            }
        }

        public static void ReleaseController(Character character)
        {
            if (character == null) return;
            if (AgentCore.Instance != null) AgentCore.Instance.Unregister(character);
            if (PlayerCore.Instance != null && PlayerCore.Instance.Controlled == character) PlayerCore.Instance.Release();
        }
        #endregion
    }
}
