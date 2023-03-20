namespace Blueprint.Blue
{
    public class QClear : QVariable, ICommand
    {
        public QClear(QEnvironment env, string text, string key) : base(env, text, key, string.Empty)
        {
            ;
        }
    }
}