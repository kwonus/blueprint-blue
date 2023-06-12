namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;
    using XBlueprintBlue;
    public class QFind : QImplicitCommand, ICommand
    {
        public bool IsQuoted { get; set; }
        public List<QSearchSegment> Segments { get; set; }
        private bool Valid;

        public override string Expand()
        {
            if (!this.Valid)
                return string.Empty;

            return this.Text;
        }

        private QFind(QContext env, string text, Parsed[] args) : base(env, text, "find")
        {
            this.Segments = new();
            this.Valid = (args.Length > 0 && args[0].children.Length > 0);
            if (this.Valid)
            {
                string fulltext = text.Trim();
                var beginQuote = fulltext.StartsWith("\"");
                var endQuote = fulltext.StartsWith("\"");

                this.IsQuoted = beginQuote && endQuote;

                this.Valid = this.IsQuoted ? true : !fulltext.Contains('"');
            }

            if (this.Valid)
            {
                string rule = this.IsQuoted ? "ordered" : "unordered";
                this.Valid = (args.Length == 1) && args[0].rule.Equals(rule, StringComparison.InvariantCultureIgnoreCase) && (args[0].children.Length > 0);
            }
            if (this.Valid)
            {
                foreach (var arg in args[0].children)
                {
                    QSearchSegment seg;

                    this.Valid = arg.rule.Equals("segment") && (arg.children.Length > 0);
                    if (this.Valid)
                    {
                        seg = new QSearchSegment(this, arg.text, arg.children, anchored: this.IsQuoted);
                    }
                    else
                    {
                        this.Valid = arg.rule.Equals("unanchored") && (arg.children.Length > 0);
                        if (!this.Valid)
                            break;
                        seg = new QSearchSegment(this, arg.children[0].text, arg.children[0].children, anchored: false);
                    }
                    if (this.Valid)
                        this.Segments.Add(seg);
                    else
                        break;
                }
            }

        }
        public static QFind? Create(QContext env, string text, Parsed[] args)
        {
            QFind? search = new QFind(env, text, args);

            return search.Valid ? search : null;
        }
        public override List<string> AsYaml()
        {
            var yaml = new List<string>();
            if (this.Valid)
            {
                yaml.Add("- find: " + this.Text);
                yaml.Add("  quoted: " + this.IsQuoted.ToString().ToLower());

                foreach (var segment in this.Segments)
                {
                    var segments_yaml = segment.AsYaml();
                    foreach (var line in segments_yaml)
                    {
                        yaml.Add("  " + line);
                    }
                }
            }
            return yaml;
        }
        public XSearch AsMessage()
        {
            var search = new XSearch { Expression = this.Text, Quoted = this.IsQuoted, Segments = new List<XSegment>() };

            foreach (var segment in this.Segments)
            {
                search.Segments.Add(segment.AsMessage());
            }
            return search;
        }
    }
}