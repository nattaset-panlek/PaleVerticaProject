# Global Drag & Drop System (GlobalDragDoc.md)

Welcome, AI Collaborator and Developer. This document is the definitive guide for integrating with and extending the **Global Drag & Drop System** (`GlobalDragManager`) in **Realm of Hero**.

The Global Drag System is a decoupled, centralized workflow designed to handle all drag-and-drop interactions across the UI—including Inventory Items, Equipment, Costumes, Hotbar Skills/Consumables, and future features like Mounts, Emotes, or Pet Commands.

---

## 1. Core Architecture & Design Philosophy

The system separates the **state/manager** from the **visual representation**:

- **`GlobalDragManager` (Singleton)**: Tracks the active drag session, stores the transfer data (`Payload`), references the originator (`Source`), and manages coordinate transformations across Canvas layers.
- **`GlobalDragObject` (Visual Model)**: Handles the UI presentation (icon, rarity frame, and sizing) of the dragged element while following the pointer.

### Crucial Architectural Rules

1. **Manual Visual Assignment**: `GlobalDragManager` does **not** auto-instantiate prefabs at runtime. You must place a `GlobalDragObject` in your UI Canvas hierarchy and manually assign its reference to `_dragObjectInstance` in the Inspector.
2. **No Background Manipulation**: The visual model never touches, hides, or modifies any base background objects. When dragging objects without rarity (such as Skills or Commands), only the Rarity frame is disabled (`SetActive(false)`), ensuring clean presentation.
3. **Decoupled Payloads**: Any C# object (`ItemInstance`, `HotbarSlotView`, custom `ScriptableObject`, or data struct) can be passed as a payload without modifying the drag manager.

---

## 2. Implementing a Drag Source (Sender)

To make a UI component draggable and capable of sending data to other UI elements, implement Unity's `IBeginDragHandler`, `IDragHandler`, and `IEndDragHandler` interfaces and delegate execution to `GlobalDragManager.Instance`.

### Standard Drag Source Structure

```csharp
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CoreSystem.UI.Utility;

namespace CoreSystem.UI.CustomSystem
{
    public class CustomFeatureSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region [Variables/Fields]
        [Header("UI References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private RectTransform _rectTransform;

        private CustomFeatureData _featureData; // The data payload to transfer
        private bool _isDragging;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
        }
        #endregion

        #region [Public Methods]
        public void Setup(CustomFeatureData data)
        {
            _featureData = data;
            if (_iconImage != null && data != null)
            {
                _iconImage.sprite = data.Icon;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || _featureData == null) return;

            _isDragging = true;
            Sprite icon = _iconImage != null ? _iconImage.sprite : null;
            Vector2 size = _rectTransform != null ? _rectTransform.sizeDelta : new Vector2(50, 50);

            // Determine rarity color if applicable (pass null for objects without rarity like skills/commands)
            Color? rarityColor = null; 

            GlobalDragManager.Instance.StartDrag(
                source: this,
                payload: _featureData,
                icon: icon,
                screenPos: eventData.position,
                customSize: size,
                raritySprite: null,
                rarityColor: rarityColor
            );
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isDragging)
            {
                GlobalDragManager.Instance.UpdatePosition(eventData.position);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isDragging)
            {
                GlobalDragManager.Instance.FinishDrag();
                _isDragging = false;
            }
        }
        #endregion
    }
}
```

---

## 3. Implementing a Drop Target (Receiver)

To allow a UI component to receive dragged items, implement Unity's `IDropHandler` interface. In `OnDrop`, inspect `GlobalDragManager.Payload` using C# pattern matching to identify supported data types.

### Standard Drop Target Structure

```csharp
using UnityEngine;
using UnityEngine.EventSystems;
using CoreSystem.UI.Utility;

namespace CoreSystem.UI.CustomSystem
{
    public class CustomFeatureReceiverUI : MonoBehaviour, IDropHandler
    {
        #region [Public Methods]
        public void OnDrop(PointerEventData eventData)
        {
            // 1. Check if the incoming payload is our custom data type
            if (GlobalDragManager.Payload is CustomFeatureData featureData)
            {
                AssignFeature(featureData);
                return;
            }

            // 2. You can also inspect where the drag originated from via GlobalDragManager.Source
            if (GlobalDragManager.Source is CustomFeatureSlotUI sourceSlot)
            {
                Debug.Log($"Received drop from slot: {sourceSlot.name}");
            }

            // 3. Log unsupported payloads for clean debugging
            if (GlobalDragManager.Payload != null)
            {
                Debug.LogWarning($"[CustomFeatureReceiverUI] Unsupported drop payload type: {GlobalDragManager.Payload.GetType().Name}");
            }
        }
        #endregion

        #region [Private Methods]
        private void AssignFeature(CustomFeatureData data)
        {
            Debug.Log($"Successfully assigned feature: {data.Name}");
            // Implement local assignment logic here
        }
        #endregion
    }
}
```

---

## 4. Key API Reference (`GlobalDragManager`)

### Properties

- **`public static object Payload { get; }`**: The arbitrary data object currently being dragged (e.g., `ItemInstance`, `HotbarSlotView`, or custom classes). Returns `null` when not dragging.
- **`public static MonoBehaviour Source { get; }`**: The UI component that initiated the drag. Can be cast to interfaces like `IItemProvider` to invoke source-specific callbacks.

### Methods

- **`StartDrag(MonoBehaviour source, object payload, Sprite icon, Vector2 screenPos, Vector2? customSize = null, Sprite raritySprite = null, Color? rarityColor = null)`**:
  - Initializes the drag session, displays `_dragObjectInstance` at `screenPos`, sets its size, and configures raycast blocking so drop targets can detect pointer release.
- **`UpdatePosition(Vector2 screenPos)`**:
  - Updates the local coordinates of the visual drag model inside its Canvas. Call this inside `OnDrag`.
- **`FinishDrag()`**:
  - Hides the visual drag model, restores raycast blocking, and clears `Payload` and `Source`. Call this inside `OnEndDrag`.
