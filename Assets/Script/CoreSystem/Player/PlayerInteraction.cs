using System;
using System.Collections.Generic;
using CoreSystem.Agent;
using CoreSystem.Data;
using CoreSystem.Entity;
using CoreSystem.UI.Inspect;
using CoreSystem.UI.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CoreSystem.Player
{
    public sealed class PlayerInteraction
    {
        #region [Variables/Fields]
        private static readonly Collider2D[] Hits = new Collider2D[16];
        #endregion

        #region [Properties]
        public static event Action<Character, Character, List<ContextMenuItem>> OnBuildOptions;
        #endregion

        #region [Public Methods]
        public bool TryOpenMenu(PlayerCore core, Vector3 worldPosition)
        {
            if (core == null || core.Controlled == null || ContextMenuManager.Instance == null) return false;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return false;

            Character target = PickCharacter(worldPosition, core.Controlled);
            if (target == null) return false;

            var items = BuildOptions(core, target);
            if (items.Count == 0) return false;

            var mouse = UnityEngine.InputSystem.Mouse.current;
            ContextMenuManager.Instance.Show(items, mouse != null ? mouse.position.ReadValue() : Vector2.zero);
            return true;
        }

        public static Character PickCharacter(Vector3 worldPosition, Character self)
        {
            int count = Physics2D.OverlapPoint(worldPosition, new ContactFilter2D().NoFilter(), Hits);
            Character fallback = null;
            for (int i = 0; i < count; i++)
            {
                var character = Hits[i] != null ? Hits[i].GetComponentInParent<Character>() : null;
                if (character == null) continue;
                if (character != self) return character;
                fallback = character;
            }
            return fallback;
        }

        public static List<ContextMenuItem> BuildOptions(PlayerCore core, Character target)
        {
            var items = new List<ContextMenuItem>();
            if (target == null) return items;

            items.Add(new ContextMenuItem("Inspect", () => CharacterInspectWindow.Open(target)));

            Character player = core != null ? core.Controlled : null;
            if (player != null && target != player && IsAlive(target))
            {
                if (FactionDatabase.IsHostile(player.Faction, target.Faction))
                {
                    items.Add(new ContextMenuItem("Attack", () => core.SetAutoAttackTarget(target)));
                }
                else if (AgentCore.Instance != null && AgentCore.Instance.GetHandle(target) != null)
                {
                    items.Add(new ContextMenuItem("Follow Me", () => AgentCore.Instance?.CommandFollow(target, player.transform)));
                    items.Add(new ContextMenuItem("Stop Following", () => AgentCore.Instance?.ClearCommand(target)));
                }
            }

            OnBuildOptions?.Invoke(player, target, items);
            return items;
        }
        #endregion

        #region [Private Methods]
        private static bool IsAlive(Character character)
        {
            var hp = character.attributeSystem != null ? character.attributeSystem.GetAttribute(AttributeType.HP) : null;
            return hp == null || hp.CurrentValue > 0f;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            OnBuildOptions = null;
        }
        #endregion
    }
}
