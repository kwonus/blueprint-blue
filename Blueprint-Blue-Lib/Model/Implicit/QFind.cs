namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;
    using YamlDotNet.Serialization;
    using static AVXLib.Framework.Numerics;

    public class QFind: IDiagnostic
    {
        private IDiagnostic Diagnostics;
        public QSettings Settings { get; private set; }
        public List<QFilter> Filters { get; private set; }
        public string Expression { get; private set; }
        public bool IsQuoted { get; set; }
        private bool Valid;
        public List<QFragment> Fragments { get; private set; }

        private QFind(IDiagnostic diagnostics, QSettings settings, string text, Parsed[] args)
        {
            this.Diagnostics = diagnostics;
            this.Settings = settings;
            this.Filters = new();
            this.Expression = text;

            this.Fragments = new();
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
                    QFragment frag;

                    this.Valid = arg.rule.Equals("fragment") && (arg.children.Length > 0);
                    if (this.Valid)
                    {
                        frag = new QFragment(this, arg.text, arg.children, anchored: this.IsQuoted);
                    }
                    else
                    {
                        this.Valid = arg.rule.Equals("unanchored") && (arg.children.Length > 0);
                        if (!this.Valid)
                            break;
                        frag = new QFragment(this, arg.children[0].text, arg.children[0].children, anchored: false);
                    }
                    if (this.Valid)
                        this.Fragments.Add(frag);
                    else
                        break;
                }
            }

        }
        public static QFind? Create(IDiagnostic diagnostics, QSettings settings, string text, Parsed[] args)
        {
            QFind? search = new QFind(diagnostics, settings, text, args);

            return search.Valid ? search : null;
        }
        public List<string> AsYaml()
        {
            return ICommand.YamlSerializer(this);
        }
        public void AddError(string message)
        {
            this.Diagnostics.AddError(message);
        }
        public void AddWarning(string message)
        {
            this.Diagnostics.AddWarning(message);
        }
    }
}