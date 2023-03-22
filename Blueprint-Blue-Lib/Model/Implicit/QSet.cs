using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QSet : QVariable, ICommand
    {
        private QSet(QEnvironment env, string text, string key, string value) : base(env, text, "set", key, value)
        {
            ;
        }
        public static QSet Create(QEnvironment env, string text, Parsed[] args)
        {
            return new QSet(env, text, "foo", "bar");
        }
    }
}