using System.Collections.Generic;
using CoreSystem.Utile;
using UnityEngine;

namespace CoreSystem.Combat
{
    public static class EffectHandlers
    {
        #region [Variables/Fields]
        private static readonly AttributedTypeRegistry<EffectKind, IEffectHandler, EffectHandlerAttribute> Registry =
            new AttributedTypeRegistry<EffectKind, IEffectHandler, EffectHandlerAttribute>(attribute => attribute.Kind);

        private static readonly Dictionary<EffectKind, IEffectHandler> Instances = new Dictionary<EffectKind, IEffectHandler>();
        #endregion

        #region [Properties]
        public static IReadOnlyDictionary<EffectKind, System.Type> RegisteredTypes => Registry.Types;
        #endregion

        #region [Public Methods]
        public static IEffectHandler For(EffectKind kind)
        {
            if (Instances.TryGetValue(kind, out var handler)) return handler;

            handler = Registry.Create(kind);
            if (handler == null) Debug.LogWarning($"[CombatCore] No effect handler registered for {kind}.");
            Instances[kind] = handler;
            return handler;
        }
        #endregion
    }
}
