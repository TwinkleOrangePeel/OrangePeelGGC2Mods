using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace OrangePeelGGC2Mods.CharacterTraits.Patches
{
    /// <summary>
    /// 子弹初始化时设置角色专属穿透
    /// </summary>
    [HarmonyPatch(typeof(Projectile), "Init", new[] { typeof(IWeaponHolder), typeof(ProjectileType), typeof(ActiveWeapon), typeof(int) })]
    public static class WeaponPenetrationInitPatch
    {
        private static FieldInfo remainingPenetrationField;

        static WeaponPenetrationInitPatch()
        {
            remainingPenetrationField = AccessTools.Field(typeof(Projectile), "remainingPenetration");
            if (ReferenceEquals(remainingPenetrationField, null))
                Debug.LogError("[WeaponPenetration] 找不到 remainingPenetration 字段");
        }

        [HarmonyPostfix]
        public static void Postfix(Projectile __instance, IWeaponHolder weaponHolder, ProjectileType projectileType)
        {
            try
            {
                if (weaponHolder == null || projectileType == null)
                    return;

                Unit unit = weaponHolder.DamagingUnit;
                if (unit == null)
                    return;

                // Zoey 使用 Spas：穿透 99
                if (unit.UnitType == UnitTypeData.zoey && ReferenceEquals(projectileType, ProjectileTypeData.spas))
                {
                    if (!ReferenceEquals(remainingPenetrationField, null))
                        remainingPenetrationField.SetValue(__instance, 99);
                }
                // Vindiana 使用 Mauser：穿透 4
                else if (unit.UnitType == UnitTypeData.vindiana && ReferenceEquals(projectileType, ProjectileTypeData.mauser))
                {
                    if (!ReferenceEquals(remainingPenetrationField, null))
                        remainingPenetrationField.SetValue(__instance, 4);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[WeaponPenetration] InitPostfix 异常: " + e);
            }
        }
    }

    /// <summary>
    /// 子弹命中时，为 Zoey 的 Spas 临时取消穿透后伤害衰减
    /// </summary>
    [HarmonyPatch(typeof(Projectile), "OnHit")]
    public static class WeaponPenetrationOnHitPatch
    {
        private static FieldInfo remainingPenetrationField;
        private static FieldInfo projectileTypeField;
        private static PropertyInfo damageAfterPenetrationProperty;
        private static float originalDamageAfterPenetration;

        static WeaponPenetrationOnHitPatch()
        {
            remainingPenetrationField = AccessTools.Field(typeof(Projectile), "remainingPenetration");
            projectileTypeField = AccessTools.Field(typeof(Projectile), "projectileType");
            damageAfterPenetrationProperty = AccessTools.Property(typeof(ProjectileType), "DamageAfterPenetration");

            if (ReferenceEquals(remainingPenetrationField, null))
                Debug.LogError("[WeaponPenetration] 找不到 remainingPenetration 字段");
            if (ReferenceEquals(projectileTypeField, null))
                Debug.LogError("[WeaponPenetration] 找不到 projectileType 字段");
            if (ReferenceEquals(damageAfterPenetrationProperty, null))
                Debug.LogError("[WeaponPenetration] 找不到 DamageAfterPenetration 属性");
        }

        [HarmonyPrefix]
        public static void Prefix(Projectile __instance)
        {
            try
            {
                if (ReferenceEquals(remainingPenetrationField, null) ||
                    ReferenceEquals(projectileTypeField, null) ||
                    ReferenceEquals(damageAfterPenetrationProperty, null))
                    return;

                int pen = (int)remainingPenetrationField.GetValue(__instance);
                ProjectileType projType = projectileTypeField.GetValue(__instance) as ProjectileType;

                if (pen == 99 && ReferenceEquals(projType, ProjectileTypeData.spas))
                {
                    // 保存原值并将zoey的霰弹枪穿透伤害临时设为3
                    originalDamageAfterPenetration = (float)damageAfterPenetrationProperty.GetValue(projType, null);
                    damageAfterPenetrationProperty.SetValue(projType, 3f, null);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[WeaponPenetration] OnHitPrefix 异常: " + e);
            }
        }

        [HarmonyPostfix]
        public static void Postfix(Projectile __instance)
        {
            try
            {
                if (ReferenceEquals(remainingPenetrationField, null) ||
                    ReferenceEquals(projectileTypeField, null) ||
                    ReferenceEquals(damageAfterPenetrationProperty, null))
                    return;

                int pen = (int)remainingPenetrationField.GetValue(__instance);
                ProjectileType projType = projectileTypeField.GetValue(__instance) as ProjectileType;

                if (pen == 99 && ReferenceEquals(projType, ProjectileTypeData.spas))
                {
                    // 恢复原值，避免影响其他子弹
                    damageAfterPenetrationProperty.SetValue(projType, originalDamageAfterPenetration, null);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[WeaponPenetration] OnHitPostfix 异常: " + e);
            }
        }
    }
}