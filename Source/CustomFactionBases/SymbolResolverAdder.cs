using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI.Group;
using static CustomFactionBase.CustomBaseUtility;

namespace CustomFactionBase
{
    internal static class SymbolResolverAdder
    {
        internal static List<ResolverStruct> resolvers = new List<ResolverStruct>();

        public static void _Resolve(this SymbolResolver _this, ResolveParams rp)
        {
            bool replaced = false;
            bool buildingsReplaced = false;
            bool pawnsReplaced = false;
            //Log.Message(resolvers.Count.ToString());
            List<string> stack = new List<string>();
            if (resolvers.Count > 0)
                foreach (ResolverStruct resolver in resolvers)
                    if (resolver.Enabler(rp))
                    {
                        if (resolver.Chance)
                        {
                            stack.Add(resolver.RuleDefName);
                            if (!replaced)
                                replaced = resolver.AddMode == SymbolAddMode.Everything;
                            if (!buildingsReplaced)
                                buildingsReplaced = resolver.AddMode == SymbolAddMode.Buildings;
                            if (!pawnsReplaced)
                                pawnsReplaced = resolver.AddMode == SymbolAddMode.Pawns;
                        }
                    }

            if (replaced)
            {
                BaseGen.symbolStack.Clear();
                BaseGen.symbolStack.PushMany(rp, stack.ToArray());
            } else
            { 
                Map map = BaseGen.globalSettings.map;
                Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction(false, false);
                CellRect cellRect = rp.rect;
                bool flag = FactionBaseSymbolResolverUtility.ShouldUseSandbags(faction);
                if (flag)
                {
                    cellRect = cellRect.ContractedBy(4);
                }
                List<RoomOutline> roomOutlines =
                    RoomOutlinesGenerator.GenerateRoomOutlines(cellRect, map, ((IntRange)GetField(typeof(SymbolResolver_FactionBase), "RoomDivisionsCount")).RandomInRange,
                    ((IntRange)GetField(typeof(SymbolResolver_FactionBase), "FinalRoomsCount")).RandomInRange, 100, 62);
                InvokeMethod(typeof(SymbolResolver_FactionBase), "AddRoomCentersToRootsToUnfog", _this, roomOutlines);
                CellRect rect = rp.rect;

                if (!pawnsReplaced)
                {
                    Lord singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell), map, null);
                    ResolveParams resolveParams = rp;
                    resolveParams.rect = rect;
                    resolveParams.faction = faction;
                    resolveParams.singlePawnLord = singlePawnLord;

                    resolveParams.pawnGroupKindDef = (rp.pawnGroupKindDef ?? PawnGroupKindDefOf.FactionBase);
                    resolveParams.singlePawnSpawnCellExtraPredicate = (rp.singlePawnSpawnCellExtraPredicate ?? ((IntVec3 x) => (bool)InvokeMethod(typeof(SymbolResolver_FactionBase), "CanReachAnyRoom", _this, null, new object[] { x, roomOutlines })));
                    if (resolveParams.pawnGroupMakerParams == null)
                    {
                        float points = (!faction.def.techLevel.IsNeolithicOrWorse()) ? ((FloatRange)GetField(typeof(SymbolResolver_FactionBase), "NonNeolithicPawnsPoints", _this)).RandomInRange : ((FloatRange)GetField(typeof(SymbolResolver_FactionBase), "NeolithicPawnsPoints", _this)).RandomInRange;
                        resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms();
                        resolveParams.pawnGroupMakerParams.map = map;
                        resolveParams.pawnGroupMakerParams.faction = faction;
                        resolveParams.pawnGroupMakerParams.points = points;
                    }
                    BaseGen.symbolStack.Push("pawnGroup", resolveParams);
                }
                if (!faction.def.techLevel.IsNeolithicOrWorse() && cellRect.Area != 0)
                {
                    int randomInRange = ((IntRange)GetField(typeof(SymbolResolver_FactionBase), "FirefoamPoppersCount", _this)).RandomInRange;
                    for (int i = 0; i < randomInRange; i++)
                    {
                        ResolveParams resolveParams2 = rp;
                        resolveParams2.rect = cellRect;
                        resolveParams2.faction = faction;
                        BaseGen.symbolStack.Push("firefoamPopper", resolveParams2);
                    }
                }
                if (map.mapTemperature.OutdoorTemp < 0f && cellRect.Area != 0)
                {
                    int randomInRange2 = ((IntRange)GetField(typeof(SymbolResolver_FactionBase), "CampfiresCount",_this)).RandomInRange;
                    for (int j = 0; j < randomInRange2; j++)
                    {
                        ResolveParams resolveParams3 = rp;
                        resolveParams3.rect = cellRect;
                        resolveParams3.faction = faction;
                        BaseGen.symbolStack.Push("outdoorsCampfire", resolveParams3);
                    }
                }
                if (!buildingsReplaced)
                {
                    RoomOutline roomOutline = roomOutlines.MinBy((RoomOutline x) => x.CellsCountIgnoringWalls);
                    for (int k = 0; k < roomOutlines.Count; k++)
                    {
                        RoomOutline roomOutline2 = roomOutlines[k];
                        if (roomOutline2 == roomOutline)
                        {
                            ResolveParams resolveParams4 = rp;
                            resolveParams4.rect = roomOutline2.rect.ContractedBy(1);
                            resolveParams4.faction = faction;
                            BaseGen.symbolStack.Push("storage", resolveParams4);
                        }
                        else
                        {
                            ResolveParams resolveParams5 = rp;
                            resolveParams5.rect = roomOutline2.rect.ContractedBy(1);
                            resolveParams5.faction = faction;
                            BaseGen.symbolStack.Push("barracks", resolveParams5);
                        }
                        ResolveParams resolveParams6 = rp;
                        resolveParams6.rect = roomOutline2.rect;
                        resolveParams6.faction = faction;
                        BaseGen.symbolStack.Push("doors", resolveParams6);
                    }
                    for (int l = 0; l < roomOutlines.Count; l++)
                    {
                        RoomOutline roomOutline3 = roomOutlines[l];
                        ResolveParams resolveParams7 = rp;
                        resolveParams7.rect = roomOutline3.rect;
                        resolveParams7.faction = faction;
                        BaseGen.symbolStack.Push("emptyRoom", resolveParams7);
                    }
                    if (flag)
                    {
                        ResolveParams resolveParams8 = rp;
                        resolveParams8.rect = rp.rect;
                        resolveParams8.faction = faction;
                        float? chanceToSkipSandbag = rp.chanceToSkipSandbag;
                        resolveParams8.chanceToSkipSandbag = new float?((!chanceToSkipSandbag.HasValue) ? 0.25f : chanceToSkipSandbag.Value);
                        BaseGen.symbolStack.Push("edgeSandbags", resolveParams8);
                    }
                }
            }
        }


        internal static object GetField(Type type, string fieldName, object obj = null)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(obj);
        }

        internal static void SetField(Type type, string fieldName, object value, object obj = null)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            field.SetValue(obj, value);
        }

        internal static object InvokeMethod(Type type, string fieldName, object obj = null, object value = null, object[] values = null)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            MethodInfo method = type.GetMethod(fieldName, bindFlags);
            return method.Invoke(obj, values != null ? values : value != null ? new object[] { value } : null);
        }
    }
}
