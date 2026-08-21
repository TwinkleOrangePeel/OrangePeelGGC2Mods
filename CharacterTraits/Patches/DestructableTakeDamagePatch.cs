using HarmonyLib;
using UnityEngine;
using static OrangePeelGGC2Mods.CharacterTraits.Patches.UnitHealPatch;

namespace OrangePeelGGC2Mods.CharacterTraits.Patches
{
    [HarmonyPatch(typeof(Destructable), "TakeDamage", new System.Type[] {
        typeof(float),
        typeof(WeaponType),
        typeof(DamageType),
        typeof(Vector2),
        typeof(Vector2),
        typeof(Vector2),
        typeof(bool),
        typeof(bool),
        typeof(Player),
        typeof(UnityEngine.Object),
        typeof(int)
    })]
    public static class DestructableTakeDamagePatch
    {
        [HarmonyPrefix]
        public static void Prefix(
            Destructable __instance,
            ref float amount,
            WeaponType weaponType,
            DamageType damageType,
            // 以下参数保留以确保签名匹配，即使不使用
            Vector2 impactPosition,
            Vector2 impactVelocity,
            Vector2 pushVelocity,
            bool nettoAmount,
            bool ignoreDamageReduction,
            Player sourcePlayer,
            UnityEngine.Object sourceObject,
            int tick)
        {
            // 基础检查
            if (sourcePlayer == null || amount <= 0f)
                return;

            UnitType sourceUnitType = sourcePlayer.UnitType;
            float finalDamage = amount;

            // 伤害因子（与 Unit 补丁保持一致）
            float factor = 1f;
            if ((sourceUnitType == UnitTypeData.vinnie && weaponType == WeaponTypeData.tommyGun) ||
                (sourceUnitType == UnitTypeData.paulie && weaponType == WeaponTypeData.mg42) ||
                (sourceUnitType == UnitTypeData.daisy && weaponType == WeaponTypeData.schmeisser) ||
                (sourceUnitType == UnitTypeData.ramboli && weaponType == WeaponTypeData.ak47) ||
                (sourceUnitType == UnitTypeData.steampunkVinnie) || //蒸汽朋克文尼对物品和boss常驻增伤
                (sourceUnitType == UnitTypeData.vindiana && (weaponType == WeaponTypeData.pistol || weaponType == WeaponTypeData.pistols || weaponType == WeaponTypeData.mauser)) ||
                (sourceUnitType == UnitTypeData.zoey && weaponType == WeaponTypeData.spas))
            {
                factor *= 1.25f;
            }
            else if (sourceUnitType == UnitTypeData.santaVinnie && damageType.IsExplosive)
            {
                factor *= 1.5f;
            }
            else if (sourceUnitType == UnitTypeData.vinnieMcClane && damageType.IsMelee)
            {
                factor *= 2f;
            }
            finalDamage *= factor;

            // 暴击
            if (sourceUnitType == UnitTypeData.luigi && CriticalHitCalculator.CheckCriticalHit())
            { 
                finalDamage *= 1.5f;
                // 路易斯暴击回血（1% 最大生命值）
                if (sourcePlayer != null && sourcePlayer.Unit != null && sourcePlayer.Unit.IsAlive)
                {
                    float maxHP = sourcePlayer.Unit.UnitType.StartHitpoints;
                    float healAmount = maxHP * 0.01f; // 1% 最大生命值

                    // 设置跳过标记
                    UnitPlayCannoliQuotePatch.SkipCannoliQuote = true;
                    try
                    {
                        sourcePlayer.Unit.Heal(healAmount);
                    }
                    finally
                    {
                        UnitPlayCannoliQuotePatch.SkipCannoliQuote = false;
                    }
                }
            }

            // 血怒（兰博文尼）
            if (sourceUnitType == UnitTypeData.ramboli)
            {
                float maxHP = sourcePlayer.Unit.UnitType.StartHitpoints;
                float currentHP = sourcePlayer.Unit.HitPoints;
                float hpRatio = currentHP / maxHP; // 0~1 之间，0 为濒死，1 为满血

                // 当 hpRatio <= 0.25 时，buff = 1.5（满额增伤）
                // 当 hpRatio >= 1.0 时，buff = 1.0（无增伤）
                // 之间线性插值
                float buff = 1f;
                float buffmax = 1.5f;
                float threshold = 0.25f;

                if (hpRatio <= 0.5f)
                    buff = buffmax;
                else if (hpRatio >= 1f)
                    buff = 1f;
                else
                    buff = 1f + (buffmax - 1f) * (1f - hpRatio) / (1f - threshold); // 线性插值

                finalDamage *= buff;
            }

            // 一击秒杀
            if (sourceUnitType == UnitTypeData.vinnieMcClane &&
                weaponType == WeaponTypeData.baseballBat &&
                OneHitCalculator.CheckOneHit())
            {
                // 直接设置为当前生命值，原方法会自动截断为当前生命值，确保击杀
                // 这样不会产生溢出，也不需要反射
                finalDamage = __instance.HitPoints;
                if (finalDamage <= 0f) finalDamage = 1f; // 防御性检查
            }

            // 将修改后的伤害写回原始参数
            amount = finalDamage;
        }
    }
}