It has been requested that mods can stop triumphs from being unlocked while active.
This mod does just that, no triumphs/achievements/challenges/whatevermajigs.

It does, however, allow xp.

---

For developers:

add this to your manifest to have modmanagers automatically enable it.
Use `[BepinDependency("Harb.DisableProgression", BepInDependency.DependencyFlags.HardDependency)]` at the top of your plugin class to prevent your mod loading without this mod installed.

-- Changelog

* 1.0.9 Fixed the GUID.