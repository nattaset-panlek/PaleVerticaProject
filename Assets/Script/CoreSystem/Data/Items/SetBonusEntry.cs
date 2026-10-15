using System.Collections.Generic;
using UnityEngine;
using CoreSystem;
using CoreSystem.Data;
using CoreSystem.Data.Items.Model;
using CoreSystem.Data.Items.Effects;
using CoreSystem.Attributes;
using CoreSystem.Data.Skills;

namespace CoreSystem.Data.Items
{
    [System.Serializable]
    public class SetBonusEntry
    {
        public int RequiredPieces;
        public List<EffectEntry> Effects;
    }
}
