using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace CustomFactionBase
{
    public static class CustomBaseUtility
    {
        public struct ResolverStruct
        {

            Predicate<ResolveParams> enabler;
            string ruleDefName;
            SymbolAddMode addMode;
            float chance;

            public Predicate<ResolveParams> Enabler
            {
                get
                {
                    return enabler;
                }
            }

            public string RuleDefName
            {
                get
                {
                    return ruleDefName;
                }
            }

            public SymbolAddMode AddMode
            {
                get
                {
                    return addMode;
                }
            }

            public bool Chance
            {
                get
                {
                    return Rand.Chance(chance / 100f);
                }
            }



            public ResolverStruct(Predicate<ResolveParams> enabler, string ruleDefName, SymbolAddMode mode, float chance)
            {
                this.enabler = enabler;
                this.ruleDefName = ruleDefName;
                this.addMode = mode;
                this.chance = chance;
            }
        }

        public enum SymbolAddMode : byte
        {
            Everything,
            Buildings,
            Pawns,
            NoReplacement
        }


        public static void addMapResolver(ResolverStruct struc)
        {
            SymbolResolverAdder.resolvers.Add(struc);
        }

        public static void replaceFloor(ResolveParams rp, TerrainDef floor)
        {
            Map map = BaseGen.globalSettings.map;
            LongEventHandler.QueueLongEvent(delegate
            {
                TerrainGrid terrainGrid = map.terrainGrid;
                TerrainDef newTerr = floor;
                CellRect.CellRectIterator iterator = rp.rect.GetIterator();
                while (!iterator.Done())
                {
                    terrainGrid.SetTerrain(iterator.Current, newTerr);
                    iterator.MoveNext();
                }
            }, "floor" + DateTime.Now.GetHashCode(), false, null);
        }

        public static void replaceWalls(ResolveParams rp, ThingDef wall, ThingDef door = null)
        {
            Map map = BaseGen.globalSettings.map;
            rp.rect = rp.rect.ExpandedBy(1);
            LongEventHandler.QueueLongEvent(delegate
            {
                foreach (IntVec3 current in rp.rect.EdgeCells)
                {

                    if (current.GetFirstThing(map, ThingDefOf.Wall) != null)
                    {
                        Thing regBar = ThingMaker.MakeThing(ThingDefOf.TemporaryRegionBarrier);
                        GenSpawn.Spawn(regBar, current, map);
                        GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Wall, wall), current, map);
                        regBar.Destroy(DestroyMode.Vanish);
                    }
                    else if (current.GetFirstThing(map, ThingDefOf.Door) != null)
                    {
                        Thing regBar = ThingMaker.MakeThing(ThingDefOf.TemporaryRegionBarrier);
                        GenSpawn.Spawn(regBar, current, map);
                        GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Door, door ?? wall), current, map);
                        regBar.Destroy(DestroyMode.Vanish);
                    }
                }
            }, "wall" + DateTime.Now.GetHashCode(), false, null);
        }
    }
}
