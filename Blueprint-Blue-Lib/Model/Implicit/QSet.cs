using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QSet : QVariable, ICommand
    {
        public QSet(QContext env, string text, string key, string value, bool persistent) : base(env, text, "set", key, value, persistent)
        {
            ;
        }
    }
}