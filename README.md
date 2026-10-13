# VFEM2 - Merchant Guild Landing Fix

Lets Vehicle Framework aircraft fly to VFE Medieval 2's travelling merchant guild and land beside it to trade.

**Problem.** Vehicle Framework builds an aircraft's arrival options from the world object under the target. The merchant
guild (`VFEMedieval.MerchantGuild`, a VEF `MovingBase`) is a `MapParent` that is neither a settlement nor a registered
landing site, so it gets no options and cannot be flown to.

**Fix.** One Harmony postfix on `Vehicles.LaunchProtocol.GetArrivalOptions` (id `DanZinagri.VFEM2.MerchantGuildLandingFix`):
when the target is the merchant guild and it has no map, the option is "Form caravan here" (land as a caravan on its
tile, where it can be traded with), or nothing if the vehicle cannot enter that tile. A guild that has a live map is left
to Vehicle Framework.

Registering the guild with `AerialVehicleCompatibility.RegisterWorldObjectType` was considered and rejected: that offers
"land and generate the map", which is an attack rather than a trade visit.

## History

Split out of Too Many Mods - Compats and Rebalances (`1.6/Mods/VFEM2_VF`, `VFEM2_VF.dll`) and rebuilt from source for
RimWorld 1.7 (net10.0). Differences from the original DLL:

- The prefix on `AerialVehicleCompatibility.CanLandIn` ("only while the guild is stopped") is gone. It never had any
  effect: the only caller of `CanLandIn` is the iterator this mod's postfix replaces without enumerating.
- A guild with a live map keeps Vehicle Framework's own "land in existing map" option instead of losing it.
- The vehicle comes from Harmony's `___vehicle` injection instead of a reflection `FieldRef`.
- `PatchAll` is given the assembly explicitly instead of finding it by walking the stack.

## Building

`dotnet build -c Release` in `Source`. Output goes to `1.7/Assemblies`. Vehicle Framework and VFE Medieval 2 had no 1.7
builds on 2026-10-13, so the project references their 1.6 DLLs (VEF is on its 1.7 build); repoint and rebuild when they
ship. Not tested in-game for the same reason.
