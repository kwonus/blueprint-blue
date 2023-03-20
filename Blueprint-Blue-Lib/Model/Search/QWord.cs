namespace Blueprint.Blue
{
    public class QWord : QTerm, ITerm
    {
        public int WordKey { get; set; }

        public QWord(string text) : base(text)
        {
            ;
        }
    }
}