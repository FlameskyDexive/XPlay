using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(LSUnitViewComponent))]
    public static partial class LSUnitViewComponentSystem
    {
        private const string DefaultUnitAssetPath = "Assets/Bundles/Unit/Player.prefab";

        [EntitySystem]
        private static void Awake(this LSUnitViewComponent self)
        {

        }
        
        [EntitySystem]
        private static void Destroy(this LSUnitViewComponent self)
        {

        }

        public static async ETTask InitAsync(this LSUnitViewComponent self)
        {
            Room room = self.Room();
            LSUnitComponent lsUnitComponent = room.LSWorld.GetComponent<LSUnitComponent>();
            Scene root = self.Root();
            foreach (long playerId in room.PlayerIds)
            {
                LSUnit lsUnit = lsUnitComponent.GetChild<LSUnit>(playerId);
                GameObject prefab = await room.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(DefaultUnitAssetPath);
                if (prefab == null)
                {
                    continue;
                }

                GlobalComponent globalComponent = root.GetComponent<GlobalComponent>();
                GameObject unitGo = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
                unitGo.transform.position = lsUnit.Position.ToVector();

                LSUnitView lsUnitView = self.AddChildWithId<LSUnitView, GameObject>(lsUnit.Id, unitGo);
                lsUnitView.AddComponent<LSAnimatorComponent>();
            }
        }
    }
}
