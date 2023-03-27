using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QClear : QVariable, ICommand
    {
        public QClear(QEnvironment env, string text, string key, bool persistent) : base(env, text, "clear", key, string.Empty, persistent)
        {
            ;
        }
    }
}