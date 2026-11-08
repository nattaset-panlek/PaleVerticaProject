namespace CoreSystem.Settings.Keybinds
{
    public sealed class KeybindDefinition
    {
        #region [Variables/Fields]
        public string Id;
        public string Label;
        public string Group;
        public int Order;
        public KeybindPair Default;
        public string ActionMap;
        public string ActionName;
        public string PartName;
        #endregion

        #region [Properties]
        public bool IsInputAction => !string.IsNullOrEmpty(ActionName);
        #endregion

        #region [Public Methods]
        public KeybindDefinition(string id, string label, string group, int order, KeybindPair defaults)
        {
            Id = id;
            Label = label;
            Group = group;
            Order = order;
            Default = defaults;
        }

        public static KeybindDefinition ForInputAction(string map, string action, string part, string label, string group, int order, KeybindPair defaults)
        {
            return new KeybindDefinition(BuildInputActionId(map, action, part), label, group, order, defaults)
            {
                ActionMap = map,
                ActionName = action,
                PartName = part
            };
        }

        public static string BuildInputActionId(string map, string action, string part)
        {
            return string.IsNullOrEmpty(part) ? $"input.{map}.{action}" : $"input.{map}.{action}.{part}";
        }
        #endregion
    }
}
