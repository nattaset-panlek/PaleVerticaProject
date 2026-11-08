using System.Collections.Generic;
using CoreSystem.Editor.NPC;
using CoreSystem.Entity;
using CoreSystem.Interaction;
using CoreSystem.Player;
using CoreSystem.Refinement;
using NUnit.Framework;
using UnityEngine;

namespace CoreSystem.Tests
{
    public class RefinementMerchantTests
    {
        #region [Variables/Fields]
        private readonly List<GameObject> _created = new List<GameObject>();
        private Character _merchant;
        private Character _player;
        private PlayerCore _playerCore;
        private RefinementMerchantSystem _system;
        #endregion

        #region [Unity Methods]
        [SetUp]
        public void SetUp()
        {
            _merchant = Create("Merchant").AddComponent<Character>();
            _player = Create("Player").AddComponent<Character>();
            _playerCore = Create("PlayerCore").AddComponent<PlayerCore>();
            _system = _merchant.gameObject.AddComponent<RefinementMerchantSystem>();
            _system.Configure(_merchant, "Refine", 1.5f, "Refinement");
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _created.Count; i++)
            {
                if (_created[i] != null) Object.DestroyImmediate(_created[i]);
            }
            _created.Clear();
        }
        #endregion

        #region [Public Methods]
        [Test]
        public void Collect_PlayerTargetingMerchant_AddsRefineOption()
        {
            var options = new List<InteractionOption>();
            _system.Collect(new InteractionContext(_player, _merchant, _playerCore), options);

            Assert.AreEqual(1, options.Count);
            Assert.AreEqual(RefinementMerchantSystem.OptionId, options[0].Id);
            Assert.AreEqual("Refine", options[0].Label);
            Assert.AreEqual(1.5f, options[0].Range);
        }

        [Test]
        public void Collect_OtherTarget_AddsNothing()
        {
            var other = Create("Other").AddComponent<Character>();
            var options = new List<InteractionOption>();
            _system.Collect(new InteractionContext(_player, other, _playerCore), options);

            Assert.AreEqual(0, options.Count);
        }

        [Test]
        public void ShouldOffer_RequiresPlayerHolderAndNotSelf()
        {
            Assert.IsTrue(RefinementMerchantSystem.ShouldOffer(new InteractionContext(_player, _merchant, _playerCore), _merchant));
            Assert.IsFalse(RefinementMerchantSystem.ShouldOffer(new InteractionContext(_player, _merchant, new object()), _merchant));
            Assert.IsFalse(RefinementMerchantSystem.ShouldOffer(new InteractionContext(_merchant, _merchant, _playerCore), _merchant));
            Assert.IsFalse(RefinementMerchantSystem.ShouldOffer(new InteractionContext(_player, _merchant, _playerCore), null));
        }

        [Test]
        public void Configure_NegativeRange_ClampsToZero_EmptyWindowUsesDefault()
        {
            _system.Configure(_merchant, "Refine", -2f, "");

            Assert.AreEqual(0f, _system.Range);
            Assert.AreEqual(RefinementMerchantSystem.DefaultWindowName, _system.WindowName);
        }

        [Test]
        public void ApplySettings_BlacksmithSpec_MakesStationaryNeutralRefineMerchant()
        {
            NpcPrefabSpec spec = AlchemiteReturnNpcBuilder.Blacksmith;
            Assert.IsTrue(NpcPrefabBuilder.ApplySettings(_merchant.gameObject, spec));

            Assert.AreEqual(CharacterControllerType.None, _merchant.DefaultController);
            Assert.IsTrue(_merchant.HasTag(CharacterTag.Merchant));
            Assert.AreEqual(NpcPrefabBuilder.NeutralFaction, _merchant.Faction);
            Assert.IsTrue(_merchant.IsInteractable);
            Assert.IsFalse(_merchant.AgentProfile.CanRoam);
            Assert.AreEqual("Refine", _system.Label);
            Assert.AreEqual(RefinementMerchantSystem.DefaultWindowName, _system.WindowName);
            Assert.AreEqual(_merchant, _system.Character);
        }
        #endregion

        #region [Private Methods]
        private GameObject Create(string name)
        {
            var go = new GameObject(name);
            _created.Add(go);
            return go;
        }
        #endregion
    }
}
