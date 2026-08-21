using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ImpossiblePlusMode
{
    [HarmonyPatch(typeof(LevelGameState), "SpawnNewAI")]
    public static class Patch_LevelGameState_SpawnNewAI
    {
        private static readonly Dictionary<UnitType, UnitType> ReplacementMap =
            new Dictionary<UnitType, UnitType>
            {
                { UnitTypeData.naziSoldierSpas, UnitTypeData.naziSoldierFlamethrower },
                { UnitTypeData.naziSoldierRocketLauncher, UnitTypeData.naziSoldierGuidedMissile },
                { UnitTypeData.zombieRunner, UnitTypeData.rat},
                { UnitTypeData.zombieWoman, UnitTypeData.rat},
                { UnitTypeData.zombieWomanWaitress, UnitTypeData.rat},
                { UnitTypeData.zombiePrisoner, UnitTypeData.ratJumper},
                { UnitTypeData.zombieSoldier, UnitTypeData.ratSpitter}
            };

        static void Prefix(
            ref UnitType unitType,
            Vector3 position,
            AIPersonalityType personality,
            SpawnAction.Tag tag,
            bool forceSpawn,
            bool aiDirectorIgnoresThis,
            int serverTick)
        {
            // 先排除巡逻状态
            if (personality.StartWithPatrolling)
                return;

            // 只有命中映射表并且概率通过时才替换
            if (ReplacementMap.TryGetValue(unitType, out UnitType replacement)&& 
                NewEnemyTypeSpawnCalculator.ShouldSpawnNewEnemyType())
            {
                unitType = replacement;
            }
        }
    }
}
