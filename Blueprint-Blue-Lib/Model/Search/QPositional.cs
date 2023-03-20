namespace Blueprint.Blue
{
    public class QPositional : QTerm, ITerm
    {
        public int Position { get; set; }

        public QPositional(string text) : base(text)
        {
            this.Position = 0;
        }
    }
}