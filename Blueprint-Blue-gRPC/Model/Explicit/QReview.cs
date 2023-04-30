using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QReview : QExplicitCommand, ICommand
    {
        string[] Arguments { get; set; }
        public QReview(QContext env, string text, Parsed[] args) : base(env, text, "review")
        {
            this.Arguments = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
                this.Arguments[i] = args[i].text;
        }
    }
}