using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ET
{
    [EntitySystemOf(typeof(PlayerMoveComponent))]
    [FriendOf(typeof(PlayerMoveComponent))]
    public static partial class PlayerMoveComponentSystem
    {

        [EntitySystem]
        private static void Destroy(this PlayerMoveComponent self)
        {
            
        }

        [EntitySystem]
        private static void Awake(this PlayerMoveComponent self)
        {
            self.IsMoving = false;
        }
        [EntitySystem]
        private static void FixedUpdate(this PlayerMoveComponent self)
        {
            self.UpdateMove();
        }

        public static void UpdateMove(this PlayerMoveComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            if (!self.IsMoving)
                return;
            float speed = unit.GetComponent<NumericComponent>().GetAsFloat(NumericType.Speed);
            speed = speed == 0 ? 3 : speed;
            float3 deltaPos = unit.Forward * speed / DefineCore.LogicFrame;
            unit.Position += deltaPos;
        }
        
        public static void StartMove(this PlayerMoveComponent self)
        {
            if (self.IsMoving)
            {
                return;
            }

            self.IsMoving = true;
            Unit unit = self.GetParent<Unit>();
            unit.GetComponent<CombatStateComponent>()?.SetMoving();
            EventSystem.Instance.Publish(self.Scene(), new MoveStart() { Unit = unit });
        }

        public static void StopMove(this PlayerMoveComponent self)
        {
            if (!self.IsMoving)
            {
                return;
            }

            self.IsMoving = false;
            Unit unit = self.GetParent<Unit>();
            unit.GetComponent<CombatStateComponent>()?.TryRestoreIdleFromMove();
            EventSystem.Instance.Publish(self.Scene(), new MoveStop() { Unit = unit });
        }
    }
}
