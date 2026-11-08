namespace CoreSystem.Settings
{
    public sealed class SettingsPage
    {
        #region [Variables/Fields]
        public string Id;
        public string Title;
        public int Order;
        public string ViewId;
        public string ResetLabel = "Default";
        #endregion

        #region [Properties]
        public bool HasCustomView => !string.IsNullOrEmpty(ViewId);
        #endregion

        #region [Public Methods]
        public SettingsPage(string id, string title, int order, string viewId = null)
        {
            Id = id;
            Title = title;
            Order = order;
            ViewId = viewId;
        }
        #endregion
    }
}
