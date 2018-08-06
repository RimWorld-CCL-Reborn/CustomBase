namespace CustomFactionBase
{
    using System.Linq;
    using System.Xml;
    using JetBrains.Annotations;
    using RimWorld;
    using Verse;

    public abstract class SubstituteBase
    {
        public string     symbol;
        public ThingDef building;

        public virtual Thing GetBuilding() =>
            ThingMaker.MakeThing(def: this.building, stuff: GenStuff.DefaultStuffFor(bd: this.building));
    }

    public class Substitute : SubstituteBase
    {
        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            this.symbol = xmlRoot.Name;
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(wanter: this, fieldName: nameof(this.building), targetDefName: xmlRoot.FirstChild.Value);
        }
    }

    public class SubstituteExtended : SubstituteBase
    {
        public ThingDef stuff;

        public override Thing GetBuilding() => ThingMaker.MakeThing(def: this.building, stuff: this.stuff ?? GenStuff.DefaultStuffFor(bd: this.building));
    }
}