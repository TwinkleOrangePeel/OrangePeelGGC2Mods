using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace OrangePeelGGC2Mods.ImpossiblePlusMode
{
    [BepInPlugin("com.orangepeel.ggc2.impossibleplusmode", "Impossible+ Mode", "1.0.0")]
    public class ImpossiblePlusModePlugin : BaseUnityPlugin
    {
        public static ImpossiblePlusModePlugin Instance;

        public static void Log(string message)
        {
            if (Instance != null)
            {
                Instance.Logger.LogInfo(message);
            }
            // 若 Instance 为 null，日志无法输出，可忽略
        }

        private void Awake()
        {
            Instance = this;
            try
            {
                Harmony harmony = new Harmony("com.orangepeel.ggc2.impossibleplusmode");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                Log("Impossible+ Mode 插件已加载");
            }
            catch (System.Exception ex)
            {
                Log("初始化失败: " + ex);
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }

    // 补丁类：在 Data.Init 执行后应用所有修改
    [HarmonyPatch(typeof(Data), "Init")]
    public static class DataInitPatch_ImpossiblePlus
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            // 确保插件实例已就绪，防止空引用
            if (ImpossiblePlusModePlugin.Instance != null)
            {
                ModConfigManage.ApplyModifications();
            }
        }
    }
}