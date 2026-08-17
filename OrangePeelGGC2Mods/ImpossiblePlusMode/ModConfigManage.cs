using System.Reflection;
using UnityEngine;

namespace OrangePeelGGC2Mods.ImpossiblePlusMode
{
    public static class ModConfigManage
    {
        // 辅助方法：设置只读属性 BulletCasingType
        private static void SetBulletCasingType(WeaponType weapon, BulletCasingType value)
        {
            var field = typeof(WeaponType).GetField("<BulletCasingType>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            if ((object)field != null)
                field.SetValue(weapon, value);
            else
                ImpossiblePlusModePlugin.Log("未找到 BulletCasingType 的 backing field");
        }

        // 辅助方法：设置只读属性 MuzzleEffect
        private static void SetMuzzleEffect(WeaponType weapon, MuzzleEffect value)
        {
            var field = typeof(WeaponType).GetField("<MuzzleEffect>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            if ((object)field != null)
                field.SetValue(weapon, value);
            else
                ImpossiblePlusModePlugin.Log("未找到 MuzzleEffect 的 backing field");
        }
        public static void ApplyModifications()
        {
            try
            {
                // -------- 难度调整（极难额外刷新率 = 2.0，原始为0.9f） --------
                DifficultyTypeData.impossible.ExtraSpawnsFactor = 2f;

                // 通用向量（原代码中用于 AIInfo）
                Vector2 vector = 10.5f * new Vector2(1.77777f, 1f);

                // -------- 左轮黑帮 --------
                WeaponTypeData.enemyRevolver.ProjectileType.TravelVelocityLimits = new Vector2(20f, 20f);
                WeaponTypeData.enemyRevolver.Damage = 15f;
                WeaponTypeData.enemyRevolver.ProjectileType.Penetration = 1;
                WeaponTypeData.enemyRevolver.ProjectileType.PenetrationOnlyAfterKill = false;
                WeaponTypeData.enemyRevolver.AIInfo = new StraightProjectileAIInfo(
                    new KeepAttackingPattern(0.2f, true), 1.8f, vector, float.PositiveInfinity, 0f, true
                );
                // 设置 prefabPath（使用实际对象类型和 NonPublic）
                var proj = WeaponTypeData.enemyRevolver.ProjectileType;
                var prefabField = proj.GetType().GetField("prefabPath", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if ((object)prefabField != null)
                {
                    prefabField.SetValue(proj, "Prefabs/Projectiles/mg42Bullet");
                    ImpossiblePlusModePlugin.Log("prefabPath 设置成功");
                }
                else
                    ImpossiblePlusModePlugin.Log("未找到 prefabPath 字段");
                // 创建并设置攻击音效
                SoundType magnumShootSound = new SoundType(
                    "Guns/magnum_357",
                    1, 1, true, false, 0.8f, new Vector2(0.9f, 1.1f), 25f, 0.25f, false, false, false, 105, null, 0.01f
                );
                // 直接赋值（如果是字段）
                WeaponTypeData.enemyRevolver.AttackSound = magnumShootSound;
                // 使用反射修改只读属性
                SetBulletCasingType(WeaponTypeData.enemyRevolver, BulletCasingTypeData.bigBullet);
                SetMuzzleEffect(WeaponTypeData.enemyRevolver, MuzzleEffectData.mauser);

                // -------- 左轮防爆盾特警 --------
                // 为该武器分配独立的 ProjectileLaunchType（使用游戏中未使用的 revolver 发射配置），
                // 避免与左轮黑帮（enemyRevolver）共享弹射物属性，防止互相干扰。
                WeaponTypeData.pistolShield.ProjectileLaunchType = WeaponTypeData.revolver.ProjectileLaunchType;
                WeaponTypeData.pistolShield.ProjectileLaunchType = WeaponTypeData.revolver.ProjectileLaunchType;
                WeaponTypeData.pistolShield.Damage = 10f;
                WeaponTypeData.pistolShield.ProjectileType.TravelVelocityLimits = new Vector2(15f, 15f);

                // -------- 毛瑟德军 --------
                WeaponTypeData.enemyMauser.ProjectileType.TravelVelocityLimits = new Vector2(20f, 20f);
                WeaponTypeData.enemyMauser.ProjectileType.Penetration = 2;
                WeaponTypeData.enemyMauser.ProjectileType.PenetrationOnlyAfterKill = false;

                // -------- 汤姆逊黑帮/特警 --------
                WeaponTypeData.enemyTommyGun.AIInfo = new StraightProjectileAIInfo(
                    new KeepAttackingPattern(0.5f, true), 0.8f, vector, float.PositiveInfinity, 0f, true
                );
                WeaponTypeData.gangsterTommyGun.AIInfo = new StraightProjectileAIInfo(
                    new KeepAttackingPattern(0.5f, true), 0.8f, vector, float.PositiveInfinity, 0f, true
                );

                // -------- 施迈瑟步兵 --------
                WeaponTypeData.enemySchmeisser.AIInfo = new StraightProjectileAIInfo(
                    new BurstAttackPattern(0.5f, 0.2f, 2, true), 0.8f, vector, float.PositiveInfinity, 0f, true
                );

                // -------- AK47步兵 --------
                WeaponTypeData.enemySturmGewehr.AIInfo = new StraightProjectileAIInfo(
                    new KeepAttackingPattern(0.5f, true), 0.8f, vector, float.PositiveInfinity, 0f, true
                );

                // -------- 霰弹枪美军/德军 --------
                WeaponTypeData.enemySpas.Damage = 15f;
                WeaponTypeData.enemySpas.KickBackVelocity = new Vector2(12f, 7f);
                WeaponTypeData.enemySpas.AIInfo = new StraightProjectileAIInfo(
                    new KeepAttackingPattern(0.3f, true), 0.7f, vector, 6f, 0f, false
                );

                // -------- 美军/德军步枪手 --------
                WeaponTypeData.enemyRifle.ProjectileType.TravelVelocityLimits = new Vector2(15f, 15f);
                WeaponTypeData.enemyRifle.AIInfo = new StraightProjectileAIInfo(
                    new KeepAttackingPattern(0.3f, true), 0.8f, vector, float.PositiveInfinity, 0f, true
                );

                // -------- 所有敌人踢击 --------
                WeaponTypeData.enemyKick.KickBackVelocity = new Vector2(12f, 0f);

                // -------- 野兽僵尸 --------
                WeaponTypeData.abominationSlam.Damage = 20f;
                WeaponTypeData.abominationSlam.KickBackVelocity = new Vector2(20f, 8f);

                // -------- 机械野兽 --------
                WeaponTypeData.abominationKnife.Damage = 25f;
                WeaponTypeData.abominationKnife.KickBackVelocity = new Vector2(20f, 8f);

                // -------- 断臂/服务员女僵尸 --------
                WeaponTypeData.zombieWomanArm.Damage = 15f;

                // -------- 小型老鼠 --------
                WeaponTypeData.ratJumpAttack.Damage = 12.5f;

                // -------- 板凳黑帮 --------
                WeaponTypeData.barstoolGoon.Damage = 12.5f;

                // -------- 扳手黑帮 --------
                WeaponTypeData.pipewrenchGoon.Damage = 17.5f;

                // -------- 防暴特警 --------
                WeaponTypeData.policeBatton.Damage = 17.5f;
                WeaponTypeData.battonShield.Damage = 17.5f;

                // -------- 德军警卫 --------
                WeaponTypeData.saberShield.Damage = 25f;

                // -------- 保镖冲锋击退 --------
                WeaponTypeData.charge.KickBackVelocity = new Vector2(17.5f, 7f);

                ImpossiblePlusModePlugin.Log("Impossible+ Mode 修改已应用");
            }
            catch (System.Exception ex)
            {
                ImpossiblePlusModePlugin.Log("Impossible+ Mode 修改过程中发生异常: " + ex);
            }
        }
    }
}
