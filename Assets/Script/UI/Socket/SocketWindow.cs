using System.Collections.Generic;
using TMPro;
using CoreSystem.UI.Inventory.Utility;
using UnityEngine;
using UnityEngine.UI;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.UI.ActionProgress;
using CoreSystem.UI.Utility;

namespace CoreSystem.UI.Socket
{
    public class SocketWindow : UIWindow, IItemInsertTarget
    {
        #region [Variables/Fields]
        [Header("Header & Item Info")]
        [SerializeField] private TMP_Text _headerTitle;
        [SerializeField] private TMP_Text _itemNameText;
        [SerializeField] private TMP_Text _itemTypeText;
        [SerializeField] private TMP_Text _rarityText;
        [SerializeField] private Image _rarityBackground;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Sprite _emptyItemSprite;
        [SerializeField] private ItemSlotArtView _itemArt;

        [Header("Cost & Buttons")]
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private Button _infuseButton;
        [SerializeField] private TMP_Text _buttonText;
        [SerializeField] private Button _closeButton;

        [Header("Socket Slots")]
        [SerializeField] private List<Button> _socketSlotButtons = new List<Button>();
        [SerializeField] private List<Image> _socketSlotBackgrounds = new List<Image>();
        [SerializeField] private List<Image> _socketSlotIcons = new List<Image>();
        [SerializeField] private List<GameObject> _socketFilledBackings = new List<GameObject>();
        [SerializeField] private List<GameObject> _socketEmptyIndicators = new List<GameObject>();
        [SerializeField] private List<GameObject> _socketActiveHighlights = new List<GameObject>();

        public const int CenterSlot = -2;
        public const int SocketCost = 5000;
        public const int MaxSocketCount = 4;
        public const string EquipmentInsertLabel = "Insert to Socket";
        public const string FragmentInsertLabel = "Insert Fragment";
        public const float InfuseLoadingSeconds = 0.5f;
        public const string InfuseActionType = "SOCKET";

        private static readonly Color SocketedIconTint = Color.black;
        private static readonly Color PendingIconTint = new Color(0f, 0f, 0f, 0.55f);

        private EquipmentInstance _targetEquipment;
        private ItemInstance _pendingFragment;
        private PlayerInventory _boundInventory;
        private Color _emptyRarityColor = Color.white;
        #endregion

        #region [Properties]
        public EquipmentInstance TargetEquipment => _targetEquipment;
        public ItemInstance PendingFragment => _pendingFragment;
        public int PendingSocketIndex => _pendingFragment != null ? SocketedCount : -1;
        public int SocketedCount => _targetEquipment != null ? _targetEquipment.SocketedFragments.Count : 0;
        public int UnlockedSocketCount => GetUnlockedSocketCount(_targetEquipment);
        public bool IsInfusing => ActionProgressWindow.IsRunningFor(this);
        #endregion

