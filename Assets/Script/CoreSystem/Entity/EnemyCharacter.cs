using CoreSystem.Attributes;

namespace CoreSystem.Entity
{
    public class EnemyCharacter : Character
    {
        #region [Public Methods]
        public override void Initialize()
        {
            base.Initialize();

            if (CharacterId == 10 || gameObject.name.Contains("Dummy"))
            {
                var maxHpAttr = attributeSystem.GetAttribute(AttributeType.MaxHP);
                if (maxHpAttr != null)
                {
                    maxHpAttr.InitialValue = 9999999f;
                    maxHpAttr.Recalculate();
                }

                var hpAttr = attributeSystem.GetAttribute(AttributeType.HP);
                if (hpAttr != null)
                {
                    hpAttr.BaseValue = maxHpAttr.CurrentValue;
                    hpAttr.Recalculate();
                }
            }
        }
        #endregion
    }
}
