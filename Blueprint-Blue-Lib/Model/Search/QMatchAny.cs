namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using XBlueprintBlue;
    using YamlDotNet.Serialization;

    public class QMatchAny
    {
        private string Text;
        public List<QFeature> Features { get; private set; }
        [JsonIgnore]
        [YamlIgnore]
        public QFind Search { get; private set; }

        public QMatchAny(QFind context, string text, Parsed[] args)
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
        public List<string> AsYaml()
        {
            return ICommand.YamlSerializer(this);
        }
        public XOption AsMessage()
        {
            var option = new XOption { Option = this.Text, Features = new List<XFeature>() };

            foreach (var feature in this.Features)
            {
                option.Features.Add(feature.AsMessage());
            }
            return option;
        }
    }
}