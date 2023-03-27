using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QFind : QImplicitCommand, ICommand
    {
        public bool IsQuoted { get; set; }
        public List<QSearchSegment> Segments { get; set; }
        private bool Valid;

        // Unquoted/Unordered segments
        private QFind(QEnvironment env, string text, Parsed[] args) : base(env, text, "find")
        {
            this.IsQuoted = false;
            this.Segments = new();
            this.Valid = (args.Length > 0) && (this.Segments != null);

            if (this.Valid)
            {
                foreach (var arg in args)
                {
                    this.Valid = arg.rule.Equals("segment") && (arg.children.Length > 0);
                    if (!this.Valid)
                        break;
                    var seg = new QSearchSegment(this, arg.text, arg.children);
                    this.Valid = seg != null;
                    if (this.Valid)
                        this.Segments.Add(seg);
                    else
                        break;
                }
            }
        }
        // Quoted/Ordered segments
        private QFind(QEnvironment env, string text, Parsed arg, Parsed[] anchors) : base(env, text, "find")
        {
            this.Valid = false;

            this.IsQuoted = true;
            this.Segments = new();
        }
        public static QFind? Create(QEnvironment env, string text, Parsed[] args)
        {
            if (args.Length > 0 && args[0].children.Length > 0)
            {
                string fulltext = text.Trim();
                var beginQuote = fulltext.StartsWith("\"");
                var endQuote = fulltext.StartsWith("\"");

                if (beginQuote && endQuote)
                    return CreateQuoted(env, text, args);
                else if (beginQuote || endQuote)
                    return null;
                else
                    return CreateUnquoted(env, text, args);
            }
            return null;
        }
        private static QFind? CreateQuoted(QEnvironment env, string text, Parsed[] args)
        {
            QFind? segments = null;
            if ((args.Length == 1) && (args[0].children.Length > 0) && args[0].rule.Equals("ordered", StringComparison.InvariantCultureIgnoreCase))
                segments = new QFind(env, text, args[0], args[0].children);

            return (segments != null) && segments.Valid ? segments : null;
        }
        private static QFind? CreateUnquoted(QEnvironment env, string text, Parsed[] args)
        {
            QFind? segments = null;
            if ((args.Length >= 1) && (args[0].children.Length > 0) && args[0].rule.Equals("segment", StringComparison.InvariantCultureIgnoreCase))
                segments = new QFind(env, text, args);

            return (segments != null) && segments.Valid ? segments : null;
        }
    }
}