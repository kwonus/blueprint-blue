using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QUtilize : QCommand, ICommand
    {
        public string Generic { get; private set; } // either
        public string? Label { get; private set; } // macro
        public UInt64? Id { get; private set; } // history
        public List<QFilter> Filters { get; private set; }
        public QSettings Settings { get; private set; }
        public QFind? Expression { get; private set; }

        private QUtilize(QContext env, string text, string invocation, string? label = null, UInt64? id = null) : base(env, text, "use")
        {
            this.Filters = new();
            this.Settings = new QSettings(env.GlobalSettings);
            this.Id = id;
            this.Label = label;
            this.Generic = invocation.Trim();

            // TO DO: (YAML?)
            /*
             * Filters need to be read in from the macro definition
             * Settings need to be read in from the macro definition
             * Expression needs to be read in from the macro definition
             */
        }

        public static QUtilize? Create(QContext env, string text, Parsed arg)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            bool numerics = arg.rule.Equals("id", StringComparison.InvariantCultureIgnoreCase);
            bool labelled = arg.rule.Equals("label", StringComparison.InvariantCultureIgnoreCase);

            if (labelled)
            {
                var invocation = new QUtilize(env, text, arg.text, label:arg.text);
                return invocation;
            }
            if (numerics)
            {
                uint id = 0;
                try
                {
                    id = uint.Parse(arg.text);
                }
                catch
                {
                    return null;
                }
                var invocation = new QUtilize(env, text, arg.text, id: id);
                return invocation;
            }
            return null;
        }

        /* CRUFT: Something like this should be in QInvoke, which is now an explicit command.
        public static QUtilizize? CreateExplicit(QContext env, string text, Parsed[] args, bool partial = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            string generic = args.Length >= 1 && args[0].rule.ToLower() == "use" && args[0].children.Length == 1 ? args[0].children[0].text : string.Empty;

            if (!string.IsNullOrWhiteSpace(generic))
            {
                bool numerics = args.Length >= 1 && args[0].children.Length == 1 && args[0].children[0].rule.ToLower() == "historic";
                bool labelled = args.Length >= 1 && args[0].children.Length == 1 && args[0].children[0].rule.ToLower() == "label";

                if (numerics || labelled)
                {
                    var invocation = new QUtilizize(env, text, generic);

                    // TO DO:
                    // load settings from context
                    // load expression from context

                    return invocation;
                }
            }
            return null;
        }
        */
    }
}