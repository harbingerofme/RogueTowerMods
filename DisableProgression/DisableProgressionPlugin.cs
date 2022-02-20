using BepInEx;
using BepInEx.Configuration;
using MonoMod.RuntimeDetour;
using System.Collections.Generic;
using System.Linq;

namespace DisableProgression
{
    [BepInPlugin("Harb.DisableProgression", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class DisableProgressionPlugin : BaseUnityPlugin
    {
        private readonly List<Hook> hooks = new();

        public static DisableProgressionPlugin instance;

        public void Awake()
        {
            instance = this;

            var isEnabled = Config.Bind(new ConfigDefinition("", "Enabled"), true, new ConfigDescription("If you see this mod as a dependency, its because the mod author has requested for progression to be disabled. However, it's your game: do what you want.")).Value;

            if (!isEnabled)
            {
                return;
            }

            var voidM = typeof(DisableProgressionPlugin).GetMethod(nameof(voidMethod), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var stringM = typeof(DisableProgressionPlugin).GetMethod(nameof(stringMethod), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var intM = typeof(DisableProgressionPlugin).GetMethod(nameof(intMethod), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            var methods = typeof(AchievementManager).GetMethods(System.Reflection.BindingFlags.DeclaredOnly).Where((m) => m.ReturnType == typeof(void));
            foreach (var method in methods)
            {
                var paramCount = method.GetParameters().Length;
                Logger.LogDebug(method.Name);
                switch (paramCount)
                {
                    case 0: hooks.Add(new Hook(method, voidM)); break;
                    case 1:
                        var parM = method.GetParameters()[0].ParameterType;
                        if (parM == typeof(string))
                        {
                            hooks.Add(new Hook(method, stringM));
                        }
                        else if (parM == typeof(int))
                        {
                            hooks.Add(new Hook(method, intM));
                        }
                        break;
                }

            }
        }

        private delegate void intDelegate(AchievementManager am, int s);
        private delegate void stringDelegate(AchievementManager am, string s);
        private delegate void voidDelegate(AchievementManager am);

        private static void voidMethod(voidDelegate del, AchievementManager _0) { instance.Logger.LogDebug($"Call to {del.Method.Name} blocked."); }
        private static void stringMethod(stringDelegate del, AchievementManager _0, string _1) { instance.Logger.LogDebug($"Call to {del.Method.Name} blocked."); }
        private static void intMethod(intDelegate del, AchievementManager _0, int _1) { instance.Logger.LogDebug($"Call to {del.Method.Name} blocked."); }

        public void OnDestroy()
        {
            foreach (Hook hook in hooks)
            {
                hook.Undo();
                hook.Free();
            }
        }
    }
}
