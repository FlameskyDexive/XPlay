
using Box2DSharp.Dynamics.Contacts;

namespace ET
{
    [Event(SceneType.Map)]
    [FriendOf(typeof(BulletComponent))]
    public class OnCollisionContactHandler: AEvent<Scene, OnCollisionContact>
    {
        protected override async ETTask Run(Scene scene, OnCollisionContact args)
        {
            Unit unitA = (Unit)args.contact.FixtureA.UserData;
            Unit unitB = (Unit)args.contact.FixtureB.UserData;
            if (unitA == null || unitB == null || unitA.IsDisposed || unitB.IsDisposed)
            {
                return;
            }
            
            //当前子弹只处理子弹伤害，子弹回血（给队友回血/技能吸血自行拓展）
            if (unitA.Type() == EUnitType.Bullet && IsCombatUnit(unitB))
            {
                BulletComponent bulletComponent = unitA.GetComponent<BulletComponent>();
                Unit owner = bulletComponent?.OwnerUnit;
                if (owner == null || owner.Id == unitB.Id)
                {
                    return;
                }

                ResolveBulletHit(unitA, unitB, bulletComponent, owner);
                scene.GetComponent<UnitComponent>()?.Remove(unitA.Id);
            }//由于box2d没有双向碰撞响应，处理不同类型的时候判断各自类型
            else if (IsCombatUnit(unitA) && unitB.Type() == EUnitType.Bullet)
            {
                BulletComponent bulletComponent = unitB.GetComponent<BulletComponent>();
                Unit owner = bulletComponent?.OwnerUnit;
                if (owner == null || owner.Id == unitA.Id)
                {
                    return;
                }

                ResolveBulletHit(unitB, unitA, bulletComponent, owner);
                scene.GetComponent<UnitComponent>()?.Remove(unitB.Id);
            }//玩家跟玩家碰撞，判定玩家重量大小，大吃小
            else if(unitA.Type() == EUnitType.Player && unitB.Type() == EUnitType.Player)
            {
                
            }//玩家吃到食物
            else if(unitA.Type() == EUnitType.Player && unitB.Type() == EUnitType.Food)
            {
                //获取食物的分量，添加给玩家，同时销毁食物单位
            }

            await ETTask.CompletedTask;
        }

        private static bool IsCombatUnit(Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return false;
            }

            EUnitType unitType = unit.Type();
            return unitType == EUnitType.Player || unitType == EUnitType.Monster;
        }

        private static void ResolveBulletHit(Unit bulletUnit, Unit target, BulletComponent bulletComponent, Unit owner)
        {
            if (bulletUnit == null || target == null || bulletComponent == null)
            {
                return;
            }

            if (bulletComponent.HitActionEventIds == null || bulletComponent.HitActionEventIds.Count == 0)
            {
                BattleHelper.HitSettle(owner, target, EHitFromType.Skill_Bullet, bulletUnit);
                return;
            }

            for (int index = 0; index < bulletComponent.HitActionEventIds.Count; ++index)
            {
                bulletComponent.CreateActionEvent(bulletComponent.HitActionEventIds[index], target);
            }
        }
    }
}
