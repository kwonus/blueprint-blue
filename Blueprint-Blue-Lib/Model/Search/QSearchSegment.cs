namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using XBlueprint;
    public class QSearchSegment
    {
        private string Text;
        public List<QSearchFragment> Fragments { get; private set; }
        public QFind Search { get; private set; }
        public bool Anchored { get; private set; }

        public QSearchSegment(QFind context, string text, Parsed[] args, bool anchored = false)
        {
            this.Text = text;
            this.Fragments = new();
            this.Search = context;
            this.Anchored = anchored;

            foreach (var arg in args)
            {
                var frag = new QSearchFragment(context, arg.text, arg.children);
                this.Fragments.Add(frag);
            }
            Anchored = anchored;
        }
        public List<string> AsYaml()
        {
            var yaml = new List<string>();

            yaml.Add("- segment: " + this.Text);
            yaml.Add("  anchored: " + this.Anchored.ToString().ToLower());

            foreach (var fragment in this.Fragments)
            {
                var fragment_yaml = fragment.AsYaml();
                foreach (var line in fragment_yaml)
                {
                    yaml.Add("  " + line);
                }
            }
            return yaml;
        }
        public XSegment AsMessage()
        {
            var segment = new XSegment { Anchored = this.Anchored, Segment = this.Text, Fragments = new List<XFragment>() };

            foreach (var fragment in this.Fragments)
            {
                segment.Fragments.Add(fragment.AsMessage());
            }
            return segment;
        }
    }
}