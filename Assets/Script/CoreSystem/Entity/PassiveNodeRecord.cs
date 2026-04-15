using System;

namespace CoreSystem.Entity
{
    [Serializable]
    public class PassiveNodeRecord
    {
        public int NodeId;
        public int AttributeId;
        public float Value;

        public PassiveNodeRecord() { }

        public PassiveNodeRecord(int nodeId, int attributeId, float value)
        {
            NodeId      = nodeId;
            AttributeId = attributeId;
            Value       = value;
        }

        public override string ToString() => $"(Node:{NodeId}, Attr:{AttributeId}, Val:{Value})";
    }
}