        #region [Unity Methods]
        protected override void Awake()
        {
            base.Awake();

            if (_infuseButton != null)
            {
                _infuseButton.onClick.AddListener(ExecuteInfuseSocket);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(CloseWindow);
            }

            if (_itemIcon != null && _emptyItemSprite == null)
            {
                _emptyItemSprite = _itemIcon.sprite;
            }

            if (_rarityBackground == null && _rarityText != null && _rarityText.transform.parent != null)
            {
                _rarityBackground = _rarityText.transform.parent.GetComponent<Image>();
            }

            if (_rarityBackground != null)
            {
                _emptyRarityColor = _rarityBackground.color;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ItemInsertRegistry.Register(this);
            BindInventory();
            UpdateUI();
        }

        private void OnDisable()
        {
            ItemInsertRegistry.Unregister(this);
            UnbindInventory();
        }
        #endregion

        #region [Public Methods]
        public override void OnOpen(object data)
        {
            base.OnOpen(data);
            _targetEquipment = data as EquipmentInstance;
            _pendingFragment = null;
            BindInventory();
            UpdateUI();
        }

        public void SetTarget(EquipmentInstance equipment)
        {
            _targetEquipment = equipment;
            _pendingFragment = null;
            UpdateUI();
        }

        public bool SetPendingFragment(ItemInstance fragment)
        {
            if (!CanStageFragment(fragment, ItemInsertRegistry.AnySlot)) return false;
            _pendingFragment = fragment;
            UpdateUI();
            return true;
        }

        public bool CanInsert(ItemInstance item, int slotIndex)
        {
            if (IsInfusing) return false;
            if (item is EquipmentInstance) return slotIndex == ItemInsertRegistry.AnySlot || slotIndex == CenterSlot;
            if (item != null && item.Item is Fragment) return slotIndex != CenterSlot && CanStageFragment(item, slotIndex);
            return false;
        }

        public bool TryInsert(ItemInstance item, int slotIndex)
        {
            if (!CanInsert(item, slotIndex)) return false;
            if (item is EquipmentInstance equipment)
            {
                SetTarget(equipment);
                return true;
            }
            _pendingFragment = item;
            UpdateUI();
            return true;
        }

        public void ClearInsert(int slotIndex)
        {
            if (IsInfusing) return;
            if (slotIndex == CenterSlot)
            {
                if (_targetEquipment != null) SetTarget(null);
                return;
            }

            if (_pendingFragment == null) return;
            if (slotIndex == ItemInsertRegistry.AnySlot || slotIndex == PendingSocketIndex)
            {
                _pendingFragment = null;
                UpdateUI();
            }
        }

        public string GetInsertLabel(ItemInstance item)
        {
            return item is EquipmentInstance ? EquipmentInsertLabel : FragmentInsertLabel;
        }

        public void ExecuteInfuseSocket()
        {
            if (IsInfusing) return;
            PlayerInventory inventory = GameData.Player != null ? GameData.Player.Inventory : null;
            if (!CanInfuse(inventory)) return;

            EquipmentInstance target = _targetEquipment;
            ItemInstance fragmentInstance = _pendingFragment;
            if (_infuseButton != null) _infuseButton.interactable = false;
            ActionProgressWindow.Show(new ActionProgressRequest
            {
                ActionName = $"{(fragmentInstance.Item != null ? fragmentInstance.Item.Name : "Fragment")} > {(target.Item != null ? target.Item.Name : "Equipment")}",
                ActionType = InfuseActionType,
                Icon = fragmentInstance.Item != null ? fragmentInstance.Item.Icon : null,
                Duration = InfuseLoadingSeconds,
                UseUnscaledTime = true,
                Color = ActionProgressColor.Cyan,
                Owner = this,
                OnCompleted = () => CompleteInfuse(target, fragmentInstance),
                OnCancelled = UpdateUI
            });
        }

        public void CompleteInfuse(EquipmentInstance target, ItemInstance fragmentInstance)
        {
            PlayerInventory inventory = GameData.Player != null ? GameData.Player.Inventory : null;
            if (target != _targetEquipment || fragmentInstance != _pendingFragment || !CanInfuse(inventory))
            {
                UpdateUI();
                return;
            }

            Fragment fragment = (Fragment)fragmentInstance.Item;
            int before = target.SocketedFragments.Count;

            if (!inventory.SpendCredit(SocketCost))
            {
                UpdateUI();
                return;
            }

            if (GameData.Player.Equipment != null)
            {
                GameData.Player.Equipment.SocketFragment(target, fragmentInstance);
            }
            else if (inventory.RemoveItemByInstance(fragmentInstance.Id, 1))
            {
                target.SocketedFragments.Add(fragment);
            }

            if (target.SocketedFragments.Count == before)
            {
                inventory.AddCredit(SocketCost);
            }

            _pendingFragment = null;
            UpdateUI();
        }

        public bool CanInfuse(PlayerInventory inventory)
        {
            if (_targetEquipment == null || _pendingFragment == null || inventory == null) return false;
            if (SocketedCount >= UnlockedSocketCount) return false;
            if (inventory.GetByInstanceId(_pendingFragment.Id) == null) return false;
            return inventory.HasCredit(SocketCost);
        }

        public static int GetUnlockedSocketCount(EquipmentInstance equipment)
        {
            Equipment template = equipment != null ? equipment.Item as Equipment : null;
            return template != null ? Mathf.Clamp(template.MaxSockets, 0, MaxSocketCount) : 0;
        }

        public void CloseWindow()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.Close(WindowName);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        #endregion

        #region [Private Methods]
        private bool CanStageFragment(ItemInstance fragment, int slotIndex)
        {
            if (_targetEquipment == null || fragment == null || !(fragment.Item is Fragment)) return false;

            int socketed = SocketedCount;
            int unlocked = UnlockedSocketCount;
            if (socketed >= unlocked) return false;
            if (slotIndex == ItemInsertRegistry.AnySlot) return true;
            return slotIndex >= socketed && slotIndex < unlocked;
        }

        private void BindInventory()
        {
            PlayerInventory inventory = GameData.Player != null ? GameData.Player.Inventory : null;
            if (inventory == _boundInventory) return;
            UnbindInventory();
            _boundInventory = inventory;
            if (_boundInventory != null) _boundInventory.OnInventoryChanged += HandleInventoryChanged;
        }

        private void UnbindInventory()
        {
            if (_boundInventory != null) _boundInventory.OnInventoryChanged -= HandleInventoryChanged;
            _boundInventory = null;
        }

        private void HandleInventoryChanged()
        {
            if (_pendingFragment != null && _boundInventory != null && _boundInventory.GetByInstanceId(_pendingFragment.Id) == null)
            {
                _pendingFragment = null;
            }
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_itemArt != null) _itemArt.Show(ItemSlotDisplay.From(_targetEquipment));
            if (_targetEquipment == null)
            {
                if (_itemNameText != null) _itemNameText.text = "SELECT EQUIPMENT";
                if (_itemTypeText != null) _itemTypeText.text = string.Empty;
                if (_rarityText != null) _rarityText.text = "None";
                if (_rarityBackground != null) _rarityBackground.color = _emptyRarityColor;
                if (_itemIcon != null)
                {
                    _itemIcon.sprite = null;
                    _itemIcon.enabled = false;
                }
                if (_costText != null) _costText.text = $"{SocketCost:N0} CREDITS";
                if (_infuseButton != null) _infuseButton.interactable = false;
                UpdateSocketVisuals();
                return;
            }

            Equipment template = _targetEquipment.Item as Equipment;

            if (_itemNameText != null)
            {
                _itemNameText.text = template != null ? template.Name.ToUpperInvariant() : "EQUIPMENT";
            }

            if (_itemTypeText != null)
            {
                _itemTypeText.text = template != null ? template.Slot.ToString().ToUpperInvariant() : "ITEM";
            }

            if (_rarityText != null)
            {
                _rarityText.text = template != null ? template.Rarity.ToString().ToUpperInvariant() : "COMMON";
            }

            if (_rarityBackground != null)
            {
                _rarityBackground.color = template != null ? template.Rarity.GetColor() : _emptyRarityColor;
            }

            if (_itemIcon != null)
            {
                _itemIcon.sprite = template != null && template.Icon != null ? template.Icon : _emptyItemSprite;
                _itemIcon.enabled = _itemIcon.sprite != null;
            }

            if (_costText != null)
            {
                _costText.text = $"{SocketCost:N0} CREDITS";
            }

            if (_infuseButton != null)
            {
                _infuseButton.interactable = !IsInfusing && CanInfuse(GameData.Player != null ? GameData.Player.Inventory : null);
            }

            UpdateSocketVisuals();
        }

