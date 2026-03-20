using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Unit))]
    public class BTComponent : Entity, IAwake<byte[], string>, IAwake<string, string>, IDestroy
    {
        public byte[] TreeBytes;

        public string TreePackageKey;

        public string TreeIdOrName;

        public long RuntimeId;

        public readonly Dictionary<string, BTSerializedValue> BlackboardOverrides = new();
    }
}
