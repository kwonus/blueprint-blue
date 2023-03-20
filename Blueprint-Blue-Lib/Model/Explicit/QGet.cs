namespace Blueprint.Blue
{
    public class QGet : QExplicitCommand, ICommand
    {
        public string Key { get; set; }
        public QGet(QEnvironment env, string text, string key) : base(env, text)
        {
            this.Key = key;
        }
    }
}