using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Interaction
{
    public static class InteractionRegistry
    {
        #region [Variables/Fields]
        private static readonly List<IInteractionProvider> Providers = new List<IInteractionProvider>();
        private static readonly List<InteractionOption> Buffer = new List<InteractionOption>();
        #endregion

        #region [Properties]
        public static int Count => Providers.Count;
        #endregion

        #region [Public Methods]
        public static void Register(IInteractionProvider provider)
        {
            if (provider == null || Providers.Contains(provider)) return;
            Providers.Add(provider);
        }

        public static void Unregister(IInteractionProvider provider)
        {
            Providers.Remove(provider);
        }

        public static List<InteractionOption> Collect(InteractionContext context)
        {
            var result = new List<InteractionOption>();
            var seen = new HashSet<string>();

            for (int i = 0; i < Providers.Count; i++)
            {
                Buffer.Clear();
                try
                {
                    Providers[i].Collect(context, Buffer);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                    continue;
                }

                for (int j = 0; j < Buffer.Count; j++)
                {
                    var option = Buffer[j];
                    if (option == null || option.Execute == null || string.IsNullOrEmpty(option.Id)) continue;
                    if (!seen.Add(option.Id)) continue;
                    result.Add(option);
                }
            }

            Buffer.Clear();
            StableSortByPriority(result);
            return result;
        }

        public static void Clear()
        {
            Providers.Clear();
            Buffer.Clear();
        }
        #endregion

        #region [Private Methods]
        private static void StableSortByPriority(List<InteractionOption> options)
        {
            for (int i = 1; i < options.Count; i++)
            {
                var current = options[i];
                int j = i - 1;
                while (j >= 0 && options[j].Priority < current.Priority)
                {
                    options[j + 1] = options[j];
                    j--;
                }
                options[j + 1] = current;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Clear();
        }
        #endregion
    }
}
