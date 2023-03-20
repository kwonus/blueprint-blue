namespace Blueprint.Blue
{
    public class QSearchFragment
    {
        private string Text;
        public List<ITerm> Terms { get; set; }
        public QSearchFragment(string text)
        {
            this.Text = text;
            this.Terms = new();
        }
    }
}