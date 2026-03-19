namespace ET
{
    public static class DamageResolveHelper
    {
        public static void ResolveDamage(Unit from, Unit to, EHitFromType hitType = EHitFromType.Skill_Normal, Unit bullet = null)
        {
            BattleHelper.HitSettle(from, to, hitType, bullet);
        }
    }
}
