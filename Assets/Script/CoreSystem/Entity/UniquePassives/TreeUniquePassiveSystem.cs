using System.Collections.Generic;

namespace CoreSystem.Entity.UniquePassives
{
    public sealed class TreeUniquePassiveSystem
    {
        #region [Variables/Fields]
        private readonly HashSet<string> _granted = new HashSet<string>();
        #endregion

        #region [Public Methods]
        public bool IsGranted(string passiveId)
        {
            return _granted.Contains(passiveId);
        }

        public void Sync(IEnumerable<string> wanted, UniquePassiveSystem target)
        {
            var desired = new HashSet<string>(wanted);
            _granted.RemoveWhere(id => !target.HasPassive(id));

            foreach (string id in new List<string>(_granted))
            {
                if (desired.Contains(id)) continue;
                target.RemovePassive(id);
                _granted.Remove(id);
            }

            foreach (string id in desired)
            {
                if (target.HasPassive(id)) continue;
                target.AddPassive(id);
                if (target.HasPassive(id)) _granted.Add(id);
            }
        }
        #endregion
    }
}
