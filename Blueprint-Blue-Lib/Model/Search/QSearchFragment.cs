namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QSearchFragment
    {
        private string Text;
        public List<QFeature> Features { get; private set; }
        public QFind Search { get; private set; }
        public QSearchFragment(QFind context, string text, Parsed[] args)
        {
            this.Text = text;
            this.Features = new();
            this.Search = context;

            foreach (var arg in args)
            {
                var feature = QFeature.Create(context, arg.text, arg);
                if (feature != null)
                    this.Features.Add(feature);
                else
                    this.Search.Context.AddError("A feature was identified that could not be parsed: " + text);
            }
        }
    }
}