using HarmonyLib;
using UnityEngine; // 添加这个，用来识别 Collider2D

namespace CharacterTraits.Patches
{
    namespace ImpossiblePlusMode
    {
        [HarmonyPatch(typeof(SearchLightBeam), "OnTriggerStay2D")]
        public static class Patch_SearchLightBeam_OnTriggerStay2D
        {
            static bool Prefix(SearchLightBeam __instance, Collider2D collider)
            {
                Unit unit = collider.GetUnit();

                // 如果单位是 Daisy 并且还活着，直接阻止探照灯攻击
                if (unit != null && unit.IsAlive && unit.UnitType == UnitTypeData.daisy)
                {
                    return false; // 跳过原方法，不调用 OnUnitEntersBeam
                }

                return true;
            }
        }
    }
}