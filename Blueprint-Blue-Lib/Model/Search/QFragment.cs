namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Collections.Generic;
    using XBlueprintBlue;
    public class QFragment
    {
        private string Text;
        public List<QOptionGroup> Options { get; private set; }
        public QFind Search { get; private set; }
        public bool Anchored { get; private set; }

        public QFragment(QFind context, string text, Parsed[] args, bool anchored = false)
        {
            this.Text = text;
            this.Options = new();
            this.Search = context;
            this.Anchored = anchored;

            foreach (var arg in args)
            {
                var option = new QOptionGroup(context, arg.text, arg.children);
                if (option != null)
                    this.Options.Add(option);
                else
                    this.Search.Context.AddError("A feature was identified that could not be parsed: " + text);
            }
        }
        public List<string> AsYaml()
        {
            var yaml = new List<string>();

            yaml.Add("anchored: " + this.Anchored.ToString().ToLower());
            yaml.Add("- options: " + this.Text);

            foreach (var feature in this.Options)
            {
                var fragment_yaml = feature.AsYaml();
                foreach (var line in fragment_yaml)
                {
                    yaml.Add("  " + line);
                }
            }
            return yaml;
        }
        public XFragment AsMessage()
        {
            var fragment = new XFragment { Fragment = this.Text, Required = new List<XOption>() };

            foreach (var feature in this.Options)
            {
                fragment.Required.Add(feature.AsMessage());
            }
            return fragment;
        }
    }
}