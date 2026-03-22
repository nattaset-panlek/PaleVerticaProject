using CoreSystem;

namespace CoreSystem.Attributes
{
    /// <summary>
    /// Template (metadata) for a single AttributeType entry.
    /// Loaded from Resources/Data/Attribute.json via AttributeDatabase.
    /// </summary>
    [System.Serializable]
    public class AttributeTemplate
    {
        /// <summary>Integer ID matching the AttributeType enum value.</summary>
        public int Id { get; set; }

        /// <summary>Resource name or path for the attribute icon sprite.</summary>
        public string Icon { get; set; }

        /// <summary>Short display name, e.g. "STR", "HP".</summary>
        public string Name { get; set; }

        /// <summary>Full descriptive name, e.g. "Strength", "Hit Points".</summary>
        public string FullName { get; set; }

        /// <summary>Description explaining what this attribute does.</summary>
        public string Description { get; set; }

        /// <summary>Indicates if this attribute is a percentage value.</summary>
        public bool IsPercent { get; set; }

        /// <summary>The AttributeType enum value this template maps to.</summary>
        public AttributeType AttributeType { get; set; }
    }
}
