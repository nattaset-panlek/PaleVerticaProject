#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System.Collections;
using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data;
using CoreSystem.Data.Items.Model;
using CoreSystem.Entity;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace ScenarioTesting
{
    public sealed class ScenarioLab
    {
        #region [Variables/Fields]
        public const int DefaultTargetId = 1001;
        public const int DefaultPotionId = 2001;
        public const float TargetDistance = 1.2f;
        public const float TankHp = 1000000f;
        public const int LabHitSkillId = 1;
        private const string LabTag = "ScenarioLab";
        private const float EmptySearchRadius = 3f;
        private const float VisibleTimeout = 40f;
        private const float ScreenMargin = 0.12f;
        private const float NearEnough = 0.8f;
        private const float CameraSettle = 0.25f;
        private const float BlockingUIWait = 3f;
        private const int TimelineLimit = 24;
        private const float WaypointReach = 0.2f;
        private const float StuckDistance = 0.05f;
        private const float StuckTime = 1f;
        private const float WalkTimeout = 20f;
        private const float AttackReach = 0.8f;

        private static readonly System.Reflection.FieldInfo AutoAttackField =
            typeof(CoreSystem.Player.PlayerCore).GetField("_autoAttackTarget", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        private static readonly HashSet<AttributeType> ChangingStats = new HashSet<AttributeType> { AttributeType.HP, AttributeType.MP };

        private readonly ScenarioRun _run;
        private readonly List<float> _lastHits = new List<float>();
        #endregion

        #region [Properties]
        public Character Target { get; private set; }
        public IReadOnlyList<float> LastHits => _lastHits;
        public float LastHitTotal { get; private set; }
        public bool LastPotionKept { get; private set; }
        public bool LastCastStarted { get; private set; }
        public string LastClickReport { get; private set; } = string.Empty;
        private Character Player => ScenarioProbe.Player;
        #endregion

        #region [Public Methods]
        public ScenarioLab(ScenarioRun run)
        {
            _run = run;
        }

        public IEnumerator Spawn(int characterId, Vector3 position, bool freeze, bool tank, System.Action<Character> onSpawned)
        {
            bool done = false;
            Character spawned = null;
            CharacterFactory.Spawn(characterId, position, null, character =>
            {
                spawned = character;
                done = true;
            });
            yield return _run.WaitFor(() => done, 10f, $"character {characterId} to spawn");
            if (spawned == null)
            {
                _run.FailStep($"character {characterId} did not spawn");
                yield break;
            }

            if (freeze) CharacterFactory.ReleaseController(spawned);
            if (tank) AddStat(spawned, AttributeType.MaxHP, TankHp);
            Refill(spawned);
            onSpawned?.Invoke(spawned);
        }

        public IEnumerator EnsureTarget(int characterId = DefaultTargetId, bool tank = true)
        {
            if (Target != null && !Target.IsDead)
            {
                Refill(Target);
                yield break;
            }
            yield return Spawn(characterId, Player.transform.position + Vector3.right * TargetDistance, true, tank, character => Target = character);
            _run.Name(ScenarioRun.TargetName, Target);
        }

        public void DropTarget()
        {
            if (Target != null) Object.Destroy(Target.gameObject);
            Target = null;
        }

        public static void Refill(Character character)
        {
            if (character == null) return;
            SetResource(character, AttributeType.HP, AttributeProbe.Read(character, AttributeType.MaxHP));
            SetResource(character, AttributeType.MP, AttributeProbe.Read(character, AttributeType.MaxMP));
        }

        public static void SetResource(Character character, AttributeType resource, float value)
        {
            var attribute = character != null ? character.attributeSystem.GetAttribute(resource) : null;
            if (attribute == null) return;
            attribute.BaseValue = value;
            attribute.Recalculate();
        }

        public static void AddStat(Character character, AttributeType type, float value)
        {
            character.attributeSystem.AddAdditional(type, 2, AdditionalOp.Add, value, LabTag);
        }

        public static void SetTotal(Character character, AttributeType type, float total)
        {
            character.attributeSystem.RemoveAdditionalByTag(type, LabTag);
            AddStat(character, type, total - AttributeProbe.Read(character, type));
        }

        public static void ClearLabStat(Character character, AttributeType type)
        {
            character.attributeSystem.RemoveAdditionalByTag(type, LabTag);
        }

        public void Give(Character character, IEnumerable<string> passiveIds)
        {
            foreach (string id in passiveIds)
            {
                character.UniquePassives.AddPassive(id);
                if (!character.UniquePassives.HasPassive(id)) _run.FailStep($"unique passive '{id}' is not registered");
            }
            character.RecalculateStats();
        }

        public void ClearPassives(Character character)
        {
            foreach (string id in character.UniquePassives.ActivePassiveIds) character.UniquePassives.RemovePassive(id);
            character.RecalculateStats();
        }

        public Dictionary<AttributeType, float> StableSnapshot(Character character)
        {
            Dictionary<AttributeType, float> values = AttributeProbe.Snapshot(character);
            foreach (AttributeType type in ChangingStats) values.Remove(type);
            return values;
        }

        public IEnumerator Attack(Character target, int hits, float timeout, bool allowNone = false)
        {
            ClearHits();
            if (target == null || target.IsDead)
            {
                _run.FailStep("no living target to attack");
                yield break;
            }

            int start = _run.Combat.Entries.Count;
            string name = CombatRecorder.NameOf(target);
            var timeline = new List<string>();
            string lastSnapshot = null;
            void Sample()
            {
                string snapshot = AttackSnapshot(target, start, name);
                if (snapshot == lastSnapshot || timeline.Count >= TimelineLimit) return;
                timeline.Add($"{_run.Elapsed:0.00}s {snapshot}");
                lastSnapshot = snapshot;
            }

            Sample();
            yield return Sampled(ClickCharacter(target, MouseButton.Left), Sample);

            float deadline = Time.time + timeout;
            string firstState = Diagnose(target);
            while (Time.time < deadline && CountHits(start, name) < hits && target != null && !target.IsDead)
            {
                Sample();
                yield return null;
            }
            string lastState = Diagnose(target);
            yield return StopAttacking();
            CollectHits(start, name);
            if (_lastHits.Count == 0 && !allowNone) _run.FailStep($"no hits landed. {LastClickReport}. Right after the click: {firstState}. At the end: {lastState}. Timeline: {string.Join(" | ", timeline)}");
        }

        private string AttackSnapshot(Character target, int start, string name)
        {
            Character player = Player;
            var core = CoreSystem.Player.PlayerCore.Instance;
            if (player == null || core == null) return "no player";
            object selected = AutoAttackField != null ? AutoAttackField.GetValue(core) : null;
            string selectedName = selected is Object unityObject && unityObject != null ? unityObject.name : selected == null ? "none" : "destroyed";
            float hp = target != null ? AttributeProbe.Read(target, AttributeType.HP) : -1f;
            int entries = _run.Combat.Entries.Count - start;
            return $"auto-attack {selectedName}, attack {(player.BasicAttackCooldownTimer > 0f ? "cooling" : "ready")}, caster {player.skillCaster.CurrentState}, target hp {hp:0}, combat events {entries} (hits {CountHits(start, name)}), interact {Actions.TalkState.Options(target)}";
        }

        private static IEnumerator Sampled(IEnumerator root, System.Action sample)
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                sample();
                IEnumerator top = stack.Peek();
                if (!top.MoveNext())
                {
                    stack.Pop();
                    continue;
                }
                object current = top.Current;
                if (current is IEnumerator nested && !(current is CustomYieldInstruction))
                {
                    stack.Push(nested);
                    continue;
                }
                yield return current;
            }
            sample();
        }

        public string Diagnose(Character target)
        {
            Character player = Player;
            var core = CoreSystem.Player.PlayerCore.Instance;
            if (player == null || core == null) return "no player";
            object selected = AutoAttackField != null ? AutoAttackField.GetValue(core) : null;
            string selectedName = selected is Object unityObject && unityObject != null ? unityObject.name : selected == null ? "none" : "destroyed";
            float distance = target != null ? Vector2.Distance(player.transform.position, target.transform.position) : -1f;
            return $"auto-attack target {selectedName}, distance {distance:0.##} / range {player.AutoAttackRange:0.##}, attack cooldown {player.BasicAttackCooldownTimer:0.##}, "
                 + $"caster {player.skillCaster.CurrentState}, aiming slot {player.skillCaster.AimedSkillSlot}, has control {core.HasControl}, virtual mouse current {_run.Input.OwnsPointer}, focused {Application.isFocused}";
        }

        public IEnumerator StopAttacking()
        {
            if (TryEmptyScreenPoint(out Vector2 screen)) yield return _run.Input.Click(screen);
        }

        public IEnumerator Cast(int hotbarSlot, Character target, float wait)
        {
            ClearHits();
            Key key = ScenarioProbe.HotbarKey(hotbarSlot);
            if (key == Key.None)
            {
                _run.FailStep($"hotbar slot {hotbarSlot} has no key");
                yield break;
            }

            int start = _run.Combat.Entries.Count;
            int readyBefore = ReadySkillCount();
            float manaBefore = AttributeProbe.Read(Player, AttributeType.MP);
            ScenarioCastProbe probe = ScenarioCastProbe.Attach(Player);
            _run.Log($"before key {key}: {CastState(hotbarSlot)}");
            yield return _run.Input.Tap(key);
            _run.Log($"after key {key}: {CastState(hotbarSlot)}");
            if (Player.skillCaster.CurrentState != SkillCasterState.Idle) LastCastStarted = true;
            if (target != null) yield return ClickCharacter(target, MouseButton.Left);
            _run.Log($"after the click: {CastState(hotbarSlot)}");

            float end = Time.time + wait;
            while (Time.time < end)
            {
                if (ReadySkillCount() < readyBefore || AttributeProbe.Read(Player, AttributeType.MP) < manaBefore || Player.skillCaster.CurrentState != SkillCasterState.Idle) LastCastStarted = true;
                yield return null;
            }
            if (probe != null)
            {
                if (probe.Executions > 0) LastCastStarted = true;
                probe.Detach();
            }
            if (target != null) CollectHits(start, CombatRecorder.NameOf(target));
        }

        public IEnumerator ClickCharacter(Character character, MouseButton button)
        {
            if (character == null) yield break;
            yield return BringOnScreen(character, VisibleTimeout);
            if (_run.StepFailed) yield break;

            float clearBy = Time.unscaledTime + BlockingUIWait;
            bool found = TryFindClickPoint(character, out Vector3 world, out Vector2 screen, out List<string> blocking);
            if (found && blocking.Count > 0)
            {
                string first = string.Join(", ", blocking);
                while (found && blocking.Count > 0 && Time.unscaledTime < clearBy && character != null)
                {
                    yield return null;
                    found = TryFindClickPoint(character, out world, out screen, out blocking);
                }
                _run.Log(blocking.Count == 0 ? $"waited for UI to clear before clicking {character.name} (was {first})" : $"UI still covers {character.name} after {BlockingUIWait:0}s: {string.Join(", ", blocking)}");
            }

            if (character == null)
            {
                _run.FailStep("the character was destroyed before the click");
                yield break;
            }
            if (!found)
            {
                _run.FailStep($"{character.name} is not on screen (player at {Player.transform.position.x:0.#},{Player.transform.position.y:0.#}, {character.name} at {character.transform.position.x:0.#},{character.transform.position.y:0.#})");
                yield break;
            }

            Collider2D hit = Physics2D.OverlapPoint(world);
            yield return _run.Input.Click(screen, button);
            CoreSystem.World.MapPortal portalHere = CoreSystem.World.MapPortal.AtPoint(world);
            LastClickReport = $"clicked {character.name} at screen {screen.x:0},{screen.y:0} world {world.x:0.##},{world.y:0.##}, collider {(hit != null ? hit.name : "none")}, "
                            + $"UI under the pointer: {(blocking.Count == 0 ? "none" : string.Join(", ", blocking))}, virtual mouse is current {_run.Input.OwnsPointer}, distance {Vector2.Distance(Player.transform.position, character.transform.position):0.#}, "
                            + $"portal under the click: {(portalHere != null ? portalHere.name : "none")}, walking to a spot: {PlayerIsApproaching()}, game sees: {GameClickView()}";
            _run.Log(LastClickReport);
        }

        public IEnumerator BringOnScreen(Character character, float timeout)
        {
            if (character == null) yield break;
            yield return BringOnScreen(character.transform, timeout);
        }

        public IEnumerator BringOnScreen(Transform target, float timeout)
        {
            float deadline = Time.time + timeout;
            while (target != null && Player != null && !Player.IsDead && Time.time <= deadline)
            {
                if (IsComfortablyVisible(target.position))
                {
                    _run.Input.Hold();
                    yield return new WaitForSeconds(CameraSettle);
                    if (target == null || IsComfortablyVisible(target.position)) yield break;
                    continue;
                }
                Vector3 offset = target.position - Player.transform.position;
                _run.Input.Hold(offset.magnitude > NearEnough ? KeysToward(offset) : new Key[0]);
                yield return null;
            }
            _run.Input.Hold();
        }

        public IEnumerator Approach(Vector3 destination, float range, float timeout)
        {
            float deadline = Time.time + timeout;
            var path = new List<Vector2>();
            int index = 0;
            bool pathed = FindPath(destination, path);
            float stuckSince = Time.time;
            Vector2 lastPosition = Player != null ? (Vector2)Player.transform.position : Vector2.zero;

            while (Player != null && Vector2.Distance(Player.transform.position, destination) > range)
            {
                if (Time.time > deadline)
                {
                    _run.Input.Hold();
                    _run.FailStep($"could not walk within {range} of {destination} (path {(pathed ? $"{path.Count} points, at point {index}" : "none, walked straight")}, player at {Player.transform.position.x:0.##},{Player.transform.position.y:0.##})");
                    yield break;
                }

                Vector2 position = Player.transform.position;
                while (pathed && index < path.Count && Vector2.Distance(position, path[index]) <= WaypointReach) index++;
                Vector2 next = pathed && index < path.Count ? path[index] : (Vector2)destination;

                if (Vector2.Distance(position, lastPosition) > StuckDistance)
                {
                    lastPosition = position;
                    stuckSince = Time.time;
                }
                else if (Time.time - stuckSince > StuckTime)
                {
                    pathed = FindPath(destination, path);
                    index = 0;
                    stuckSince = Time.time;
                    _run.Log($"stuck at {position.x:0.##},{position.y:0.##} on the way to {destination.x:0.##},{destination.y:0.##}, path again ({(pathed ? $"{path.Count} points" : "none")})");
                }

                _run.Input.Hold(KeysToward((Vector3)next - Player.transform.position));
                yield return null;
            }
            _run.Input.Hold();
            yield return null;
        }

        public IEnumerator WalkInRange(Character target, float range, float timeout)
        {
            if (target == null || Player == null) yield break;
            if (Vector2.Distance(Player.transform.position, target.transform.position) <= range) yield break;
            yield return Approach(target.transform.position, range, timeout);
        }

        public IEnumerator HoldKeys(Key[] keys, float seconds)
        {
            _run.Input.Hold(keys);
            yield return new WaitForSeconds(seconds);
            _run.Input.Hold();
            yield return null;
        }

        public IEnumerator TakeHits(Character attacker, int count, float amount, float gap)
        {
            if (attacker == null)
            {
                _run.FailStep("no attacker for the hits");
                yield break;
            }
            for (int i = 0; i < count; i++)
            {
                var payload = new SkillPayload
                {
                    Caster = attacker,
                    SkillID = LabHitSkillId,
                    SkillLevel = 1,
                    DamageType = DamageType.Physical,
                    SkillElement = ElementType.Neutral,
                    BaseDamage = amount
                };
                CombatCore.Execute(CombatAction.Damage(payload), attacker, Player);
                yield return new WaitForSeconds(gap);
            }
        }

        public IEnumerator UseItem(int itemId)
        {
            var inventory = GameData.Player.Inventory;
            inventory.AddItem(itemId);
            var instance = inventory.GetFirstByModelId(itemId);
            var consumable = instance != null ? instance.Item as Consumable : null;
            if (consumable == null)
            {
                _run.FailStep($"item {itemId} is not a consumable");
                yield break;
            }

            consumable.OnUse();
            LastPotionKept = !consumable.LastUseConsumed;
            if (consumable.LastUseConsumed) inventory.RemoveItemByInstance(instance.Id, 1);
            yield return null;
        }

        public static int SummonCount(Character owner)
        {
            int count = 0;
            if (owner == null) return count;
            foreach (SummonedUnit unit in SummonRegistry.Units) if (unit != null && unit.Owner == owner) count++;
            return count;
        }

        public static Key[] KeysToward(Vector3 direction)
        {
            var keys = new List<Key>(2);
            if (direction.y > 0.15f) keys.Add(Key.W);
            if (direction.y < -0.15f) keys.Add(Key.S);
            if (direction.x > 0.15f) keys.Add(Key.D);
            if (direction.x < -0.15f) keys.Add(Key.A);
            return keys.ToArray();
        }
        #endregion

        #region [Private Methods]
        private static bool IsComfortablyVisible(Vector3 world)
        {
            return ScenarioInput.TryScreenOf(world, out Vector2 screen) && ScenarioInput.IsInside(screen, ScreenMargin);
        }

        private static bool TryFindClickPoint(Character character, out Vector3 world, out Vector2 screen, out List<string> blocking)
        {
            world = character.transform.position;
            screen = Vector2.zero;
            blocking = new List<string>();
            bool found = false;
            foreach (Vector3 candidate in ClickCandidates(character))
            {
                if (!ScenarioInput.TryScreenOf(candidate, out Vector2 point)) continue;
                List<string> under = ScenarioInput.UIUnder(point);
                if (!found || under.Count < blocking.Count)
                {
                    found = true;
                    world = candidate;
                    screen = point;
                    blocking = under;
                }
                if (under.Count == 0) break;
            }
            return found;
        }

        private static IEnumerable<Vector3> ClickCandidates(Character character)
        {
            Collider2D collider = character.GetComponentInChildren<Collider2D>();
            if (collider == null)
            {
                yield return character.transform.position;
                yield break;
            }
            Bounds bounds = collider.bounds;
            Vector3 extents = bounds.extents * 0.6f;
            Vector3[] offsets = { Vector3.zero, new Vector3(0f, extents.y), new Vector3(0f, -extents.y), new Vector3(extents.x, 0f), new Vector3(-extents.x, 0f) };
            foreach (Vector3 offset in offsets)
            {
                Vector3 point = bounds.center + offset;
                if (collider.OverlapPoint(point)) yield return point;
            }
            yield return character.transform.position;
        }

        private void ClearHits()
        {
            _lastHits.Clear();
            LastHitTotal = 0f;
            LastCastStarted = false;
        }

        private int CountHits(int start, string target)
        {
            int count = 0;
            var entries = _run.Combat.Entries;
            for (int i = start; i < entries.Count; i++) if (entries[i].Kind == CombatRecorder.HitKind && entries[i].Target == target) count++;
            return count;
        }

        private void CollectHits(int start, string target)
        {
            _lastHits.Clear();
            LastHitTotal = 0f;
            var entries = _run.Combat.Entries;
            for (int i = start; i < entries.Count; i++)
            {
                if (entries[i].Kind != CombatRecorder.HitKind || entries[i].Target != target) continue;
                _lastHits.Add(entries[i].Amount);
                LastHitTotal += entries[i].Amount;
            }
        }

        public static string SkillSlotText(int slot)
        {
            Character player = ScenarioProbe.Player;
            if (player == null || player.skillCaster == null) return "no player";
            var equipped = player.skillCaster.EquippedSkills;
            if (slot < 0 || slot >= equipped.Count) return $"missing ({equipped.Count} slots)";
            var skill = equipped[slot];
            return skill != null && skill.SkillData != null ? $"skill {skill.SkillData.Id} on {player.name}#{player.GetInstanceID()}" : $"empty on {player.name}#{player.GetInstanceID()}";
        }

        public static string SkillLoadout()
        {
            Character player = ScenarioProbe.Player;
            if (player == null || player.skillCaster == null) return "no player";
            var caster = player.skillCaster;
            var granted = new List<string>();
            foreach (var skill in caster.GrantedSkills) granted.Add(skill != null && skill.SkillData != null ? skill.SkillData.Id.ToString() : "null");
            var equipped = new List<string>();
            foreach (var skill in caster.EquippedSkills) equipped.Add(skill != null && skill.SkillData != null ? skill.SkillData.Id.ToString() : "-");
            var bar = new List<string>();
            var hotbar = CoreSystem.UI.Hotbar.HotbarManager.Instance;
            if (hotbar != null && hotbar.ActiveRows.Count > 0 && hotbar.ActiveRows[0] != null)
            {
                foreach (var slot in hotbar.ActiveRows[0].Slots) bar.Add(slot == null || slot.SlotData == null || slot.SlotData.IsEmpty ? "-" : $"{slot.SlotData.ActionType}:{slot.SlotData.SkillId}");
            }
            return $"granted [{string.Join(",", granted)}], equipped [{string.Join(",", equipped)}], hotbar row 0 [{string.Join(",", bar)}]";
        }

        private static readonly System.Reflection.FieldInfo ApproachField =
            typeof(CoreSystem.Player.PlayerCore).GetField("_approach", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        private string GameClickView()
        {
            var core = CoreSystem.Player.PlayerCore.Instance;
            Character player = Player;
            if (core == null || player == null) return "no player";
            Vector3 mouseWorld = core.GetMouseWorldPosition();
            Collider2D first = Physics2D.OverlapPoint(mouseWorld);
            IEntity entity = first != null ? first.GetComponentInParent<IEntity>() : null;
            string entityName = entity is Object unityObject && unityObject != null ? unityObject.name : entity == null ? "none" : "destroyed";
            bool self = ReferenceEquals(entity, player);
            bool hostile = entity != null && !self && TeamRegistry.IsHostile(player, entity);
            var all = new List<string>();
            foreach (Collider2D collider in Physics2D.OverlapPointAll(mouseWorld)) all.Add(collider.name);
            return $"mouse world {mouseWorld.x:0.##},{mouseWorld.y:0.##}, first collider {(first != null ? first.name : "none")} of {all.Count} [{string.Join(", ", all)}], entity {entityName}{(self ? " (the player)" : string.Empty)}, "
                 + $"hostile {hostile}, player faction {player.Faction}, target faction {(entity != null ? entity.Faction.ToString() : "-")}, pointer over UI {CoreSystem.UI.UIManager.IsPointerOverUI()}, portal {(CoreSystem.World.MapPortal.AtPoint(mouseWorld) != null)}";
        }

        private bool FindPath(Vector3 destination, List<Vector2> path)
        {
            path.Clear();
            var grid = CoreSystem.Pathfinding.WalkableGridManager.Instance;
            if (grid == null || Player == null) return false;
            CoreSystem.Pathfinding.PathOutcome outcome = grid.FindPath(Player.transform.position, destination, path);
            return CoreSystem.Pathfinding.GridPathFormula.Found(outcome) && path.Count > 0;
        }

        public static bool PlayerIsApproaching()
        {
            var core = CoreSystem.Player.PlayerCore.Instance;
            return core != null && ApproachField != null && ApproachField.GetValue(core) is CoreSystem.Player.PlayerApproach approach && approach.IsActive;
        }

        private string CastState(int hotbarSlot)
        {
            Character player = Player;
            if (player == null || player.skillCaster == null) return "no player";
            var caster = player.skillCaster;
            var equipped = hotbarSlot >= 0 && hotbarSlot < caster.EquippedSkills.Count ? caster.EquippedSkills[hotbarSlot] : null;
            string equippedText = equipped != null && equipped.SkillData != null ? $"skill {equipped.SkillData.Id} ready {equipped.IsReady()}" : "empty";
            string hotbarText = "no hotbar";
            var hotbar = CoreSystem.UI.Hotbar.HotbarManager.Instance;
            if (hotbar != null && hotbar.ActiveRows.Count > 0 && hotbar.ActiveRows[0] != null)
            {
                var slot = hotbar.ActiveRows[0].GetSlot(hotbarSlot);
                hotbarText = slot == null ? "no slot" : slot.SlotData == null || slot.SlotData.IsEmpty ? "empty" : $"{slot.SlotData.ActionType} {slot.SlotData.SkillId}";
            }
            bool granted = equipped != null && equipped.SkillData != null && caster.GrantedSkills.Exists(s => s.SkillData != null && s.SkillData.Id == equipped.SkillData.Id);
            return $"hotbar slot {hotbarText}, caster slot {equippedText}, granted {granted}, aiming slot {caster.AimedSkillSlot}, caster {caster.CurrentState}";
        }

        private int ReadySkillCount()
        {
            int count = 0;
            if (Player == null || Player.skillCaster == null) return count;
            foreach (var skill in Player.skillCaster.EquippedSkills) if (skill != null && skill.IsReady()) count++;
            return count;
        }

        private bool TryEmptyScreenPoint(out Vector2 screen)
        {
            screen = Vector2.zero;
            if (Player == null) return false;
            for (int i = 0; i < 16; i++)
            {
                float angle = i * 22.5f * Mathf.Deg2Rad;
                Vector3 point = Player.transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * EmptySearchRadius;
                if (Physics2D.OverlapPoint(point) != null) continue;
                if (ScenarioInput.TryScreenOf(point, out screen)) return true;
            }
            return false;
        }
        #endregion
    }
}
#endif
