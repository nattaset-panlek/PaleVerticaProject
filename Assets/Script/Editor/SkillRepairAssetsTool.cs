using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Logic.Effects;
using CoreSystem.Skills.UniqueSkill;
using UnityEditor;
using UnityEngine;

namespace CoreSystem.Editor
{
    public static class SkillRepairAssetsTool
    {
        #region [Variables/Fields]
        private const string Root = "Assets/Resources/Skill/ActiveSkill/";
        private const string MindControlPath = Root + "Unique/Mind_Control_1019.asset";
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/Skills/Apply Skill Repairs")]
        public static void Apply()
        {
            var log = new List<string>();
            RepairChainLightning(log);
            RepairParry(log);
            RepairBlinkStrike(log);
            RepairBlink(log);
            RepairChronosField(log);
            CreateMindControl(log);
            AssetDatabase.SaveAssets();
            Debug.Log("[SkillRepair] " + string.Join(" | ", log));
        }
        #endregion

        #region [Private Methods]
        private static T Load<T>(string relativePath, List<string> log) where T : ActiveSkill
        {
            T skill = AssetDatabase.LoadAssetAtPath<T>(Root + relativePath);
            if (skill == null) log.Add("MISSING " + relativePath);
            return skill;
        }

        private static void RepairChainLightning(List<string> log)
        {
            var skill = Load<ChainLightningSkill>("Unique/Chain_Lightning_1012.asset", log);
            if (skill == null) return;
            skill.MaxJumps = 5;
            skill.JumpRadius = 5f;
            skill.DamageDecayPerJump = 0.8f;
            Save(skill, log);
        }

        private static void RepairParry(List<string> log)
        {
            var skill = Load<ParrySkill>("Unique/Perfect_Parry_1011.asset", log);
            if (skill == null) return;
            skill.ParryWindow = 0.5f;
            skill.CounterDamageMult = 2f;
            Save(skill, log);
        }

        private static void RepairBlinkStrike(List<string> log)
        {
            var skill = Load<BlinkStrikeSkill>("Unique/Blink_Strike_1013.asset", log);
            if (skill == null) return;
            skill.StrikeDamageMult = 1.5f;
            skill.BehindDistance = 1f;
            if (!skill.Effects.Exists(effect => effect is DamageEffect)) skill.Effects.Add(new DamageEffect());
            Save(skill, log);
        }

        private static void RepairBlink(List<string> log)
        {
            var skill = Load<ActiveSkill>("Blink_1016.asset", log);
            if (skill == null) return;
            foreach (var effect in skill.Effects)
            {
                if (effect is MovementEffectLogic movement && movement.Type == MovementType.TeleportToTarget) movement.Distance = 5f;
            }
            Save(skill, log);
        }

        private static void RepairChronosField(List<string> log)
        {
            var skill = Load<ActiveSkill>("Chornos_Field_1017.asset", log);
            if (skill == null) return;
            skill.IsContinuousArea = true;
            skill.AreaDuration = 6f;
            skill.AreaTickRate = 0.5f;
            skill.BaseTargeting = new SphereAOETargeting { AlignmentAction = TargetAlignment.Enemy };
            skill.Effects.Clear();
            skill.Effects.Add(new ApplyTemporaryBuffEffect
            {
                TargetAttribute = AttributeType.MoveSpeed,
                ModifierValue = -0.5f,
                ModType = ModifierType.PercentAdd,
                Duration = 1f,
                MaxStacks = 1,
                BuffName = "Time Slow",
                BuffCode = "Chronos_Slow",
                Category = BuffCategory.Effect
            });
            Save(skill, log);
        }

        private static void CreateMindControl(List<string> log)
        {
            if (AssetDatabase.LoadAssetAtPath<MindControlSkill>(MindControlPath) != null)
            {
                log.Add("Mind Control already exists");
                return;
            }
            var skill = ScriptableObject.CreateInstance<MindControlSkill>();
            skill.Id = 1019;
            skill.Name = "Mind Control";
            skill.Duration = 4f;
            skill.BaseTargeting = new SingleUnitTargeting { AlignmentAction = TargetAlignment.Enemy, CastRange = 8f };
            AssetDatabase.CreateAsset(skill, MindControlPath);
            log.Add("Created Mind Control");
        }

        private static void Save(ActiveSkill skill, List<string> log)
        {
            EditorUtility.SetDirty(skill);
            log.Add("OK " + skill.name);
        }
        #endregion
    }
}
