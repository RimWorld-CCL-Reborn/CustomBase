namespace CustomFactionBase
{
    using System;
    using System.Linq;
    using Harmony;
    using RimWorld;
    using RimWorld.BaseGen;
    using Verse;

    [StaticConstructorOnStartup]
    public static class CustomBaseUtility
    {
        private static CustomFactionBaseDef[] factionBaseDefs;

        public static CustomFactionBaseDef[] FactionBaseDefs => factionBaseDefs ?? (factionBaseDefs = DefDatabase<CustomFactionBaseDef>.AllDefs.ToArray());

        static CustomBaseUtility()
        {
            HarmonyInstance harmony = HarmonyInstance.Create(id: "rimworld.erdelf.customBaseUtility");

            harmony.Patch(original: AccessTools.Method(type: typeof(SymbolResolver_Settlement), name: nameof(SymbolResolver_Settlement.Resolve)), 
                prefix: new HarmonyMethod(type: typeof(CustomBaseUtility), name: nameof(FactionBasePrefix)), postfix: null);
        }


        private static bool FactionBasePrefix(ref ResolveParams rp)
        {            
            rp.faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction();

            foreach(CustomFactionBaseDef rs in FactionBaseDefs)
            {
                if (!rs.Active(rp: rp)) continue;
                rp.rect = rs.GetRect(rp: rp);
                ReplaceFloor(rect: rp.rect.ExpandedBy(dist: 2), floor: TerrainDefOf.Concrete);
                rs.GenerateBase(rp: rp);
                
                BaseGen.symbolStack.Push(symbol: "pawnGroup", resolveParams: rp);
                return false;
            }
            
            return true;
        }

        public static void ReplaceFloor(CellRect rect, TerrainDef floor)
        {
            Map map = BaseGen.globalSettings.map;
            LongEventHandler.QueueLongEvent(action: delegate
            {
                TerrainGrid terrainGrid = map.terrainGrid;
                TerrainDef newTerr = floor;

                foreach(IntVec3 cell in rect)
                    terrainGrid.SetTerrain(c: cell, newTerr: newTerr);
            }, textKey: "floor" + DateTime.Now.GetHashCode(), doAsynchronously: false, exceptionHandler: null);
        }
    }
}
