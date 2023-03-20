namespace Blueprint.Blue
{
   public interface ICommand
    {
        public string Text { get; set; }
        public string Verb { get; set; }
        public string Help { get; set; }
        public bool IsExplicit { get; }
    }

    public class QCommand
    {
        public string Text { get; set; }
        public string Verb { get; set; }
        public string Help { get; set; }
        public QEnvironment Env { get; protected set; }

        public QCommand(QEnvironment env, string text)
        {
            this.Env = env;
            this.Text = text;
        }
    }
}