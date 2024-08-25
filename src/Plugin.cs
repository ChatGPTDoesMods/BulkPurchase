using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace BuyAllButton;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    internal static HarmonyLib.Harmony Harmony;
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Harmony = new("mod.supermarkettogether.buyallbutton");
        Harmony.PatchAll();
    }
}
