using System.Collections.Generic;

namespace CoreSystem.Attributes
{
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
}
