using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace FinallyDecentMaps
{
    [BepInPlugin("Harb.FinallyDecentMaps", "FinallyDecentMaps", "1.0.0")]
    [BepInDependency("Harb.DisableProgression", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            IL.TileManager.SpawnNewTile += TileManager_SpawnNewTile;
        }

        private void TileManager_SpawnNewTile(ILContext il)
        {
            ILCursor c = new(il);
            int index = 7;
            //Find our jumpto label to only add left tiles when possible
            //deadEndTiles is pretty unique marker
            c.GotoNext(x => x.MatchLdfld<TileManager>("deadEndTiles"));
            //Right before the if
            c.GotoPrev(x => x.MatchBrtrue(out _));
            //at the start of the 'line'
            c.GotoPrev(MoveType.Before, x => x.MatchLdloc(out index));
            // mark it
            var label = c.MarkLabel();
            //And place it after the Ltiles have been added.
            c.GotoPrev(
                MoveType.After,
                x => x.MatchLdfld<TileManager>("Ltiles"));
            c.Index++;
            c.Emit(OpCodes.Br, label);

            //Position our cursor after the last TileManager loadfield we do NOT want to replace
            c.GotoNext(MoveType.After, x => x.MatchLdfld<TileManager>("Ttiles"));

            //Fucking yeet the other tiles 
            while (c.TryGotoNext(MoveType.Before,
                x => x.OpCode == OpCodes.Ldfld && !x.MatchLdfld<TileManager>("deadEndTiles"),
                x => x.OpCode == OpCodes.Callvirt))
            {
                c.Emit(OpCodes.Pop);
                c.Emit(OpCodes.Pop);
                c.RemoveRange(2);
            }
        }
    }
}
