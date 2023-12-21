namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using YamlDotNet.Serialization;

    public class QFragment
    {
        private string Text;
        public List<QMatchAny> MatchAll { get; private set; }
        [JsonIgnore]
        [YamlIgnore]
        public QFind Search { get; private set; }
        public bool Anchored { get; private set; }

        public QFragment(QFind context, string text, Parsed[] args, bool anchored = false)
        {
            this.Text = text;
            this.MatchAll = new();
            this.Search = context;
            this.Anchored = anchored;

            foreach (var arg in args)
            {
                var option = new QMatchAny(context, arg.text, arg.children);
                if (option != null)
                    this.MatchAll.Add(option);
                else
                    this.Search.AddError("A feature was identified that could not be parsed: " + text);
            }
        }
        public List<string> AsYaml()
        {
            return ICommand.YamlSerializer(this);
        }
    }
}