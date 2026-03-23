using System.Collections.Generic;
using UnityEngine;
using CoreSystem;

namespace CoreSystem.Attributes
{
    /// <summary>
    /// Static database for AttributeTemplate entries.
    /// Loads from Resources/Data/Attribute.json on Initialize().
    /// </summary>
    public static class AttributeDatabase
    {
        private static Dictionary<int, AttributeTemplate> _byId
            = new Dictionary<int, AttributeTemplate>();

        private static Dictionary<AttributeType, AttributeTemplate> _byType
            = new Dictionary<AttributeType, AttributeTemplate>();

        public static bool IsInitialized { get; private set; } = false;

        // ─── Initialise ──────────────────────────────────────────────────────────

        public static void Initialize()
        {
            _byId.Clear();
            _byType.Clear();

            TextAsset jsonFile = Resources.Load<TextAsset>("Data/Attribute");
            if (jsonFile == null)
            {
                Debug.LogError("AttributeDatabase: Attribute.json not found in Resources/Data/");
                return;
            }

            AttributeDataList dataList = JsonUtility.FromJson<AttributeDataList>(jsonFile.text);
            if (dataList == null || dataList.Attributes == null)
            {
                Debug.LogError("AttributeDatabase: Failed to parse Attribute.json or Attributes list is null.");
                return;
            }

            Debug.Log($"AttributeDatabase: Parsing {dataList.Attributes.Count} entries from Attribute.json");

            foreach (var entry in dataList.Attributes)
            {
                if (!System.Enum.IsDefined(typeof(AttributeType), entry.Id))
                {
                    Debug.LogWarning($"AttributeDatabase: No AttributeType with ID {entry.Id} — skipping.");
                    continue;
                }

                AttributeType attrType = (AttributeType)entry.Id;

                var template = new AttributeTemplate
                {
                    Id          = entry.Id,
                    Icon        = entry.Icon,
                    Name        = entry.Name,
                    FullName    = entry.FullName,
                    Description = entry.Description,
                    IsPercent   = entry.IsPercent,
                    AttributeType = attrType
                };

                _byId[entry.Id]    = template;
                _byType[attrType]  = template;
            }

            IsInitialized = true;
            Debug.Log($"AttributeDatabase: Successfully loaded {_byId.Count} attributes.");
        }

        // ─── Query API ────────────────────────────────────────────────────────────

        /// <summary>Get template by AttributeType enum.</summary>
        public static AttributeTemplate GetAttribute(AttributeType type)
        {
            _byType.TryGetValue(type, out AttributeTemplate template);
            return template;
        }

        /// <summary>Get template by integer ID.</summary>
        public static AttributeTemplate GetAttribute(int id)
        {
            _byId.TryGetValue(id, out AttributeTemplate template);
            return template;
        }

        /// <summary>Returns all loaded attribute templates.</summary>
        public static List<AttributeTemplate> GetAllAttributes()
            => new List<AttributeTemplate>(_byId.Values);
    }

    // ─── Serialisation helpers (used by JsonUtility) ──────────────────────────

    [System.Serializable]
    public class AttributeDataList
    {
        public List<AttributeEntry> Attributes;
    }

    [System.Serializable]
    public class AttributeEntry
    {
        public int    Id;
        public string Icon;
        public string Name;
        public string FullName;
        public string Description;
        public bool   IsPercent;
        public string Mapping;
    }
}
