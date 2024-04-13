namespace Blueprint.Blue
{
    using AVSearch;
    using AVSearch.Interfaces;
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

        public QFragment(QFind context, Parsed frag, bool anchored = false): base(frag.text)
        {
            this.Search = context;
            this.Anchored = anchored;

            var option = new QMatchAny(context, frag.text, frag.children);
            if (option != null)
                this.AllOf.Add(option);
            else
                this.Search.AddError("A feature was identified that could not be parsed: " + frag.text);
        }
        public List<string> AsYaml()
        {
            return ICommand.YamlSerializer(this);
        }
    }
}