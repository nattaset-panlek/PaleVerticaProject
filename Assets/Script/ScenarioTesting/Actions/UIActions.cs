#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System.Collections;
using CoreSystem.Settings.Keybinds;
using CoreSystem.UI;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace ScenarioTesting.Actions
{
    internal static class UIFlow
    {
        #region [Variables/Fields]
        public const string SkillWindow = "CharacterSkillManage";
        public const string InventoryWindow = "Inventory";
        public const string EquipmentWindow = "CharacterEquipment";
        private const float WindowWait = 3f;
        #endregion

        #region [Public Methods]
        public static bool IsOpen(string window)
        {
            return UIManager.Instance != null && UIManager.Instance.IsOpen(window);
        }

        public static IEnumerator SetWindow(ScenarioRun run, string window, bool open)
        {
            if (IsOpen(window) == open) yield break;
            if (!KeybindSetting.TryGetWindowKeys(window, out KeybindPair pair) || pair.Get(0) == Key.None)
            {
                run.FailStep($"no key toggles the {window} window");
                yield break;
            }
            yield return run.Input.Tap(pair.Get(0));
            yield return run.WaitFor(() => IsOpen(window) == open, WindowWait, $"the {window} window to {(open ? "open" : "close")} after pressing {pair.Get(0)}");
            yield return null;
        }

        public static IEnumerator Find(ScenarioRun run, JToken spec, float max, System.Action<RectTransform> found)
        {
            RectTransform rect = null;
            string description = null;
            yield return run.WaitFor(() => ScenarioUITarget.TryFind(spec, out rect, out description), max, $"UI target {spec?.ToString(Newtonsoft.Json.Formatting.None)}");
            if (rect != null) found(rect);
        }

        public static IEnumerator ClickLabel(ScenarioRun run, string label, float max = 3f)
        {
            yield return DialogueInput.ClickLabel(run, label, max);
        }
        #endregion
    }

    [ScenarioAction("OpenWindow", "UI", "{\"do\":\"OpenWindow\",\"window\":\"Inventory\"}  presses the window's key (Inventory E, CharacterSkillManage J, CharacterEquipment Q, CharacterStatus U, QuestLog L)")]
    internal sealed class OpenWindowAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            yield return UIFlow.SetWindow(run, step.String("window"), true);
        }
        #endregion
    }

    [ScenarioAction("CloseWindow", "UI", "{\"do\":\"CloseWindow\",\"window\":\"Inventory\"}  presses the window's key again")]
    internal sealed class CloseWindowAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            yield return UIFlow.SetWindow(run, step.String("window"), false);
        }
        #endregion
    }

    [ScenarioAction("Menu", "UI", "{\"do\":\"Menu\",\"item\":\"Save Game\",\"confirm\":\"Return\"}  Escape opens the System Menu, clicks the item, then the confirm button if given")]
    internal sealed class MenuAction : ScenarioAction
    {
        #region [Variables/Fields]
        private const string MenuTitle = "Character";
        #endregion

        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            if (ScenarioInput.FindButton(MenuTitle) == null) yield return run.Input.Tap(Key.Escape);
            yield return UIFlow.ClickLabel(run, step.String("item"));
            if (run.StepFailed || !step.Has("confirm")) yield break;
            yield return UIFlow.ClickLabel(run, step.String("confirm"));
        }
        #endregion
    }

    [ScenarioAction("ClickOn", "UI", "{\"do\":\"ClickOn\",\"target\":{\"item\":4001},\"button\":\"right\",\"double\":false}  target: " + ScenarioUITarget.Usage)]
    internal sealed class ClickOnAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            RectTransform rect = null;
            yield return UIFlow.Find(run, step.Raw["target"], step.Float("max", 3f), found => rect = found);
            if (rect == null) yield break;

            Vector2 screen = ScenarioUITarget.ScreenOf(rect);
            MouseButton button = step.String("button", "left") == "right" ? MouseButton.Right : MouseButton.Left;
            if (step.Bool("double")) yield return run.Input.DoubleClick(screen, button);
            else yield return run.Input.Click(screen, button);
            run.Log($"clicked {rect.name} at {screen.x:0},{screen.y:0} ({button}{(step.Bool("double") ? ", double" : "")}), UI on top: {string.Join(", ", ScenarioInput.UIUnder(screen))}");
            yield return new WaitForSeconds(0.2f);
        }
        #endregion
    }

    [ScenarioAction("Drag", "UI", "{\"do\":\"Drag\",\"from\":{\"skill\":1001},\"to\":{\"hotbar\":1}}  presses, slides sideways first (the game needs that), moves and drops; targets: " + ScenarioUITarget.Usage)]
    internal sealed class DragAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            yield return UIDrag.Run(run, step.Raw["from"], step.Raw["to"], step.Float("max", 3f));
        }
        #endregion
    }

    internal static class UIDrag
    {
        #region [Public Methods]
        public static IEnumerator Run(ScenarioRun run, JToken from, JToken to, float max)
        {
            RectTransform source = null;
            RectTransform target = null;
            yield return UIFlow.Find(run, from, max, found => source = found);
            if (source == null) yield break;
            yield return UIFlow.Find(run, to, max, found => target = found);
            if (target == null) yield break;

            Vector2 start = ScenarioUITarget.ScreenOf(source);
            Vector2 end = ScenarioUITarget.ScreenOf(target);
            yield return run.Input.Drag(start, end);
            run.Log($"dragged {source.name} ({start.x:0},{start.y:0}) to {target.name} ({end.x:0},{end.y:0})");
            yield return new WaitForSeconds(0.2f);
        }
        #endregion
    }

    [ScenarioAction("AssignSkill", "UI", "{\"do\":\"AssignSkill\",\"skill\":1001,\"slot\":1,\"how\":\"menu\"}  opens Skills (J) and puts the skill on hotbar slot N by right-click menu or \"how\":\"drag\"")]
    internal sealed class AssignSkillAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            int skill = step.Int("skill");
            int slot = step.Int("slot", 1);
            yield return UIFlow.SetWindow(run, UIFlow.SkillWindow, true);
            if (run.StepFailed) yield break;

            if (step.String("how", "menu") == "drag")
            {
                yield return UIDrag.Run(run, new JObject { ["skill"] = skill }, new JObject { ["hotbar"] = slot }, 3f);
            }
            else
            {
                RectTransform row = null;
                yield return UIFlow.Find(run, new JObject { ["skill"] = skill }, 3f, found => row = found);
                if (row == null) yield break;
                yield return run.Input.Click(ScenarioUITarget.ScreenOf(row), MouseButton.Right);
                yield return UIFlow.ClickLabel(run, $"Slot #{slot}");
            }
            if (run.StepFailed) yield break;

            yield return UIFlow.SetWindow(run, UIFlow.SkillWindow, false);
            CoreSystem.Entity.Character player = ScenarioProbe.Player;
            var equipped = player != null ? player.skillCaster.EquippedSkills : null;
            bool placed = equipped != null && slot - 1 >= 0 && slot - 1 < equipped.Count && equipped[slot - 1] != null && equipped[slot - 1].SkillData != null && equipped[slot - 1].SkillData.Id == skill;
            if (!placed) run.FailStep($"skill {skill} is not in slot {slot} after assigning it ({ScenarioLab.SkillLoadout()})");
            else run.Note($"skill {skill} in slot {slot}");
        }
        #endregion
    }

    [ScenarioAction("EquipItem", "UI", "{\"do\":\"EquipItem\",\"item\":4001,\"how\":\"double\"}  opens Inventory (E) and equips by double-click, \"menu\" (right-click, Equip) or \"drag\" with \"slot\":\"WeaponOneHandRight\"")]
    internal sealed class EquipItemAction : ScenarioAction
    {
        #region [Variables/Fields]
        private const string EquipLabel = "Equip";
        #endregion

        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            int item = step.Int("item");
            string how = step.String("how", "double");
            yield return UIFlow.SetWindow(run, UIFlow.InventoryWindow, true);
            if (run.StepFailed) yield break;

            if (how == "drag")
            {
                yield return UIFlow.SetWindow(run, UIFlow.EquipmentWindow, true);
                if (run.StepFailed) yield break;
                yield return UIDrag.Run(run, new JObject { ["item"] = item }, new JObject { ["equip"] = step.String("slot") }, 3f);
                yield return UIFlow.SetWindow(run, UIFlow.EquipmentWindow, false);
            }
            else
            {
                RectTransform slot = null;
                yield return UIFlow.Find(run, new JObject { ["item"] = item }, 3f, found => slot = found);
                if (slot == null) yield break;
                Vector2 screen = ScenarioUITarget.ScreenOf(slot);
                if (how == "menu")
                {
                    yield return run.Input.Click(screen, MouseButton.Right);
                    yield return UIFlow.ClickLabel(run, EquipLabel);
                }
                else yield return run.Input.DoubleClick(screen);
            }
            if (run.StepFailed) yield break;

            yield return new WaitForSeconds(0.2f);
            yield return UIFlow.SetWindow(run, UIFlow.InventoryWindow, false);
            if (!IsEquipped(item)) run.FailStep($"item {item} is not equipped after {how}");
            else run.Note($"item {item} equipped by {how}");
        }
        #endregion

        #region [Private Methods]
        private static bool IsEquipped(int itemId)
        {
            foreach (CoreSystem.Data.Items.Model.EquipmentSlot slot in System.Enum.GetValues(typeof(CoreSystem.Data.Items.Model.EquipmentSlot)))
            {
                var equipped = CoreSystem.Data.GameData.Player.Equipment.GetEquipment(slot);
                if (equipped != null && equipped.ItemId == itemId) return true;
            }
            return false;
        }
        #endregion
    }
}
#endif
