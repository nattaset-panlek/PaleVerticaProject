using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CoreSystem.Data;
using CoreSystem.Localization;
using UnityEngine;

namespace CoreSystem.Editor.Localization
{
    public static class LocalizationCoverageFormula
    {
        #region [Variables/Fields]
        public const string Tag = "[Localization]";
        private const int MaxDepth = 10;

        private static readonly (string path, Type type)[] DataFiles =
        {
            ("Assets/Resources/Data/Item.json", typeof(CoreSystem.Data.Items.ItemDataList)),
            ("Assets/Resources/Data/Quest.json", typeof(CoreSystem.Quests.QuestDataList)),
            ("Assets/Resources/Data/Dialogue.json", typeof(CoreSystem.Dialogue.DialogueDataList))
        };
        #endregion

        #region [Public Methods]
        public static List<string> AuditProject()
        {
            var issues = new List<string>();
            issues.AddRange(AuditTable(LoadTable(LocalizationDatabase.AssetPath), Path.GetFileName(LocalizationDatabase.AssetPath)));
            foreach ((string path, Type type) in DataFiles)
            {
                if (!File.Exists(path)) continue;
                object data = JsonUtility.FromJson(File.ReadAllText(path), type);
                issues.AddRange(AuditData(data, Path.GetFileName(path)));
            }
            return issues;
        }

        public static LocalizationTable LoadTable(string path)
        {
            return File.Exists(path) ? LocalizationDatabase.Parse(File.ReadAllText(path)) : new LocalizationTable();
        }

        public static List<string> AuditTable(LocalizationTable table, string label)
        {
            var issues = new List<string>();
            if (table == null || table.Entries == null) return issues;
            var seen = new HashSet<string>();
            for (int i = 0; i < table.Entries.Count; i++)
            {
                LocalizationEntry entry = table.Entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.Key))
                {
                    issues.Add($"{Tag} {label} entry {i}: empty key");
                    continue;
                }
                if (!seen.Add(entry.Key)) issues.Add($"{Tag} {label} '{entry.Key}': duplicate key");
                List<string> missing = LocalizationFormula.MissingLanguages(entry.Text);
                if (missing.Count > 0) issues.Add($"{Tag} {label} '{entry.Key}': missing {string.Join(", ", missing)}");
            }
            return issues;
        }

        public static List<string> AuditData(object root, string label)
        {
            var issues = new List<string>();
            Walk(root, label, 0, issues);
            return issues;
        }
        #endregion

        #region [Private Methods]
        private static void Walk(object value, string path, int depth, List<string> issues)
        {
            if (value == null || depth > MaxDepth) return;
            if (value is LocalizedString text)
            {
                if (HasAnyText(text) && !text.IsComplete) issues.Add($"{Tag} {path}: missing {string.Join(", ", LocalizationFormula.MissingLanguages(text))}");
                return;
            }

            Type type = value.GetType();
            if (type.IsPrimitive || type.IsEnum || value is string || value is UnityEngine.Object) return;

            if (value is IList list)
            {
                for (int i = 0; i < list.Count; i++) Walk(list[i], $"{path}[{i}]", depth + 1, issues);
                return;
            }

            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!field.IsPublic && field.GetCustomAttribute<SerializeField>() == null) continue;
                if (field.IsNotSerialized) continue;
                Walk(field.GetValue(value), $"{path}{Describe(value)}.{field.Name}", depth + 1, issues);
            }
        }

        private static string Describe(object value)
        {
            foreach (string name in new[] { "ItemId", "Id", "QuestId", "DialogueId", "Key" })
            {
                FieldInfo field = value.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);
                if (field != null)
                {
                    object id = field.GetValue(value);
                    if (id != null) return $"({id})";
                }
            }
            return string.Empty;
        }

        private static bool HasAnyText(LocalizedString text)
        {
            foreach (string language in LocalizationFormula.Languages)
            {
                if (!string.IsNullOrWhiteSpace(LocalizationFormula.Raw(text, language))) return true;
            }
            return false;
        }
        #endregion
    }
}
