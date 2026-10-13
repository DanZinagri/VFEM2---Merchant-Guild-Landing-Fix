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
    [StaticConstructorOnStartup]
    public static class MerchantGuildLandingFix
    {
        static MerchantGuildLandingFix()
        {
            // The assembly is passed explicitly: the parameterless PatchAll() finds it by walking the stack.
            new Harmony("DanZinagri.VFEM2.MerchantGuildLandingFix").PatchAll(typeof(MerchantGuildLandingFix).Assembly);
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
