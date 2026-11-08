using System.Collections.Generic;
using CoreSystem.Agent;
using CoreSystem.Data;
using CoreSystem.Entity;
using CoreSystem.Interaction;
using CoreSystem.UI.Inspect;
using CoreSystem.UI.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CoreSystem.Player
{
    public sealed class PlayerInteraction : IInteractionProvider
    {
        #region [Variables/Fields]
        public const string CoreOptionPrefix = "core.";
        public const string InspectId = "core.inspect";
        public const string AttackId = "core.attack";
        public const string FollowId = "core.follow";
        public const string StopFollowId = "core.stop_follow";

        private static readonly Collider2D[] Hits = new Collider2D[16];
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

        public bool TryPrimaryInteract(PlayerCore core, Vector3 worldPosition)
        {
            if (core == null || core.Controlled == null) return false;

            Character player = core.Controlled;
            Character target = PickCharacter(worldPosition, player);
            if (target == null || target == player) return false;

            var interacts = FilterInteractOptions(InteractionCore.GetOptions(player, target, core));
            if (interacts.Count == 0) return false;

            if (interacts.Count == 1)
            {
                InteractionCore.Ensure().Request(player, target, interacts[0], core);
                return true;
            }

            if (ContextMenuManager.Instance == null) return false;
            var items = new List<ContextMenuItem>();
            for (int i = 0; i < interacts.Count; i++)
            {
                var option = interacts[i];
                items.Add(new ContextMenuItem(option.Label, () => InteractionCore.Ensure().Request(player, target, option, core)));
            }
            var mouse = UnityEngine.InputSystem.Mouse.current;
            ContextMenuManager.Instance.Show(items, mouse != null ? mouse.position.ReadValue() : Vector2.zero);
            return true;
        }

        public static bool IsCoreOption(string optionId)
        {
            return !string.IsNullOrEmpty(optionId) && optionId.StartsWith(CoreOptionPrefix);
        }

        public static List<InteractionOption> FilterInteractOptions(IReadOnlyList<InteractionOption> options)
        {
            var result = new List<InteractionOption>();
            if (options == null) return result;
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i] != null && !IsCoreOption(options[i].Id)) result.Add(options[i]);
            }
            return result;
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
            Character player = core != null ? core.Controlled : null;
            if (player == null || target == null) return items;

            var options = InteractionCore.GetOptions(player, target, core);
            for (int i = 0; i < options.Count; i++)
            {
                var option = options[i];
                items.Add(new ContextMenuItem(option.Label, () => InteractionCore.Ensure().Request(player, target, option, core)));
            }
            return items;
        }

        public void Collect(InteractionContext context, List<InteractionOption> options)
        {
            var target = context.Target;
            options.Add(new InteractionOption(InspectId, "Inspect", ctx => CharacterInspectWindow.Open(ctx.Target), priority: 1000));

            if (!(context.Holder is PlayerCore core) || context.IsSelf || target.IsDead) return;

            var player = context.Actor;
            if (FactionDatabase.IsHostile(player.Faction, target.Faction))
            {
                options.Add(new InteractionOption(AttackId, "Attack", ctx => core.SetAutoAttackTarget(ctx.Target), priority: 900));
            }
            else if (AgentCore.Instance != null && AgentCore.Instance.GetHandle(target) != null)
            {
                options.Add(new InteractionOption(FollowId, "Follow Me", ctx => AgentCore.Instance?.CommandFollow(ctx.Target, ctx.Actor.transform), priority: -100));
                options.Add(new InteractionOption(StopFollowId, "Stop Following", ctx => AgentCore.Instance?.ClearCommand(ctx.Target), priority: -110));
            }
        }
        #endregion
    }
}
