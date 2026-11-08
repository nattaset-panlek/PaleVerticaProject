using System;

namespace CoreSystem.Interaction
{
    public sealed class InteractionOption
    {
        #region [Variables/Fields]
        public const float AnyRange = 0f;
        #endregion

        #region [Properties]
        public string Id { get; }
        public string Label { get; }
        public float Range { get; }
        public int Priority { get; }
        public Action<InteractionContext> Execute { get; }
        public bool RequiresRange => Range > AnyRange;
        #endregion

        #region [Public Methods]
        public InteractionOption(string id, string label, Action<InteractionContext> execute, float range = AnyRange, int priority = 0)
        {
            Id = id;
            Label = string.IsNullOrEmpty(label) ? id : label;
            Execute = execute;
            Range = range < AnyRange ? AnyRange : range;
            Priority = priority;
        }
        #endregion
    }
}
