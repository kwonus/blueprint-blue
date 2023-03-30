namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QSearchSegment
    {
        private string Text;
        public List<QSearchFragment> Fragments { get; private set; }
        public QFind Context { get; private set; }

        public QSearchSegment(QFind context, string text, Parsed[] args)
        {
            this.Text = text;
            this.Fragments = new();
            this.Context = context;

            foreach (var arg in args)
            {
                var frag = new QSearchFragment(context, arg.text, arg.children);
                this.Fragments.Add(frag);
            }
        }
    }
}