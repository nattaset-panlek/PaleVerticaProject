#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Data;
using CoreSystem.Data.Save;
using CoreSystem.Dialogue;
using CoreSystem.Entity;
using CoreSystem.Quests;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScenarioTesting
{
    public sealed class ScenarioRun
    {
        #region [Variables/Fields]
        public const string PlayerName = "player";
        public const string TargetName = "target";

        private readonly Dictionary<string, Character> _entities = new Dictionary<string, Character>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _errors = new List<string>();
        private readonly float _startTime;
        private ScenarioStepResult _step;
        private bool _finished;

        public event Action OnChanged;
        #endregion

        #region [Properties]
        public ScenarioDefinition Definition { get; }
        public ScenarioReport Report { get; }
        public ScenarioInput Input { get; }
        public CombatRecorder Combat { get; } = new CombatRecorder();
        public ScenarioLab Lab { get; }
        public ScenarioStatus Status => Report.Status;
        public int StepIndex => _step != null ? _step.Index : -1;
        public ScenarioStep CurrentStep => StepIndex >= 0 && StepIndex < Definition.Steps.Count ? Definition.Steps[StepIndex] : null;
        public bool StepFailed => _step != null && _step.Status == ScenarioStatus.Failed;
        public bool StopRequested { get; set; }
        public int ErrorCount => _errors.Count;
        public float Elapsed => Time.time - _startTime;
        public Dictionary<string, float> Variables => Report.Variables;
        public Dictionary<string, Dictionary<AttributeType, float>> Snapshots { get; } = new Dictionary<string, Dictionary<AttributeType, float>>();
        #endregion

        #region [Public Methods]
        public ScenarioRun(ScenarioDefinition definition, ScenarioInput input)
        {
            Definition = definition;
            Input = input;
            _startTime = Time.time;
            Lab = new ScenarioLab(this);
            Report = new ScenarioReport
            {
                Scenario = definition.Name,
                File = definition.File != null ? ScenarioLibrary.RelativePath(definition.File) : string.Empty,
                Agent = SaveProfileFormula.Sanitize(definition.Agent),
                StartedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }

        public void Begin()
        {
            if (Definition.ClearSaves) SaveStorage.DeleteProfile(Report.Agent);
            SaveStorage.UseProfile(Report.Agent);
            Report.SaveProfile = SaveStorage.Profile;
            Application.logMessageReceived += HandleLog;
            UnityEngine.Random.InitState(Definition.Seed);
            Log($"begin, seed {Definition.Seed}, saves in {SaveStorage.SaveFolderPath}");
        }

        public void BeginStep(ScenarioStep step)
        {
            _step = new ScenarioStepResult { Index = step.Index, Action = step.Action, Arguments = step.Describe(), Status = ScenarioStatus.Running, StartTime = Elapsed };
            Report.StepResults.Add(_step);
            AttachCombat();
            Log($"step {step.Index + 1}: {step.Describe()}");
            Changed();
        }

        public void EndStep()
        {
            if (_step == null) return;
            _step.Duration = Elapsed - _step.StartTime;
            if (_step.Status == ScenarioStatus.Running) _step.Status = ScenarioStatus.Passed;
            Changed();
        }

        public void FailStep(string message)
        {
            if (_step == null) return;
            if (_step.Status == ScenarioStatus.Failed) return;
            _step.Status = ScenarioStatus.Failed;
            _step.Message = message;
            Log($"step {_step.Index + 1} failed: {message}");
            Record($"step {_step.Index + 1} {_step.Action} completes", ScenarioCheckStatus.Failed, "completes", message, CurrentStep?.Note);
            Changed();
        }

        public void Note(string message)
        {
            if (_step != null) _step.Message = message;
            Log(message);
        }

        public string Finish()
        {
            if (_finished) return Report.Summary();
            _finished = true;

            Application.logMessageReceived -= HandleLog;
            Combat.Detach();
            Report.Combat.AddRange(Combat.Entries);
            Report.Errors.AddRange(_errors);
            Report.DurationSeconds = Elapsed;
            Report.Status = Report.Evaluate();
            SaveStorage.UseDefaultProfile();

            string path = Report.Write(System.IO.Path.GetDirectoryName(ScenarioLibrary.ReportPath(Definition)));
            Report.Write(ScenarioLibrary.HistoryFolder(Definition), $"{DateTime.Now:yyyyMMdd_HHmmss}_{Report.Status}.json");
            ScenarioResultsIndex.Rebuild(Definition.Agent);
            string summary = Report.Summary();
            if (Report.Status == ScenarioStatus.Failed) Debug.LogWarning($"{summary}\n{path}");
            else Debug.Log($"{summary}\n{path}");
            Changed();
            return summary;
        }

        public void Log(string message)
        {
            Report.Log.Add($"{Elapsed:0.00}s {message}");
        }

        public void Measure(string name, float value)
        {
            Report.Metrics[name] = value;
        }

        public void SetVariable(string name, float value)
        {
            Report.Variables[name] = value;
        }

        public ScenarioCheck Record(string name, ScenarioCheckStatus status, string expected, string actual, string note = null, string screenshot = null)
        {
            var check = new ScenarioCheck { Name = name, Status = status, Expected = expected, Actual = actual, Note = note, Screenshot = screenshot, Step = StepIndex, Time = Elapsed };
            Report.Checks.Add(check);
            if (status == ScenarioCheckStatus.Passed) Report.ChecksPassed++;
            else if (status == ScenarioCheckStatus.Review) Report.ChecksReview++;
            else Report.ChecksFailed++;
            Changed();
            return check;
        }

        public bool Check(string name, bool condition, string actual = null)
        {
            Record(name, condition ? ScenarioCheckStatus.Passed : ScenarioCheckStatus.Failed, "true", actual ?? condition.ToString().ToLowerInvariant());
            return condition;
        }

        public void Name(string name, Character character)
        {
            if (string.IsNullOrEmpty(name) || character == null) return;
            _entities[name] = character;
        }

        public Character Entity(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Equals(PlayerName, StringComparison.OrdinalIgnoreCase)) return ScenarioProbe.Player;
            if (_entities.TryGetValue(name, out Character named) && named != null) return named;
            if (name.Equals(TargetName, StringComparison.OrdinalIgnoreCase)) return Lab.Target;
            Character player = ScenarioProbe.Player;
            return int.TryParse(name, out int characterId) && player != null ? ScenarioProbe.FindCharacter(characterId, player.transform.position) : null;
        }

        public Character EntityFrom(ScenarioStep step, string key, string fallback)
        {
            if (step.IsNumber(key)) return Entity(step.Int(key).ToString());
            return Entity(step.String(key, fallback));
        }

        public bool TryEvaluate(string expression, out float value)
        {
            if (ScenarioExpression.TryEvaluate(expression, Resolve, out value, out string error)) return true;
            FailStep(error);
            return false;
        }

        public float Number(ScenarioStep step, string key, float fallback)
        {
            if (!step.Has(key)) return fallback;
            if (step.IsNumber(key)) return step.Float(key);
            return TryEvaluate(step.String(key), out float value) ? value : fallback;
        }

        public IEnumerator WaitFor(Func<bool> condition, float timeout, string what)
        {
            float deadline = Time.time + timeout;
            while (!condition())
            {
                if (Time.time > deadline)
                {
                    FailStep($"waited {timeout:0.#}s for {what}");
                    yield break;
                }
                yield return null;
            }
        }

        public float? Resolve(string name)
        {
            string[] parts = name.Split('.');
            string head = parts[0].ToLowerInvariant();
            Character player = ScenarioProbe.Player;

            switch (head)
            {
                case "var":
                    return parts.Length > 1 && Report.Variables.TryGetValue(parts[1], out float variable) ? variable : (float?)null;
                case "hit":
                    return parts.Length > 1 && int.TryParse(parts[1], out int index) ? (index < Lab.LastHits.Count ? Lab.LastHits[index] : 0f) : (float?)null;
                case "hits":
                    if (parts.Length < 2) return Lab.LastHits.Count;
                    if (parts[1] == "count") return Lab.LastHits.Count;
                    if (parts[1] == "total") return Lab.LastHitTotal;
                    if (parts[1] == "avg") return Lab.LastHits.Count > 0 ? Lab.LastHitTotal / Lab.LastHits.Count : 0f;
                    return null;
                case "dealt": return Combat.DamageDealt;
                case "taken": return Combat.DamageTaken;
                case "kills": return Combat.Kills;
                case "errors": return _errors.Count;
                case "time": return Elapsed;
                case "summons": return ScenarioLab.SummonCount(player);
                case "cast": return Lab.LastCastStarted ? 1f : 0f;
                case "potion": return Lab.LastPotionKept ? 1f : 0f;
                case "crit": return CoreSystem.Combat.DamageFormula.BaseCriticalMultiplier + AttributeProbe.Read(player, AttributeType.CriticalDamage);
                case "scene": return parts.Length > 1 && SceneManager.GetActiveScene().name.Equals(parts[1], StringComparison.OrdinalIgnoreCase) ? 1f : 0f;
                case "passive": return parts.Length > 1 && player != null && player.UniquePassives.HasPassive(parts[1]) ? 1f : 0f;
                case "items": return parts.Length > 1 && int.TryParse(parts[1], out int itemId) ? ItemCount(itemId) : (float?)null;
                case "status": return parts.Length > 1 && Enum.TryParse(parts[1], true, out QuestStatus status) ? (int)status : (float?)null;
                case "quest": return QuestValue(parts);
                case "dialogue": return DialogueValue(parts);
                case "distance": return parts.Length > 2 ? Distance(Entity(parts[1]), Entity(parts[2])) : (float?)null;
            }

            return EntityValue(parts);
        }
        #endregion

        #region [Private Methods]
        private void AttachCombat()
        {
            Character player = ScenarioProbe.Player;
            if (player != null && Combat.Subject != player) Combat.Attach(player);
        }

        private float? EntityValue(string[] parts)
        {
            if (parts.Length < 2) return null;
            Character character = Entity(parts[0]);
            string field = parts[1];
            if (field.Equals("exists", StringComparison.OrdinalIgnoreCase)) return character != null ? 1f : 0f;
            if (character == null) return null;
            if (field.Equals("dead", StringComparison.OrdinalIgnoreCase)) return character.IsDead ? 1f : 0f;
            if (field.Equals("x", StringComparison.OrdinalIgnoreCase)) return character.transform.position.x;
            if (field.Equals("y", StringComparison.OrdinalIgnoreCase)) return character.transform.position.y;
            if (field.Equals("level", StringComparison.OrdinalIgnoreCase)) return character.BaseLevel;
            if (field.Equals("baseExp", StringComparison.OrdinalIgnoreCase)) return character.CurrentBaseExp;
            if (field.Equals("passivePoints", StringComparison.OrdinalIgnoreCase)) return character.PassivePoints;
            return Enum.TryParse(field, true, out AttributeType type) ? AttributeProbe.Read(character, type) : (float?)null;
        }

        private float? QuestValue(string[] parts)
        {
            if (parts.Length < 3 || !int.TryParse(parts[1], out int questId)) return null;
            if (parts[2] == "status") return (int)ScenarioProbe.QuestStatusOf(questId);
            if (parts[2] == "count" && parts.Length > 3 && int.TryParse(parts[3], out int objective)) return ScenarioProbe.QuestCount(questId, objective);
            return null;
        }

        private static float? DialogueValue(string[] parts)
        {
            DialogueConversation conversation = ScenarioProbe.Conversation;
            bool open = conversation != null && !conversation.IsClosed;
            if (parts.Length < 2 || parts[1] == "open") return open ? 1f : 0f;
            if (!open) return -1f;
            switch (parts[1])
            {
                case "node": return conversation.CurrentNode != null ? conversation.CurrentNode.Id : -1f;
                case "choices": return conversation.Choices.Count;
                case "character": return conversation.CharacterId;
                case "quest": return conversation.SelectedQuest != null ? conversation.SelectedQuest.Id : -1f;
                default: return null;
            }
        }

        private static float? Distance(Character a, Character b)
        {
            return a != null && b != null ? Vector3.Distance(a.transform.position, b.transform.position) : (float?)null;
        }

        private static float ItemCount(int itemId)
        {
            int total = 0;
            foreach (var instance in GameData.Player.Inventory.GetAllItems()) if (instance != null && instance.ItemId == itemId) total += instance.Quantity;
            return total;
        }

        private void HandleLog(string message, string stackTrace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            if (message.StartsWith("[Scenario]")) return;
            string frame = string.IsNullOrEmpty(stackTrace) ? string.Empty : stackTrace.Split('\n')[0];
            _errors.Add($"{type}: {message} {frame}".Trim());
            Log($"error logged: {message}");
        }

        private void Changed()
        {
            OnChanged?.Invoke();
        }
        #endregion
    }
}
#endif
