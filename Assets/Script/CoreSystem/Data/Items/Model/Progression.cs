using CoreSystem.Data.Items;
using CoreSystem;

namespace CoreSystem.Data.Items.Model
{
    public enum ProgressionKind
    {
        BaseExp,
        JobExp,
        General
    }

    public class Progression : Item
    {
        #region [Variables/Fields]
        #endregion

        #region [Properties]
        public ProgressionKind ProgressionKind { get; set; } = ProgressionKind.General;
        #endregion

        #region [Public Methods]
        public override void OnUse()
        {
        }

        public override Item Clone()
        {
            return new Progression
            {
                Id = this.Id,
                Code = this.Code,
                Name = this.Name,
                Description = this.Description,
                IsStackable = this.IsStackable,
                Type = this.Type,
                Rarity = this.Rarity,
                ProgressionKind = this.ProgressionKind,
                IconPath = this.IconPath
            };
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
