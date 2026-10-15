using System.Collections.Generic;

namespace CoreSystem.Attributes
{
    public enum AdditionalOp
    {
        Add,
        Multiply
    }

    [System.Serializable]
    public class AdditionalEntry
    {
        public int Order;
        public AdditionalOp Op;
        public float Value;
        public string Tag;

        public AdditionalEntry(int order, AdditionalOp op, float value, string tag = "")
        {
            Order = order;
            Op = op;
            Value = value;
            Tag = tag;
        }

        public float Apply(float input)
        {
            return Op == AdditionalOp.Add ? input + Value : input * Value;
        }

        public override string ToString()
        {
            string sym = Op == AdditionalOp.Add ? "+" : "*";
            string tagPart = string.IsNullOrEmpty(Tag) ? "" : $" [{Tag}]";
            return $"({Order},{sym}{Value}{tagPart})";
        }
    }

    public class AdditionalChain
    {
        private readonly List<AdditionalEntry> _entries = new List<AdditionalEntry>();

        public IReadOnlyList<AdditionalEntry> Entries => _entries;

        public void Add(AdditionalEntry entry)
        {
            int index = _entries.Count;
            while (index > 0 && _entries[index - 1].Order > entry.Order) index--;
            _entries.Insert(index, entry);
        }

        public void Add(int order, AdditionalOp op, float value, string tag = "")
            => Add(new AdditionalEntry(order, op, value, tag));

        public bool Remove(AdditionalEntry entry) => _entries.Remove(entry);

        public void RemoveByTag(string tag)
            => _entries.RemoveAll(e => e.Tag == tag);

        public void Clear() => _entries.Clear();

        public float Apply(float baseValue)
        {
            float result = baseValue;
            for (int i = 0; i < _entries.Count; i++)
                result = _entries[i].Apply(result);
            return result;
        }

        public override string ToString()
        {
            if (_entries.Count == 0) return "(empty)";
            return string.Join(" ", _entries);
        }
    }
}
