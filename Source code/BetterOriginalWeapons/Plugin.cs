using BepInEx;
using HarmonyLib;

namespace BetterOriginalWeapons
{
    [BepInPlugin("com.orangepeel.ggc2mods.betteroriginalweapons", "Better Original Weapons", "1.0.0")]
    public class ModPlugin : BaseUnityPlugin
    {
        public static ModPlugin Instance { get; private set; }

        // 简单的静态日志方法，方便其他类调用
        public static void Log(string message)
        {
            if (Instance != null && Instance.Logger != null)
            {
                Instance.Logger.LogInfo(message);
            }
            else
            {
                UnityEngine.Debug.Log(message);
            }
        }

        private void Awake()
        {
            Instance = this;

            // 应用 Harmony 补丁
            Harmony harmony = new Harmony("com.orangepeel.ggc2mods.betteroriginalweapons");
            harmony.PatchAll();

            Log("BetterOriginalWeapons 插件加载成功！");
        }
    }

    // Harmony 补丁：拦截 Data.Init，在初始化完成后应用武器修改
    [HarmonyPatch(typeof(Data), "Init", new System.Type[0])]
    public static class DataInitPatch
    {
        static void Postfix()
        {
            ModConfigManage.ApplyModifications();
        }
    }
}