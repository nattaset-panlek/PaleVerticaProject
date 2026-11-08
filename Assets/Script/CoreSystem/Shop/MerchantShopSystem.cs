using System.Collections.Generic;
using CoreSystem.Entity;
using CoreSystem.Interaction;
using CoreSystem.Player;
using UnityEngine;

namespace CoreSystem.Shop
{
    [DisallowMultipleComponent]
    public class MerchantShopSystem : MonoBehaviour, IInteractionProvider
    {
        #region [Variables/Fields]
        public const string OptionId = "shop.merchant";
        public const string DefaultWindowName = "Merchant Stall";

        [SerializeField] private Character _character;
        [SerializeField] private string _shopId = "general_merchant";
        [SerializeField] private string _label = "Shop";
        [SerializeField, Min(0f)] private float _range = 1.5f;
        [SerializeField] private int _priority = 210;
        [SerializeField] private string _windowName = DefaultWindowName;
        #endregion

        #region [Properties]
        public Character Character => _character;
        public string ShopId => _shopId;
        public string WindowName => string.IsNullOrEmpty(_windowName) ? DefaultWindowName : _windowName;
        #endregion

        #region [Unity Methods]
        private void Reset()
        {
            _character = GetComponentInParent<Character>();
        }

        private void Awake()
        {
            if (_character == null) _character = GetComponentInParent<Character>();
        }

        private void OnEnable()
        {
            InteractionRegistry.Register(this);
        }

        private void OnDisable()
        {
            InteractionRegistry.Unregister(this);
        }
        #endregion

        #region [Public Methods]
        public void Configure(Character character, string shopId, string label, float range)
        {
            _character = character;
            _shopId = shopId;
            _label = label;
            _range = Mathf.Max(0f, range);
        }

        public void Collect(InteractionContext context, List<InteractionOption> options)
        {
            if (context.Actor == null || context.Target != _character || _character == null) return;
            if (context.IsSelf || _character.IsDead || !(context.Holder is PlayerCore)) return;
            if (ShopDatabase.Get(_shopId) == null) return;
            options.Add(new InteractionOption(OptionId, _label, _ => OpenWindow(), _range, _priority));
        }

        public void OpenWindow()
        {
            MerchantFactory.OpenShop(ShopDatabase.Get(_shopId), WindowName);
        }
        #endregion
    }
}
