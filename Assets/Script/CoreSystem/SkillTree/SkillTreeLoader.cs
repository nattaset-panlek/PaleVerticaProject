using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using CoreSystem.SkillTree.Models;

namespace CoreSystem.SkillTree
{
    public static class SkillTreeLoader
    {
        #region Variables/Fields

        private const string JSON_PATH = "Data/SkillTree";

        [System.Serializable]
        private class RawRoot
        {
            public List<RawNode>       nodes;
            public List<SkillTreeEdge> edges;
        }

        [System.Serializable]
        private class RawNode
        {
            public int    id;
            public string label;
            public string kind;
            public string role;
            public float  x;
            public float  y;
            public bool   isActive;
            public string linkStyle;
            public string shape;
            public int    polySides;
            public string iconPath;
            public List<AttributeBonus> extras;
        }

        #endregion

        #region Public Methods

        /// <summary>โหลดและแปลง SkillTree.json → SkillTreeData</summary>
        public static SkillTreeData Load()
        {
            TextAsset file = Resources.Load<TextAsset>(JSON_PATH);
            if (file == null)
            {
                Debug.LogError($"[SkillTreeLoader] ไม่พบ Resources/{JSON_PATH}.json");
                return null;
            }

            try
            {
                var raw = JsonConvert.DeserializeObject<RawRoot>(file.text);
                return Convert(raw);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SkillTreeLoader] parse ล้มเหลว: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Private Methods

        private static SkillTreeData Convert(RawRoot raw)
        {
            if (raw == null) return null;

            var data = new SkillTreeData
            {
                nodes = new List<SkillTreeNode>(),
                edges = raw.edges ?? new List<SkillTreeEdge>()
            };

            if (raw.nodes == null) return data;

            foreach (var r in raw.nodes)
            {
                data.nodes.Add(new SkillTreeNode
                {
                    id        = r.id,
                    label     = r.label,
                    kind      = r.kind,
                    role      = r.role,
                    x         = r.x,
                    y         = r.y,
                    isActive  = r.isActive,
                    linkStyle = r.linkStyle,
                    shape     = r.shape,
                    polySides = r.polySides,
                    iconPath  = r.iconPath,
                    extras    = r.extras ?? new List<AttributeBonus>()
                });
            }

            return data;
        }

        #endregion
    }
}
