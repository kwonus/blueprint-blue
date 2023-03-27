using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QPartOfSpeech : QTerm, ITerm
    {
        public int PnPos12 { get; set; }
        public int Pos32 { get; set; }

        public QPartOfSpeech(string text, Parsed parse) : base(text, parse)
        {
            this.PnPos12 = 0;
            this.Pos32 = 0;
        }
    }
}