using System.Collections.Generic;
using CoreSystem.Data.Items.Effects;
using UnityEngine;

namespace CoreSystem.Data.Items
{
    [System.Serializable]
    public class SetBonusLevel
    {
        public int RequiredPieces;
        public List<EquipmentEffect> BonusEffects = new List<EquipmentEffect>();
    }
}
