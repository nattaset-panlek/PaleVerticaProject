using System;
using System.Collections.Generic;
using System.Linq;
using CoreSystem.Data;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.UI.DevTools
{
    public sealed class DevToolCharacterSpawnPage
    {
        #region [Variables/Fields]
        private const int MinCount = 1;
        private const int MaxCount = 20;
        private const float SpawnDistance = 4f;
        private const float SpawnScatter = 1.5f;

        private readonly List<GameObject> _spawned = new List<GameObject>();
        private int _count = 1;
        private string _status = "Press Spawn to create a character near the player.";
        #endregion

        #region [Public Methods]
        public void Build(UIBuilderContext ui, Transform player, string search, Action refresh)
        {
            ui.Label(_status);
            ui.Row(row =>
            {
                row.Label("How Many");
                row.Input(_count.ToString(), value => _count = ParseCount(value, _count)).Width(80f);
                row.Button($"Remove Spawned ({AliveCount()})", () => RemoveSpawned(refresh)).Fill();
            });

            foreach (CharacterModel model in Filter(Database.Characters, search))
            {
                CharacterModel captured = model;
                ui.Row(row =>
                {
                    row.Label(Describe(model)).Fill();
                    row.Button("Spawn", () => Spawn(captured, player, refresh), UIVariant.Primary).Width(100f);
                });
            }
        }

        public static int ParseCount(string text, int fallback)
        {
            return int.TryParse(text, out int value) ? Mathf.Clamp(value, MinCount, MaxCount) : fallback;
        }

        public static List<CharacterModel> Filter(IEnumerable<CharacterModel> models, string search)
        {
            IEnumerable<CharacterModel> found = models.Where(model => !string.IsNullOrEmpty(model.PrefabPath));
            if (!string.IsNullOrEmpty(search))
            {
                found = found.Where(model => model.Id.ToString().Contains(search) || (model.Name ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            return found.OrderBy(model => model.Id).ToList();
        }
        #endregion

        #region [Private Methods]
        private static string Describe(CharacterModel model)
        {
            return model.UseDataStats ? $"[{model.Id}] {model.Name}  Lv {model.Level}" : $"[{model.Id}] {model.Name}";
        }

        private void Spawn(CharacterModel model, Transform player, Action refresh)
        {
            if (player == null) return;

            for (int i = 0; i < _count; i++)
            {
                Vector2 offset = Vector2.right * SpawnDistance + UnityEngine.Random.insideUnitCircle * SpawnScatter;
                CharacterFactory.SpawnFromData(model, player.position + (Vector3)offset, character =>
                {
                    if (character != null) _spawned.Add(character.gameObject);
                    _status = character != null ? $"Spawned {model.Name}." : $"Could not spawn {model.Name}. See the Console.";
                    refresh();
                });
            }
        }

        private int AliveCount()
        {
            _spawned.RemoveAll(item => item == null);
            return _spawned.Count;
        }

        private void RemoveSpawned(Action refresh)
        {
            foreach (GameObject item in _spawned.Where(item => item != null)) UnityEngine.Object.Destroy(item);
            _spawned.Clear();
            _status = "Removed spawned characters.";
            refresh();
        }
        #endregion
    }
}
