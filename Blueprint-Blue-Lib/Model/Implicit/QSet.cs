namespace Blueprint.Blue
{
    public class QSet : QVariable, ICommand
    {
        public QSet(QEnvironment env, string text, string key, string value) : base(env, text, key, value)
        {
            ;
        }
    }
}