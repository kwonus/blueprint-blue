namespace Blueprint.Blue
{
    public class QSearchSegment
    {
        private string Text;
        public List<QSearchFragment> Fragments { get; set; }

        public QSearchSegment(string text)
        {
            this.Text = text;
            this.Fragments = new();
        }
    }
}