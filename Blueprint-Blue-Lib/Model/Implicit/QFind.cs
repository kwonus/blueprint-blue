using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QFind : QImplicitCommand, ICommand
    {
        public bool IsQuoted { get; set; }
        public List<QSearchSegment> Segments { get; set; }

        private QFind(QEnvironment env, string text) : base(env, text, "find")
        {
            this.IsQuoted = false;
            this.Segments = new();
        }
        public static QFind Create(QEnvironment env, string text, Parsed[] args)
        {
            return new QFind(env, text);
        }
    }
}