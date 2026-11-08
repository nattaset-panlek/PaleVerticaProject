using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.Editor.CharacterAsset;
using CoreSystem.Editor.NPC;
using CoreSystem.Entity;
using CoreSystem.Refinement;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CoreSystem.Tests
{
    public class AlchemiteReturnNpcTests
    {
        #region [Variables/Fields]
        private const string CharacterJsonPath = "Assets/Resources/Data/Character.json";
        private static readonly IEnumerable<NpcPrefabSpec> Specs = AlchemiteReturnNpcBuilder.All;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [TestCaseSource(nameof(Specs))]
        public void PsbPassesTheCharacterStandard(NpcPrefabSpec spec)
        {
            List<CharacterCheckResult> results = CharacterPsbValidator.Validate(spec.PsbPath, CharacterAssetSettings.Instance);
            List<string> errors = results.FindAll(r => r.Severity == CharacterCheckSeverity.Error).ConvertAll(r => r.Message);
            Assert.IsEmpty(errors, string.Join("\n", errors));
        }

        [TestCaseSource(nameof(Specs))]
        public void PrefabUsesTheSixPartsOfItsOwnPsb(NpcPrefabSpec spec)
        {
            GameObject prefab = LoadPrefab(spec);
            foreach (CharacterPartSpec part in CharacterAssetSettings.Instance.Parts)
            {
                Transform node = prefab.transform.Find(part.RendererPath);
                Assert.IsNotNull(node, part.RendererPath);
                SpriteRenderer renderer = node.GetComponent<SpriteRenderer>();
                Assert.IsNotNull(renderer.sprite, part.RendererPath);
                Assert.AreEqual(part.Name, renderer.sprite.name, part.RendererPath);
                Assert.AreEqual(spec.PsbPath, AssetDatabase.GetAssetPath(renderer.sprite), part.RendererPath);
            }
        }

        [TestCaseSource(nameof(Specs))]
        public void PrefabIsANeutralStandingNpc(NpcPrefabSpec spec)
        {
            Character character = LoadPrefab(spec).GetComponent<Character>();
            Assert.IsNotNull(character);
            Assert.AreNotEqual(CharacterControllerType.Player, character.DefaultController);
            Assert.AreEqual(NpcPrefabBuilder.NeutralFaction, character.Faction);
            Assert.IsTrue(character.IsInteractable);
            Assert.AreEqual(spec.NpcId, character.NpcId);
            Assert.IsTrue(character.HasTag(spec.Tags));
            if (character.DefaultController == CharacterControllerType.None) Assert.IsFalse(character.AgentProfile.CanRoam);
        }

        [TestCaseSource(nameof(Specs))]
        public void OnlyMerchantsCarryTheMerchantSystem(NpcPrefabSpec spec)
        {
            GameObject prefab = LoadPrefab(spec);
            RefinementMerchantSystem merchant = prefab.GetComponent<RefinementMerchantSystem>();
            if (!spec.IsMerchant)
            {
                Assert.IsNull(merchant);
                return;
            }
            Assert.IsNotNull(merchant);
            Assert.AreEqual(spec.MerchantWindow, merchant.WindowName);
            Assert.AreEqual(prefab.GetComponent<Character>(), merchant.Character);
        }

        [TestCaseSource(nameof(Specs))]
        public void PrefabMatchesItsCharacterJsonEntry(NpcPrefabSpec spec)
        {
            Character character = LoadPrefab(spec).GetComponent<Character>();
            Assert.AreEqual(spec.CharacterId, character.CharacterId);
            Assert.AreEqual(spec.Name, character.AgentProfile.AgentName);
            TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>(CharacterJsonPath);
            Assert.IsNotNull(json, CharacterJsonPath);
            CharacterModel entry = JsonUtility.FromJson<CharacterDataList>(json.text).Characters.Find(c => c.Id == spec.CharacterId);
            Assert.IsNotNull(entry, $"Character.json has no entry {spec.CharacterId} for {spec.Name}.");
            Assert.AreEqual(spec.PrefabPath, entry.PrefabPath);
            Assert.AreEqual(NpcPrefabBuilder.NeutralFaction, entry.Faction);
        }

        [Test]
        public void AlchemistIsASocketMerchant()
        {
            Assert.AreEqual("Socket", AlchemiteReturnNpcBuilder.Alchemist.MerchantWindow);
            Assert.IsTrue(AlchemiteReturnNpcBuilder.Alchemist.IsMerchant);
        }

        [Test]
        public void BlacksmithIsARefineMerchant()
        {
            NpcPrefabSpec blacksmith = AlchemiteReturnNpcBuilder.Blacksmith;
            Assert.IsTrue(blacksmith.IsMerchant);
            Assert.AreEqual(RefinementMerchantSystem.DefaultWindowName, blacksmith.MerchantWindow);
            Assert.AreEqual("Refine", blacksmith.MerchantLabel);
            Assert.AreEqual(CharacterTag.Merchant, blacksmith.Tags);
        }
        #endregion

        #region [Private Methods]
        private static GameObject LoadPrefab(NpcPrefabSpec spec)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.PrefabPath);
            Assert.IsNotNull(prefab, spec.PrefabPath + " is missing; run Pale Vertica > Entity > AlchemiteReturn NPCs > Build Prefabs.");
            return prefab;
        }
        #endregion
    }
}
