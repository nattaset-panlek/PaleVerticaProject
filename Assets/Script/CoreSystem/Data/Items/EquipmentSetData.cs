using System.Collections.Generic;
using CoreSystem.Data.Items.Effects;
using UnityEngine;

namespace CoreSystem.Data.Items
{
    [System.Serializable]
    public class EquipmentSetData
    {
        public string SetID;
        public string SetName;
        public List<SetBonusLevel> BonusLevels = new List<SetBonusLevel>();
    }
}
