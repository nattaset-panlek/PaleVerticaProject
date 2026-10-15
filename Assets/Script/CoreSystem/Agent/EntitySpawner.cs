using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Enums;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using CoreSystem.Data;

namespace CoreSystem.Agent
{
    // ─── EntitySpawner ────────────────────────────────────────────────────────────
    public class EntitySpawner : MonoBehaviour
    {
        [Header("Zone Shape")]
        public SpawnZoneShape ZoneShape = SpawnZoneShape.Circle;
        [Tooltip("Radius (Circle) or Half-Size (Box).")]
        public float Radius = 5f;
        public Vector2 BoxSize = new Vector2(10f, 10f);

        [Header("Timing")]
        [Tooltip("Delay (seconds) before first spawn wave.")]
        [Min(0f)] public float InitialDelay = 0f;
        [Tooltip("Cooldown between each spawn wave.")]
        [Min(0f)] public float WaveCooldown = 5f;
        [Tooltip("Delay between individual spawns within one wave.")]
        [Min(0f)] public float SpawnInterval = 0.3f;
        [Tooltip("0 = infinite waves.")]
        public int MaxWaves = 0;
        [Tooltip("Maximum number of alive spawned entities at any time. 0 = unlimited.")]
        public int MaxAlive = 0;

        [Header("Spawn Table")]
        public List<SpawnEntry> SpawnTable = new List<SpawnEntry>();

        [Header("Debug")]
        [SerializeField] private int _waveCount;
        [SerializeField] private int _aliveCount;

        private List<GameObject> _aliveEntities = new List<GameObject>();
        private Coroutine _spawnRoutine;

        // ─── Lifecycle ────────────────────────────────────────────

        private void Start()
        {
            _spawnRoutine = StartCoroutine(SpawnLoop());
        }

        private IEnumerator SpawnLoop()
        {
            if (InitialDelay > 0f) yield return new WaitForSeconds(InitialDelay);

            while (MaxWaves == 0 || _waveCount < MaxWaves)
            {
                yield return StartCoroutine(SpawnWave());
                _waveCount++;
                yield return new WaitForSeconds(WaveCooldown);
            }
        }

        private IEnumerator SpawnWave()
        {
            var selected = SelectEntriesForWave();

            foreach (var entry in selected)
            {
                int count = Random.Range(entry.MinCount, entry.MaxCount + 1);
                for (int i = 0; i < count; i++)
                {
                    CleanDestroyedEntities();
                    if (MaxAlive > 0 && _aliveCount >= MaxAlive) yield break;

                    SpawnOne(entry);
                    if (SpawnInterval > 0f) yield return new WaitForSeconds(SpawnInterval);
                }
            }
        }

        private List<SpawnEntry> SelectEntriesForWave()
        {
            // Weighted random selection (each entry evaluated independently by weight)
            var result = new List<SpawnEntry>();
            foreach (var entry in SpawnTable)
            {
                if (entry.CharacterId <= 0) continue;
                float roll = Random.Range(0f, 100f);
                if (roll <= entry.Weight)
                    result.Add(entry);
            }
            // Fallback: if nothing gets past probability, pick the highest-weight entry
            if (result.Count == 0 && SpawnTable.Count > 0)
            {
                SpawnEntry best = null;
                foreach (var e in SpawnTable)
                    if (e.CharacterId > 0 && (best == null || e.Weight > best.Weight)) best = e;
                if (best != null) result.Add(best);
            }
            return result;
        }

