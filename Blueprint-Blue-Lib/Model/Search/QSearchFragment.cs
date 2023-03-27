using BlueprintBlue.Model.Search;
using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QSearchFragment
    {
        private string Text;
        public List<QSearchFeature> Features { get; private set; }
        public QFind Context { get; private set; }
        public QSearchFragment(QFind context, string text, Parsed[] args)
        {
            this.Text = text;
            this.Features = new();
            this.Context = context;

            foreach (var arg in args)
            {
                var feature = new QSearchFeature(context, arg.text, arg.children);
                this.Features.Add(feature);
            }
        }
    }
}