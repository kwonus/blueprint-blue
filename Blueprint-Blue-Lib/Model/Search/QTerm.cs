namespace Blueprint.Blue
{
    public interface ITerm
    {
        public string Text { get; set; }
    }

    public class QTerm : ITerm
    {
        public string Text { get; set; }
        public QTerm(string text)
        {
            this.Text = text;
        }
    }
}