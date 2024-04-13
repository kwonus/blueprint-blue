using AVSearch.Interfaces;
using Pinshot.PEG;

namespace Blueprint.Blue
{
    public enum TagType
    {
        UNKNOWN = 0,
        History = 1,
        Macro = 2,
    }
    public class QUtilize : QCommand, ICommand
    {
        public string Tag { get; private set; } // macro
        public List<QFilter> Filters { get; private set; }
        public QSettings Settings { get; private set; }
        public QFind? Expression  { get; private set; }
        public TagType TagType { get; private set; }

        private QUtilize(QContext env, string text, string invocation, TagType type) : base(env, text, "use")
        {
            this.Filters = new();
            this.Settings = new QSettings(env.GlobalSettings);
            this.Tag = invocation;
            this.TagType = type;
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
            bool labelled = arg.rule.Equals("tag", StringComparison.InvariantCultureIgnoreCase);

            if (labelled)
            {
                var invocation = new QUtilize(env, text, arg.text, TagType.Macro);
                return invocation;
            }
            if (numerics)
            {
                var invocation = new QUtilize(env, text, arg.text, TagType.History);
                return invocation;
            }
            return null;
        }
    }
}