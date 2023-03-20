namespace Blueprint.Blue
{
    public class QVariable : QImplicitCommand, ICommand
    {
        public string Key { get; protected set; }
        public string Value { get; protected set; }

        public QVariable(QEnvironment env, string text, string key, string value) : base(env, text)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}