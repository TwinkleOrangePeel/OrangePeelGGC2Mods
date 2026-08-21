using HarmonyLib;
using UnityEngine;

namespace OrangePeelGGC2Mods.CharacterTraits.Patches
{
    /// <summary>
    /// 降低敌人对 Daisy 的攻击意愿（50% 概率放弃攻击）
    /// </summary>
    [HarmonyPatch(typeof(Unit), "GoodIdeaToAttack")]
    public static class GoodIdeaToAttackPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Unit __instance, DamageableMonoBehaviour damageable, ref bool __result)
        {
            // 只处理目标是 Unit 的情况
            Unit targetUnit = damageable as Unit;
            if (targetUnit == null)
                return true; // 不是 Unit，保持原逻辑

            // 只处理 Daisy
            if (!ReferenceEquals(targetUnit.UnitType, UnitTypeData.daisy))
                return true;

            // 只处理敌对关系
            if (!__instance.IsEnemy(targetUnit))
                return true;

            // 50% 概率放弃攻击
            if (Random.value < 0.5f)
            {
                __result = false;
                return false; // 跳过原方法
            }

            // 其余情况正常执行原方法
            return true;
        }
    }
}