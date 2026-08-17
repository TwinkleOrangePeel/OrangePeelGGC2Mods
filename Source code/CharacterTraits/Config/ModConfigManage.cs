using UnityEngine;

namespace OrangePeelGGC2Mods.CharacterTraits
{
    public static class ModConfigManage
    {
        public static void ApplyModifications()
        {
            try
            {
                UnitTypeData.ramboli.StartHitpoints = 300f;
                UnitTypeData.steampunkVinnie.JumpVelocity = 18f;
                UnitTypeData.steampunkVinnie.MinFallDamageExtraVelocity = 12.5f;
                UnitTypeData.slavVinnie.MaxSpeed = 7.5f;
                UnitTypeData.slavVinnie.MaxCrouchSpeed = 3f;
                UnitTypeData.slavVinnie.MaxRollSpeed = 18f;
                UnitTypeData.slavVinnie.MinRollSpeed = 7.5f;
                UnitTypeData.vinnieMcClane.PushbackDuration = 0f;
                UnitTypeData.vinnieMcClane.PushbackTransitionTime = 0f;
                UnitTypeData.vinnieMcClane.PushbackFactor = 0f;
                UnitTypeData.vindicator.CanBeHeadshot = true;
                WeaponTypeData.mauser.ProjectileType.PenetrationOnlyAfterKill = false;
                CharacterTraitsPlugin.Log("Character Traits 静态修改已应用");
            }
            catch (System.Exception ex)
            {
                CharacterTraitsPlugin.Log("Character Traits 静态修改过程中发生异常: " + ex);
            }
        }
    }
}