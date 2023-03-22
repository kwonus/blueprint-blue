namespace Blueprint.Blue
{
   public interface ICommand
    {
        public string Text { get; }
        public string Verb { get; }
        public QEnvironment Env { get; }
        public bool IsExplicit { get; }
    }

    public class QCommand
    {
        public string Text { get; set; }
        public string Verb { get; set; }
        public QEnvironment Env { get; set; }
        public QHelpDoc HelpDoc { get => QHelpDoc.GetDocument(this.Verb); }

        protected QCommand(QEnvironment env, string text, string verb)
        {
            this.Env = env;
            this.Text = text;
            this.Verb = verb;
        }
    }
}