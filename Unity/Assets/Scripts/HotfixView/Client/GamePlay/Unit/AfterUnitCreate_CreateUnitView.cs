using UnityEngine;

namespace ET.Client
{
    [Event(SceneType.Current)]
    public class AfterUnitCreate_CreateUnitView: AEvent<Scene, AfterUnitCreate>
    {
        private const string DefaultUnitAssetPath = "Assets/Bundles/Unit/Player.prefab";

        protected override async ETTask Run(Scene scene, AfterUnitCreate args)
        {
            Unit unit = args.Unit;
            EntityRef<Scene> sceneRef = scene;
            EntityRef<Unit> unitRef = unit;
            GameObject go = null;
            switch (unit.Type())
            {
                case EUnitType.Player:
                case EUnitType.Monster:
                {
                    string resName = GetBattleUnitResName(unit);
                    GameObject prefab = await scene.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(resName);
                    scene = sceneRef;
                    unit = unitRef;
                    if (scene == null || unit == null || unit.IsDisposed || prefab == null)
                    {
                        return;
                    }

                    GlobalComponent globalComponent = scene.Root().GetComponent<GlobalComponent>();
                    go = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
                    Transform iconTrans = go.transform.Find("Root/Sprite");
                    if (unit.Type() == EUnitType.Player && iconTrans != null)
                    {
                        SpriteRenderer spriteRenderer = iconTrans.GetComponent<SpriteRenderer>();
                        if (spriteRenderer != null)
                        {
                            spriteRenderer.sprite = scene.GetComponent<ResourcesLoaderComponent>().LoadAssetSync<Sprite>($"Avatar{unit.Config().Id - 1000}");
                        }
                    }

                    int hp = unit.GetComponent<NumericComponent>().GetAsInt(NumericType.Hp);
                    if (hp > 10)
                    {
                        go.transform.localScale = Vector3.one * hp / 50f;
                    }

                    break;
                }
                case EUnitType.Bullet:
                {
                    GameObject prefab = await scene.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>("Bullet_001");
                    scene = sceneRef;
                    unit = unitRef;
                    if (scene == null || unit == null || unit.IsDisposed || prefab == null)
                    {
                        return;
                    }

                    GlobalComponent globalComponent = scene.Root().GetComponent<GlobalComponent>();
                    go = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
                    break;
                }
            }

            if (go == null)
            {
                return;
            }

            go.transform.position = unit.Position;
            unit.AddComponent<GameObjectComponent>().Init(go);
            if (unit.Type() == EUnitType.Player || unit.Type() == EUnitType.Monster)
            {
                unit.AddComponent<CombatAnimancerComponent>();
                unit.AddComponent<CombatAnimStateComponent>().InitializeFromLogicState();
            }

            await ETTask.CompletedTask;
        }

        private static async ETTask<GameObject> LoadBattleUnitPrefab(Scene scene)
        {
            return await scene.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(DefaultUnitAssetPath);
        }

        private static string GetBattleUnitResName(Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return DefaultUnitAssetPath;
            }

            string resName = unit.Config()?.ResName;
            if (string.IsNullOrWhiteSpace(resName))
            {
                return DefaultUnitAssetPath;
            }

            if (resName.Contains("/") || resName.Contains("\\"))
            {
                return resName;
            }

            return $"Assets/Bundles/Unit/{resName}.prefab";
        }
    }
}
