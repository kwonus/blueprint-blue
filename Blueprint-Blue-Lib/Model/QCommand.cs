namespace Blueprint.Blue
{
    using System.Collections.Generic;
    using XBlueprintBlue;

    public interface ICommand
    {
        string Text { get; }
        string Verb { get; }
        QContext Context { get; }
        bool IsExplicit { get; }

        string Expand();
        List<string> AsYaml();
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
        virtual public List<string> AsYaml()
        {
            return new List<string>();
        }
    }
}