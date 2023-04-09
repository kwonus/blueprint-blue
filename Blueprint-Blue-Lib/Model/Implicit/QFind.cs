using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QFind : QImplicitCommand, ICommand
    {
        public bool IsQuoted { get; set; }
        public IPolarity Polarity { get; set; }
        public List<QSearchSegment> Segments { get; set; }
        private bool Valid;

        public override string Expand()
        {
            if (!this.Valid)
                return string.Empty;

            var polarity = this.Polarity.Text;
            if (polarity != string.Empty)
                return polarity + ' ' + this.Text;

            return polarity;
        }

        private QFind(QContext env, string text, Parsed[] args) : base(env, text, "find")
        {
            this.Segments = new();
            this.Polarity = QPolarityPositive.POLARITY_DEFAULT; // all search clauses are positive unless a -- polarity flag is encountered
            this.Valid = (args.Length > 0 && args[0].children.Length > 0);
            if (this.Valid)
            {
                string fulltext = text.Trim();
                var beginQuote = fulltext.StartsWith("\"");
                var endQuote = fulltext.StartsWith("\"");

                this.IsQuoted = true;

                this.Valid = !(fulltext.Contains('"') && !this.IsQuoted);
                    return;
            }

            if (this.Valid)
            {
                string rule = this.IsQuoted ? "ordered" : "unordered";
                this.Valid = (args.Length == 1 && args[0].rule == rule && args[0].children.Length > 0);
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
    }
}