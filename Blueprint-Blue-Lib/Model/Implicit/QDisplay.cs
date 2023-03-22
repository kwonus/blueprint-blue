using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QDisplay : QImplicitCommand, ICommand
    {
        public List<uint> Fields { get; set; }

        private QDisplay(QEnvironment env, string text, IEnumerable<uint> fields) : base(env, text, "display")
        {
            this.Fields = new();

            foreach (var field in fields)
                this.Fields.Add(field);
        }
        public static QDisplay Create(QEnvironment env, string text, Parsed[] args)
        {
            return new QDisplay(env, text, new List<uint>());
        }
    }
}