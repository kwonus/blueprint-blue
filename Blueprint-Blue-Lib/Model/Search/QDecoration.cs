namespace Blueprint.Blue
{
    public class QDecoration : QTerm, ITerm
    {
        public int Decoration { get; set; }

        public QDecoration(string text) : base(text)
        {
            this.Decoration = 0;
        }
    }
}