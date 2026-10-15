using System.Collections.Generic;
using CoreSystem.Data.Items.Effects;
using CoreSystem.Data.Items;
using CoreSystem.Data;

using CoreSystem;

namespace CoreSystem.Data.Items.Model
{
    public enum FragmentType
    {
        BasicFragment, // Attribute bonuses
        Rune,          // Skills or Auto-casts
        Essence        // Complex conditional bonuses
    }
}
