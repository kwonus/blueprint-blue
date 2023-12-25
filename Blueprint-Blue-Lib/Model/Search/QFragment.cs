namespace Blueprint.Blue
{
    using AVSearch;
    using AVSearch.Model.Expressions;
    using Pinshot.PEG;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using YamlDotNet.Serialization;

    public class QFragment: SearchFragment
    {
        [JsonIgnore]
        [YamlIgnore]
        public QFind Search { get; private set; }

        public QFragment(QFind context, string text, Parsed[] args, bool anchored = false)
        {
            this.Search = context;
            this.Anchored = anchored;

            foreach (var arg in args)
            {
                var option = new QMatchAny(context, arg.text, arg.children);
                if (option != null)
                    this.AllOf.Add(option);
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