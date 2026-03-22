using CoreSystem;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Effects;
using CoreSystem.Data.Items.Model;
using CoreSystem.UI.Utile;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.Inventory
{
    public class ItemInformationWindow : MonoBehaviour
    {

        [Header("Main Containers")]
        [SerializeField] private GameObject itemInfomationRect;
        [SerializeField] private GameObject itemTemplateSection;
        [SerializeField] private GameObject descriptionSection;
        [SerializeField] private GameObject itemEffectSection;
        [SerializeField] private GameObject attributeSection;
        [SerializeField] private GameObject socketSection;

        [Header("Header Elements")]
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private Button closeButton;

        [Header("Item Template Elements")]
        [SerializeField] private TMP_Text itemTypeSelectionText;
        [SerializeField] private TMP_Text quantityText;
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text rarityText;
        [SerializeField] private Image rarityLable;
        [SerializeField] private GameObject refineSection;
        [SerializeField] private TMP_Text refineValueText;

        [Header("Section Content Texts")]
        [SerializeField] private TMP_Text descriptionContentText;
        [SerializeField] private TMP_Text effectContentText;

        [Header("Scroll Content Parents")]
        [SerializeField] private Transform attributeContent;
        [SerializeField] private Transform socketContent;

        [Header("Prefabs")]
        [SerializeField] private GameObject attributeBlockPrefab;

        private ItemInstance _currentItem;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
        }

        public void Setup(ItemInstance instance)
        {
            if (instance == null || instance.Item == null) return;

            _currentItem = instance;
            Debug.Log($"[Item Info] Setting up info for {instance.Name} ({instance.Item.Name})");

            UpdateDisplay(instance);
        }

        public void Close()
        {
            if (ItemInformationManager.Instance != null && _currentItem != null)
            {
                ItemInformationManager.Instance.OnWindowClosed(_currentItem);
            }
            Destroy(gameObject);
        }


        private void UpdateDisplay(ItemInstance instance)
        {
            Item item = instance.Item;

            string ItemTypeText = "";
            string ItemSocketInfo = "";

            if (instance.Item is Equipment equipment)
            {
                ItemTypeText = equipment.Slot switch
                {
                    EquipmentSlot.WeaponOneHandLeft => "Weapon (L)",
                    EquipmentSlot.WeaponOneHandRight => "Weapon (R)",
                    EquipmentSlot.WeaponTwoHand => "Weapon",

                    EquipmentSlot.AccessoryLeft => "Accessory (L)",
                    EquipmentSlot.AccessoryRight => "Accessory (R)",
                    EquipmentSlot.Accessory => "Accessory",

                    _ => equipment.Slot.ToString()
                };

                if (equipment.MaxSockets > 0)
                {
                    ItemSocketInfo = $"[{equipment.MaxSockets}] ";
                }

            }
            else
            {
                ItemTypeText = item.Type.ToString();
            }

            itemNameText.text = $"{ItemSocketInfo}{item.Name}";

            itemTypeSelectionText.text = ItemTypeText;
            quantityText.text = instance.Quantity > 1 ? $"x{instance.Quantity}" : "x1";

            // Icon
            if (itemIcon != null)
                itemIcon.sprite = item.Icon;

            // Rarity
            if (rarityText != null)
            {
                rarityText.text = $"{item.Rarity.ToString()}";
            }

            if (rarityLable != null)
            {
                rarityLable.color = item.Rarity.GetColor();
            }

            // Refine Info (Advancement)
            if (instance is EquipmentInstance equipInst && equipInst.Advancement > 0)
            {
                refineSection.SetActive(true);
                refineValueText.text = $"{equipInst.Advancement}";
            }
            else
            {
                refineSection.SetActive(false);
            }

            // Logic for visibility
            ResetSections();

            switch (item.Type)
            {
                case ItemType.Equipment:
                    ShowEquipment(instance);
                    break;
                case ItemType.Fragment:
                    ShowFragment(item as Fragment);
                    break;
                case ItemType.SkillRune:
                    ShowSkillRune(item as SkillRune);
                    break;
                case ItemType.Consumable:
                    ShowConsumable(item as Consumable);
                    break;
                default:
                    ShowOther(item);
                    break;
            }
        }
        

        private void ResetSections()
        {
            descriptionSection.SetActive(false);
            itemEffectSection.SetActive(false);
            attributeSection.SetActive(false);
            socketSection.SetActive(false);
        }

        private void ShowEquipment(ItemInstance instance)
        {
            Equipment equip = instance.Item as Equipment;

            // Equipment shows everything according to rules
            descriptionSection.SetActive(true);



            if (instance.Item is Equipment equipment)
            {

                bool isWeapon = equipment.Slot == EquipmentSlot.WeaponOneHandLeft ||
                                equipment.Slot == EquipmentSlot.WeaponOneHandRight ||
                                equipment.Slot == EquipmentSlot.WeaponTwoHand;

                if (isWeapon)
                {
                    descriptionContentText.text = $"<color=#C62828>Physical Attack: {equip.BasePhysical}</color>, <color=#1565C0>Magic Attack: {equip.BaseMagic}</color>\n" +
                                                  $"{equip.Description}";
                }
                else
                {
                    descriptionContentText.text = $"<color=#2E7D32>Defense: {equip.BasePhysical}</color>, <color=#6A1B9A>Magic Defense: {equip.BaseMagic}</color>\n" +
                                                  $"{equip.Description}";
                }
            }
            else
            {
                descriptionContentText.text = equip.Description;
            }

            // Split Effects
            List<AttributeBonusEffect> attrEffects = new List<AttributeBonusEffect>();
            List<string> otherEffects = new List<string>();

            foreach (var effect in equip.Effects)
            {
                if (effect is AttributeBonusEffect attr)
                    attrEffects.Add(attr);
                else
                    otherEffects.Add(GetEffectDescription(effect));
            }

            if (attrEffects.Count > 0)
            {
                attributeSection.SetActive(true);
                PopulateAttributeList(attributeContent, attrEffects);
            }

            if (otherEffects.Count > 0)
            {
                itemEffectSection.SetActive(true);
                effectContentText.text = string.Join("\n", otherEffects);
            }

            // Sockets (Only if has fragments)
            if (instance is EquipmentInstance equipInstance && equipInstance.SocketedFragments != null && equipInstance.SocketedFragments.Any(f => f != null))
            {
                socketSection.SetActive(true);
                PopulateSocketList(socketContent, equipInstance.SocketedFragments);
            }
        }

        private void ShowFragment(Fragment fragment)
        {
            if (fragment == null) return;

            // Show description
            descriptionSection.SetActive(true);
            descriptionContentText.text = fragment.Description;

            // Fragment shows item effect and attribute
            List<AttributeBonusEffect> attrEffects = new List<AttributeBonusEffect>();
            List<string> otherEffects = new List<string>();

            foreach (var effect in fragment.Effects)
            {
                if (effect is AttributeBonusEffect attr)
                    attrEffects.Add(attr);
                else
                    otherEffects.Add(GetEffectDescription(effect));
            }

            if (attrEffects.Count > 0)
            {
                attributeSection.SetActive(true);
                PopulateAttributeList(attributeContent, attrEffects);
            }

            if (otherEffects.Count > 0)
            {
                itemEffectSection.SetActive(true);
                effectContentText.text = string.Join("\n", otherEffects);
            }
        }

        private void ShowSkillRune(SkillRune skillRune)
        {
            if (skillRune == null) return;
            // Skill shows item effect
            itemEffectSection.SetActive(true);
            effectContentText.text = skillRune.Description;
        }

        private void ShowConsumable(Consumable consumable)
        {
            if (consumable == null) return;
            // Consumable shows item effect
            itemEffectSection.SetActive(true);
            effectContentText.text = consumable.Description;
        }

        private void ShowOther(Item item)
        {
            if (item == null) return;
            // Others show only description
            descriptionSection.SetActive(true);
            descriptionContentText.text = item.Description;
        }

        private string GetEffectDescription(EquipmentEffect effect)
        {
            if (effect == null) return "";
            string desc = effect.GetDescription();
            if (string.IsNullOrEmpty(desc))
            {
                return effect.GetType().Name;
            }
            return desc;
        }

        private void PopulateAttributeList(Transform container, List<AttributeBonusEffect> effects)
        {
            ClearContainer(container);
            foreach (var effect in effects)
            {
                var obj = Instantiate(attributeBlockPrefab, container);
                var block = obj.GetComponent<AttributeListBlock>();
                if (block != null)
                {
                    var template = CoreSystem.Attributes.AttributeDatabase.GetAttribute(effect.Attribute);
                    block.Setup(template, effect.Value);
                }
            }
        }

        private void PopulateSocketList(Transform container, List<Fragment> fragments)
        {
            ClearContainer(container);
            foreach (var frag in fragments)
            {
                if (frag == null) continue;
                var obj = Instantiate(attributeBlockPrefab, container);
                var block = obj.GetComponent<AttributeListBlock>();
                if (block != null)
                {
                    block.Setup(frag.Name, "Socketed");
                }

                // Standard Item Hover format
                string hexColor = ItemRarityExtensions.GetHexColor(frag.Rarity);
                obj.AddComponent<HoverableUI>().Setup(
                    $"<color=#{hexColor}>{frag.Name}</color>",
                    frag.Description
                );

                // Add Clickable behavior
                var clicker = obj.AddComponent<ClickableUI>();
                clicker.OnClicked += (eventData) => {
                    // Show item info for the fragment
                    var fragInstance = new ItemInstance(-1, frag.Id, 1);
                    ItemInformationManager.Instance.Open(fragInstance);
                };
            }
        }

        // Helper component for handling clicks on socket blocks
        private class ClickableUI : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
        {
            public System.Action<UnityEngine.EventSystems.PointerEventData> OnClicked;
            public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
            {
                OnClicked?.Invoke(eventData);
            }
        }

        private void ClearContainer(Transform container)
        {
            foreach (Transform child in container)
                Destroy(child.gameObject);
        }
    }
}
