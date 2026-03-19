using System.Collections.Generic;

namespace ET
{
	[ComponentOf(typeof(Scene))]
	public class MonsterSpawnRuntimeComponent : Entity, IAwake, IDestroy
	{
		public HashSet<int> SpawnedConfigIds = new HashSet<int>();
	}
}
