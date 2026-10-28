using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld.Planet;
using Vehicles;
using Vehicles.World;
using Verse;
using VFEMedieval;

namespace VFEM2MerchantGuildLandingFix
{
    // Patches from the Mod constructor rather than a StaticConstructorOnStartup class, so the work
    // happens during mod loading instead of in the UI freeze at the end of startup. The assembly is
    // passed explicitly: on .NET 10 (1.7) the parameterless PatchAll() reads the calling assembly off
    // the stack, which breaks when the JIT inlines the call (Harmony issue #395).
    public class MerchantGuildLandingFix : Mod
    {
        public MerchantGuildLandingFix(ModContentPack content) : base(content)
        {
            new Harmony("DanZinagri.VFEM2.MerchantGuildLandingFix").PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    // Vehicle Framework builds an aircraft's arrival options from the world object under the target. The merchant
    // guild is a MapParent that is neither a settlement nor a registered landing site, so it gets no options at all
    // and cannot be flown to. Registering it with AerialVehicleCompatibility would not help: that offers "land and
    // generate the map", which is an attack, not a trade visit.
    //
    // This runs once per target the player picks on the world map, never per tick.
    [HarmonyPatch(typeof(LaunchProtocol), nameof(LaunchProtocol.GetArrivalOptions))]
    public static class Patch_LaunchProtocol_GetArrivalOptions
    {
        public static IEnumerable<ArrivalOption> Postfix(IEnumerable<ArrivalOption> __result, VehiclePawn ___vehicle, GlobalTargetInfo target)
        {
            // A guild with a live map is left to Vehicle Framework, which offers landing in that map.
            if (!(target.WorldObject is MerchantGuild guild) || guild.HasMap || ___vehicle == null)
            {
                return __result;
            }
            if (!WorldVehiclePathGrid.Instance.Passable(target.Tile, ___vehicle.VehicleDef))
            {
                return Enumerable.Empty<ArrivalOption>();
            }
            // Same option Vehicle Framework gives an empty tile: land as a caravan, which can then trade with the guild.
            return new[] { new ArrivalOption("FormCaravanHere".Translate(), new ArrivalAction_LandToCaravan(___vehicle)) };
        }
    }
}
