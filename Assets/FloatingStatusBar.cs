using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingStatusBar : MonoBehaviour
{
    public Character character;

    [Header("Reference UI")]
    public GameObject healthBar;
    public GameObject manaBar;

    public Image healthFill;
    public Image manaFill;

    private void Awake()
    {
        // Try finding references if not assigned in inspector
        if (healthFill == null || manaFill == null)
            FindReferences();
    }

    private void Start()
    {
        if (character != null)
        {
            Initialize(character);
        }
    }

    public void Initialize(Character target)
    {
        // Unsubscribe from previous if any
        if (character != null && character.attributeSystem != null)
        {
            character.attributeSystem.OnAttributesChanged -= UpdateUI;
        }

        character = target;

        if (character != null && character.attributeSystem != null)
        {
            character.attributeSystem.OnAttributesChanged += UpdateUI;
            UpdateUI();

            // Setup visibility based on character type
            if (character is PlayerCharacter)
            {
                if (healthBar != null) healthBar.SetActive(true);
                if (manaBar != null) manaBar.SetActive(true);
            }
            else
            {
                if (healthBar != null) healthBar.SetActive(true);
                if (manaBar != null) manaBar.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        if (character != null && character.attributeSystem != null)
        {
            character.attributeSystem.OnAttributesChanged -= UpdateUI;
        }
    }

    private void FindReferences()
    {
        // Structure: Root -> LocalStatus -> [HealthSection/ManaSection] -> [Health/Mana] -> Value
        Transform localStatus = transform.Find("LocalStatus");
        if (localStatus == null) return;

        // Find Health
        Transform hpSec = localStatus.Find("HealthSection");
        if (hpSec != null)
        {
            healthBar = hpSec.gameObject;
            Transform valueObj = hpSec.Find("Health/Value");
            if (valueObj != null) healthFill = valueObj.GetComponent<Image>();
        }

        // Find Mana
        Transform mpSec = localStatus.Find("ManaSection");
        if (mpSec != null)
        {
            manaBar = mpSec.gameObject;
            Transform valueObj = mpSec.Find("Mana/Value");
            if (valueObj != null) manaFill = valueObj.GetComponent<Image>();
        }
    }

    public void UpdateUI()
    {
        if (character == null || character.attributeSystem == null) return;

        var hpAttr = character.attributeSystem.GetAttribute(AttributeType.HP);
        var maxHpAttr = character.attributeSystem.GetAttribute(AttributeType.MaxHP);
        var mpAttr = character.attributeSystem.GetAttribute(AttributeType.MP);
        var maxMpAttr = character.attributeSystem.GetAttribute(AttributeType.MaxMP);

        if (healthFill != null && hpAttr != null && maxHpAttr != null)
        {
            float maxHp = maxHpAttr.CurrentValue;
            healthFill.fillAmount = maxHp > 0 ? hpAttr.CurrentValue / maxHp : 0f;
        }

        if (manaFill != null && mpAttr != null && maxMpAttr != null)
        {
            float maxMp = maxMpAttr.CurrentValue;
            manaFill.fillAmount = maxMp > 0 ? mpAttr.CurrentValue / maxMp : 0f;
        }
    }
}
