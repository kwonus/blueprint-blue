namespace Blueprint.Blue
{
    public class QPartOfSpeech : QTerm, ITerm
    {
        public int PnPos12 { get; set; }
        public int Pos32 { get; set; }

        public QPartOfSpeech(string text) : base(text)
        {
            this.PnPos12 = 0;
            this.Pos32 = 0;
        }
    }
}