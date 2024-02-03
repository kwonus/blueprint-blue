namespace Blueprint.Blue
{
    using AVSearch.Model.Expressions;
    using AVSearch.Model.Features;
    using AVSearch.Model.Types;
    using Pinshot.PEG;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using YamlDotNet.Serialization;

    public class QMatchAny: SearchMatchAny
    {
        [JsonIgnore]
        [YamlIgnore]
        public QFind Search { get; private set; }

        public QMatchAny(QFind context, string text, Parsed[] args): base(text)
        {
            if (this.AnyFeature.Count > 0)
                this.AnyFeature.Clear();
            this.Search = context;

            foreach (var arg in args)
            {
                var feature = FeatureFactory.Create(context, arg.text, arg);
                if (feature != null)
                    this.AnyFeature.Add(feature);
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