namespace Blueprint.Blue
{
    public class QDisplay : QImplicitCommand, ICommand
    {
        public List<uint> Fields { get; set; }

        public QDisplay(QEnvironment env, string text, IEnumerable<uint> fields) : base(env, text)
        {
            foreach (var field in fields)
                this.Fields.Add(field);
        }
    }
}