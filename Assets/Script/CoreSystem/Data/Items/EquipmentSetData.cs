using System.Collections.Generic;
using CoreSystem.Data.Items.Effects;
using UnityEngine;

namespace CoreSystem.Data.Items
{
    [System.Serializable]
    public class SetBonusLevel
    {
        public int RequiredPieces;
        [SerializeReference] public List<EquipmentEffect> BonusEffects = new List<EquipmentEffect>();
    }

    [CreateAssetMenu(fileName = "NewEquipmentSet", menuName = "Item System/Equipment Set")]
    public class EquipmentSetData : ScriptableObject
    {
        public string SetName;
        public List<SetBonusLevel> BonusLevels = new List<SetBonusLevel>();
    }
}
