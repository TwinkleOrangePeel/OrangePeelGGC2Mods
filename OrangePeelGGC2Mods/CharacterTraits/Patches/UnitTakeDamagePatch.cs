using HarmonyLib;
using System.Reflection;
using UnityEngine;
using static OrangePeelGGC2Mods.CharacterTraits.Patches.UnitHealPatch;

namespace OrangePeelGGC2Mods.CharacterTraits.Patches
{
    [HarmonyPatch(typeof(Unit), "TakeDamage", new System.Type[] {
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
    public static class UnitTakeDamagePatch
    {
        private static void Prefix(
            Unit __instance,
            ref float amount,
            WeaponType weaponType,
            DamageType damageType,
            Vector2 impactPosition,
            Vector2 impactVelocity,
            ref Vector2 pushVelocity,
            bool isNettoAmount,
            bool ignoreDamageReduction,
            Player sourcePlayer,
            UnityEngine.Object sourceObject,
            int tick)
        {
            // 如果不需要修正或没有来源玩家，直接返回
            if (sourcePlayer == null || amount <= 0f)
                return;

            UnitType sourceUnitType = sourcePlayer.UnitType;
            UnitType targetUnitType = __instance.UnitType;

            // 伤害倍率修正（原始伤害）
            float factor = 1f;
            if ((sourceUnitType == UnitTypeData.vinnie && (weaponType == WeaponTypeData.tommyGun || targetUnitType.IsGangster || targetUnitType.IsZombie)) ||
                (sourceUnitType == UnitTypeData.paulie && (weaponType == WeaponTypeData.mg42 || targetUnitType.IsNazi)) ||
                (sourceUnitType == UnitTypeData.daisy && weaponType == WeaponTypeData.schmeisser) ||
                (sourceUnitType == UnitTypeData.ramboli && (weaponType == WeaponTypeData.ak47 || targetUnitType.IsPolice || targetUnitType.IsArmy)) ||
                (sourceUnitType == UnitTypeData.vindiana && (weaponType == WeaponTypeData.pistol || weaponType == WeaponTypeData.pistols || weaponType == WeaponTypeData.mauser || targetUnitType.IsNazi)) ||
                (sourceUnitType == UnitTypeData.vindicator && (targetUnitType.IsGangster || targetUnitType.IsPolice || targetUnitType.IsArmy || targetUnitType.IsNazi))||
                (sourceUnitType == UnitTypeData.steampunkVinnie && (weaponType == WeaponTypeData.flamethrower || targetUnitType == UnitTypeData.abominationNazi)) ||
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
            amount *= factor;

            // 暴击
            if (sourceUnitType == UnitTypeData.luigi && CriticalHitCalculator.CheckCriticalHit())
            {
                amount *= 1.5f;
                pushVelocity += new Vector2(0f, 0.1f);
                // 路易斯暴击回血（1% 最大生命值）
                if (sourcePlayer != null && sourcePlayer.Unit != null && sourcePlayer.Unit.IsAlive)
                {
                    float maxHP = sourcePlayer.Unit.UnitType.StartHitpoints;
                    float healAmount = maxHP * 0.01f;

                    // 设置跳过标记
                    UnitPlayCannoliQuotePatch.SkipCannoliQuote = true;
                    try
                    {
                        sourcePlayer.Unit.Heal(healAmount);
                    }
                    finally
                    {
                        // 确保标记被重置
                        UnitPlayCannoliQuotePatch.SkipCannoliQuote = false;
                    }
                }
            }

            // 潜行攻击：Daisy 秒杀未警觉的巡逻敌人
            if (sourcePlayer != null &&
                sourceUnitType == UnitTypeData.daisy &&
                UnitAllianceTypeData.AreEnemies(__instance.AllianceType, UnitAllianceTypeData.players) &&
                __instance.Personality != null &&
                __instance.Personality.StartWithPatrolling &&
                !__instance.IsAlerted)
            {
                // 获取 unitController，检查是否在巡逻状态
                FieldInfo unitControllerField = AccessTools.Field(typeof(Unit), "unitController");
                bool isPatrolling = true; // 默认认为在巡逻

                // 使用 ReferenceEquals 而不是 !=
                if (!ReferenceEquals(unitControllerField, null))
                {
                    UnitController controller = unitControllerField.GetValue(__instance) as UnitController;
                    if (controller != null)
                    {
                        isPatrolling = controller.WantToPatrol;
                    }
                }

                if (isPatrolling)
                {
                    amount = __instance.HitPoints;
                }
            }

            // -------- 血怒（兰博文尼） --------
            if (sourceUnitType == UnitTypeData.ramboli && sourcePlayer.Unit != null)
            {
                float maxHP = sourcePlayer.Unit.UnitType.StartHitpoints;
                float currentHP = sourcePlayer.Unit.HitPoints;
                float hpRatio = currentHP / maxHP;
                float maxbuff = 1.5f;
                float CriticalhpRatio = 0.25f;

                float buff;
                if (hpRatio <= CriticalhpRatio)
                    buff = 1.5f;
                else if (hpRatio >= 1f)
                    buff = 1f;
                else
                    buff = 1f + (maxbuff - 1f) * (1f - hpRatio) / (1 - CriticalhpRatio); // 简化为 1f + 0.5f * (1f - hpRatio)

                amount *= buff;
            }

            // ======== 抗性 ========
            float resistance = 1f;
            if (targetUnitType == UnitTypeData.paulie)
            {
                resistance *= 0.75f;
                if (sourceUnitType.IsArmy)
                    resistance = 0.5f;
            }
            else if (targetUnitType == UnitTypeData.vinnieMcClane && (damageType.IsMelee || sourceUnitType.IsPolice))
                resistance *= 0.5f;
            else if (targetUnitType == UnitTypeData.santaVinnie && damageType.IsExplosive)
                resistance *= 0.1f;
            else if (targetUnitType == UnitTypeData.santaVinnie && damageType.IsFire)
                resistance *= 0f;  // 免疫火焰伤害
            amount *= resistance;

            // 爆头修正
            bool isHeadshot = damageType.CanHeadshot && (__instance.TopPosition.y - __instance.UnitType.HeadHeight <= impactPosition.y);
            if (isHeadshot && targetUnitType == UnitTypeData.vindicator)
                amount *= 0.75f;

            // 一击秒杀
            if (sourceUnitType == UnitTypeData.vinnieMcClane &&
                weaponType == WeaponTypeData.baseballBat &&
                OneHitCalculator.CheckOneHit())
            {
                amount = __instance.HitPoints;
            }

            // 击退修正
            if (sourceUnitType == UnitTypeData.vinnieMcClane && weaponType == WeaponTypeData.baseballBat)
                pushVelocity += new Vector2(0f, 0.1f);
            else if (sourceUnitType == UnitTypeData.vindiana && weaponType == WeaponTypeData.mauser)
                pushVelocity += new Vector2(0.1f, 0f);

            if (sourceUnitType == UnitTypeData.slavVinnie)
                pushVelocity *= 2f;
        }
    }
}