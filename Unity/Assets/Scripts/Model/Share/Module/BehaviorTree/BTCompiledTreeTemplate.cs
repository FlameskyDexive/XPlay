using System;
using System.Collections.Generic;

namespace ET
{
    [EnableClass]
    public sealed class BTCompiledTreeEntryTemplate
    {
        public BTDefinition Definition;

        public BTRoot Root;

        public Dictionary<int, BTNode> Nodes = new();
    }

    [EnableClass]
    public sealed class BTCompiledTreeTemplate
    {
        public readonly string PackageKey;

        public readonly BTPackage Package;

        public readonly Dictionary<string, BTCompiledTreeEntryTemplate> EntriesByTreeId = new();

        public readonly Dictionary<string, BTCompiledTreeEntryTemplate> EntriesByTreeName = new(StringComparer.OrdinalIgnoreCase);

        public BTCompiledTreeTemplate(string packageKey, BTPackage package)
        {
            this.PackageKey = packageKey ?? string.Empty;
            this.Package = package;
        }

        public void AddEntry(BTDefinition definition, BTRoot root, Dictionary<int, BTNode> nodes)
        {
            if (definition == null || root == null || nodes == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            BTCompiledTreeEntryTemplate entry = new()
            {
                Definition = definition,
                Root = root,
                Nodes = new Dictionary<int, BTNode>(nodes),
            };

            if (!string.IsNullOrWhiteSpace(definition.TreeId))
            {
                this.EntriesByTreeId[definition.TreeId] = entry;
            }

            if (!string.IsNullOrWhiteSpace(definition.TreeName))
            {
                this.EntriesByTreeName[definition.TreeName] = entry;
            }
        }

        public BTCompiledTreeEntryTemplate ResolveEntry(string treeIdOrName)
        {
            if (string.IsNullOrWhiteSpace(treeIdOrName))
            {
                if (!string.IsNullOrWhiteSpace(this.Package?.EntryTreeId)
                    && this.EntriesByTreeId.TryGetValue(this.Package.EntryTreeId, out BTCompiledTreeEntryTemplate entryById))
                {
                    return entryById;
                }

                if (!string.IsNullOrWhiteSpace(this.Package?.EntryTreeName)
                    && this.EntriesByTreeName.TryGetValue(this.Package.EntryTreeName, out BTCompiledTreeEntryTemplate entryByName))
                {
                    return entryByName;
                }

                return null;
            }

            if (this.EntriesByTreeId.TryGetValue(treeIdOrName, out BTCompiledTreeEntryTemplate directById))
            {
                return directById;
            }

            return this.EntriesByTreeName.TryGetValue(treeIdOrName, out BTCompiledTreeEntryTemplate directByName) ? directByName : null;
        }
    }
}
