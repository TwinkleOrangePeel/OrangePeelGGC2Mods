using HarmonyLib;
using UnityEngine;

namespace OrangePeelGGC2Mods.CharacterTraits.Patches
{
    /// <summary>
    /// 文尼（Vinnie）治疗量提升 1.5 倍
    /// </summary>
    [HarmonyPatch(typeof(Unit), "Heal")]
    public static class UnitHealPatch
    {
        [HarmonyPrefix]
        public static void Prefix(Unit __instance, ref float amount)
        {
            if (__instance.UnitType == UnitTypeData.vinnie)
            {
                amount *= 1.5f;
            }
        }
    }

    /// <summary>
    /// 跳过 Cannoli 语音（用于暴击回血等场景）
    /// </summary>
    [HarmonyPatch(typeof(Unit), "PlayCannoliQuote")]
    public static class UnitPlayCannoliQuotePatch
    {
        public static bool SkipCannoliQuote { get; set; } = false;

        [HarmonyPrefix]
        public static bool Prefix(Unit __instance)
        {
            if (SkipCannoliQuote)
            {
                return false; // 阻止原方法执行
            }
            return true;
        }
    }
}