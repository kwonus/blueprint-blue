namespace Blueprint.Blue
{
   public interface ICommand
    {
        public string Text { get; }
        public string Verb { get; }
        public QContext Context { get; }
        public bool IsExplicit { get; }

        public string Expand();
    }

    public class QCommand
    {
        public string Text { get; set; }
        public string Verb { get; set; }
        public QContext Context { get; set; }
        public QHelpDoc HelpDoc { get => QHelpDoc.GetDocument(this.Verb); }

        protected QCommand(QContext context, string text, string verb)
        {
            this.Context = context;
            this.Text = text;
            this.Verb = verb;
        }
    }
}