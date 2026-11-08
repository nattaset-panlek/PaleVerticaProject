using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CoreSystem.Entity;
using CoreSystem.Skills;

namespace CoreSystem.UI.DevTools
{
    public class SkillStatusUI
    {
        #region [Variables/Fields]
        private readonly Character _player;
        private UIGlobalModal _modal;
        private TMP_Text _skillStateText;
        private TMP_Text _skillCastingText;
        private TMP_Text _skillCastDelayText;
        private readonly List<TMP_Text> _skillSlotTexts = new List<TMP_Text>();
        #endregion

        #region [Properties]
        public bool IsOpen => _modal != null && _modal.IsOpen;
        #endregion

        #region [Public Methods]
        public SkillStatusUI(Character player)
        {
            _player = player;
        }

        public void Open()
        {
            if (_modal != null && _modal.IsOpen) return;

            _modal = UIGlobalModal.Create("Skill System Status");
            if (_modal == null) return;

            _modal.SetTitle("Skill System Status")
                  .SetWindowSize(new Vector2(260, 290))
                  .SetScrollMovementType(ScrollRect.MovementType.Clamped)
                  .SetWindowAnchors(new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1))
                  .SetWindowPosition(new Vector2(20, -20))
                  .SetCloseButtonVisible(false);

            _modal.OnClosed += () => 
            {
                _modal = null;
                _skillSlotTexts.Clear();
            };

            BuildContent();
        }

        public void Close()
        {
            if (_modal != null && _modal.IsOpen)
            {
                _modal.CloseSelf();
            }
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public void SetVisible(bool visible)
        {
            if (_modal != null && _modal.gameObject.activeSelf != visible)
            {
                _modal.gameObject.SetActive(visible);
            }
        }

        public void Update(bool isDead)
        {
            if (_modal == null || !_modal.IsOpen) return;
            if (_player == null || _player.skillCaster == null) return;

            if (isDead)
            {
                SetVisible(false);
                return;
            }
            else
            {
                SetVisible(true);
            }

            var caster = _player.skillCaster;

            if (_skillStateText != null)
            {
                _skillStateText.text = $"State: <color=#00e6ff>{caster.CurrentState}</color>";
            }

            if (_skillCastingText != null)
            {
                if (caster.CurrentState == SkillCasterState.Casting && caster.ActiveSkillInstance?.SkillData != null)
                {
                    if (!_skillCastingText.gameObject.activeSelf) _skillCastingText.gameObject.SetActive(true);
                    _skillCastingText.text = $"Casting: <b>{caster.ActiveSkillInstance.SkillData.Name}</b> ({caster.CurrentCastTimer:F2}/{caster.CurrentCastTimeTotal:F2}s)";
                }
                else if (_skillCastingText.gameObject.activeSelf)
                {
                    _skillCastingText.gameObject.SetActive(false);
                }
            }

            if (_skillCastDelayText != null)
            {
                if (caster.CurrentState == SkillCasterState.CastDelay)
                {
                    if (!_skillCastDelayText.gameObject.activeSelf) _skillCastDelayText.gameObject.SetActive(true);
                    _skillCastDelayText.text = $"Cast Delay: {caster.GlobalCastDelayTimer:F2}s";
                }
                else if (_skillCastDelayText.gameObject.activeSelf)
                {
                    _skillCastDelayText.gameObject.SetActive(false);
                }
            }

            if (caster.EquippedSkills != null)
            {
                for (int i = 0; i < caster.EquippedSkills.Count; i++)
                {
                    if (i >= _skillSlotTexts.Count || _skillSlotTexts[i] == null) continue;

                    var skill = caster.EquippedSkills[i];
                    if (skill?.SkillData != null)
                    {
                        string colorHex = skill.IsReady() ? "#00ff66" : "#ff4444";
                        string cdText = skill.IsReady() ? "Ready" : $"{skill.CurrentCooldown:F2}s";
                        _skillSlotTexts[i].text = $"Slot {i + 1}: {skill.SkillData.Name} - <color={colorHex}>{cdText}</color>";
                    }
                    else
                    {
                        _skillSlotTexts[i].text = $"Slot {i + 1}: <color=#777777><i>Empty</i></color>";
                    }
                }
            }
        }
        #endregion

        #region [Private Methods]
        private void BuildContent()
        {
            if (_modal == null) return;

            _skillSlotTexts.Clear();

            _modal.Build(ctx => 
            {
                ctx.Vertical("Div", content => 
                {
                    _skillStateText = content.Text("Text", "State: Idle")
                                             .SetFontSize(14)
                                             .SetFontStyle(FontStyles.Bold)
                                             .SetTextColor(new Color(0.3f, 0.9f, 1f))
                                             .TextComponent;

                    _skillCastingText = content.Text("Text", "")
                                               .SetFontSize(12)
                                               .SetTextColor(Color.yellow)
                                               .TextComponent;
                    if (_skillCastingText != null) _skillCastingText.gameObject.SetActive(false);

                    _skillCastDelayText = content.Text("Text", "")
                                                 .SetFontSize(12)
                                                 .SetTextColor(new Color(1f, 0.6f, 0.2f))
                                                 .TextComponent;
                    if (_skillCastDelayText != null) _skillCastDelayText.gameObject.SetActive(false);

                    content.Text("Text", "<b>Equipped Skills</b>")
                           .SetFontSize(13)
                           .SetTextAlignment(TextAlignmentOptions.Center)
                           .SetPadding(0, 0, 6, 2);

                    var caster = _player != null ? _player.skillCaster : null;
                    int slotCount = caster != null && caster.EquippedSkills != null ? caster.EquippedSkills.Count : 4;

                    for (int i = 0; i < slotCount; i++)
                    {
                        int slotIndex = i;
                        content.Horizontal("Div", row => 
                        {
                            var txt = row.Text("Text", $"Slot {slotIndex + 1}: Empty")
                                         .SetFontSize(12)
                                         .SetFlexibleSize(1, -1)
                                         .TextComponent;
                            _skillSlotTexts.Add(txt);
                        }).SetPadding(4, 4, 2, 2)
                          .SetBackgroundColor(new Color(0.1f, 0.12f, 0.18f, 0.8f));
                    }
                }).SetPadding(10, 10, 8, 8)
                  .SetSpacing(4)
                  .SetFitParent();
            });
        }
        #endregion
    }
}
