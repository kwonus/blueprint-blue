using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QExport : QImplicitCommand, ICommand
    {
        public string FileSpec { get; set; }

        private QExport(QEnvironment env, string text, string spec) : base(env, text, "export")
        {
            this.FileSpec = spec;
        }
        public static QExport Create(QEnvironment env, string text, Parsed[] args)
        {
            return new QExport(env, text, "foo");
        }
    }
}