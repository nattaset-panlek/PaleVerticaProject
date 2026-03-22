using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;

namespace CoreSystem.Data.Items
{
    public enum ConditionOperator
    {
        GreaterThan,
        LessThan,
        Equal,
        GreaterOrEqual,
        LessOrEqual
    }

    public enum ConditionOperatorJoin
    {
        None,
        Add,
        Subtract
    }

    [System.Serializable]
    public class ConditionLogic
    {
        public AttributeType PrimaryAttribute;
        
        public ConditionOperatorJoin JoinOperation = ConditionOperatorJoin.None;
        public AttributeType SecondaryAttribute;

        public ConditionOperator Operator;
        public float TargetValue;

        public bool Evaluate(Character player)
        {
            if (player == null || player.attributeSystem == null) return false;
            
            float value1 = player.attributeSystem.GetAttribute(PrimaryAttribute);
            float value2 = 0f;
            
            if (JoinOperation != ConditionOperatorJoin.None)
            {
                value2 = player.attributeSystem.GetAttribute(SecondaryAttribute);
            }

            float finalValue = value1;
            if (JoinOperation == ConditionOperatorJoin.Add) finalValue += value2;
            else if (JoinOperation == ConditionOperatorJoin.Subtract) finalValue -= value2;

            switch (Operator)
            {
                case ConditionOperator.GreaterThan: return finalValue > TargetValue;
                case ConditionOperator.LessThan: return finalValue < TargetValue;
                case ConditionOperator.Equal: return Mathf.Approximately(finalValue, TargetValue);
                case ConditionOperator.GreaterOrEqual: return finalValue >= TargetValue;
                case ConditionOperator.LessOrEqual: return finalValue <= TargetValue;
                default: return false;
            }
        }

        public string GetDescription()
        {
            string opStr = Operator switch
            {
                ConditionOperator.GreaterThan => ">",
                ConditionOperator.LessThan => "<",
                ConditionOperator.Equal => "=",
                ConditionOperator.GreaterOrEqual => ">=",
                ConditionOperator.LessOrEqual => "<=",
                _ => "?"
            };

            string attrStr = PrimaryAttribute.ToString();
            if (JoinOperation != ConditionOperatorJoin.None)
            {
                string joinStr = JoinOperation == ConditionOperatorJoin.Add ? "+" : "-";
                attrStr = $"({PrimaryAttribute} {joinStr} {SecondaryAttribute})";
            }

            return $"If {attrStr} {opStr} {TargetValue}";
        }
    }
}
