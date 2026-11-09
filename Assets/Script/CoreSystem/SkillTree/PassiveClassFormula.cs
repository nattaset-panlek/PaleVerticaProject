using System;
using System.Collections.Generic;
using System.Linq;
using CoreSystem.Entity;

namespace CoreSystem.SkillTree
{
    public static class PassiveClassFormula
    {
        #region [Variables/Fields]
        public const int NoOrigin = -1;
        public const int RequiredJobLevel = 1;
        #endregion

        #region [Public Methods]
        public static int OriginOf(IEnumerable<PassiveNodeRecord> records, Func<int, bool> isOrigin)
        {
            foreach (PassiveNodeRecord record in records)
            {
                if (record != null && isOrigin(record.NodeId)) return record.NodeId;
            }
            return NoOrigin;
        }

        public static bool CanChoose(IEnumerable<PassiveNodeRecord> records, Func<int, bool> isOrigin, int jobLevel)
        {
            return jobLevel >= RequiredJobLevel && OriginOf(records, isOrigin) == NoOrigin;
        }

        public static int ResetKeepingOrigin(List<PassiveNodeRecord> records, Func<int, bool> isOrigin)
        {
            int refund = records.Where(r => r != null && !isOrigin(r.NodeId)).Select(r => r.NodeId).Distinct().Count();
            records.RemoveAll(r => r == null || !isOrigin(r.NodeId));
            return refund;
        }

        public static int ChangeOrigin(List<PassiveNodeRecord> records, Func<int, bool> isOrigin, IEnumerable<PassiveNodeRecord> newOrigin)
        {
            int refund = ResetKeepingOrigin(records, isOrigin);
            records.Clear();
            records.AddRange(newOrigin);
            return refund;
        }
        #endregion
    }
}
