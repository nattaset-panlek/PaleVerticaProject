using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;
using CoreSystem.SkillTree.Models;
using CoreSystem.UI.Utility;
using CoreSystem;

public class PassiveSkillNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region [Variables/Fields]

    public PassiveSkillType Type;
    public SkillTreeNode Node;

    public Image Icon;
    public GameObject CanUpgradeFrame;
    public GameObject UpgradeFrame;
    public GameObject SelectedFrame;

    private Button _button;

    #endregion

    #region [Unity Methods]

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    #endregion

    #region [Public Methods]

    public void SetUp(SkillTreeNode node, Vector2 position, PassiveSkillState state)
    {
        this.Node = node;
        
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = position;
        else transform.localPosition = position;

        if (_button == null) _button = GetComponent<Button>();
        if (_button != null) 
        {
            _button.interactable = (state != PassiveSkillState.CannotUpgrade);
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => {
                PassiveSkillLoader.Instance?.OnNodeClick(node.id);
            });
        }

        UpdateVisuals(state);
    }

    #endregion

    #region [Private Methods]

    private void UpdateVisuals(PassiveSkillState state)
    {
        if (CanUpgradeFrame == null || UpgradeFrame == null) return;

        switch (state)
        {
            case PassiveSkillState.CanUpgrade:
                CanUpgradeFrame.SetActive(true);
                UpgradeFrame.SetActive(false);
                break;
            case PassiveSkillState.Upgraded:
                CanUpgradeFrame.SetActive(false);
                UpgradeFrame.SetActive(true);
                break;
            case PassiveSkillState.CannotUpgrade:
                CanUpgradeFrame.SetActive(false);
                UpgradeFrame.SetActive(false);
                if (SelectedFrame != null) SelectedFrame.SetActive(false);
                break;
            case PassiveSkillState.Selected:
                CanUpgradeFrame.SetActive(false);
                UpgradeFrame.SetActive(false);
                if (SelectedFrame != null) SelectedFrame.SetActive(true);
                break;
        }
    }

    #endregion

    #region [UI Hover Events]

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Node == null) return;
        
        string title = string.IsNullOrEmpty(Node.label) ? "Unknown Node" : Node.label;
        StringBuilder context = new StringBuilder();

        if(Node.IsBaseClass)
        {
            context.AppendLine($"<b>Unlock Knowledge Classes <color=#62FFFF>{Node.label}</color></b>");
        }
            
        if (Node.HasStats)
        {
            context.AppendLine("<b>Bonus Stats</b>");
            foreach (var bonus in Node.extras)
            {
                AttributeType type = (AttributeType)bonus.id;
                string label = type.ToString();
                
                // Formatting improvements for Percentage attributes
                if (label.EndsWith("Percent"))
                {
                    label = label.Replace("Percent", " (%)");
                }
                
                context.AppendLine($"+{bonus.value} {label}");
            }
        }
        
        // Show tooltip 
        HoverTextManager.Instance?.Show(title, context.ToString().TrimEnd());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HoverTextManager.Instance?.Hide();
    }

    #endregion
}