        private void SpawnOne(SpawnEntry entry)
        {
            if (entry.CharacterId <= 0) return;
            var characterData = Database.GetCharacter(entry.CharacterId);
            if (characterData == null || string.IsNullOrEmpty(characterData.PrefabPath))
            {
                Debug.LogWarning($"[EntitySpawner] Character {entry.CharacterId} has no valid PrefabPath.");
                return;
            }

            Vector3 pos = GetRandomPositionInZone();
            
            // Because InstantiateAsync can take a frame or more, we pre-increment _aliveCount 
            // to prevent exceeding MaxAlive if multiple spawns are requested in the same frame.
            _aliveCount++; 

            Addressables.InstantiateAsync(characterData.PrefabPath, pos, Quaternion.identity, transform).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject go = handle.Result;
                    if (go == null) 
                    {
                        _aliveCount--;
                        return;
                    }

                    ApplyOverrides(go, entry);
                    _aliveEntities.Add(go);
                }
                else
                {
                    Debug.LogError($"[EntitySpawner] Failed to load Addressable {characterData.PrefabPath}");
                    _aliveCount--;
                }
            };
        }

        private void ApplyOverrides(GameObject go, SpawnEntry entry)
        {
            var character = go.GetComponent<CoreSystem.Entity.Character>();
            if (character != null)
            {
                var profile = character.AgentProfile;
                if (entry.OverrideBehavior)        profile.Behavior        = entry.Behavior;
                if (entry.OverrideCombatStyle)     profile.Combat          = entry.Combat;
                if (entry.OverrideGroupID)         profile.GroupID         = entry.GroupID;
                if (entry.OverrideDetectionRadius) profile.DetectionRadius = entry.DetectionRadius;
                if (entry.OverrideAttackRange)     profile.OverrideAttackRange(character, entry.AttackRange);

                var characterData = Database.GetCharacter(entry.CharacterId);
                if (characterData != null)
                {
                    character.faction = characterData.Faction;
                }

                if (entry.OverrideLevel && character.attributeSystem != null)
                {
                    var lvl = character.attributeSystem.GetAttribute(AttributeType.BaseLevel);
                    if (lvl != null)
                    {
                        lvl.BaseValue = entry.Level;
                        character.RecalculateStats();
                    }
                }
            }
        }

        private Vector3 GetRandomPositionInZone()
        {
            Vector3 origin = transform.position;
            if (ZoneShape == SpawnZoneShape.Circle)
            {
                Vector2 rnd = Random.insideUnitCircle * Radius;
                return origin + new Vector3(rnd.x, rnd.y, 0f);
            }
            else // Box
            {
                float x = Random.Range(-BoxSize.x * 0.5f, BoxSize.x * 0.5f);
                float y = Random.Range(-BoxSize.y * 0.5f, BoxSize.y * 0.5f);
                return origin + new Vector3(x, y, 0f);
            }
        }

        private void CleanDestroyedEntities()
        {
            int before = _aliveEntities.Count;
            _aliveEntities.RemoveAll(e => e == null);
            int removed = before - _aliveEntities.Count;
            _aliveCount = Mathf.Max(0, _aliveCount - removed);
        }

        // ─── Public API ───────────────────────────────────────────

        public void StartSpawning()
        {
            if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
            _waveCount = 0;
            _spawnRoutine = StartCoroutine(SpawnLoop());
        }

        public void StopSpawning()
        {
            if (_spawnRoutine != null) { StopCoroutine(_spawnRoutine); _spawnRoutine = null; }
        }

        public void DestroyAll()
        {
            foreach (var e in _aliveEntities)
                if (e != null) Destroy(e);
            _aliveEntities.Clear();
            _aliveCount = 0;
        }

        // ─── Gizmos ───────────────────────────────────────────────

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
            if (ZoneShape == SpawnZoneShape.Circle)
            {
                Handles.color = new Color(1f, 0.5f, 0f, 0.18f);
                Handles.DrawSolidDisc(transform.position, Vector3.forward, Radius);
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
                Gizmos.DrawWireSphere(transform.position, Radius);
            }
            else
            {
                Gizmos.DrawCube(transform.position, new Vector3(BoxSize.x, BoxSize.y, 0.1f));
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
                Gizmos.DrawWireCube(transform.position, new Vector3(BoxSize.x, BoxSize.y, 0.1f));
            }

            // Label
            Handles.Label(transform.position + Vector3.up * (ZoneShape == SpawnZoneShape.Circle ? Radius + 0.5f : BoxSize.y * 0.5f + 0.5f),
                $"Spawner  W:{_waveCount}  Alive:{_aliveCount}",
                new GUIStyle { normal = { textColor = new Color(1f, 0.7f, 0.2f) }, fontStyle = FontStyle.Bold });
        }
#endif
    }
}
