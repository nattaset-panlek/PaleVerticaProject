using System;
using System.Collections.Generic;

namespace CoreSystem.Refinement
{
    [Serializable]
    public class RefinementMaterialRequirement
    {
        #region [Variables/Fields]
        public int ItemId;
        public int BaseQuantity = 1;
        public int QuantityScaleInterval = 5;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public int GetRequiredQuantity(int currentRefine)
        {
            if (QuantityScaleInterval <= 0) return BaseQuantity;
            return BaseQuantity + (currentRefine / QuantityScaleInterval);
        }
        #endregion

        #region [Private Methods]
        #endregion
    }

    [Serializable]
    public class RefinementRecipe
    {
        #region [Variables/Fields]
        public string RecipeId;
        public int TargetItemId = -1;
        public List<RefinementMaterialRequirement> Materials = new List<RefinementMaterialRequirement>();
        public float CostMultiplier = 1.0f;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        #endregion

        #region [Private Methods]
        #endregion
    }
}
