using System;
using System.Collections.Generic;

namespace ET
{
    [Code]
    public class BTCompiledTreeRegistry : Singleton<BTCompiledTreeRegistry>, ISingletonAwake
    {
        private readonly Dictionary<string, IBTCompiledTreeProvider> providers = new(StringComparer.OrdinalIgnoreCase);

        public bool IsInitialized;

        public void Awake()
        {
        }

        public bool TryGetTemplate(string packageKey, out BTCompiledTreeTemplate template)
        {
            template = null;
            if (string.IsNullOrWhiteSpace(packageKey))
            {
                return false;
            }

            EnsureInitialized(this);
            if (!this.providers.TryGetValue(packageKey, out IBTCompiledTreeProvider provider))
            {
                return false;
            }

            template = provider.Template;
            return template != null;
        }

        public bool HasTemplate(string packageKey)
        {
            return this.TryGetTemplate(packageKey, out _);
        }

        private static void EnsureInitialized(BTCompiledTreeRegistry self)
        {
            if (self.IsInitialized)
            {
                return;
            }

            self.IsInitialized = true;
            HashSet<Type> types = CodeTypes.Instance.GetTypes(typeof(BTCompiledTreeProviderAttribute));
            foreach (Type type in types)
            {
                if (type.IsAbstract)
                {
                    continue;
                }

                if (Activator.CreateInstance(type) is not IBTCompiledTreeProvider provider)
                {
                    throw new Exception($"behavior tree compiled provider invalid: {type.FullName}");
                }

                if (string.IsNullOrWhiteSpace(provider.PackageKey))
                {
                    throw new Exception($"behavior tree compiled provider missing package key: {type.FullName}");
                }

                if (!self.providers.TryAdd(provider.PackageKey, provider))
                {
                    throw new Exception($"behavior tree compiled provider duplicate package key: {provider.PackageKey}");
                }
            }
        }
    }
}
