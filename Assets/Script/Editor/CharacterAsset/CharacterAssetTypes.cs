using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Editor.CharacterAsset
{
    public enum CharacterCheckSeverity
    {
        Pass,
        Info,
        Warning,
        Error
    }

    [Serializable]
    public class CharacterPartSpec
    {
        #region [Variables/Fields]
        public string Name;
        public Vector2 Anchor;
        public Vector2 Joint;
        public RectInt ReferenceBox;
        public int SortingOrder;
        public string RendererPath;
        public bool BodyCoversJoint;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public CharacterPartSpec()
        {
        }

        public CharacterPartSpec(string name, Vector2 anchor, Vector2 joint, RectInt referenceBox, int sortingOrder, string rendererPath, bool bodyCoversJoint)
        {
            Name = name;
            Anchor = anchor;
            Joint = joint;
            ReferenceBox = referenceBox;
            SortingOrder = sortingOrder;
            RendererPath = rendererPath;
            BodyCoversJoint = bodyCoversJoint;
        }
        #endregion

        #region [Private Methods]
        #endregion
    }

    public readonly struct CharacterCheckResult
    {
        #region [Variables/Fields]
        public readonly CharacterCheckSeverity Severity;
        public readonly string Message;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public CharacterCheckResult(CharacterCheckSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }
        #endregion

        #region [Private Methods]
        #endregion
    }

    public class CharacterLayerInfo
    {
        #region [Variables/Fields]
        public string Name;
        public bool IsGroup;
        public bool IsVisible;
        public bool IsImported;
        public int ParentIndex;
        public int LayerId;
        public string SpriteId;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        #endregion

        #region [Private Methods]
        #endregion
    }

    public class CharacterSpriteInfo
    {
        #region [Variables/Fields]
        public string Name;
        public string SpriteId;
        public Rect Rect;
        public Vector2 Pivot;
        public SpriteAlignment Alignment;
        public Vector2 CanvasPosition;
        #endregion

        #region [Properties]
        public Rect CanvasRect => new Rect(CanvasPosition, Rect.size);
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        #endregion

        #region [Private Methods]
        #endregion
    }

    public class CharacterPsbData
    {
        #region [Variables/Fields]
        public string AssetPath;
        public Vector2Int CanvasSize;
        public int FileVersion;
        public readonly List<CharacterLayerInfo> Layers = new List<CharacterLayerInfo>();
        public readonly List<CharacterSpriteInfo> Sprites = new List<CharacterSpriteInfo>();
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public CharacterSpriteInfo FindSprite(string name)
        {
            return Sprites.Find(s => s.Name == name);
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
