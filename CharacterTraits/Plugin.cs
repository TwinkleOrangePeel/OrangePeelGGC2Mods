using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace OrangePeelGGC2Mods.CharacterTraits
{
    [BepInPlugin("com.orangepeel.ggc2.charactertraits", "Character Traits & Secret Character Pack", "1.0.0")]
    public class CharacterTraitsPlugin : BaseUnityPlugin
    {
        public static CharacterTraitsPlugin Instance;

        public static void Log(string message)
        {
            if (Instance != null)
                Instance.Logger.LogInfo(message);
        }

        private void Awake()
        {
            Instance = this;
            try
            {
                Harmony harmony = new Harmony("com.orangepeel.ggc2.charactertraits");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                Log("Character Traits & Secret Character Pack 插件已加载");
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

    // 补丁：在 Data.Init 执行后应用静态修改
    [HarmonyPatch(typeof(Data), "Init")]

    public static class DataInitPatch_CharacterTraits
    {
        public static void Postfix()
        {
            if (CharacterTraitsPlugin.Instance != null)
            {
                ModConfigManage.ApplyModifications();
            }
        }
    }
}