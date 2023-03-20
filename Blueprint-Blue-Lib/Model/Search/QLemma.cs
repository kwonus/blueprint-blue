namespace Blueprint.Blue
{
    public class QLemma : QTerm, ITerm
    {
        public int LemmaKey { get; set; }

        public QLemma(string text) : base(text)
        {
            this.Text = text;
        }
    }
}