namespace Blueprint.Blue
{
    public class QFind : QImplicitCommand, ICommand
    {
        public bool IsQuoted { get; set; }
        public List<QSearchSegment> Segments { get; set; }

        public QFind(QEnvironment env, string text) : base(env, text)
        {
            this.IsQuoted = false;
            this.Segments = new();
        }
    }
}