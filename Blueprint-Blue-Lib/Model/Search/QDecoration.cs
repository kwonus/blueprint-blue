namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Collections.Generic;

    public class QDecoration : QFeature, IFeature
    {
        override public string Type { get => QFeature.GetTypeName(this); }
        public byte Decoration { get; set; }

        public QDecoration(QFind search, string text, Parsed parse, bool negate) : base(search, text, parse, negate)
        {
            this.Decoration = 0;
        }
    }
}