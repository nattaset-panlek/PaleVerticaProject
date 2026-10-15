using System.Collections.Generic;
using CoreSystem.Utility;
using UnityEngine;

namespace CoreSystem.Skills
{
    public abstract class SupportModifier : ISupportModifier
    {
        #region [Variables/Fields]
        private static readonly AttributedTypeRegistry<int, SupportModifier, SupportModifierIdAttribute> Registry =
            new AttributedTypeRegistry<int, SupportModifier, SupportModifierIdAttribute>(attribute => attribute.Id);

        public int ID;
        public ChromaType Chroma;
        public MineralType Mineral;

        public string Name;
        public string Description;
        public Sprite Icon;
        public string IconPath;
        #endregion

        #region [Properties]
        public static IReadOnlyDictionary<int, System.Type> RegisteredTypes => Registry.Types;
        #endregion

        #region [Public Methods]
        public static SupportModifier Create(int id)
        {
            var modifier = Registry.Create(id);
            if (modifier != null) modifier.ID = id;
            return modifier;
        }

        public abstract void ModifyPayload(ref SkillPayload payload);

        public virtual bool IsCompatible(ActiveSkill activeSkill)
        {
            return true;
        }

        public virtual string GetDescription(string template)
        {
            return template;
        }
        #endregion
    }
}
