using UnityEngine;
using TMPro;
using System.Collections.Generic;
using CoreSystem;

namespace CoreSystem.UI
{
    public class DamageTextManager : MonoBehaviour
    {
        private static DamageTextManager _instance;
        public static DamageTextManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<DamageTextManager>();
                return _instance;
            }
        }

        [Header("Settings")]
        public GameObject DamageTextPrefab;
        public Canvas UICanvas;

        [Header("Icon - Type")]
        public Sprite IconPhysical;
        public Sprite IconCritical;
        public Sprite IconMagic;
        public Sprite IconStatus;
        public Sprite IconElemental;



        [Header("Colors - Types")]
        public Color ColorPhysical = Color.white;
        public Color ColorCritcal = Color.red;
        public Color ColorMagic = new Color(0.6f, 0.4f, 1.0f); // Purple
        public Color ColorStatus = Color.yellow;

        [Header("Colors - Elements")]
        public Color ColorFire = Color.red;
        public Color ColorWater = new Color(0.2f, 0.6f, 1.0f);
        public Color ColorEarth = new Color(0.6f, 0.4f, 0.2f);
        public Color ColorHoly = new Color(1.0f, 0.9f, 0.5f);
        public Color ColorElectric = Color.yellow;
        public Color ColorGhost = new Color(0.7f, 0.7f, 1.0f);

        private void Awake()
        {
            if (_instance == null) _instance = this;
            else if (_instance != this) Destroy(gameObject);
            
            if (UICanvas == null)
                UICanvas = GameObject.FindObjectOfType<Canvas>();
        }

        public void ShowDamage(Vector3 worldPos, float amount, DamageType type, ElementType element = ElementType.Neutral, bool isCritical = false)
        {
            if (DamageTextPrefab == null) { Debug.LogWarning("[DamageTextManager] DamageTextPrefab is not assigned!"); return; }
            if (UICanvas == null) { Debug.LogWarning("[DamageTextManager] UICanvas is not assigned!"); return; }
            
            Debug.Log($"[DamageTextManager] ShowDamage: {amount}, Type: {type}, Element: {element}, Critical: {isCritical}");

            GameObject go = Instantiate(DamageTextPrefab, UICanvas.transform);
            var behaviour = go.GetComponent<DamageTextBehaviour>();
            
            if (behaviour == null)
                behaviour = go.AddComponent<DamageTextBehaviour>();

            Color targetColor = GetColor(type, element);
            if (isCritical)
            {
                targetColor = ColorCritcal;
            }

            Vector3 spawnPos = worldPos + (Vector3)Random.insideUnitCircle * 0.3f;
            
            Sprite targetIcon = GetIcon(type, element, isCritical);
            Debug.Log($"[DamageTextManager] Selected Icon: {(targetIcon != null ? targetIcon.name : "null")}");
            behaviour.Initialize(spawnPos, amount, targetColor, UICanvas, targetIcon, isCritical);
        }

        private Color GetColor(DamageType type, ElementType element)
        {
            if (type == DamageType.Elemental || element != ElementType.Neutral)
            {
                switch (element)
                {
                    case ElementType.Fire: return ColorFire;
                    case ElementType.Water: return ColorWater;
                    case ElementType.Earth: return ColorEarth;
                    case ElementType.Holy: return ColorHoly;
                    case ElementType.Electric: return ColorElectric;
                    case ElementType.Ghost: return ColorGhost;
                }
            }

            switch (type)
            {
                case DamageType.Physical: return ColorPhysical;
                case DamageType.Magic: return ColorMagic;
                case DamageType.Status: return ColorStatus;
                default: return Color.white;
            }
        }

        private Sprite GetIcon(DamageType type, ElementType element, bool isCritical)
        {
            if (isCritical) return IconCritical != null ? IconCritical : IconPhysical;

            if (type == DamageType.Elemental || element != ElementType.Neutral)
            {
                return IconElemental;
            }

            switch (type)
            {
                case DamageType.Physical: return IconPhysical;
                case DamageType.Magic: return IconMagic;
                case DamageType.Status: return IconStatus;
                default: return null;
            }
        }
    }
}
