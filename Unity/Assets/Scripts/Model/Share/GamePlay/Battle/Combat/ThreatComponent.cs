using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET
{
    [ComponentOf(typeof(Unit))]
    public class ThreatComponent : Entity, IAwake, ITransfer, IDestroy
    {
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<long, long> ThreatMap = new Dictionary<long, long>();

        public long PrimaryTargetId;
        public long LastThreatUpdateTime;
    }
}
