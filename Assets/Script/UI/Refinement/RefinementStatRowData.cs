namespace CoreSystem.UI.Refinement
{
    public struct RefinementStatRowData
    {
        #region [Variables/Fields]
        public string Label;
        public string ValueFormatted;
        public bool IsHighlighted;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public RefinementStatRowData(string label, string valueFormatted, bool isHighlighted = false)
        {
            Label = label;
            ValueFormatted = valueFormatted;
            IsHighlighted = isHighlighted;
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
