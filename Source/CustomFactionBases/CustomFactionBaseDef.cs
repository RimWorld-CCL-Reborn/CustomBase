namespace CustomFactionBase
{
    using System.Collections.Generic;
    using System.Linq;
    using RimWorld;
    using RimWorld.BaseGen;
    using Verse;

    public class CustomFactionBaseDef : Def
    {
        public FactionDef faction;

        [Unsaved]
        public Dictionary<char, SubstituteBase> substituteDictionary;

        public List<Substitute> substitutes = new List<Substitute>();
        public List<SubstituteExtended> extendedSubstitutes = new List<SubstituteExtended>();

        public string baseBuildings;


        public override void ResolveReferences()
        {
            base.ResolveReferences();
            this.substituteDictionary = this.substitutes.Cast<SubstituteBase>().Concat(second: this.extendedSubstitutes.Cast<SubstituteBase>()).
                ToDictionary(keySelector: s => s.symbol.First(), elementSelector: s => s);
        }


        public bool Active(ResolveParams rp) =>
            this.faction == rp.faction.def;

        public CellRect GetRect(ResolveParams rp)
        {
            string[] buildingLines = this.baseBuildings.Split('\n');
            int width = buildingLines.Max(selector: s => s.Length);

            return new CellRect(minX: rp.rect.CenterCell.x - width / 2, minZ: rp.rect.CenterCell.z - buildingLines.Length / 2,
                width: width, height: buildingLines.Length);
        }

        public void GenerateBase(ResolveParams rp)
        {
            string[] buildingLines = this.baseBuildings.Split('\n');
            
            for (int z = rp.rect.minZ; z <= rp.rect.maxZ; z++)
            {
                for (int x = rp.rect.minX; x <= rp.rect.maxX && x - rp.rect.minX < buildingLines[z-rp.rect.minZ].Length; x++)
                {
                    if (this.substituteDictionary.TryGetValue(key: buildingLines[z - rp.rect.minZ][index: x - rp.rect.minX], value: out SubstituteBase sub))
                        GenSpawn.Spawn(newThing: sub.GetBuilding(), loc: new IntVec3(newX: x, newY: 0, newZ: z), map: BaseGen.globalSettings.map);
                }
            }
        }
    }
}
