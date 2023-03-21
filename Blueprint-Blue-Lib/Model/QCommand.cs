namespace Blueprint.Blue
{
   public interface ICommand
    {
        public string Text { get; set; }
        public string Verb { get; set; }
        public string HelpDoc { get; set; }
        public bool IsExplicit { get; }
    }

    public class QCommand
    {
        public string Text { get; set; }
        public string Verb { get; set; }
        public string HelpDoc { get; set; }
        public QEnvironment Env { get; protected set; }

        protected QCommand(QEnvironment env, string text)
        {
            this.Env = env;
            this.Text = text;
        }
    }
}