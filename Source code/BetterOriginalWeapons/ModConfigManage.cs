using System.Reflection;
using UnityEngine;

namespace BetterOriginalWeapons
{
    public static class ModConfigManage
    {
        // 反射设置只读属性 DefaultMaxAmmo（自动属性生成的私有字段）
        private static void SetDefaultMaxAmmo(AmmoType ammoType, int value)
        {
            var field = typeof(AmmoType).GetField("<DefaultMaxAmmo>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            if ((object)field != null)
            {
                field.SetValue(ammoType, value);
            }
            else
            {
                ModPlugin.Log("错误：未找到 DefaultMaxAmmo 的 backing field");
            }
        }

        public static void ApplyModifications()
        {
            try
            {
                // 汤姆逊冲锋枪
                WeaponTypeData.tommyGun.Damage = 5f;
                WeaponTypeData.tommyGun.MaxClipAmmo = 50;
                SetDefaultMaxAmmo(AmmoTypeData.tommygun, 500);

                // 施迈瑟冲锋枪
                WeaponTypeData.schmeisser.Damage = 4.5f;
                SetDefaultMaxAmmo(AmmoTypeData.schmeisser, 300);

                // MG42机枪
                WeaponTypeData.mg42.Damage = 5f;
                WeaponTypeData.mg42.TimeBetweenAttacks = 0.06f;
                WeaponTypeData.mg42.ProjectileType.KickbackVelocity = new Vector2(5f, 0f);
                SetDefaultMaxAmmo(AmmoTypeData.mg42, 240);

                // AK47步枪
                WeaponTypeData.ak47.Damage = 6f;
                WeaponTypeData.ak47.MaxClipAmmo = 30;
                SetDefaultMaxAmmo(AmmoTypeData.ak47, 300);

                // 手枪
                WeaponTypeData.pistol.Damage = 6f;
                WeaponTypeData.pistol.MaxClipAmmo = 15;

                // 双手枪
                WeaponTypeData.pistols.Damage = 5f;
                WeaponTypeData.pistols.MaxClipAmmo = 30;
                SetDefaultMaxAmmo(AmmoTypeData.pistol, 120);

                // 毛瑟枪
                WeaponTypeData.mauser.Damage = 30f;
                WeaponTypeData.mauser.ProjectileType.TravelVelocityLimits = new Vector2(35f, 35f);
                WeaponTypeData.mauser.ProjectileType.Penetration = 2;
                WeaponTypeData.mauser.ProjectileType.PenetrationOnlyAfterKill = false;

                // 踢击
                WeaponTypeData.kick.Damage = 7.5f;
                WeaponTypeData.kick.KickBackVelocity = new Vector2(15f, 0f);

                // 球棍
                WeaponTypeData.baseballBat.Damage = 22.5f;

                // 电锯
                WeaponTypeData.chainsaw.Damage = 8f;
                SetDefaultMaxAmmo(AmmoTypeData.chainsaw, 1000);

                // 火箭筒
                ExplosionTypeData.rocket.Range = 2.55f;
                ExplosionTypeData.rocket.MaxDamagePerTick = 85f;
                SetDefaultMaxAmmo(AmmoTypeData.rocketLauncher, 15);

                // 榴弹发射器
                ExplosionTypeData.grenade40mm.MaxDamagePerTick = 50f;

                // 88炮炮弹
                SetDefaultMaxAmmo(AmmoTypeData.flakCannonShell, 3);

                // 霰弹枪
                WeaponTypeData.spas.Damage = 7.5f;
                WeaponTypeData.spas.ProjectileType.KickbackVelocity = new Vector2(15f, 0f);
                WeaponTypeData.spas.MaxClipAmmo = 10;
                SetDefaultMaxAmmo(AmmoTypeData.spas, 60);

                // 火焰喷射器
                WeaponTypeData.flamethrower.Damage = 8f;

                ModPlugin.Log("BetterOriginalWeapons 修改已应用");
            }
            catch (System.Exception ex)
            {
                ModPlugin.Log("修改过程中发生异常: " + ex);
            }
        }
    }
}
