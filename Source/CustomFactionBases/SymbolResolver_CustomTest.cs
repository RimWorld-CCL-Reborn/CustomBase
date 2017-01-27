using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace CustomFactionBase
{

    class SymbolResolver_CustomTest : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction(false, false);
            CellRect cellRect = rp.rect;
            bool flag = FactionBaseSymbolResolverUtility.ShouldUseSandbags(faction);
            if (flag)
            {
                cellRect = cellRect.ContractedBy(4);
            }
            CellRect rect = rp.rect;
            Lord singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell), map, null);
            ResolveParams resolveParams = rp;
            resolveParams.rect = rect;
            resolveParams.faction = faction;
            resolveParams.singlePawnLord = singlePawnLord;

            resolveParams.pawnGroupKindDef = (rp.pawnGroupKindDef ?? PawnGroupKindDefOf.FactionBase);
            if (resolveParams.pawnGroupMakerParams == null)
            {
                float points = 50000;
                resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms();
                resolveParams.pawnGroupMakerParams.map = map;
                resolveParams.pawnGroupMakerParams.faction = faction;
                resolveParams.pawnGroupMakerParams.points = points;
            }
            BaseGen.symbolStack.Push("pawnGroup", resolveParams);
        }
    }
}
