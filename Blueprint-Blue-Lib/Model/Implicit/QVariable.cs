namespace Blueprint.Blue
{
    public class QVariable : QImplicitCommand, ICommand
    {
        public string Key { get; protected set; }
        public string Value { get; protected set; }

        protected QVariable(QEnvironment env, string text, string verb, string key, string value) : base(env, text, verb)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}