        private void UpdateSocketVisuals()
        {
            int socketedCount = SocketedCount;
            int unlockedCount = UnlockedSocketCount;
            int pendingIndex = PendingSocketIndex;

            for (int i = 0; i < _socketSlotButtons.Count; i++)
            {
                bool isUnlocked = i < unlockedCount;
                bool isSocketed = i < socketedCount;
                bool isPending = i == pendingIndex;

                if (_socketSlotButtons[i] != null)
                {
                    _socketSlotButtons[i].interactable = isUnlocked;
                }

                if (_socketEmptyIndicators.Count > i && _socketEmptyIndicators[i] != null)
                {
                    _socketEmptyIndicators[i].SetActive(isUnlocked && !isSocketed && !isPending);
                }

                Sprite icon = null;
                if (isSocketed)
                {
                    Fragment frag = _targetEquipment.SocketedFragments[i];
                    icon = frag != null ? frag.Icon : null;
                }
                else if (isPending && _pendingFragment.Item != null)
                {
                    icon = _pendingFragment.Item.Icon;
                }

                if (_socketSlotIcons.Count > i && _socketSlotIcons[i] != null)
                {
                    Image iconImage = _socketSlotIcons[i];
                    iconImage.sprite = icon;
                    iconImage.color = isPending ? PendingIconTint : SocketedIconTint;
                    iconImage.enabled = icon != null;
                    iconImage.gameObject.SetActive(icon != null);
                }

                if (_socketFilledBackings.Count > i && _socketFilledBackings[i] != null)
                {
                    _socketFilledBackings[i].SetActive(isSocketed || isPending);
                }

                if (_socketActiveHighlights.Count > i && _socketActiveHighlights[i] != null)
                {
                    _socketActiveHighlights[i].SetActive(isPending);
                }
            }
        }
        #endregion
    }
}
