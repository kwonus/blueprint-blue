namespace Blueprint.Blue
{
    using AVSearch.Model.Features;
    using Pinshot.PEG;
    using static AVXLib.Framework.Numerics;

    public class QEntity : FeatureEntity
    {
        public QEntity(QFind search, string text, Parsed parse, bool negate) : base(text, negate, search.Settings)
        {
            switch (parse.text)
            {
                case "person":      this.Entity = (UInt16)(Entities.men | Entities.women); return;
                case "man":         this.Entity = Entities.men; return;
                case "woman":       this.Entity = Entities.women; return;
                case "tribe":       this.Entity = Entities.tribes; return;
                case "city":        this.Entity = Entities.cities; return;
                case "river":       this.Entity = Entities.rivers; return;
                case "mountain":    this.Entity = Entities.mountains; return;
                case "animal":      this.Entity = Entities.animals; return;
                case "gemstone":    this.Entity = Entities.gemstones; return;
                case "measurement": this.Entity = Entities.measurements; return;
                case "hitchcock_any":
                case "any_hitchcock":
                case "hitchcock":   this.Entity = Entities.Hitchcock; return;
                case "any":         this.Entity = 0xFFFF; return;
            }

            this.Entity = 0;
        }
    }
}