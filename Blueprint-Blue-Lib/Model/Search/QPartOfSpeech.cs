namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QPartOfSpeech : QFeature, IFeature
    {
        public int PnPos12 { get; set; }
        public int Pos32 { get; set; }

        public QPartOfSpeech(QFind search, string text, Parsed parse) : base(search, text, parse)
        {
            this.PnPos12 = 0;
            this.Pos32 = 0;
        }
    }
}