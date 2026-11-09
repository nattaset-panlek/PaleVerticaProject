using System;
using System.Collections.Generic;
using System.Linq;
using CoreSystem.Entity;
using CoreSystem.Player;
using CoreSystem.SkillTree;
using CoreSystem.SkillTree.Models;

namespace CoreSystem.Data
{
    public class PlayerClass
    {
        #region [Variables/Fields]
        public event Action OnClassChanged;
        #endregion

        #region [Properties]
        public int OriginNodeId
        {
            get
            {
                Character player = PlayerCore.GetPlayer();
                return player != null ? PassiveClassFormula.OriginOf(player.PassiveNodes, IsOrigin) : PassiveClassFormula.NoOrigin;
            }
        }

        public bool HasClass => OriginNodeId != PassiveClassFormula.NoOrigin;
        #endregion

        #region [Public Methods]
        public bool ChooseClass(int originNodeId)
        {
            Character player = PlayerCore.GetPlayer();
            if (player == null || !IsOrigin(originNodeId)) return false;
            if (!PassiveClassFormula.CanChoose(player.PassiveNodes, IsOrigin, player.JobLevel)) return false;

            player.PassiveNodes.AddRange(RecordsFor(originNodeId));
            Apply(player);
            return true;
        }

        public int ResetAllPassives()
        {
            Character player = PlayerCore.GetPlayer();
            if (player == null) return 0;

            int refund = PassiveClassFormula.ResetKeepingOrigin(player.PassiveNodes, IsOrigin);
            player.PassivePoints += refund;
            Apply(player);
            return refund;
        }

        public int ChangeClass(int originNodeId)
        {
            Character player = PlayerCore.GetPlayer();
            if (player == null || !IsOrigin(originNodeId)) return 0;

            int refund = PassiveClassFormula.ChangeOrigin(player.PassiveNodes, IsOrigin, RecordsFor(originNodeId));
            player.PassivePoints += refund;
            Apply(player);
            return refund;
        }
        #endregion

        #region [Private Methods]
        private static bool IsOrigin(int nodeId)
        {
            SkillTreeNode node = SkillTreeDatabase.GetNode(nodeId);
            return node != null && node.IsBaseClass;
        }

        private static List<PassiveNodeRecord> RecordsFor(int nodeId)
        {
            SkillTreeNode node = SkillTreeDatabase.GetNode(nodeId);
            if (!node.HasStats) return new List<PassiveNodeRecord> { new PassiveNodeRecord(nodeId, 0, 0f) };
            return node.extras.Select(bonus => new PassiveNodeRecord(nodeId, bonus.id, bonus.value)).ToList();
        }

        private void Apply(Character player)
        {
            player.RecalculateStats();
            OnClassChanged?.Invoke();
        }
        #endregion
    }
}
