using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QUtilizize : QCommand, ICommand
    {
        public string Generic { get; private set; } // either
        public string? Label { get; private set; } // macro
        public UInt64? Id { get; private set; } // history
        public List<QFilter> Filters { get; private set; }
        public QSettings Settings { get; private set; }
        public QFind? Expression { get; private set; }
        public bool Partial { get; private set; }

        private QUtilizize(QContext env, string text, string invocation, bool partial) : base(env, text, "use")
        {
            this.Filters = new();
            this.Settings = new QSettings(env.GlobalSettings);
            this.Id = null;
            this.Label = null;
            this.Generic = invocation.Trim();
            this.Partial = partial;

            // TO DO: (YAML?)
            /*
             * Filters need to be read in from the macro definition
             * Settings need to be read in from the macro definition
             * Expression needs to be read in from the macro definition
             */
        }

        private QUtilizize(QContext env, string text, uint id, bool partial) : base(env, text, "use")
        {
            this.Filters = new();
            this.Settings = new QSettings(env.GlobalSettings);

            this.Id = id;
            this.Label = null;
            this.Generic = id.ToString();
            this.Partial = partial;

            // TO DO: (YAML?)
            /*
             * Filters need to be read in from the History file
             * Settings need to be read in from the History file
             * Expression needs to be read in from the History file
             */
        }

        public static QUtilizize? Create(QContext env, string text, Parsed[] args)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            bool partial = true;
            string generic = args.Length >= 1 && args[0].rule.ToLower().StartsWith("utilization_") == true && args[0].children.Length == 1 ? args[0].children[0].text : string.Empty;
            if (generic.Length >= 2 && (generic[0] == '#' || generic[0] == '$'))
            {
                generic = generic.Substring(1);
                partial = generic[0] == '#';
            }

            if (!string.IsNullOrWhiteSpace(generic))
            {
                bool numerics = args.Length >= 1 && args[0].children.Length == 1 && args[0].children[0].rule.ToLower().StartsWith("historic_") == true;
                bool labelled = args.Length >= 1 && args[0].children.Length == 1 && args[0].children[0].rule.ToLower().StartsWith("label_") == true;

                if (labelled)
                {
                    var invocation = QUtilizize.Create(env, text, generic, args, partial);
                    return invocation;
                }
                if (numerics)
                {
                    uint id = 0;
                    try
                    {
                        id = uint.Parse(generic);
                    }
                    catch
                    {
                        return null;
                    }
                    var invocation = QUtilizize.Create(env, text, id, args, partial);
                    return invocation;
                }
            }
            return null;
        }

        private static QUtilizize? Create(QContext env, string text, string label, Parsed[] args, bool partial)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var invocation = new QUtilizize(env, text, label, partial);

            return invocation;
        }
        private static QUtilizize? Create(QContext env, string text, uint id, Parsed[] args, bool partial)
        {
            if (string.IsNullOrWhiteSpace(text) || (id == 0))
                return null;

            var invocation = new QUtilizize(env, text, id, partial);
            return invocation;
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