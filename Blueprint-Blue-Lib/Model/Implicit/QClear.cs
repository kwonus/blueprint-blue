using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QClear : QVariable, ICommand
    {
        private QClear(QEnvironment env, string text, string key) : base(env, text, "clear", key, string.Empty)
        {
            ;
        }
        public static QClear Create(QEnvironment env, string text, Parsed[] args)
        {
            return new QClear(env, text, "foo");
        }
    }
}