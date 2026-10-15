using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Data
{
    public static class FactionDatabase
    {
        private static Dictionary<int, FactionEntry> _factions = new Dictionary<int, FactionEntry>();
        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;

            TextAsset json = Resources.Load<TextAsset>("Data/Faction");
            if (json != null)
            {
                FactionList data = JsonUtility.FromJson<FactionList>(json.text);
                if (data != null && data.Factions != null)
                {
                    foreach (var f in data.Factions)
                    {
                        if (f.Positive == null) f.Positive = new List<int>();
                        if (f.Negative == null) f.Negative = new List<int>();
                        _factions[f.Id] = f;
                    }
                }
            }
            else
            {
                Debug.LogError("FactionDatabase: Failed to load Data/Faction.json");
            }
            _initialized = true;
        }

        public static FactionEntry GetFaction(int id)
        {
            if (!_initialized) Initialize();
            if (id <= 0) return null;
            if (_factions.TryGetValue(id, out var entry)) return entry;
            return null;
        }

        public static List<FactionEntry> GetAllFactions()
        {
            if (!_initialized) Initialize();
            return new List<FactionEntry>(_factions.Values);
        }

        public static bool IsHostile(int sourceFaction, int targetFaction)
        {
            if (sourceFaction <= 0 || targetFaction <= 0) return false;
            
            // Rule 3: Neutral cannot attack and cannot be attacked
            if (sourceFaction == 3 || targetFaction == 3) return false;

            // Rule 1: ID 1 (Unaffiliated) can attack each other
            if (sourceFaction == 1 && targetFaction == 1) return true;

            // Same faction (for IDs > 1) cannot attack each other
            if (sourceFaction == targetFaction) return false;

            // Rule 2: Hostile to All
            if (sourceFaction == 2 || targetFaction == 2) return true;

            // Data-driven Rule: Mutual hostility
            var source = GetFaction(sourceFaction);
            var target = GetFaction(targetFaction);
            
            if (source != null && source.Negative.Contains(targetFaction)) return true;
            if (target != null && target.Negative.Contains(sourceFaction)) return true;

            return false;
        }

        public static bool IsAlly(int sourceFaction, int targetFaction)
        {
            if (sourceFaction <= 0 || targetFaction <= 0) return false;
            
            if (sourceFaction == 3 || targetFaction == 3) return false;
            if (sourceFaction == 1 && targetFaction == 1) return false; // ID 1 has no inherent allies
            if (sourceFaction == 2 || targetFaction == 2) return false; // ID 2 has no allies, not even themselves

            // Same faction is ally
            if (sourceFaction == targetFaction) return true;

            // Data-driven Rule: Mutual alliance
            var source = GetFaction(sourceFaction);
            var target = GetFaction(targetFaction);

            if (source != null && source.Positive.Contains(targetFaction)) return true;
            if (target != null && target.Positive.Contains(sourceFaction)) return true;
            
            return false;
        }
    }
}
