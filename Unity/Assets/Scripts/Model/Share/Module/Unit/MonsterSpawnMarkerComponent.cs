namespace ET
{
	[ComponentOf(typeof(Unit))]
	public class MonsterSpawnMarkerComponent : Entity, IAwake<int, int>, IDestroy
	{
		public int SpawnConfigId;
		public int SpawnIndex;
	}
}
